namespace KagamiMD;

partial class Form1
{
    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem saveEncodingToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem utf8BomToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem utf8NoBomToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem shiftJisToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem newlineToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem crlfToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem lfToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem wordWrapToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem previewUpdateToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem tabSettingsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem tab1ToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem tab2ToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem tab4ToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem fontToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem savePdfToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem previewToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem realtimePreviewToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem manualPreviewToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem templateToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem tableToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem textColorToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem linkToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem scrollToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem independentScrollToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem syncedScrollToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem displayContentToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem showBothToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem showLeftOnlyToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem showRightOnlyToolStripMenuItem;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.VScrollBar syncVScrollBar;
    private System.Windows.Forms.TextBox editorTextBox;
    private Microsoft.Web.WebView2.WinForms.WebView2 previewWebView;
    private System.Windows.Forms.ToolStripMenuItem wysiwygToolStripMenuItem;

    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.menuStrip1 = new System.Windows.Forms.MenuStrip();
        this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.saveEncodingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.utf8BomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.utf8NoBomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.shiftJisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.newlineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.crlfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.lfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.wordWrapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.previewUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.tabSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.tab1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.tab2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.tab4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.fontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.savePdfToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
        this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
        this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
        this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.previewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.realtimePreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.manualPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.templateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.tableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.textColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.linkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.scrollToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.independentScrollToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.syncedScrollToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.displayContentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.showBothToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.showLeftOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.showRightOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.splitContainer1 = new System.Windows.Forms.SplitContainer();
        this.syncVScrollBar = new System.Windows.Forms.VScrollBar();
        this.editorTextBox = new System.Windows.Forms.TextBox();
        this.previewWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
        this.wysiwygToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        this.menuStrip1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
        this.splitContainer1.Panel1.SuspendLayout();
        this.splitContainer1.Panel2.SuspendLayout();
        this.splitContainer1.SuspendLayout();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1100, 700);
        this.Controls.Add(this.splitContainer1);
        this.Controls.Add(this.menuStrip1);
        this.MainMenuStrip = this.menuStrip1;
        this.MinimumSize = new System.Drawing.Size(900, 600);
        this.Name = "Form1";
        this.Text = "KagamiMD";
        this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Top;
        this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.fileToolStripMenuItem,
        this.editToolStripMenuItem,
        this.viewToolStripMenuItem,
        this.templateToolStripMenuItem,
        this.settingsToolStripMenuItem});
        this.menuStrip1.Location = new System.Drawing.Point(0, 0);
        this.menuStrip1.Name = "menuStrip1";
        this.menuStrip1.Size = new System.Drawing.Size(1100, 24);
        this.menuStrip1.TabIndex = 0;
        this.menuStrip1.Text = "menuStrip1";
        this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.openToolStripMenuItem,
        this.saveToolStripMenuItem,
        this.saveAsToolStripMenuItem,
        this.savePdfToolStripMenuItem,
        this.saveEncodingToolStripMenuItem,
        this.newlineToolStripMenuItem});
        this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        this.fileToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
        this.fileToolStripMenuItem.Text = "ファイル";
        this.openToolStripMenuItem.Name = "openToolStripMenuItem";
        this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.openToolStripMenuItem.Text = "ファイルを開く";
        this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O));
        this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.saveToolStripMenuItem.Text = "上書き保存";
        this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S));
        this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
        this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.saveAsToolStripMenuItem.Text = "名前をつけて保存";
        this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S));
        // 
        // savePdfToolStripMenuItem
        // 
        this.savePdfToolStripMenuItem.Name = "savePdfToolStripMenuItem";
        this.savePdfToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.savePdfToolStripMenuItem.Text = "PDFとして保存...";
        this.savePdfToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P));
        this.saveEncodingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.utf8BomToolStripMenuItem,
        this.utf8NoBomToolStripMenuItem,
        this.shiftJisToolStripMenuItem});
        this.saveEncodingToolStripMenuItem.Name = "saveEncodingToolStripMenuItem";
        this.saveEncodingToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.saveEncodingToolStripMenuItem.Text = "保存文字コード";
        this.utf8BomToolStripMenuItem.Checked = true;
        this.utf8BomToolStripMenuItem.CheckOnClick = true;
        this.utf8BomToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
        this.utf8BomToolStripMenuItem.Name = "utf8BomToolStripMenuItem";
        this.utf8BomToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.utf8BomToolStripMenuItem.Text = "UTF-8 (BOM 付き)";
        this.utf8NoBomToolStripMenuItem.CheckOnClick = true;
        this.utf8NoBomToolStripMenuItem.Name = "utf8NoBomToolStripMenuItem";
        this.utf8NoBomToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.utf8NoBomToolStripMenuItem.Text = "UTF-8 (BOM なし)";
        this.shiftJisToolStripMenuItem.CheckOnClick = true;
        this.shiftJisToolStripMenuItem.Name = "shiftJisToolStripMenuItem";
        this.shiftJisToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.shiftJisToolStripMenuItem.Text = "Shift-JIS (CP932)";
        this.newlineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.crlfToolStripMenuItem,
        this.lfToolStripMenuItem});
        this.newlineToolStripMenuItem.Name = "newlineToolStripMenuItem";
        this.newlineToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.newlineToolStripMenuItem.Text = "改行コード";
        this.crlfToolStripMenuItem.Checked = true;
        this.crlfToolStripMenuItem.CheckOnClick = true;
        this.crlfToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
        this.crlfToolStripMenuItem.Name = "crlfToolStripMenuItem";
        this.crlfToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.crlfToolStripMenuItem.Text = "CRLF";
        this.lfToolStripMenuItem.CheckOnClick = true;
        this.lfToolStripMenuItem.Name = "lfToolStripMenuItem";
        this.lfToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.lfToolStripMenuItem.Text = "LF";
        this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.wordWrapToolStripMenuItem,
        this.wysiwygToolStripMenuItem,
        this.previewToolStripMenuItem,
        this.previewUpdateToolStripMenuItem,
        this.displayContentToolStripMenuItem,
        this.scrollToolStripMenuItem});
        this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
        this.viewToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
        this.viewToolStripMenuItem.Text = "表示";
        this.wordWrapToolStripMenuItem.Checked = true;
        this.wordWrapToolStripMenuItem.CheckOnClick = true;
        this.wordWrapToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
        this.wordWrapToolStripMenuItem.Name = "wordWrapToolStripMenuItem";
        this.wordWrapToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.wordWrapToolStripMenuItem.Text = "テキスト折り返し";
        // 
        // wysiwygToolStripMenuItem
        // 
        this.wysiwygToolStripMenuItem.CheckOnClick = true;
        this.wysiwygToolStripMenuItem.Name = "wysiwygToolStripMenuItem";
        this.wysiwygToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.wysiwygToolStripMenuItem.Text = "WYSIWYGモード";
        // 
        // previewToolStripMenuItem
        // 
        this.previewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.realtimePreviewToolStripMenuItem,
        this.manualPreviewToolStripMenuItem});
        this.previewToolStripMenuItem.Name = "previewToolStripMenuItem";
        this.previewToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.previewToolStripMenuItem.Text = "プレビュー";
        // 
        // realtimePreviewToolStripMenuItem
        // 
        this.realtimePreviewToolStripMenuItem.Name = "realtimePreviewToolStripMenuItem";
        this.realtimePreviewToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.realtimePreviewToolStripMenuItem.Text = "リアルタイム";
        this.realtimePreviewToolStripMenuItem.CheckOnClick = true;
        // 
        // manualPreviewToolStripMenuItem
        // 
        this.manualPreviewToolStripMenuItem.Name = "manualPreviewToolStripMenuItem";
        this.manualPreviewToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.manualPreviewToolStripMenuItem.Text = "手動更新";
        this.manualPreviewToolStripMenuItem.CheckOnClick = true;
        // 
        // previewUpdateToolStripMenuItem
        // 
        this.previewUpdateToolStripMenuItem.Text = "プレビュー更新";
        this.previewUpdateToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
        // 
        // displayContentToolStripMenuItem
        // 
        this.displayContentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.showBothToolStripMenuItem,
        this.showLeftOnlyToolStripMenuItem,
        this.showRightOnlyToolStripMenuItem});
        this.displayContentToolStripMenuItem.Name = "displayContentToolStripMenuItem";
        this.displayContentToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.displayContentToolStripMenuItem.Text = "表示内容";
        // 
        // showBothToolStripMenuItem
        // 
        this.showBothToolStripMenuItem.Checked = true;
        this.showBothToolStripMenuItem.Name = "showBothToolStripMenuItem";
        this.showBothToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.showBothToolStripMenuItem.Text = "両方";
        // 
        // showLeftOnlyToolStripMenuItem
        // 
        this.showLeftOnlyToolStripMenuItem.Name = "showLeftOnlyToolStripMenuItem";
        this.showLeftOnlyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.showLeftOnlyToolStripMenuItem.Text = "左側のみ";
        // 
        // showRightOnlyToolStripMenuItem
        // 
        this.showRightOnlyToolStripMenuItem.Name = "showRightOnlyToolStripMenuItem";
        this.showRightOnlyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.showRightOnlyToolStripMenuItem.Text = "右側のみ";
        // 
        // scrollToolStripMenuItem
        // 
        this.scrollToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.independentScrollToolStripMenuItem,
        this.syncedScrollToolStripMenuItem});
        this.scrollToolStripMenuItem.Name = "scrollToolStripMenuItem";
        this.scrollToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.scrollToolStripMenuItem.Text = "スクロール";
        // 
        // independentScrollToolStripMenuItem
        // 
        this.independentScrollToolStripMenuItem.Checked = true;
        this.independentScrollToolStripMenuItem.Name = "independentScrollToolStripMenuItem";
        this.independentScrollToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.independentScrollToolStripMenuItem.Text = "左右独立";
        // 
        // syncedScrollToolStripMenuItem
        // 
        this.syncedScrollToolStripMenuItem.Name = "syncedScrollToolStripMenuItem";
        this.syncedScrollToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.syncedScrollToolStripMenuItem.Text = "左右連動";
        // 
        // templateToolStripMenuItem
        // 
        this.templateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.tableToolStripMenuItem,
        this.textColorToolStripMenuItem,
        this.linkToolStripMenuItem});
        this.templateToolStripMenuItem.Name = "templateToolStripMenuItem";
        this.templateToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
        this.templateToolStripMenuItem.Text = "テンプレート";
        // 
        // tableToolStripMenuItem
        // 
        this.tableToolStripMenuItem.Name = "tableToolStripMenuItem";
        this.tableToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.tableToolStripMenuItem.Text = "表";
        // 
        // textColorToolStripMenuItem
        // 
        this.textColorToolStripMenuItem.Name = "textColorToolStripMenuItem";
        this.textColorToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.textColorToolStripMenuItem.Text = "文字色変更";
        this.templateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.tableToolStripMenuItem,
        this.textColorToolStripMenuItem,
        this.linkToolStripMenuItem});
        this.templateToolStripMenuItem.Name = "templateToolStripMenuItem";
        this.templateToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
        this.templateToolStripMenuItem.Text = "テンプレート";
        // 
        // tableToolStripMenuItem
        // 
        this.tableToolStripMenuItem.Name = "tableToolStripMenuItem";
        this.tableToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.tableToolStripMenuItem.Text = "表";
        // 
        // textColorToolStripMenuItem
        // 
        this.textColorToolStripMenuItem.Name = "textColorToolStripMenuItem";
        this.textColorToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.textColorToolStripMenuItem.Text = "文字色変更";
        // 
        // linkToolStripMenuItem
        // 
        this.linkToolStripMenuItem.Name = "linkToolStripMenuItem";
        this.linkToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.linkToolStripMenuItem.Text = "リンク";
        // 
        // editToolStripMenuItem
        // 
        this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.undoToolStripMenuItem,
        this.redoToolStripMenuItem,
        this.toolStripSeparator1,
        this.cutToolStripMenuItem,
        this.copyToolStripMenuItem,
        this.pasteToolStripMenuItem,
        this.toolStripSeparator2,
        this.selectAllToolStripMenuItem,
        this.toolStripSeparator3,
        this.findToolStripMenuItem,
        this.replaceToolStripMenuItem});
        this.editToolStripMenuItem.Name = "editToolStripMenuItem";
        this.editToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
        this.editToolStripMenuItem.Text = "編集";
        // 
        // undoToolStripMenuItem
        // 
        this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
        this.undoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.undoToolStripMenuItem.Text = "元に戻す";
        this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z));
        // 
        // redoToolStripMenuItem
        // 
        this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
        this.redoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.redoToolStripMenuItem.Text = "やり直し";
        this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y));
        // 
        // toolStripSeparator1
        // 
        this.toolStripSeparator1.Name = "toolStripSeparator1";
        this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
        // 
        // cutToolStripMenuItem
        // 
        this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
        this.cutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.cutToolStripMenuItem.Text = "切り取り";
        this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X));
        // 
        // copyToolStripMenuItem
        // 
        this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
        this.copyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.copyToolStripMenuItem.Text = "コピー";
        this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C));
        // 
        // pasteToolStripMenuItem
        // 
        this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
        this.pasteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.pasteToolStripMenuItem.Text = "貼り付け";
        this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V));
        // 
        // toolStripSeparator2
        // 
        this.toolStripSeparator2.Name = "toolStripSeparator2";
        this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
        // 
        // selectAllToolStripMenuItem
        // 
        this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
        this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.selectAllToolStripMenuItem.Text = "すべて選択";
        this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A));
        // 
        // toolStripSeparator3
        // 
        this.toolStripSeparator3.Name = "toolStripSeparator3";
        this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
        // 
        // findToolStripMenuItem
        // 
        this.findToolStripMenuItem.Name = "findToolStripMenuItem";
        this.findToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.findToolStripMenuItem.Text = "検索...";
        this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F));
        // 
        // replaceToolStripMenuItem
        // 
        this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
        this.replaceToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.replaceToolStripMenuItem.Text = "置換...";
        this.replaceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H));
        // 
        // settingsToolStripMenuItem
        // 
        this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.tabSettingsToolStripMenuItem,
        this.fontToolStripMenuItem});
        this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
        this.settingsToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
        this.settingsToolStripMenuItem.Text = "設定";
        // 
        // tabSettingsToolStripMenuItem
        // 
        this.tabSettingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.tab1ToolStripMenuItem,
        this.tab2ToolStripMenuItem,
        this.tab4ToolStripMenuItem});
        this.tabSettingsToolStripMenuItem.Name = "tabSettingsToolStripMenuItem";
        this.tabSettingsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.tabSettingsToolStripMenuItem.Text = "タブ設定";
        // 
        // tab1ToolStripMenuItem
        // 
        this.tab1ToolStripMenuItem.Name = "tab1ToolStripMenuItem";
        this.tab1ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.tab1ToolStripMenuItem.Text = "1";
        // 
        // tab2ToolStripMenuItem
        // 
        this.tab2ToolStripMenuItem.Name = "tab2ToolStripMenuItem";
        this.tab2ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.tab2ToolStripMenuItem.Text = "2";
        // 
        // tab4ToolStripMenuItem
        // 
        this.tab4ToolStripMenuItem.Name = "tab4ToolStripMenuItem";
        this.tab4ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.tab4ToolStripMenuItem.Text = "4";
        // 
        // fontToolStripMenuItem
        // 
        this.fontToolStripMenuItem.Name = "fontToolStripMenuItem";
        this.fontToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
        this.fontToolStripMenuItem.Text = "フォント...";
        this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.None;
        this.splitContainer1.Location = new System.Drawing.Point(0, 0);
        this.splitContainer1.Name = "splitContainer1";
        this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Vertical;
        this.splitContainer1.SplitterDistance = 520;
        this.splitContainer1.TabIndex = 0;
        this.splitContainer1.Panel1.Controls.Add(this.syncVScrollBar);
        this.splitContainer1.Panel1.Controls.Add(this.editorTextBox);
        this.splitContainer1.Panel2.Controls.Add(this.previewWebView);
        // 
        // syncVScrollBar
        // 
        this.syncVScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
        this.syncVScrollBar.Location = new System.Drawing.Point(503, 0);
        this.syncVScrollBar.Name = "syncVScrollBar";
        this.syncVScrollBar.Size = new System.Drawing.Size(17, 700);
        this.syncVScrollBar.TabIndex = 1;
        this.syncVScrollBar.Visible = false;
        this.editorTextBox.AcceptsTab = true;
        this.editorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.editorTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.editorTextBox.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.editorTextBox.Location = new System.Drawing.Point(0, 0);
        this.editorTextBox.Multiline = true;
        this.editorTextBox.Name = "editorTextBox";
        this.editorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        this.editorTextBox.ShortcutsEnabled = true;
        this.editorTextBox.MaxLength = int.MaxValue;
        this.editorTextBox.Size = new System.Drawing.Size(520, 700);
        this.editorTextBox.TabIndex = 0;
        this.editorTextBox.WordWrap = true;
        this.editorTextBox.HideSelection = false;
        this.previewWebView.AllowExternalDrop = true;
        this.previewWebView.CreationProperties = null;
        this.previewWebView.DefaultBackgroundColor = System.Drawing.Color.White;
        this.previewWebView.Dock = System.Windows.Forms.DockStyle.Fill;
        this.previewWebView.Location = new System.Drawing.Point(0, 0);
        this.previewWebView.Name = "previewWebView";
        this.previewWebView.Size = new System.Drawing.Size(576, 700);
        this.previewWebView.TabIndex = 0;
        this.previewWebView.ZoomFactor = 1D;
        this.menuStrip1.ResumeLayout(false);
        this.menuStrip1.PerformLayout();
        this.splitContainer1.Panel1.ResumeLayout(false);
        this.splitContainer1.Panel1.PerformLayout();
        this.splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
        this.splitContainer1.ResumeLayout(false);
    }

    #endregion
}
