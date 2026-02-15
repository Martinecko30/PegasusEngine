using System.Collections.Concurrent;
using PegasusEngine.Scripting;

namespace PegasusEngine.Project.Scenes.Serialization;

public sealed class ComponentYamlRegistry
{
    private sealed record Writer(string TypeId, Func<Component, Dictionary<string, object?>> DataFactory);

    private readonly ConcurrentDictionary<Type, Writer> _writers = new();

    public void Register<TComponent>(string typeId, Func<TComponent, Dictionary<string, object?>> dataFactory)
        where TComponent : Component
    {
        if (string.IsNullOrWhiteSpace(typeId))
            throw new ArgumentException("typeId cannot be null/empty.", nameof(typeId));
        if (dataFactory is null)
            throw new ArgumentNullException(nameof(dataFactory));

        _writers[typeof(TComponent)] = new Writer(
            typeId,
            c => dataFactory((TComponent)c)
        );
    }

    public bool TryWrite(Component component, out ComponentDto dto)
    {
        dto = new ComponentDto();

        if (!_writers.TryGetValue(component.GetType(), out var writer))
            return false;

        dto.Type = writer.TypeId;
        dto.Data = writer.DataFactory(component);
        return true;
    }
}