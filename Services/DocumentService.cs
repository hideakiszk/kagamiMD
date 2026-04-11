using System.Text;

namespace KagamiMD.Services;

public class DocumentService
{
    public string ReadFile(string filePath, out Encoding detectedEncoding)
    {
        detectedEncoding = DetectEncoding(filePath);
        var content = File.ReadAllText(filePath, detectedEncoding);
        return NormalizeLineEndings(content, "\r\n");
    }

    public void WriteFile(string filePath, string text, Encoding encoding, string lineEnding)
    {
        var normalized = NormalizeLineEndings(text, lineEnding);
        File.WriteAllText(filePath, normalized, encoding);
    }

    public Encoding DetectEncoding(string filePath)
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

    public static string NormalizeLineEndings(string text, string lineEnding)
    {
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        return lineEnding == "\n" ? normalized : normalized.Replace("\n", lineEnding);
    }
}
