using ImGuiNET;
using PegasusEngine.Editor.Widgets;

namespace PegasusEngine.Editor;

public class Editor
{
    // Private
    private List<Widget> widgets;
    
    public Editor(List<string> args)
    {
        // TODO: Init engine

        ImGui.CreateContext();
        
        var io = ImGui.GetIO();
        io.ConfigFlags                      |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags                      |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags                      |= ImGuiConfigFlags.ViewportsEnable;
        io.ConfigFlags                      |= ImGuiConfigFlags.NoMouseCursorChange;
        io.ConfigWindowsResizeFromEdges     = true;
    }

    ~Editor()
    {
        if (ImGui.GetCurrentContext() != 0)
        {
            ImGui.DestroyContext();
        }
    }

    public void Update()
    {
        
    }

    private void BeginWindow()
    {
        
    }

    public T GetWidget<T>() where T : Widget
    {
        foreach (var widget in widgets)
            if (widget is T)
                return (T)widget;
        
        return null;
    }
}