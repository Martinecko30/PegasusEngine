using PegasusEngine.Pegasus.Core.Events;

namespace PegasusEngine.Pegasus.Core.Layers;

public interface ILayer
{
    void OnAttach();
    void OnDetach();
    void OnUpdate(float deltaTime);
    void OnEvent(IEvent e);
}

public abstract class Layer : ILayer
{
    public virtual void OnAttach() { }

    public virtual void OnDetach() { }

    public virtual void OnUpdate(float deltaTime) { }

    public virtual void OnEvent(IEvent e) { }
}