using System;

namespace PegasusEngine.Objects.Components;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class RequireComponent : Attribute
{
    public Type RequiredComponentType { get; private set; }
    public RequireComponent(Type componentType)
    {
        RequiredComponentType = componentType;
    }
}