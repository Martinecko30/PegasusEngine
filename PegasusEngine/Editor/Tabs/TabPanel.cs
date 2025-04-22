using PegasusEngine.Engine;

namespace PegasusEngine.Editor.Tabs;

public abstract class TabPanel
{
    public string Title;

    public abstract void Start(EngineWindow engine);
    public abstract void Render();
}