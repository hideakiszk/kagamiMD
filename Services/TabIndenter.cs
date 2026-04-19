using System.Text;

namespace KagamiMD.Services;

/// <summary>
/// テキストエディタのタブインデント・デインデント処理を担う純粋な静的クラス。
/// UI依存がないためユニットテスト可能。
/// </summary>
public static class TabIndenter
{
    /// <summary>
    /// 指定した行範囲に対してインデントまたはデインデントを行い、
    /// 置換後のテキストと選択範囲を返す。
    /// </summary>
    /// <param name="lines">TextBox.Lines と同等の、改行を含まない行の配列</param>
    /// <param name="startLine">インデント対象の開始行インデックス（0始まり）</param>
    /// <param name="endLine">インデント対象の終了行インデックス（0始まり、inclusive）</param>
    /// <param name="tabWidth">スペースインデント解除時に除去するスペース数</param>
    /// <param name="deindent">true = インデント解除、false = インデント追加</param>
    /// <returns>変換後テキストと、その文字列内での新しい選択長</returns>
    public static (string NewText, int NewSelectionLength) Apply(
        string[] lines, int startLine, int endLine, int tabWidth, bool deindent)
    {
        var sb = new StringBuilder();

        for (int i = startLine; i <= endLine; i++)
        {
            string line = lines[i];

            if (deindent)
            {
                if (line.StartsWith('\t'))
                {
                    line = line.Substring(1);
                }
                else if (line.StartsWith(' '))
                {
                    int spaceCount = 0;
                    while (spaceCount < tabWidth && spaceCount < line.Length && line[spaceCount] == ' ')
                    {
                        spaceCount++;
                    }
                    line = line.Substring(spaceCount);
                }
            }
            else
            {
                line = "\t" + line;
            }

            sb.Append(line);

            // lines 配列は改行を含まないため、後続行が存在する場合のみ CRLF を付与する
            if (i < lines.Length - 1)
            {
                sb.Append("\r\n");
            }
        }

        string newText = sb.ToString();
        return (newText, newText.Length);
    }
}
