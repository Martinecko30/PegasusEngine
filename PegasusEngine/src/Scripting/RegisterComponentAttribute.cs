using PegasusEngine.Objects.Components;

namespace PegasusEngine.Scripting;

public abstract class RegisterComponentAttribute : Attribute
{
    public Type ComponentType { get; }

    private protected RegisterComponentAttribute(Type componentType)
    {
        ComponentType = componentType;
    }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class RegisterComponentAttribute<T> : RegisterComponentAttribute
    where T : Component
{
    public RegisterComponentAttribute()
        : base(typeof(T))
    {
    }
}