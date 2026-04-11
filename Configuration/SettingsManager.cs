using System.Text.Json;

namespace KagamiMD.Configuration;

public static class SettingsManager
{
    private static readonly string JsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
    private static AppConfig _currentConfig = new AppConfig();

    public static AppConfig Current => _currentConfig;

    public static void LoadSettings(Font defaultFont)
    {
        if (File.Exists(JsonPath))
        {
            try
            {
                string json = File.ReadAllText(JsonPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json);
                if (config != null)
                {
                    _currentConfig = config;
                    return;
                }
            }
            catch
            {
                // Fallback to default on error
            }
        }

        // Initialize defaults
        _currentConfig = new AppConfig
        {
            EditorFont = defaultFont,
            TabWidth = 4,
            RealTimePreview = true,
            FileHistory = new List<string>()
        };
        Save();
    }

    public static void SaveTabWidth(int tabWidth)
    {
        _currentConfig.TabWidth = tabWidth;
        Save();
    }

    public static void SaveFont(Font font)
    {
        _currentConfig.EditorFont = font;
        Save();
    }

    public static void SavePreviewMode(bool realTime)
    {
        _currentConfig.RealTimePreview = realTime;
        Save();
    }

    public static void SaveFileHistory(List<string> history)
    {
        _currentConfig.FileHistory = history.Take(10).ToList();
        Save();
    }

    private static void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_currentConfig, options);
            File.WriteAllText(JsonPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}
