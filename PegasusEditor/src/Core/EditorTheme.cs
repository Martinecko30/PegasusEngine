using System.Numerics;
using ImGuiNET;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PegasusEditor;

public enum EditorCol
{
    Primary1, Primary2, Primary3,
    Secondary1, Secondary2,
    Accent1, Accent2,
    Text1, Text2,
    Background1, Background2, Background3, Background4,
    Error, Warning, Success,
    X, Y, Z,
    Count
}

public class EditorTheme
{
    public const string FileExtension = ".pgtheme";
    private readonly Vector4[] _colorPalette = new Vector4[(int)EditorCol.Count];

    public EditorTheme()
    {
        LoadDefaultDark();
        ApplyAllToImgui();
    }

    public Vector4 this[EditorCol col]
    {
        get => _colorPalette[(int)col];
        set => _colorPalette[(int)col] = value;
    }

    public void PushColor(ImGuiCol widget, EditorCol editorCol, float alpha = 1f)
    {
        var color = _colorPalette[(int)editorCol];
        color.W = alpha;
        ImGui.PushStyleColor(widget, color);
    }
    
    public void PopColor(int count = 1) => ImGui.PopStyleColor(count);
    
    public static Vector4 RGBA(byte r, byte g, byte b, byte a = 255) => new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);

    public (bool Success, string Error) SaveToFile(string filepath)
    {
        if (Path.GetExtension(filepath) != FileExtension)
            return (false, $"Invalid extension: {filepath}");

        try
        {
            var dict = new Dictionary<string, float[]>();
            for (int i = 0; i < (int)EditorCol.Count; i++)
            {
                var col = (EditorCol)i;
                var val = _colorPalette[i];
                dict[col.ToString()] = new[] {val.X, val.Y, val.Z, val.W };
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            File.WriteAllText(filepath, serializer.Serialize(dict));
            return (true, string.Empty);
        } catch (Exception e)
        {
            return (false, e.Message);
        }
    }
    
    public (bool Success, string Error) LoadFromFile(string filepath)
    {
        if (!File.Exists(filepath)) return (false, "File not found");

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var yaml = File.ReadAllText(filepath);
            var dict = deserializer.Deserialize<Dictionary<string, float[]>>(yaml);

            foreach (var entry in dict)
            {
                if (Enum.TryParse<EditorCol>(entry.Key, out var col) && entry.Value.Length == 4)
                {
                    this[col] = new Vector4(entry.Value[0], entry.Value[1], entry.Value[2], entry.Value[3]);
                }
            }

            ApplyAllToImgui();
            return (true, string.Empty);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }
    
    
    public void LoadDefaultDark()
    {
        this[EditorCol.Primary1] = RGBA(77, 77, 79);
        this[EditorCol.Primary2] = RGBA(70, 70, 77);
        this[EditorCol.Primary3] = RGBA(30, 30, 30);
        this[EditorCol.Secondary1] = RGBA(20, 20, 20);
        this[EditorCol.Secondary2] = RGBA(55, 55, 61);
        this[EditorCol.Accent1] = RGBA(66, 150, 250);
        this[EditorCol.Accent2] = RGBA(96, 115, 181);
        this[EditorCol.Text1] = RGBA(255, 255, 255);
        this[EditorCol.Text2] = RGBA(128, 128, 128);
        this[EditorCol.Background1] = RGBA(37, 37, 38);
        this[EditorCol.Background2] = RGBA(30, 30, 30);
        this[EditorCol.Background3] = RGBA(51, 51, 51);
        this[EditorCol.Background4] = RGBA(0, 0, 0);
        this[EditorCol.Error] = RGBA(219, 72, 115);
        this[EditorCol.Warning] = RGBA(213, 152, 87);
        this[EditorCol.Success] = RGBA(174, 243, 87);
        this[EditorCol.X] = RGBA(219, 72, 115);
        this[EditorCol.Y] = RGBA(174, 243, 87);
        this[EditorCol.Z] = RGBA(118, 162, 250);
        ApplyAllToImgui();
    }
    
    public void ApplyAllToImgui()
    {
        var style = ImGui.GetStyle();
        style.Colors[(int)ImGuiCol.WindowBg] = this[EditorCol.Background1];
        style.Colors[(int)ImGuiCol.PopupBg] = this[EditorCol.Background2];
        style.Colors[(int)ImGuiCol.Border] = this[EditorCol.Secondary2];
        style.Colors[(int)ImGuiCol.Header] = this[EditorCol.Primary3];
        style.Colors[(int)ImGuiCol.HeaderHovered] = this[EditorCol.Primary2];
        style.Colors[(int)ImGuiCol.HeaderActive] = this[EditorCol.Secondary2];
        style.Colors[(int)ImGuiCol.Button] = this[EditorCol.Primary3];
        style.Colors[(int)ImGuiCol.ButtonHovered] = this[EditorCol.Primary1];
        style.Colors[(int)ImGuiCol.ButtonActive] = this[EditorCol.Primary2];
        style.Colors[(int)ImGuiCol.CheckMark] = this[EditorCol.Text1];
        style.Colors[(int)ImGuiCol.SliderGrab] = this[EditorCol.Secondary2];
        style.Colors[(int)ImGuiCol.SliderGrabActive] = this[EditorCol.Accent1];
        style.Colors[(int)ImGuiCol.FrameBg] = this[EditorCol.Primary3];
        style.Colors[(int)ImGuiCol.FrameBgHovered] = this[EditorCol.Primary1];
        style.Colors[(int)ImGuiCol.FrameBgActive] = this[EditorCol.Primary2];
        style.Colors[(int)ImGuiCol.Tab] = this[EditorCol.Background2];
        style.Colors[(int)ImGuiCol.TabHovered] = this[EditorCol.Secondary2];
        // TODO: Check if this is correct
        // style.Colors[(int)ImGuiCol.TabActive] = this[EditorCol.Secondary2];
        style.Colors[(int)ImGuiCol.TabSelectedOverline] = this[EditorCol.Accent1];
        style.Colors[(int)ImGuiCol.TabDimmedSelectedOverline] = this[EditorCol.Primary1];
        // style.Colors[(int)ImGuiCol.TabUnfocused] = this[EditorCol.Secondary2];
        // style.Colors[(int)ImGuiCol.TabUnfocusedActive] = this[EditorCol.Secondary2];
        style.Colors[(int)ImGuiCol.TableRowBg] = this[EditorCol.Background2];
        style.Colors[(int)ImGuiCol.TableRowBgAlt] = this[EditorCol.Background1];
        style.Colors[(int)ImGuiCol.TitleBg] = this[EditorCol.Background2];
        style.Colors[(int)ImGuiCol.TitleBgActive] = this[EditorCol.Background2];
        style.Colors[(int)ImGuiCol.TitleBgCollapsed] = this[EditorCol.Background2];
        style.Colors[(int)ImGuiCol.ScrollbarGrab] = this[EditorCol.Secondary2];
        style.Colors[(int)ImGuiCol.Separator] = this[EditorCol.Primary2];
        style.Colors[(int)ImGuiCol.Text] = this[EditorCol.Text1];
        style.Colors[(int)ImGuiCol.TextDisabled] = this[EditorCol.Text2];
        style.Colors[(int)ImGuiCol.MenuBarBg] = this[EditorCol.Secondary1];
    }

}