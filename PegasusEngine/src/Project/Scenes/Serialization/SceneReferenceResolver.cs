using System.Reflection;
using PegasusEngine.Common;
using PegasusEngine.Core;
using PegasusEngine.Objects;
using PegasusEngine.Objects.Components;
using PegasusEngine.Project.Assets;

namespace PegasusEngine.Project.Scenes.Serialization;

/// <summary>
/// Handles the two-pass deserialization of scene references.
/// Queues GUIDs to be resolved later.
/// </summary>
public static class SceneReferenceResolver
{
    private static readonly Queue<(object Target, FieldInfo Field, GUID WantedGuid)> queue = new();
    
    public static void QueueResolution(object target, FieldInfo field, GUID wantedGuid) => queue.Enqueue((target, field, wantedGuid));

    public static void ResolverAll(Scene loadedScene)
    {
        if (queue.Count <= 0)
            return;

        while (queue.Count > 0)
        {
            var (target, field, wantedGuid) = queue.Dequeue();
            
            if (loadedScene.Entities.TryGetValue(wantedGuid, out var linkedEntity))
            {
                if (field.FieldType == typeof(GameObject))
                {
                    field.SetValue(target, linkedEntity);
                    continue;
                }

                if (typeof(Component).IsAssignableFrom(field.FieldType))
                {
                    var component = linkedEntity.GetComponent(field.FieldType);
                    if (component != null)
                        field.SetValue(target, component);
                    continue;
                }
            }
            
            
        }
        
        queue.Clear();
    }
    
}