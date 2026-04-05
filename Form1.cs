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
        Resize += (_, _) => CenterSplitters();
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

        _undoTimer.Interval = 800;
        _undoTimer.Tick += (s, e) =>
        {
            _undoTimer.Stop();
            RecordUndoState();
        };

        editorTextBox.KeyDown += EditorTextBox_KeyDown;
        // 初期状態を記録
        _undoManager.Clear(editorTextBox.Text, 0);

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
        };

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
        var availableWidth = splitContainer1.ClientSize.Width - splitContainer1.SplitterWidth;
        if (availableWidth <= 0)
        {
            return;
        }

        splitContainer1.SplitterDistance = availableWidth / 2;
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
        _undoManager.Clear(editorTextBox.Text, 0); // 履歴をリセット
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
        _undoManager.RecordState(editorTextBox.Text, editorTextBox.SelectionStart);
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

    private void ApplyState((string Text, int SelectionStart) state)
    {
        _isUndoingRedoing = true;
        editorTextBox.Text = state.Text;
        editorTextBox.SelectionStart = Math.Min(state.SelectionStart, editorTextBox.Text.Length);
        editorTextBox.ScrollToCaret();
        _isUndoingRedoing = false;
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
        else if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
        {
            RecordUndoState();
        }
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
        editorTextBox.ScrollBars = enabled ? ScrollBars.Vertical : ScrollBars.Both;
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
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int[] lParam);

    private const int EM_SETTABSTOPS = 0x00CB;

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
}
