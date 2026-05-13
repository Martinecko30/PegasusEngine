using PegasusEngine.Core.Events;

namespace PegasusEditor.TabPanels;

/// <summary>
/// Represents a dockable editor tab panel that can be started, rendered, updated, and disposed.
/// </summary>
public abstract class TabPanel : IDisposable
{
    /// <summary>
    /// The display title of the tab panel.
    /// </summary>
    public string Title;

    /// <summary>
    /// Called when the panel is initialized.
    /// </summary>
    public abstract void Start();
    
    /// <summary>
    /// Renders the panel contents to the screen.
    /// </summary>
    public abstract void Render();
    
    /// <summary>
    /// Updates panel logic outside of the render pass.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Handles an incoming engine or editor event.
    /// </summary>
    /// <param name="e">The event to handle.</param>
    public abstract void OnEvent(IEvent e);

    /// <summary>
    /// Releases resources used by the panel.
    /// </summary>
    public abstract void Dispose();
}