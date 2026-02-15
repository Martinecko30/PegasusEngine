using PegasusEngine.Core;

namespace PegasusEngine.Project.Scenes.Serialization;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public sealed class GUIDYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(GUID);
    
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        if (ulong.TryParse(scalar.Value, out ulong result))
        {
            return new GUID(result);
        }
        return GUID.INVALID;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var guid = (GUID)value!;
        emitter.Emit(new Scalar(guid.ToString()));
    }
}