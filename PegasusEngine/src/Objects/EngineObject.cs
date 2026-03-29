using PegasusEngine.Core;

namespace PegasusEngine.Objects;

[Serializable]
public class EngineObject
{
    public GUID Guid { get; protected set; }
}