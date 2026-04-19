using System.Text.RegularExpressions;
using KagamiMD.Services;

namespace KagamiMD;

public partial class SearchReplaceForm : Form
{
    private readonly TextBox _targetTextBox;

    public SearchReplaceForm(TextBox targetTextBox)
    {
        InitializeComponent();
        _targetTextBox = targetTextBox;

        findNextButton.Click += (_, _) => FindNext();
        replaceButton.Click += (_, _) => Replace();
        replaceAllButton.Click += (_, _) => ReplaceAll();
        closeButton.Click += (_, _) => Close();
        
        // Default focus on find textbox
        Shown += (_, _) => findTextBox.Focus();
    }

    public void SetReplaceMode(bool replace)
    {
        label2.Visible = replace;
        replaceTextBox.Visible = replace;
        replaceButton.Visible = replace;
        replaceAllButton.Visible = replace;
        this.Text = replace ? "置換" : "検索";
    }

    private bool FindNext(bool showMessage = true)
    {
        string searchText = findTextBox.Text;
        if (string.IsNullOrEmpty(searchText)) return false;

        string source = _targetTextBox.Text;
        int startIndex = _targetTextBox.SelectionStart + _targetTextBox.SelectionLength;

        try
        {
            var result = SearchEngine.Find(
                source,
                searchText,
                startIndex,
                useRegexCheckBox.Checked,
                matchCaseCheckBox.Checked);

            if (result.Success)
            {
                _targetTextBox.Select(result.Index, result.Length);
                _targetTextBox.Focus();
                _targetTextBox.ScrollToCaret();
                return true;
            }
            else
            {
                if (showMessage)
                {
                    MessageBox.Show(this, "見つかりませんでした。", "検索", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return false;
            }
        }
        catch (ArgumentException ex)
        {
            if (showMessage)
            {
                MessageBox.Show(this, "正規表現のエラー: " + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
    }

    private void Replace()
    {
        string searchText = findTextBox.Text;
        if (string.IsNullOrEmpty(searchText)) return;

        string selectedText = _targetTextBox.SelectedText;
        bool isMatch = SearchEngine.IsMatch(
            selectedText,
            searchText,
            useRegexCheckBox.Checked,
            matchCaseCheckBox.Checked);

        if (isMatch)
        {
            _targetTextBox.SelectedText = replaceTextBox.Text;
            // Successfully replaced, now find next but don't show "Not found" 
            // if this was the last one.
            FindNext(showMessage: false);
        }
        else
        {
            // Current selection doesn't match, so just find the next occurrence.
            FindNext(showMessage: true);
        }
    }

    private void ReplaceAll()
    {
        string searchText = findTextBox.Text;
        if (string.IsNullOrEmpty(searchText)) return;

        string source = _targetTextBox.Text;
        string replacement = replaceTextBox.Text;

        try
        {
            var (result, count) = SearchEngine.ReplaceAll(
                source,
                searchText,
                replacement,
                useRegexCheckBox.Checked,
                matchCaseCheckBox.Checked);

            if (count > 0)
            {
                _targetTextBox.Text = result;
                MessageBox.Show(this, $"{count} 個の箇所を置換しました。", "置換", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "一致する箇所が見つかりませんでした。", "置換", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(this, "正規表現のエラー: " + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
