namespace KagamiMD;

partial class SearchReplaceForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.label1 = new System.Windows.Forms.Label();
        this.findTextBox = new System.Windows.Forms.TextBox();
        this.label2 = new System.Windows.Forms.Label();
        this.replaceTextBox = new System.Windows.Forms.TextBox();
        this.matchCaseCheckBox = new System.Windows.Forms.CheckBox();
        this.useRegexCheckBox = new System.Windows.Forms.CheckBox();
        this.findNextButton = new System.Windows.Forms.Button();
        this.replaceButton = new System.Windows.Forms.Button();
        this.replaceAllButton = new System.Windows.Forms.Button();
        this.closeButton = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(12, 15);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(95, 15);
        this.label1.TabIndex = 0;
        this.label1.Text = "検索する文字列:";
        // 
        // findTextBox
        // 
        this.findTextBox.Location = new System.Drawing.Point(113, 12);
        this.findTextBox.Name = "findTextBox";
        this.findTextBox.Size = new System.Drawing.Size(259, 23);
        this.findTextBox.TabIndex = 1;
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(12, 44);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(95, 15);
        this.label2.TabIndex = 2;
        this.label2.Text = "置換後の文字列:";
        // 
        // replaceTextBox
        // 
        this.replaceTextBox.Location = new System.Drawing.Point(113, 41);
        this.replaceTextBox.Name = "replaceTextBox";
        this.replaceTextBox.Size = new System.Drawing.Size(259, 23);
        this.replaceTextBox.TabIndex = 3;
        // 
        // matchCaseCheckBox
        // 
        this.matchCaseCheckBox.AutoSize = true;
        this.matchCaseCheckBox.Location = new System.Drawing.Point(113, 70);
        this.matchCaseCheckBox.Name = "matchCaseCheckBox";
        this.matchCaseCheckBox.Size = new System.Drawing.Size(142, 19);
        this.matchCaseCheckBox.TabIndex = 4;
        this.matchCaseCheckBox.Text = "大文字小文字を区別";
        this.matchCaseCheckBox.UseVisualStyleBackColor = true;
        // 
        // useRegexCheckBox
        // 
        this.useRegexCheckBox.AutoSize = true;
        this.useRegexCheckBox.Location = new System.Drawing.Point(113, 95);
        this.useRegexCheckBox.Name = "useRegexCheckBox";
        this.useRegexCheckBox.Size = new System.Drawing.Size(98, 19);
        this.useRegexCheckBox.TabIndex = 5;
        this.useRegexCheckBox.Text = "正規表現を使用";
        this.useRegexCheckBox.UseVisualStyleBackColor = true;
        // 
        // findNextButton
        // 
        this.findNextButton.Location = new System.Drawing.Point(378, 11);
        this.findNextButton.Name = "findNextButton";
        this.findNextButton.Size = new System.Drawing.Size(94, 23);
        this.findNextButton.TabIndex = 6;
        this.findNextButton.Text = "次を検索";
        this.findNextButton.UseVisualStyleBackColor = true;
        // 
        // replaceButton
        // 
        this.replaceButton.Location = new System.Drawing.Point(378, 40);
        this.replaceButton.Name = "replaceButton";
        this.replaceButton.Size = new System.Drawing.Size(94, 23);
        this.replaceButton.TabIndex = 7;
        this.replaceButton.Text = "置換";
        this.replaceButton.UseVisualStyleBackColor = true;
        // 
        // replaceAllButton
        // 
        this.replaceAllButton.Location = new System.Drawing.Point(378, 69);
        this.replaceAllButton.Name = "replaceAllButton";
        this.replaceAllButton.Size = new System.Drawing.Size(94, 23);
        this.replaceAllButton.TabIndex = 8;
        this.replaceAllButton.Text = "すべて置換";
        this.replaceAllButton.UseVisualStyleBackColor = true;
        // 
        // closeButton
        // 
        this.closeButton.Location = new System.Drawing.Point(378, 98);
        this.closeButton.Name = "closeButton";
        this.closeButton.Size = new System.Drawing.Size(94, 23);
        this.closeButton.TabIndex = 9;
        this.closeButton.Text = "閉じる";
        this.closeButton.UseVisualStyleBackColor = true;
        // 
        // SearchReplaceForm
        // 
        this.AcceptButton = this.findNextButton;
        this.CancelButton = this.closeButton;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoSize = true;
        this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ClientSize = new System.Drawing.Size(484, 160);
        this.Controls.Add(this.closeButton);
        this.Padding = new System.Windows.Forms.Padding(0, 0, 10, 10);
        this.Controls.Add(this.replaceAllButton);
        this.Controls.Add(this.replaceButton);
        this.Controls.Add(this.findNextButton);
        this.Controls.Add(this.useRegexCheckBox);
        this.Controls.Add(this.matchCaseCheckBox);
        this.Controls.Add(this.replaceTextBox);
        this.Controls.Add(this.label2);
        this.Controls.Add(this.findTextBox);
        this.Controls.Add(this.label1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SearchReplaceForm";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "検索と置換";
        this.TopMost = true;
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox findTextBox;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox replaceTextBox;
    private System.Windows.Forms.CheckBox matchCaseCheckBox;
    private System.Windows.Forms.CheckBox useRegexCheckBox;
    private System.Windows.Forms.Button findNextButton;
    private System.Windows.Forms.Button replaceButton;
    private System.Windows.Forms.Button replaceAllButton;
    private System.Windows.Forms.Button closeButton;
}
