namespace PegasusEngine.Editor.Tabs;

public abstract class TabPanel : IDisposable
{
    public string Title;

    /// <summary>
    /// Start is called on loading the engine.
    /// </summary>
    public abstract void Start();
    /// <summary>
    /// Renders current panel on screen.
    /// </summary>
    public abstract void Render();
    
    /// <summary>
    /// Updates any logic.
    /// Use outside of rendering to keep consistency.
    /// </summary>
    public abstract void Update();

    public void Dispose()
    {
        // TODO: Disposing
    }
}