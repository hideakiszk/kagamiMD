namespace KagamiMD.Services;

/// <summary>
/// テンプレート挿入用の文字列定数を管理するクラス。
/// </summary>
public static class EditorTemplates
{
    public const string Table = "|Title1|Title2|Title3|\r\n|---|---|---|\r\n|aaa|aaa|aaa|\r\n|aaa|aaa|aaa|";

    public const string TextColorPrefix = "<span style=\"color:red\">";
    public const string TextColorSuffix = "</span>";

    public const string LinkTemplate = "[リンクのテキスト](リンクのアドレス \"リンクのタイトル\")";
    public const string LinkPrefix = "[";
    public const string LinkSuffix = "](リンクのアドレス \"リンクのタイトル\")";

    /// <summary>「リンクのテキスト」の文字数（選択対象として使用）</summary>
    public const int LinkTextLength = 7;

    /// <summary>「リンクのアドレス」の文字数（選択対象として使用）</summary>
    public const int LinkAddressLength = 8;
}
