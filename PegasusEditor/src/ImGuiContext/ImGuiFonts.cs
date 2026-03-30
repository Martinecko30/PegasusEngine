using ImGuiNET;
using PegasusEngine.Core;
using PegasusEngine.Debug;

namespace PegasusEditor.ImGuiContext;

public enum Fonts
{
    Default,
    NotoSans,
    WumpusMono
}

public class ImGuiFonts
{
    private static readonly Dictionary<Fonts, ImFontPtr> _loaded = new();
    private static readonly Stack<Fonts> _stack = new();
    
    public static bool IsLoaded(Fonts font) => _loaded.ContainsKey(font);

    /// <summary>
    /// Call once after you create/load fonts (io.Fonts.AddFont...) and before you start rendering.
    /// </summary>
    public static unsafe void Register(Fonts key, ImFontPtr font) => _loaded[key] = font;

    /// <summary>
    /// Optional: clear registry + stack when recreating the whole ImGui context/atlas.
    /// </summary>
    public static void Reset()
    {
        _loaded.Clear();
        PopFont(_stack.Count);
    }
    
    public static void PushFont(Fonts font)
    {
        if (!_loaded.TryGetValue(font, out var imFont))
            throw new InvalidOperationException($"Font '{font}' is not loaded/registered. Register it before using PushFont().");
        
        ImGui.PushFont(imFont);
        _stack.Push(font);
    }
    
    public static void PopFont()
    {
        PopFont(1);
    }
    
    public static void PopFont(int amount)
    {
        if (amount <= 0)
        {
            Log.EditorWarn("Tried to pop 0 or negative amount of fonts.");
            return;
        }
        
        for (int i = 0; i < amount; i++)
        {
            if (_stack.Count == 0) 
                throw new InvalidOperationException("PopFont called more times than PushFont.");
            
            ImGui.PopFont();
            _stack.Pop();
        }
    }
    
    /// <summary>
    /// Convenience: using(ImGuiFonts.Scoped(Fonts.Mono)) { ... }
    /// </summary>
    public static FontScope Scoped(Fonts font) => new(font);

    public readonly struct FontScope : IDisposable
    {
        private readonly bool _active;

        public FontScope(Fonts font)
        {
            PushFont(font);
            _active = true;
        }

        public void Dispose()
        {
            if (_active) PopFont();
        }
    }
}