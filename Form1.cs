using Markdig;
using Microsoft.Web.WebView2.Core;
using System.Runtime.InteropServices;
using System.Text;

namespace KagamiMD;

public partial class Form1 : Form
{
    private readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    private bool previewReady;
    private bool _previewInitialized;
    private string? currentFilePath;
    private int _currentTabWidth = 4;
    private bool _isModified;
    private SearchReplaceForm? _searchReplaceForm;
    private readonly UndoManager _undoManager = new UndoManager();
    private bool _isUndoingRedoing;
    private readonly System.Windows.Forms.Timer _undoTimer = new System.Windows.Forms.Timer();
    private List<string> _fileHistory = new List<string>();

    public Form1()
    {
        InitializeComponent();
        try
        {
            if (File.Exists("KagamiMD.ico"))
            {
                this.Icon = new Icon("KagamiMD.ico");
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
            await previewWebView.EnsureCoreWebView2Async();
            
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
                if (e.IsUserInitiated && (e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://")) && !e.Uri.StartsWith("https://markdown-preview/"))
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
                if ((e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://")) && !e.Uri.StartsWith("https://markdown-preview/"))
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
        independentScrollToolStripMenuItem.Click += (_, _) => SelectScrollMode(false);
        syncedScrollToolStripMenuItem.Click += (_, _) => SelectScrollMode(true);
        syncVScrollBar.Scroll += SyncVScrollBar_Scroll;

        _undoTimer.Interval = 800;
        _undoTimer.Tick += (s, e) =>
        {
            _undoTimer.Stop();
            RecordUndoState();
        };

        editorTextBox.KeyDown += EditorTextBox_KeyDown;
        // 初期状態を記録
        _undoManager.Clear(editorTextBox.Text, 0, (int)SendMessage(editorTextBox.Handle, EM_GETFIRSTVISIBLELINE, 0, IntPtr.Zero));

        editorTextBox.TextChanged += (_, _) => 
        {
            if (!_isModified)
            {
                _isModified = true;
                UpdateWindowTitle();
            }
            if (realtimePreviewToolStripMenuItem.Checked)
            {
                UpdatePreview();
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

        previewWebView.NavigationCompleted += (_, _) => UpdateSyncScrollBar();

        FormClosing += Form1_FormClosing;

        // Load settings from INI
        SettingsManager.LoadSettings(out var loadedTabWidth, out var loadedFont, out var loadedRealTime, out var loadedHistory, editorTextBox.Font);
        editorTextBox.Font = loadedFont;
        SetTabWidth(loadedTabWidth);
        SetPreviewMode(loadedRealTime);
        _fileHistory = loadedHistory;
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

    private void SelectScrollMode(bool synced)
    {
        independentScrollToolStripMenuItem.Checked = !synced;
        syncedScrollToolStripMenuItem.Checked = synced;

        syncVScrollBar.Visible = synced;
        ApplyWordWrap(wordWrapToolStripMenuItem.Checked); // 更新してスクロールバーの状態を反映

        if (synced)
        {
            UpdateSyncScrollBar();
            // WebView2のスクロールバーを表示させたまま、CSSで隠す（ホイール入力を有効にするため）
            previewWebView.CoreWebView2?.ExecuteScriptAsync("""
                (function() {
                    var styleId = 'kagami-scroll-hide';
                    if (!document.getElementById(styleId)) {
                        var style = document.createElement('style');
                        style.id = styleId;
                        style.innerHTML = '*::-webkit-scrollbar { display: none !important; } body { -ms-overflow-style: none !important; scrollbar-width: none !important; }';
                        document.head.appendChild(style);
                    }
                })()
                """);
        }
        else
        {
            // CSSを削除してスクロールバーを表示する
            previewWebView.CoreWebView2?.ExecuteScriptAsync("var s = document.getElementById('kagami-scroll-hide'); if (s) s.remove();");
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
        int delta = -e.Delta / 40; // 通常1ノッチ120なので、3行分に相当
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
        if (!syncedScrollToolStripMenuItem.Checked) return;

        try
        {
            var ratioStr = e.TryGetWebMessageAsString();
            if (double.TryParse(ratioStr, out double ratio))
            {
                int range = Math.Max(0, syncVScrollBar.Maximum - syncVScrollBar.LargeChange + 1);
                int lineIndex = (int)(range * ratio);
                
                if (syncVScrollBar.Value != lineIndex)
                {
                    syncVScrollBar.Value = lineIndex;
                    // エディタ側のみ同期（WebView側は自身ですでに移動済みのため、ループを防ぐ）
                    int firstVisibleLine = (int)SendMessage(editorTextBox.Handle, EM_GETFIRSTVISIBLELINE, 0, IntPtr.Zero);
                    int diff = lineIndex - firstVisibleLine;
                    SendMessage(editorTextBox.Handle, EM_LINESCROLL, 0, (IntPtr)diff);
                }
            }
        }
        catch { }
    }

    private void UpdateSyncFromEditor()
    {
        if (!syncedScrollToolStripMenuItem.Checked) return;

        int firstVisibleLine = (int)SendMessage(editorTextBox.Handle, EM_GETFIRSTVISIBLELINE, 0, IntPtr.Zero);
        
        // 最大値を超えないように制限
        int range = Math.Max(0, syncVScrollBar.Maximum - syncVScrollBar.LargeChange + 1);
        int constrainedLine = Math.Min(range, firstVisibleLine);

        if (syncVScrollBar.Value != constrainedLine)
        {
            syncVScrollBar.Value = constrainedLine;
            SyncWebViewToLine(constrainedLine);
        }
    }

    private void SyncToLineIndex(int lineIndex)
    {
        // エディタをスクロール
        int firstVisibleLine = (int)SendMessage(editorTextBox.Handle, EM_GETFIRSTVISIBLELINE, 0, IntPtr.Zero);
        int diff = lineIndex - firstVisibleLine;
        SendMessage(editorTextBox.Handle, EM_LINESCROLL, 0, (IntPtr)diff);

        // プレビューをスクロール
        SyncWebViewToLine(lineIndex);
    }

    private async void SyncWebViewToLine(int lineIndex)
    {
        if (previewWebView.CoreWebView2 != null)
        {
            // 割合計算
            double range = Math.Max(1, syncVScrollBar.Maximum - syncVScrollBar.LargeChange + 1);
            double ratio = (double)lineIndex / range;
            
            // WebView上の高さを取得してスクロール
            // 無限ループを防ぐため、JS側で一時的にイベントを無効化する
            string script = $$"""
                (function() {
                    var height = document.body.scrollHeight - window.innerHeight;
                    window.isInternalScrolling = true;
                    window.scrollTo(0, Math.max(0, height * {{ratio}}));
                })()
                """;
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
            _fileHistory.RemoveAll(x => string.Equals(x, filePath, StringComparison.OrdinalIgnoreCase));
            SettingsManager.SaveFileHistory(_fileHistory);
            UpdateFileMenuHistory();
            return;
        }

        currentFilePath = filePath;
        _previewInitialized = false;
        previewWebView.CoreWebView2?.ExecuteScriptAsync("sessionStorage.removeItem('previewScrollPos');");
        var detectedEncoding = DetectEncoding(currentFilePath);
        UpdateEncodingMenu(detectedEncoding);
        editorTextBox.Text = NormalizeLineEndings(File.ReadAllText(currentFilePath, detectedEncoding), "\r\n");
        editorTextBox.SelectionStart = 0;
        editorTextBox.SelectionLength = 0;
        _isModified = false;
        _undoManager.Clear(editorTextBox.Text, 0, (int)SendMessage(editorTextBox.Handle, EM_GETFIRSTVISIBLELINE, 0, IntPtr.Zero)); // 履歴をリセット
        UpdatePreview();
        UpdateWindowTitle();
        AddToHistory(currentFilePath);
    }

    private void AddToHistory(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        // 既存のリストから削除して先頭に追加する (LRU)
        _fileHistory.RemoveAll(x => string.Equals(x, filePath, StringComparison.OrdinalIgnoreCase));
        _fileHistory.Insert(0, filePath);

        // 10個を超えたら古いものを削除
        if (_fileHistory.Count > 10)
        {
            _fileHistory.RemoveRange(10, _fileHistory.Count - 10);
        }

        SettingsManager.SaveFileHistory(_fileHistory);
        UpdateFileMenuHistory();
    }

    private void UpdateFileMenuHistory()
    {
        // 既存の履歴アイテムを削除
        // 最初の6個（開く、保存、名前保存、PDF保存、文字コード、改行コード）は残す
        while (fileToolStripMenuItem.DropDownItems.Count > 6)
        {
            fileToolStripMenuItem.DropDownItems.RemoveAt(6);
        }

        if (_fileHistory.Count > 0)
        {
            fileToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            for (int i = 0; i < _fileHistory.Count; i++)
            {
                var path = _fileHistory[i];
                var menuItem = new ToolStripMenuItem($"{i + 1}: {path}");
                menuItem.Click += (s, e) => OpenFile(path);
                fileToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }
    }

    private void RecordUndoState()
    {
        if (_isUndoingRedoing) return;
        int firstVisibleLine = (int)SendMessage(editorTextBox.Handle, EM_GETFIRSTVISIBLELINE, 0, IntPtr.Zero);
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

        // テキスト書き換え前に現在のスクロール位置を取得
        int currentFirstVisibleLine = (int)SendMessage(editorTextBox.Handle, EM_GETFIRSTVISIBLELINE, 0, IntPtr.Zero);

        // 描画を一時停止してちらつきを防止
        SendMessage(editorTextBox.Handle, WM_SETREDRAW, 0, IntPtr.Zero);
        try
        {
            editorTextBox.Text = state.Text;
            editorTextBox.SelectionStart = Math.Min(state.SelectionStart, editorTextBox.Text.Length);

            // Text代入後はスクロールが0行目にリセットされるため、元の位置まで相対移動
            // ScrollToCaret() は画面下端ジャンプの原因になるため使用しない
            SendMessage(editorTextBox.Handle, EM_LINESCROLL, 0, (IntPtr)currentFirstVisibleLine);
        }
        finally
        {
            // 描画を再開して一度だけ再描画（ちらつきなし）
            SendMessage(editorTextBox.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
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

        // もし選択範囲がなく、かつShiftが押されていない場合は通常のタブ挿入
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

        string[] currentLines = editorTextBox.Lines;
        StringBuilder sb = new StringBuilder();

        for (int i = startLine; i <= endLine; i++)
        {
            string line = currentLines[i];
            if (shift)
            {
                // インデント解除
                if (line.StartsWith("\t"))
                {
                    line = line.Substring(1);
                }
                else if (line.StartsWith(" "))
                {
                    int spaceCount = 0;
                    while (spaceCount < _currentTabWidth && spaceCount < line.Length && line[spaceCount] == ' ')
                    {
                        spaceCount++;
                    }
                    line = line.Substring(spaceCount);
                }
            }
            else
            {
                // インデント追加
                line = "\t" + line;
            }
            sb.Append(line);
            
            // 最後の行であっても、元々改行があった場合は付与する必要がある
            // TextBox.Lines は改行を含まないが、SelectedText で全行置換する場合は
            // 範囲内の各行末に改行が必要。
            // 最後の行以前、またはファイル末尾ではない場合
            if (i < currentLines.Length - 1)
            {
                sb.Append("\r\n");
            }
        }

        // 選択範囲を全行分に拡大
        editorTextBox.SelectionStart = startPos;
        editorTextBox.SelectionLength = lastLineEndPos - startPos;
        
        string newText = sb.ToString();
        // ファイル末尾の選択で、元々末尾に空行があった場合の調整
        if (lastLineEndPos == editorTextBox.TextLength && editorTextBox.Text.EndsWith("\r\n") && !newText.EndsWith("\r\n"))
        {
            // newText += "\r\n"; // 必要に応じて
        }

        editorTextBox.SelectedText = newText;

        // 選択範囲を保持
        editorTextBox.SelectionStart = startPos;
        editorTextBox.SelectionLength = newText.Length;
    }

    private Encoding DetectEncoding(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        // 1. Check for UTF-8 BOM
        byte[] bom = new byte[4];
        int read = fs.Read(bom, 0, 4);
        fs.Position = 0;

        if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        {
            return new UTF8Encoding(true);
        }

        // 2. Try strict UTF-8
        try
        {
            var utf8Strict = new UTF8Encoding(false, true);
            using var reader = new StreamReader(fs, utf8Strict, false, 1024, true);
            reader.ReadToEnd();
            return new UTF8Encoding(false);
        }
        catch (DecoderFallbackException)
        {
            // Fallback to Shift-JIS
            return Encoding.GetEncoding("shift_jis");
        }
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
        if (string.IsNullOrWhiteSpace(currentFilePath))
        {
            SaveFileAs();
            return;
        }

        WriteFile(currentFilePath);
        _isModified = false;
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

        if (!string.IsNullOrWhiteSpace(currentFilePath))
        {
            dialog.FileName = Path.GetFileName(currentFilePath);
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        currentFilePath = dialog.FileName;
        WriteFile(currentFilePath);
        _isModified = false;
        UpdateWindowTitle();
        AddToHistory(currentFilePath);
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
        var fileName = string.IsNullOrWhiteSpace(currentFilePath) ? "KagamiMD" : $"{Path.GetFileName(currentFilePath)} - KagamiMD";
        Text = _isModified ? fileName + "*" : fileName;
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_isModified) return;

        var result = MessageBox.Show(this, "変更内容を保存しますか？", "KagamiMD", MessageBoxButtons.YesNoCancel, MessageBoxIcon.None);

        if (result == DialogResult.Yes)
        {
            SaveFile();
            // If SaveFile was cancelled (e.g. in SaveAs dialog), result might be different
            // but for simplicity, we assume if we reach here we either saved or handled it.
            if (_isModified) e.Cancel = true; // Still modified means saving was canceled
        }
        else if (result == DialogResult.Cancel)
        {
            e.Cancel = true;
        }
    }

    private void WriteFile(string filePath)
    {
        var text = NormalizeLineEndings(editorTextBox.Text ?? string.Empty, GetSelectedLineEnding());
        File.WriteAllText(filePath, text, GetSelectedEncoding());
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

    private static string NormalizeLineEndings(string text, string lineEnding)
    {
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        return lineEnding == "\n" ? normalized : normalized.Replace("\n", lineEnding);
    }

    private void UpdatePreview()
    {
        if (!previewReady || previewWebView.CoreWebView2 is null)
        {
            return;
        }

        ConfigurePreviewResourceMapping();
        var htmlBody = Markdown.ToHtml(editorTextBox.Text ?? string.Empty, pipeline);

        if (!_previewInitialized)
        {
            var html = $$"""
                <!doctype html>
                <html>
                <head>
                    <meta charset="utf-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                    <base href="{{GetPreviewBaseHref()}}" />
                    <script src="https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js"></script>
                    <style>
                        body {
                            font-family: 'Segoe UI', sans-serif;
                            margin: 24px;
                            line-height: 1.65;
                            color: #1f2937;
                            background: #ffffff;
                        }

                        h1, h2, h3, h4, h5, h6 {
                            line-height: 1.25;
                            margin-top: 1.2em;
                        }

                        pre {
                            background: #f3f4f6;
                            padding: 16px;
                            border-radius: 8px;
                            overflow: auto;
                        }

                        code {
                            background: #f3f4f6;
                            padding: 0 4px;
                            border-radius: 4px;
                        }

                        blockquote {
                            margin: 1em 0;
                            padding: 0 16px;
                            border-left: 4px solid #cbd5e1;
                            color: #475569;
                        }

                        table {
                            border-collapse: collapse;
                            width: 100%;
                        }

                        th, td {
                            border: 1px solid #d1d5db;
                            padding: 8px 10px;
                        }

                        img {
                            max-width: 100%;
                        }
                    </style>
                </head>
                <body>
                <div id="content">{{htmlBody}}</div>
                <script>
                    // Restore scroll position
                    const scrollPos = sessionStorage.getItem('previewScrollPos');
                    if (scrollPos) {
                        window.scrollTo(0, parseInt(scrollPos, 10));
                    }

                    // Save scroll position
                    window.addEventListener('scroll', () => {
                        sessionStorage.setItem('previewScrollPos', window.scrollY);
                    });

                    mermaid.initialize({
                        startOnLoad: false,
                        securityLevel: 'loose',
                        theme: 'default'
                    });

                    function renderMermaid() {
                        document.querySelectorAll('#content pre > code.language-mermaid').forEach((codeBlock) => {
                            const parent = codeBlock.parentElement;
                            const diagram = document.createElement('div');
                            diagram.className = 'mermaid';
                            diagram.textContent = codeBlock.textContent;
                            parent.replaceWith(diagram);
                        });
                        mermaid.run();
                    }

                    renderMermaid();
                </script>
                </body>
                </html>
                """;

            previewWebView.NavigateToString(html);
            _previewInitialized = true;
        }
        else
        {
            var escapedHtml = System.Text.Json.JsonSerializer.Serialize(htmlBody);
            var script = $$"""
                var contentDiv = document.getElementById('content');
                if (contentDiv) {
                    contentDiv.innerHTML = {{escapedHtml}};
                    if (typeof renderMermaid === 'function') {
                        renderMermaid();
                    }
                }
                """;
            previewWebView.CoreWebView2.ExecuteScriptAsync(script);
        }

        // スクロールイベントの登録（連動用）
        if (syncedScrollToolStripMenuItem.Checked)
        {
            previewWebView.CoreWebView2.ExecuteScriptAsync("""
                (function() {
                    if (!window.hasScrollBound) {
                        window.addEventListener('scroll', function() {
                            if (window.isInternalScrolling) {
                                window.isInternalScrolling = false;
                                return;
                            }
                            var height = document.body.scrollHeight - window.innerHeight;
                            var ratio = height <= 0 ? 0 : window.scrollY / height;
                            window.chrome.webview.postMessage(ratio.toString());
                        });
                        window.hasScrollBound = true;
                    }
                    // 連動時はスクロールバーを隠すCSSを再適用
                    var styleId = 'kagami-scroll-hide';
                    if (!document.getElementById(styleId)) {
                        var style = document.createElement('style');
                        style.id = styleId;
                        style.innerHTML = '*::-webkit-scrollbar { display: none !important; } body { -ms-overflow-style: none !important; scrollbar-width: none !important; }';
                        document.head.appendChild(style);
                    }
                })()
                """);
        }
    }

    private void ConfigurePreviewResourceMapping()
    {
        var directoryPath = GetPreviewBaseDirectoryPath();
        previewWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "markdown-preview",
            directoryPath,
            CoreWebView2HostResourceAccessKind.Allow);
    }

    private string GetPreviewBaseHref()
    {
        return "https://markdown-preview/";
    }

    private string GetPreviewBaseDirectoryPath()
    {
        var directoryPath = !string.IsNullOrWhiteSpace(currentFilePath)
            ? Path.GetDirectoryName(currentFilePath)
            : AppContext.BaseDirectory;

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            directoryPath = AppContext.BaseDirectory;
        }

        return Path.GetFullPath(directoryPath);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int[]? lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

    private const int EM_SETTABSTOPS = 0x00CB;
    private const int EM_LINESCROLL = 0x00B6;
    private const int EM_GETFIRSTVISIBLELINE = 0x00CE;
    private const int WM_SETREDRAW = 0x000B;

    private void SetTabWidth(int tabWidth)
    {
        // 1 tab stops unit is 1/4 of the average character width.
        // So for 4 characters, it's 4 * 4 = 16.
        int[] tabStops = { tabWidth * 4 };
        _currentTabWidth = tabWidth;
        SendMessage(editorTextBox.Handle, EM_SETTABSTOPS, 1, tabStops);
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

        if (!string.IsNullOrWhiteSpace(currentFilePath))
        {
            dialog.FileName = Path.GetFileNameWithoutExtension(currentFilePath) + ".pdf";
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
        var table = "|Title1|Title2|Title3|\r\n|---|---|---|\r\n|aaa|aaa|aaa|\r\n|aaa|aaa|aaa|";
        editorTextBox.SelectedText = table;
    }

    private void InsertTextColorTemplate()
    {
        RecordUndoState();
        var start = editorTextBox.SelectionStart;
        var len = editorTextBox.SelectionLength;
        var selected = editorTextBox.SelectedText;
        
        var prefix = "<span style=\"color:red\">";
        var suffix = "</span>";
        
        editorTextBox.SelectedText = $"{prefix}{selected}{suffix}";
        
        if (len == 0)
        {
            // 選択範囲がなかった場合は、タグの間にカーソルを移動
            editorTextBox.SelectionStart = start + prefix.Length;
        }
        else
        {
            // 選択範囲があった場合は、タグ全体を選択するか、末尾に移動するか
            // ここではタグも含めた全体を選択状態にする（再度色を変えたり削除したりしやすくするため）
            editorTextBox.SelectionStart = start;
            editorTextBox.SelectionLength = prefix.Length + len + suffix.Length;
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
            var text = "[リンクのテキスト](リンクのアドレス \"リンクのタイトル\")";
            editorTextBox.SelectedText = text;
            // 「リンクのテキスト」部分を選択（1文字目から7文字分）
            editorTextBox.SelectionStart = start + 1;
            editorTextBox.SelectionLength = 7;
        }
        else
        {
            var prefix = "[";
            var middle = "](リンクのアドレス \"リンクのタイトル\")";
            editorTextBox.SelectedText = $"{prefix}{selected}{middle}";
            // 「リンクのアドレス」部分を選択状態にする
            editorTextBox.SelectionStart = start + prefix.Length + len + 2; // "[" + text + "]("
            editorTextBox.SelectionLength = 8; // "リンクのアドレス"
        }
    }
}
