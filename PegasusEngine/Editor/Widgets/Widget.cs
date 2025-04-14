using System.Numerics;
using ImGuiNET;

namespace PegasusEngine.Editor.Widgets;

public class Widget
{
    // Protected
    protected bool _isWindow            = true;
    protected ImGuiWindowFlags _flags   = ImGuiWindowFlags. NoCollapse;
    protected float _alpha              = default;
    protected Vector2 _initialSize      = Vector2.Zero;
    protected Vector2 _minSize          = Vector2.Zero;
    protected Vector2 _maxSize          = Vector2.PositiveInfinity;
    protected Vector2 _padding          = Vector2.Zero;

    private int varPushCount            = 0;
    
    protected Editor _editor            = null;
    
    // Public
    public float Height                 = 0;
    public bool Visible                 = true;
    public string Title                 = "Title";
    
    
    public Widget(Editor editor)
    {
        this._editor = editor;
    }

    // Always called
    public virtual void Update()
    {
        if (!_isWindow || !Visible)
            return;
        
        // Begin
        if (_initialSize != Vector2.Zero)
            ImGui.SetNextWindowSize(_initialSize, ImGuiCond.FirstUseEver);
        
        if (_minSize != Vector2.Zero || _maxSize != Vector2.PositiveInfinity)
            ImGui.SetNextWindowSizeConstraints(_minSize, _maxSize);

        if (_padding != Vector2.Zero)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, _padding);
            varPushCount++;
        }

        if (_alpha != default)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, _alpha);
            varPushCount++;
        }
        
        OnPreBegin();

        if (ImGui.Begin(Title, ref Visible, _flags))
            Height = ImGui.GetWindowHeight();
        
        if (ImGui.IsWindowAppearing())
            OnVisible();
        else if (!Visible)
            OnInvisible();
        
        OnUpdateVisible();
        
        // End
        ImGui.End();
        ImGui.PopStyleVar(varPushCount);
        varPushCount = 0;
    }
    
    // Called only when the widget is visible
    public virtual void OnUpdateVisible() {}
    
    // Called when the window becomes visible
    public virtual void OnVisible() {}
    
    // Called when the window becomes invisible
    public virtual void OnInvisible() {}

    // Called just before ImGui.Begin()
    public virtual void OnPreBegin()
    {
        Viewport viewport = _editor.GetWidget<Viewport>();
        if (viewport == null)
            return;
        
        var pos = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();
        Vector2 center = new (pos.X + size.X * 0.5f, pos.Y + size.Y * 0.5f);
        Vector2 pivot = new (0.5f, 0.5f);
        
        ImGui.SetNextWindowPos(center, ImGuiCond.FirstUseEver, pivot);
    }

    public Vector2 GetCenter()
    {
        var pos = ImGui.GetWindowPos();
        var size = ImGui.GetWindowSize();
        Vector2 center = new (pos.X + size.X * 0.5f, pos.Y + size.Y * 0.5f);
        
        return center;
    }
}