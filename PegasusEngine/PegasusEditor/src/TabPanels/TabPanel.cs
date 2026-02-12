using PegasusEngine.Pegasus.Core.Events;

namespace PegasusEngine.PegasusEditor.TabPanels;

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

    /// <summary>
    /// This
    /// </summary>
    /// <param name="e"></param>
    public abstract void OnEvent(IEvent e);
    
    public void Dispose()
    {
        // TODO: Disposing
    }
}