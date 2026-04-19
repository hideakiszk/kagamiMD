namespace KagamiMD.Services;

/// <summary>
/// 現在開いているドキュメントの状態（ファイルパス・変更フラグ）を管理するクラス。
/// UI依存を持たないため、ウィンドウタイトルの文字列生成もここで担当する。
/// </summary>
public class DocumentSession
{
    private const string AppName = "KagamiMD";

    public string? CurrentFilePath { get; private set; }
    public bool IsModified { get; private set; }

    /// <summary>ドキュメントを新規状態にリセットする。</summary>
    public void Clear()
    {
        CurrentFilePath = null;
        IsModified = false;
    }

    /// <summary>ファイルを開いたときに呼び出す。</summary>
    public void Open(string filePath)
    {
        CurrentFilePath = filePath;
        IsModified = false;
    }

    /// <summary>ファイルを保存したときに呼び出す。</summary>
    public void Save(string filePath)
    {
        CurrentFilePath = filePath;
        IsModified = false;
    }

    /// <summary>テキストが変更されたときに呼び出す。変更済みになる。</summary>
    public void MarkModified()
    {
        IsModified = true;
    }

    /// <summary>
    /// ウィンドウタイトルの文字列を返す。
    /// 例: "readme.md - KagamiMD*"（変更あり）、"KagamiMD"（新規）
    /// </summary>
    public string GetWindowTitle()
    {
        var baseName = string.IsNullOrWhiteSpace(CurrentFilePath)
            ? AppName
            : $"{Path.GetFileName(CurrentFilePath)} - {AppName}";

        return IsModified ? baseName + "*" : baseName;
    }
}
