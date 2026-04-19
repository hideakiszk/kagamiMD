using Microsoft.Web.WebView2.Core;
using System.Text;
using KagamiMD.Configuration;
using KagamiMD.Interop;
using KagamiMD.Services;

namespace KagamiMD;

public partial class Form1 : Form
{
    private const int UndoTimerIntervalMs = 800;
    private const int PreviewDebounceIntervalMs = 300;
    private const int MouseWheelScrollDivisor = 40;
    private const int FileMenuBaseItemCount = 6;

    private readonly DocumentService _documentService = new DocumentService();
    private readonly MarkdownPreviewService _previewService = new MarkdownPreviewService();
    private readonly ScrollSyncCoordinator _scrollSyncCoordinator = new ScrollSyncCoordinator();
    private readonly DocumentSession _documentSession = new DocumentSession();

    private bool previewReady;
    private bool _previewInitialized;
    private int _currentTabWidth = 4;
    private bool _isSyncingFromWysiwyg;
    private SearchReplaceForm? _searchReplaceForm;
    private readonly UndoManager _undoManager = new UndoManager();
    private bool _isUndoingRedoing;
    private readonly System.Windows.Forms.Timer _undoTimer = new System.Windows.Forms.Timer();
    private readonly System.Windows.Forms.Timer _previewDebounceTimer = new System.Windows.Forms.Timer();

    public Form1()
    {
        InitializeComponent();
        try
        {
            using var iconStream = typeof(Form1).Assembly.GetManifestResourceStream("KagamiMD.KagamiMD.ico");
            if (iconStream != null)
            {
                this.Icon = new Icon(iconStream);
            }
        }
        catch { }
        openToolStripMenuItem.Click += (_, _) => OpenFile();
        saveToolStripMenuItem.Click += (_, _) => SaveFile();
        saveAsToolStripMenuItem.Click += (_, _) => SaveFileAs();
        savePdfToolStripMenuItem.Click += (_, _) => _ = SaveAsPdf();
        utf8BomToolStripMenuItem.Click += (_, _) => SelectSaveEncoding(utf8BomToolStripMenuItem);
        utf8NoBomToolStripMenuItem.Click += (_, _) => SelectSaveEncoding(utf8NoBomToolStripMenuItem);
        shiftJisToolStripMenuItem.Click += (_, _) => SelectSaveEncoding(shiftJisToolStripMenuItem);
        crlfToolStripMenuItem.Click += (_, _) => SelectLineEnding(crlfToolStripMenuItem, lfToolStripMenuItem);
        lfToolStripMenuItem.Click += (_, _) => SelectLineEnding(lfToolStripMenuItem, crlfToolStripMenuItem);
        wordWrapToolStripMenuItem.CheckedChanged += (_, _) => ApplyWordWrap(wordWrapToolStripMenuItem.Checked);
        ApplyWordWrap(true);
        Resize += (_, _) => {
            CenterSplitters();
            UpdateSyncScrollBar();
        };
        previewUpdateToolStripMenuItem.Click += (_, _) => UpdatePreview();
        Shown += async (_, _) =>
        {
            // EDR検知回避: WebView2のキャッシュを%LocalAppData%配下に配置し、
            // 実行ファイルの隣にexe/dllが動的生成されるパターン（マルウェア誤検知の原因）を回避する
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KagamiMD", "WebView2");
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await previewWebView.EnsureCoreWebView2Async(env);
            
            // Webブラウザ特有の操作を制限する
            var settings = previewWebView.CoreWebView2.Settings;
            settings.AreDefaultContextMenusEnabled = false; // 右クリックメニューを禁止
            settings.IsSwipeNavigationEnabled = false; // スワイプでの前へ/次へを禁止
            try
            {
                // バージョンによってはプロパティが存在しない可能性があるため、
                // Type経由でのアクセスかtry-catchで囲って念のため保護
                // .NETのラッパーとして定義されていれば通常は呼び出せる
                settings.GetType().GetProperty("AreBrowserAcceleratorKeysEnabled")?.SetValue(settings, false); // ショートカットキー(Alt+左など)を禁止
            }
            catch { }

            previewWebView.CoreWebView2.NavigationStarting += (sender, e) => 
            {
                if (e.IsUserInitiated && (e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://")) && !e.Uri.StartsWith("https://markdown-preview/") && !e.Uri.StartsWith("https://kagami-assets/"))
                {
                    e.Cancel = true;
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = e.Uri, UseShellExecute = true });
                    }
                    catch { }
                }
            };

            previewWebView.CoreWebView2.NewWindowRequested += (sender, e) => 
            {
                if ((e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://")) && !e.Uri.StartsWith("https://markdown-preview/") && !e.Uri.StartsWith("https://kagami-assets/"))
                {
                    e.Handled = true;
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = e.Uri, UseShellExecute = true });
                    }
                    catch { }
                }
            };

