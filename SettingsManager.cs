using System.Runtime.InteropServices;
using System.Text;

namespace KagamiMD;

public static class SettingsManager
{
    private static readonly string IniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

    public static void SaveTabWidth(int tabWidth)
    {
        WriteValue("Editor", "TabWidth", tabWidth.ToString());
    }

    public static void SaveFont(Font font)
    {
        WriteValue("Editor", "FontName", font.Name);
        WriteValue("Editor", "FontSize", font.Size.ToString());
        WriteValue("Editor", "FontStyle", ((int)font.Style).ToString());
    }

    public static void SavePreviewMode(bool realTime)
    {
        WriteValue("Preview", "RealTime", realTime.ToString());
    }

    public static void LoadSettings(out int tabWidth, out Font font, out bool realTime, out List<string> fileHistory, Font defaultFont)
    {
        string section = "Editor";
        string tabStr = ReadValue(section, "TabWidth", "4");
        if (!int.TryParse(tabStr, out tabWidth)) tabWidth = 4;

        string fontName = ReadValue(section, "FontName", defaultFont.Name);
        string fontSizeStr = ReadValue(section, "FontSize", defaultFont.Size.ToString());
        string fontStyleStr = ReadValue(section, "FontStyle", ((int)defaultFont.Style).ToString());

        float fontSize;
        if (!float.TryParse(fontSizeStr, out fontSize)) fontSize = defaultFont.Size;

        int fontStyleInt;
        FontStyle fontStyle;
        if (int.TryParse(fontStyleStr, out fontStyleInt))
            fontStyle = (FontStyle)fontStyleInt;
        else
            fontStyle = defaultFont.Style;

        try
        {
            font = new Font(fontName, fontSize, fontStyle);
        }
        catch
        {
            font = defaultFont;
        }

        string realTimeStr = ReadValue("Preview", "RealTime", "True");
        realTime = bool.Parse(realTimeStr);

        fileHistory = LoadFileHistory();
    }

    public static List<string> LoadFileHistory()
    {
        var history = new List<string>();
        for (int i = 1; i <= 10; i++)
        {
            string path = ReadValue("History", "File" + i, "");
            if (!string.IsNullOrEmpty(path))
            {
                history.Add(path);
            }
        }
        return history;
    }

    public static void SaveFileHistory(List<string> history)
    {
        // 既存の履歴を一旦クリア（10個まで）
        for (int i = 1; i <= 10; i++)
        {
            WriteValue("History", "File" + i, "");
        }

        // 新しい履歴を保存
        for (int i = 0; i < Math.Min(history.Count, 10); i++)
        {
            WriteValue("History", "File" + (i + 1), history[i]);
        }
    }

    private static void WriteValue(string section, string key, string value)
    {
        WritePrivateProfileString(section, key, value, IniPath);
    }

    private static string ReadValue(string section, string key, string defaultValue)
    {
        var sb = new StringBuilder(1024); // パスが長い場合を考慮して拡張
        GetPrivateProfileString(section, key, defaultValue, sb, 1024, IniPath);
        return sb.ToString();
    }
}
