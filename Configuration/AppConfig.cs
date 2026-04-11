using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;

namespace KagamiMD.Configuration;

/// <summary>
/// Serializable configuration model
/// </summary>
public class AppConfig
{
    public int TabWidth { get; set; } = 4;
    public string FontName { get; set; } = "Segoe UI";
    public float FontSize { get; set; } = 9.0f;
    public int FontStyle { get; set; } = (int)System.Drawing.FontStyle.Regular;
    public bool RealTimePreview { get; set; } = true;
    public List<string> FileHistory { get; set; } = new List<string>();

    [JsonIgnore]
    public Font EditorFont
    {
        get
        {
            try
            {
                return new Font(FontName, FontSize, (FontStyle)FontStyle);
            }
            catch
            {
                return new Font("Segoe UI", 9.0f, System.Drawing.FontStyle.Regular);
            }
        }
        set
        {
            FontName = value.Name;
            FontSize = value.Size;
            FontStyle = (int)value.Style;
        }
    }
}