            previewWebView.CoreWebView2.WebMessageReceived += PreviewWebView_WebMessageReceived;
            await previewWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                window.addEventListener('keydown', (e) => {
                    if (e.key === 'F6') {
                        window.chrome.webview.postMessage('F6');
                        e.preventDefault();
                    } else if (e.key === 'F5') {
                        window.chrome.webview.postMessage('F5');
                        e.preventDefault();
                    }
                });
            ");
            previewReady = true;
            CenterSplitters();
            UpdatePreview();
        };
        UpdateWindowTitle();
        editorTextBox.AllowDrop = true;
        editorTextBox.DragEnter += EditorTextBox_DragEnter;
        editorTextBox.DragDrop += EditorTextBox_DragDrop;
        tab1ToolStripMenuItem.Click += (_, _) => SetTabWidth(1);
        tab2ToolStripMenuItem.Click += (_, _) => SetTabWidth(2);
        tab4ToolStripMenuItem.Click += (_, _) => SetTabWidth(4);
        fontToolStripMenuItem.Click += (_, _) => SelectFont();
        findToolStripMenuItem.Click += (_, _) => ShowSearchReplace(false);
        replaceToolStripMenuItem.Click += (_, _) => ShowSearchReplace(true);
        realtimePreviewToolStripMenuItem.Click += (_, _) => SetPreviewMode(true);
        manualPreviewToolStripMenuItem.Click += (_, _) => SetPreviewMode(false);
        undoToolStripMenuItem.Click += (_, _) => Undo();
        redoToolStripMenuItem.Click += (_, _) => Redo();
        cutToolStripMenuItem.Click += (_, _) => editorTextBox.Cut();
        copyToolStripMenuItem.Click += (_, _) => editorTextBox.Copy();
        pasteToolStripMenuItem.Click += (_, _) => editorTextBox.Paste();
        editToolStripMenuItem.DropDownOpening += (_, _) => UpdateEditMenu();
        tableToolStripMenuItem.Click += (_, _) => InsertTableTemplate();
        textColorToolStripMenuItem.Click += (_, _) => InsertTextColorTemplate();
        linkToolStripMenuItem.Click += (_, _) => InsertLinkTemplate();
        showBothToolStripMenuItem.Click += (_, _) => SelectDisplayMode(true, true);
        showLeftOnlyToolStripMenuItem.Click += (_, _) => SelectDisplayMode(true, false);
        showRightOnlyToolStripMenuItem.Click += (_, _) => SelectDisplayMode(false, true);
        wysiwygToolStripMenuItem.CheckedChanged += WysiwygToolStripMenuItem_CheckedChanged;
        independentScrollToolStripMenuItem.Click += (_, _) => SelectScrollMode(false);
        syncedScrollToolStripMenuItem.Click += (_, _) => SelectScrollMode(true);
        syncVScrollBar.Scroll += SyncVScrollBar_Scroll;

        _undoTimer.Interval = UndoTimerIntervalMs;
        _undoTimer.Tick += (s, e) =>
        {
            _undoTimer.Stop();
            RecordUndoState();
        };

        // プレビュー更新のデバウンス（連続入力中の無駄なMarkdownパース+DOM更新を抑制）
        _previewDebounceTimer.Interval = PreviewDebounceIntervalMs;
        _previewDebounceTimer.Tick += (s, e) =>
        {
            _previewDebounceTimer.Stop();
            if (wysiwygToolStripMenuItem.Checked && !_isSyncingFromWysiwyg)
            {
                var escapedMarkdown = System.Text.Json.JsonSerializer.Serialize(editorTextBox.Text ?? string.Empty);
                previewWebView.CoreWebView2?.ExecuteScriptAsync($"if(window.updateWysiwygEditor) window.updateWysiwygEditor({escapedMarkdown});");
            }
            else if (!wysiwygToolStripMenuItem.Checked)
            {
                UpdatePreview();
            }
        };

        editorTextBox.KeyDown += EditorTextBox_KeyDown;
        // 初期状態を記録
        _undoManager.Clear(editorTextBox.Text, 0, EditorInterop.GetFirstVisibleLine(editorTextBox.Handle));

        editorTextBox.TextChanged += (_, _) => 
        {
            if (!_documentSession.IsModified)
            {
                _documentSession.MarkModified();
                UpdateWindowTitle();
            }
            if (realtimePreviewToolStripMenuItem.Checked)
            {
                // WYSIWYGからの同期中（JavaScript側がオリジンの変更）は、JS側へ送り返さないようにタイマーを開始しない
                if (wysiwygToolStripMenuItem.Checked && _isSyncingFromWysiwyg)
                {
                    // 何もしない
                }
                else
                {
                    // デバウンス: 連続入力中はタイマーをリセットし、入力が落ち着いてから更新
                    _previewDebounceTimer.Stop();
                    _previewDebounceTimer.Start();
                }
            }

            if (!_isUndoingRedoing)
            {
                _undoTimer.Stop();
                _undoTimer.Start();
            }
            UpdateSyncScrollBar();
        };

        editorTextBox.KeyUp += (_, _) => UpdateSyncFromEditor();
        editorTextBox.MouseDown += (_, _) => UpdateSyncFromEditor();
        editorTextBox.MouseWheel += EditorTextBox_MouseWheel;

        previewWebView.NavigationCompleted += (_, _) => {
            UpdateSyncScrollBar();
            if (wysiwygToolStripMenuItem.Checked && _previewInitialized)
            {
                var escapedMarkdown = System.Text.Json.JsonSerializer.Serialize(editorTextBox.Text ?? string.Empty);
                previewWebView.CoreWebView2?.ExecuteScriptAsync($"if(window.initWysiwygEditorFromCSharp) window.initWysiwygEditorFromCSharp({escapedMarkdown});");
            }
        };

        FormClosing += Form1_FormClosing;

        // Load settings from AppConfig
        SettingsManager.LoadSettings(editorTextBox.Font);
        editorTextBox.Font = SettingsManager.Current.EditorFont;
        SetTabWidth(SettingsManager.Current.TabWidth);
        SetPreviewMode(SettingsManager.Current.RealTimePreview);
        UpdateFileMenuHistory();
    }

    private void SetPreviewMode(bool realTime)
    {
        realtimePreviewToolStripMenuItem.Checked = realTime;
        manualPreviewToolStripMenuItem.Checked = !realTime;
        SettingsManager.SavePreviewMode(realTime);
        if (realTime)
        {
            UpdatePreview();
        }
    }

    private void WysiwygToolStripMenuItem_CheckedChanged(object? sender, EventArgs e)
    {
        editorTextBox.ReadOnly = wysiwygToolStripMenuItem.Checked;
        _previewInitialized = false;
        UpdatePreview();
    }

    private void EditorTextBox_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data is not null && e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
        }
    }

    private void EditorTextBox_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data is not null && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
        {
            OpenFile(files[0]);
        }
    }

    private void CenterSplitters()
    {
        if (splitContainer1.Panel1Collapsed || splitContainer1.Panel2Collapsed)
        {
            return;
        }

        var availableWidth = splitContainer1.ClientSize.Width - splitContainer1.SplitterWidth;
        if (availableWidth <= 0)
        {
            return;
        }

        splitContainer1.SplitterDistance = availableWidth / 2;
    }

    private void SelectDisplayMode(bool showLeft, bool showRight)
    {
        splitContainer1.Panel1Collapsed = !showLeft;
        splitContainer1.Panel2Collapsed = !showRight;

        showBothToolStripMenuItem.Checked = showLeft && showRight;
        showLeftOnlyToolStripMenuItem.Checked = showLeft && !showRight;
        showRightOnlyToolStripMenuItem.Checked = !showLeft && showRight;

        if (!showLeft && showRight && syncedScrollToolStripMenuItem.Checked)
        {
            // 右側のみ表示かつ左右連動中の場合、独立スクロールに切り替える（操作不能になるのを防ぐ）
            SelectScrollMode(false);
        }

        if (showLeft && showRight)
        {
            CenterSplitters();
        }
    }

    /// <summary>
    /// 表示モードを「両方」→「左側のみ」→「右側のみ」の順でサイクル切り替えします。
    /// </summary>
    private void ToggleDisplayMode()
    {
        bool leftVisible = !splitContainer1.Panel1Collapsed;
        bool rightVisible = !splitContainer1.Panel2Collapsed;

        if (leftVisible && rightVisible)
        {
            // 両方 -> 左側のみ
            SelectDisplayMode(true, false);
        }
        else if (leftVisible && !rightVisible)
        {
            // 左側のみ -> 右側のみ
            SelectDisplayMode(false, true);
        }
        else
        {
            // 右側のみ（または何らかの異常状態） -> 両方
            SelectDisplayMode(true, true);
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F6)
        {
            ToggleDisplayMode();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void SelectScrollMode(bool synced)
    {
        independentScrollToolStripMenuItem.Checked = !synced;
        syncedScrollToolStripMenuItem.Checked = synced;

        syncVScrollBar.Visible = synced;
        ApplyWordWrap(wordWrapToolStripMenuItem.Checked); // 更新してスクロールバーの状態を反映

        if (synced)
        {
            UpdateSyncScrollBar();
            previewWebView.CoreWebView2?.ExecuteScriptAsync(ScrollSyncCoordinator.ScrollHideScript);
        }
        else
        {
            previewWebView.CoreWebView2?.ExecuteScriptAsync(ScrollSyncCoordinator.ScrollShowScript);
        }
    }

    private void UpdateSyncScrollBar()
    {
        if (!syncedScrollToolStripMenuItem.Checked) return;

        // 簡易的に行数に基づいたスクロール範囲を設定
        int totalLines = editorTextBox.Lines.Length;
        syncVScrollBar.Minimum = 0;
        syncVScrollBar.Maximum = Math.Max(0, totalLines);
        syncVScrollBar.LargeChange = Math.Max(1, editorTextBox.Height / editorTextBox.Font.Height);
    }

    private void SyncVScrollBar_Scroll(object? sender, ScrollEventArgs e)
    {
        SyncToLineIndex(e.NewValue);
    }

    private void EditorTextBox_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (!syncedScrollToolStripMenuItem.Checked) return;

        // マウスホイールによる手動スクロール（スクロールバーを非表示にしているため）
        int delta = -e.Delta / MouseWheelScrollDivisor; // 1ノッチ=120、÷40で3行分に相当
        int newValue = syncVScrollBar.Value + delta;
        
        int range = Math.Max(0, syncVScrollBar.Maximum - syncVScrollBar.LargeChange + 1);
        newValue = Math.Clamp(newValue, syncVScrollBar.Minimum, range);

        if (newValue != syncVScrollBar.Value)
        {
            syncVScrollBar.Value = newValue;
            SyncToLineIndex(newValue);
        }
    }

    private void PreviewWebView_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var msgStr = e.TryGetWebMessageAsString();

            if (msgStr == "F6")
            {
                ToggleDisplayMode();
                return;
            }
            if (msgStr == "F5")
            {
                UpdatePreview();
                return;
            }

            if (wysiwygToolStripMenuItem.Checked)
            {
                if (_isSyncingFromWysiwyg) return;
                
                if (msgStr != null)
                {
                    // WinForms TextBox requires CRLF for proper newline rendering
                    msgStr = msgStr.Replace("\r\n", "\n").Replace("\n", "\r\n");
                    
                    if (editorTextBox.Text != msgStr)
                    {
                        int currentLine = EditorInterop.GetFirstVisibleLine(editorTextBox.Handle);
                        _isSyncingFromWysiwyg = true;
                        EditorInterop.SuspendDrawing(editorTextBox.Handle);
                        try
                        {
                            editorTextBox.Text = msgStr;
                            EditorInterop.ScrollLines(editorTextBox.Handle, currentLine);
                            
                            if (!_documentSession.IsModified)
                            {
                                _documentSession.MarkModified();
                                UpdateWindowTitle();
                            }
                        
                        if (!_isUndoingRedoing)
                        {
                            _undoTimer.Stop();
                            _undoTimer.Start();
                        }
                    }
                    finally
                    {
                        EditorInterop.ResumeDrawing(editorTextBox.Handle);
                        editorTextBox.Invalidate();
                        editorTextBox.Update();
                        _isSyncingFromWysiwyg = false;
                    }
                    }
                    return;
                }
            }

            if (!syncedScrollToolStripMenuItem.Checked) return;

            if (double.TryParse(msgStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double ratio))
            {
                int val = syncVScrollBar.Value;
                _scrollSyncCoordinator.SyncEditorToRatio(editorTextBox.Handle, ratio, syncVScrollBar.Maximum, syncVScrollBar.LargeChange, ref val);
                syncVScrollBar.Value = val;
            }
        }
        catch { }
    }

    private void UpdateSyncFromEditor()
    {
        if (!syncedScrollToolStripMenuItem.Checked) return;

        int firstVisibleLine = EditorInterop.GetFirstVisibleLine(editorTextBox.Handle);
        
        // 最大値を超えないように制限
        int range = Math.Max(0, syncVScrollBar.Maximum - syncVScrollBar.LargeChange + 1);
        int constrainedLine = Math.Min(range, firstVisibleLine);

        if (syncVScrollBar.Value != constrainedLine)
        {
            syncVScrollBar.Value = constrainedLine;
            SyncWebViewToRatio((double)constrainedLine / Math.Max(1, range));
        }
    }

    private void SyncToLineIndex(int lineIndex)
    {
        _scrollSyncCoordinator.SyncToLineIndex(editorTextBox.Handle, lineIndex, out double ratio, syncVScrollBar.Maximum, syncVScrollBar.LargeChange);
        SyncWebViewToRatio(ratio);
    }

    private async void SyncWebViewToRatio(double ratio)
    {
        if (previewWebView.CoreWebView2 != null)
        {
            string script = ScrollSyncCoordinator.BuildWebViewScrollScript(ratio);
            await previewWebView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }

    private void SelectSaveEncoding(ToolStripMenuItem selectedItem)
    {
        utf8BomToolStripMenuItem.Checked = false;
        utf8NoBomToolStripMenuItem.Checked = false;
        shiftJisToolStripMenuItem.Checked = false;
        selectedItem.Checked = true;
    }

    private void SelectLineEnding(ToolStripMenuItem selectedItem, ToolStripMenuItem otherItem)
    {
        selectedItem.Checked = true;
        otherItem.Checked = false;
    }

    private void OpenFile(string? filePath = null)
    {
        if (filePath == null)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*",
                Title = "ファイルを開く"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            filePath = dialog.FileName;
        }

        if (!File.Exists(filePath))
        {
            MessageBox.Show(this, $"ファイルが見つかりません:\n{filePath}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            var history = SettingsManager.Current.FileHistory ?? new List<string>();
            history.RemoveAll(x => string.Equals(x, filePath, StringComparison.OrdinalIgnoreCase));
            SettingsManager.SaveFileHistory(history);
            UpdateFileMenuHistory();
            return;
        }

        _documentSession.Open(filePath);
        _previewInitialized = false;
        previewWebView.CoreWebView2?.ExecuteScriptAsync("sessionStorage.removeItem('previewScrollPos');");
        var content = _documentService.ReadFile(_documentSession.CurrentFilePath!, out var detectedEncoding);
        UpdateEncodingMenu(detectedEncoding);
        editorTextBox.Text = content;
        editorTextBox.SelectionStart = 0;
        editorTextBox.SelectionLength = 0;
        _undoManager.Clear(editorTextBox.Text, 0, EditorInterop.GetFirstVisibleLine(editorTextBox.Handle));
        UpdatePreview();
        UpdateWindowTitle();
        AddToHistory(_documentSession.CurrentFilePath!);
    }

    private void AddToHistory(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var history = SettingsManager.Current.FileHistory ?? new List<string>();
        history.RemoveAll(x => string.Equals(x, filePath, StringComparison.OrdinalIgnoreCase));
        history.Insert(0, filePath);
        SettingsManager.SaveFileHistory(history);
        UpdateFileMenuHistory();
    }

    private void UpdateFileMenuHistory()
    {
        while (fileToolStripMenuItem.DropDownItems.Count > FileMenuBaseItemCount)
        {
            fileToolStripMenuItem.DropDownItems.RemoveAt(6);
        }

        var history = SettingsManager.Current.FileHistory;
        if (history != null && history.Count > 0)
        {
            fileToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            for (int i = 0; i < history.Count; i++)
            {
                var path = history[i];
                var menuItem = new ToolStripMenuItem($"{i + 1}: {path}");
                menuItem.Click += (s, e) => OpenFile(path);
                fileToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }
    }

    private void RecordUndoState()
    {
        if (_isUndoingRedoing) return;
        int firstVisibleLine = EditorInterop.GetFirstVisibleLine(editorTextBox.Handle);
        _undoManager.RecordState(editorTextBox.Text, editorTextBox.SelectionStart, firstVisibleLine);
    }

    private void Undo()
    {
        if (_undoTimer.Enabled)
        {
            _undoTimer.Stop();
            RecordUndoState();
        }

        var state = _undoManager.Undo();
        if (state.HasValue)
        {
            ApplyState(state.Value);
        }
    }

    private void Redo()
    {
        if (_undoTimer.Enabled)
        {
            _undoTimer.Stop();
            RecordUndoState();
        }

        var state = _undoManager.Redo();
        if (state.HasValue)
        {
            ApplyState(state.Value);
        }
    }

    private void ApplyState((string Text, int SelectionStart, int FirstVisibleLine) state)
    {
        _isUndoingRedoing = true;

        // 描画を一時停止してちらつきを防止
        EditorInterop.SuspendDrawing(editorTextBox.Handle);
        try
        {
            editorTextBox.Text = state.Text;
            editorTextBox.SelectionStart = Math.Min(state.SelectionStart, editorTextBox.Text.Length);

            // Text代入後はスクロールが0行目にリセットされるため、保存済みスクロール位置まで相対移動
            EditorInterop.ScrollLines(editorTextBox.Handle, state.FirstVisibleLine);
        }
        finally
        {
            // 描画を再開して一度だけ再描画（ちらつきなし）
            EditorInterop.ResumeDrawing(editorTextBox.Handle);
            editorTextBox.Invalidate();
            editorTextBox.Update();
        }

        _isUndoingRedoing = false;

        // 同期スクロールバーを更新（UpdateSyncFromEditor はスクロール位置を上書きするため呼ばない）
        UpdateSyncScrollBar();
    }

    private void UpdateEditMenu()
    {
        // タイマーが動いている＝未記録の変更がある場合も Undo 可能とする
        undoToolStripMenuItem.Enabled = _undoManager.CanUndo || _undoTimer.Enabled;
        redoToolStripMenuItem.Enabled = _undoManager.CanRedo;

        cutToolStripMenuItem.Enabled = editorTextBox.SelectionLength > 0;
        copyToolStripMenuItem.Enabled = editorTextBox.SelectionLength > 0;
        pasteToolStripMenuItem.Enabled = Clipboard.ContainsText();
        selectAllToolStripMenuItem.Enabled = editorTextBox.TextLength > 0;
    }

    private void EditorTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Z)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            Undo();
        }
        else if (e.Control && e.KeyCode == Keys.Y)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            Redo();
        }
        else if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            RecordUndoState();
        }
        else if (e.KeyCode == Keys.Tab)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            ProcessTab(e.Shift);
        }
    }

    private void ProcessTab(bool shift)
    {
        int start = editorTextBox.SelectionStart;
        int length = editorTextBox.SelectionLength;

        // 選択範囲がなく Shift も押されていない場合は通常のタブ文字挿入
        if (length == 0 && !shift)
        {
            RecordUndoState();
            editorTextBox.SelectedText = "\t";
            return;
        }

        int startLine = editorTextBox.GetLineFromCharIndex(start);
        int endChar = start + length;
        int endLine = editorTextBox.GetLineFromCharIndex(endChar);

        // 範囲選択の末尾が次の行の先頭にある場合、その最終行は選択に含めない（一般的なエディタの挙動）
        if (length > 0 && editorTextBox.GetFirstCharIndexFromLine(endLine) == endChar)
        {
            endLine--;
        }

        if (startLine > endLine) return;

        RecordUndoState();

        int startPos = editorTextBox.GetFirstCharIndexFromLine(startLine);
        int lastLineEndPos = (endLine + 1 < editorTextBox.Lines.Length)
            ? editorTextBox.GetFirstCharIndexFromLine(endLine + 1)
            : editorTextBox.TextLength;

        var (newText, newLen) = TabIndenter.Apply(editorTextBox.Lines, startLine, endLine, _currentTabWidth, deindent: shift);

        // 選択範囲を全行分に拡大してから置換
        editorTextBox.SelectionStart = startPos;
        editorTextBox.SelectionLength = lastLineEndPos - startPos;
        editorTextBox.SelectedText = newText;

        // 選択範囲を保持
        editorTextBox.SelectionStart = startPos;
        editorTextBox.SelectionLength = newLen;
    }



    private void UpdateEncodingMenu(Encoding encoding)
    {
        utf8BomToolStripMenuItem.Checked = false;
        utf8NoBomToolStripMenuItem.Checked = false;
        shiftJisToolStripMenuItem.Checked = false;

        if (encoding is UTF8Encoding utf8)
        {
            if (utf8.GetPreamble().Length > 0)
                utf8BomToolStripMenuItem.Checked = true;
            else
                utf8NoBomToolStripMenuItem.Checked = true;
        }
        else if (encoding.CodePage == 932)
        {
            shiftJisToolStripMenuItem.Checked = true;
        }
    }

    private void SaveFile()
    {
        if (string.IsNullOrWhiteSpace(_documentSession.CurrentFilePath))
        {
            SaveFileAs();
            return;
        }

        WriteFile(_documentSession.CurrentFilePath);
        _documentSession.Save(_documentSession.CurrentFilePath);
        UpdateWindowTitle();
    }

    private void SaveFileAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Markdown files (*.md)|*.md|All files (*.*)|*.*",
            DefaultExt = "md",
            Title = "名前をつけて保存"
        };

        if (!string.IsNullOrWhiteSpace(_documentSession.CurrentFilePath))
        {
            dialog.FileName = Path.GetFileName(_documentSession.CurrentFilePath);
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var newPath = dialog.FileName;
        WriteFile(newPath);
        _documentSession.Save(newPath);
        UpdateWindowTitle();
        AddToHistory(newPath);
    }

    private void ApplyWordWrap(bool enabled)
    {
        editorTextBox.WordWrap = enabled;
        if (syncedScrollToolStripMenuItem.Checked)
        {
            editorTextBox.ScrollBars = ScrollBars.None;
        }
        else
        {
            editorTextBox.ScrollBars = enabled ? ScrollBars.Vertical : ScrollBars.Both;
        }
    }

    private void UpdateWindowTitle()
    {
        Text = _documentSession.GetWindowTitle();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_documentSession.IsModified) return;

        var result = MessageBox.Show(this, "変更内容を保存しますか？", "KagamiMD", MessageBoxButtons.YesNoCancel, MessageBoxIcon.None);

        if (result == DialogResult.Yes)
        {
            SaveFile();
            if (_documentSession.IsModified) e.Cancel = true; // Still modified means saving was canceled
        }
        else if (result == DialogResult.Cancel)
        {
            e.Cancel = true;
        }
    }

    private void WriteFile(string filePath)
    {
        _documentService.WriteFile(filePath, editorTextBox.Text ?? string.Empty, GetSelectedEncoding(), GetSelectedLineEnding());
    }

    private Encoding GetSelectedEncoding()
    {
        if (shiftJisToolStripMenuItem.Checked)
        {
            return Encoding.GetEncoding("shift_jis");
        }

        return utf8NoBomToolStripMenuItem.Checked
            ? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            : new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
    }

    private string GetSelectedLineEnding()
    {
        return lfToolStripMenuItem.Checked ? "\n" : "\r\n";
    }



    private void UpdatePreview()
    {
        if (!previewReady || previewWebView.CoreWebView2 is null)
        {
            return;
        }

        var directoryPath = _previewService.GetPreviewBaseDirectoryPath(_documentSession.CurrentFilePath);
        previewWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "markdown-preview",
            directoryPath,
            CoreWebView2HostResourceAccessKind.Allow);

        var assetsPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        previewWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "kagami-assets",
            assetsPath,
            CoreWebView2HostResourceAccessKind.Allow);

        if (wysiwygToolStripMenuItem.Checked)
        {
            if (!_previewInitialized)
            {
                previewWebView.CoreWebView2.Navigate("https://kagami-assets/wysiwyg.html");
                _previewInitialized = true;
            }
            else
            {
                // The editor is already initialized and visible, just send updates
                var escapedMarkdown = System.Text.Json.JsonSerializer.Serialize(editorTextBox.Text ?? string.Empty);
                previewWebView.CoreWebView2.ExecuteScriptAsync($"if(window.updateWysiwygEditor) window.updateWysiwygEditor({escapedMarkdown});");
            }
            return;
        }

        if (!_previewInitialized)
        {
            var html = _previewService.GenerateHtmlWrapper(editorTextBox.Text ?? string.Empty, "https://markdown-preview/");
            previewWebView.NavigateToString(html);
            _previewInitialized = true;
        }
        else
        {
            var script = _previewService.GenerateUpdateScript(editorTextBox.Text ?? string.Empty);
            previewWebView.CoreWebView2.ExecuteScriptAsync(script);
        }

        if (syncedScrollToolStripMenuItem.Checked)
        {
            previewWebView.CoreWebView2.ExecuteScriptAsync(ScrollSyncCoordinator.ScrollEventScript);
        }
    }

    private void SetTabWidth(int tabWidth)
    {
        _currentTabWidth = tabWidth;
        EditorInterop.SetTabWidth(editorTextBox.Handle, tabWidth);
        editorTextBox.Refresh();
        UpdateTabWidthMenu(tabWidth);
        SettingsManager.SaveTabWidth(tabWidth);
    }

    private void UpdateTabWidthMenu(int tabWidth)
    {
        tab1ToolStripMenuItem.Checked = (tabWidth == 1);
        tab2ToolStripMenuItem.Checked = (tabWidth == 2);
        tab4ToolStripMenuItem.Checked = (tabWidth == 4);
    }

    private void SelectFont()
    {
        using var dialog = new FontDialog
        {
            Font = editorTextBox.Font,
            ShowColor = false,
            AllowScriptChange = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            editorTextBox.Font = dialog.Font;
            // Tab width needs to be re-applied to match the new font metrics
            SetTabWidth(_currentTabWidth);
            SettingsManager.SaveFont(dialog.Font);
        }
    }

    private void ShowSearchReplace(bool replace)
    {
        if (_searchReplaceForm == null || _searchReplaceForm.IsDisposed)
        {
            _searchReplaceForm = new SearchReplaceForm(editorTextBox);
        }

        _searchReplaceForm.SetReplaceMode(replace);
        _searchReplaceForm.Show();
        _searchReplaceForm.BringToFront();
    }

    private async Task SaveAsPdf()
    {
        if (!previewReady || previewWebView.CoreWebView2 == null)
        {
            MessageBox.Show(this, "プレビューの準備ができていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
            DefaultExt = "pdf",
            Title = "PDFとして保存"
        };

        if (!string.IsNullOrWhiteSpace(_documentSession.CurrentFilePath))
        {
            dialog.FileName = Path.GetFileNameWithoutExtension(_documentSession.CurrentFilePath) + ".pdf";
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var settings = previewWebView.CoreWebView2.Environment.CreatePrintSettings();
            settings.ShouldPrintHeaderAndFooter = false;
            settings.ShouldPrintBackgrounds = true;

            bool success = await previewWebView.CoreWebView2.PrintToPdfAsync(dialog.FileName, settings);

            if (success)
            {
                MessageBox.Show(this, "PDFが保存されました。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "PDFの保存に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "エラーが発生しました: " + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InsertTableTemplate()
    {
        RecordUndoState();
        editorTextBox.SelectedText = EditorTemplates.Table;
    }

    private void InsertTextColorTemplate()
    {
        RecordUndoState();
        var start = editorTextBox.SelectionStart;
        var len = editorTextBox.SelectionLength;
        var selected = editorTextBox.SelectedText;

        editorTextBox.SelectedText = $"{EditorTemplates.TextColorPrefix}{selected}{EditorTemplates.TextColorSuffix}";

        if (len == 0)
        {
            editorTextBox.SelectionStart = start + EditorTemplates.TextColorPrefix.Length;
        }
        else
        {
            // タグも含めた全体を選択状態にする（再度色を変えたり削除したりしやすくするため）
            editorTextBox.SelectionStart = start;
            editorTextBox.SelectionLength = EditorTemplates.TextColorPrefix.Length + len + EditorTemplates.TextColorSuffix.Length;
        }
    }

    private void InsertLinkTemplate()
    {
        RecordUndoState();
        var start = editorTextBox.SelectionStart;
        var len = editorTextBox.SelectionLength;
        var selected = editorTextBox.SelectedText;

        if (len == 0)
        {
            editorTextBox.SelectedText = EditorTemplates.LinkTemplate;
            // 「リンクのテキスト」部分を選択（先頭の "[" の次から LinkTextLength 文字分）
            editorTextBox.SelectionStart = start + 1;
            editorTextBox.SelectionLength = EditorTemplates.LinkTextLength;
        }
        else
        {
            editorTextBox.SelectedText = $"{EditorTemplates.LinkPrefix}{selected}{EditorTemplates.LinkSuffix}";
            // 「リンクのアドレス」部分を選択状態にする（"[" + text + "](" の後）
            editorTextBox.SelectionStart = start + EditorTemplates.LinkPrefix.Length + len + 2;
            editorTextBox.SelectionLength = EditorTemplates.LinkAddressLength;
        }
    }
}
