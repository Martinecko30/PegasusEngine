using PegasusEngine.Core.Events;

namespace PegasusEngine.Core.Layers;

public interface IEventDispatcher
{
    void DispatchEvent(IEvent e);
}

public class LayerStack : IEventDispatcher
{
    private readonly List<ILayer> _layers = new();
    
    public void PushLayer(ILayer layer)
    {
        _layers.Add(layer);
        layer.OnAttach();
    }

    public void PopLayer(ILayer layer)
    {
        if (_layers.Remove(layer)) layer.OnDetach();
    }

    public void OnUpdate()
    {
        foreach (var layer in _layers)
            layer.OnUpdate();
    }

    public void OnDetach()
    {
        foreach (var layer in _layers) layer.OnDetach();
        _layers.Clear();
    }

    public void DispatchEvent(IEvent e)
    {
        // In event systems, we usually iterate backwards (top-to-bottom)
        // so the top-most layer can consume the event first.
        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            _layers[i].OnEvent(e);
            if (e.IsConsumed)
                break;
        }
    }
}