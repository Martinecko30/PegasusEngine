using System.Collections;
using System.Reflection;

namespace PegasusEngine.Pegasus.Project.Scenes.Serialization;


public sealed class ReflectionYamlAutoDeserializer
{
    public void ApplyObjectGraph(object target, Dictionary<string, object?> data)
    {
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (data is null) throw new ArgumentNullException(nameof(data));

        var type = target.GetType();

        foreach (var field in GetSerializableFields(type))
        {
            if (!data.TryGetValue(field.Name, out var rawValue))
                continue;

            try
            {
                var converted = ConvertToFieldType(rawValue, field.FieldType);
                field.SetValue(target, converted);
            }
            catch
            {
                // If something doesn't convert cleanly, skip it (or log).
            }
        }
    }

    private static IEnumerable<FieldInfo> GetSerializableFields(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        for (var cur = type; cur != null; cur = cur.BaseType)
        {
            foreach (var f in cur.GetFields(flags))
            {
                if (f.IsStatic) continue;
                if (f.IsLiteral) continue;      // const
                if (f.IsInitOnly) continue;     // readonly
                if (Attribute.IsDefined(f, typeof(NonSerializedAttribute))) continue;

                bool isPublic = f.IsPublic;
                bool hasSerializeField = Attribute.IsDefined(f, typeof(SerializeFieldAttribute));

                if (!isPublic && !hasSerializeField) continue;

                yield return f;
            }
        }
    }

    private object? ConvertToFieldType(object? raw, Type fieldType)
    {
        if (raw is null)
            return null;

        // Already assignable (common for scalars)
        if (fieldType.IsInstanceOfType(raw))
            return raw;

        // Enums stored as string (from serializer) or numeric
        if (fieldType.IsEnum)
        {
            if (raw is string s)
                return Enum.Parse(fieldType, s, ignoreCase: true);

            return Enum.ToObject(fieldType, raw);
        }

        // Scalars
        if (IsScalar(fieldType))
            return Convert.ChangeType(raw, fieldType);

        // Array
        if (fieldType.IsArray)
        {
            var elemType = fieldType.GetElementType()!;
            if (raw is IEnumerable seq)
            {
                var tmp = new List<object?>();
                foreach (var item in seq)
                    tmp.Add(ConvertToFieldType(item, elemType));

                var arr = Array.CreateInstance(elemType, tmp.Count);
                for (int i = 0; i < tmp.Count; i++)
                    arr.SetValue(tmp[i], i);

                return arr;
            }
        }

        // List<T>
        if (IsGenericList(fieldType, out var itemType))
        {
            if (raw is IEnumerable seq)
            {
                var list = (IList)Activator.CreateInstance(fieldType)!;
                foreach (var item in seq)
                    list.Add(ConvertToFieldType(item, itemType));
                return list;
            }
        }

        // Nested [Serializable] object (Dictionary<string, object?> expected)
        if (IsCustomSerializable(fieldType) && raw is Dictionary<string, object?> map)
        {
            var nested = Activator.CreateInstance(fieldType);
            if (nested is null) return null;

            ApplyObjectGraph(nested, map);
            return nested;
        }

        // If you later support object references, handle them here.
        return null;
    }

    private static bool IsScalar(Type t)
        => t.IsPrimitive || t == typeof(string) || t == typeof(decimal);

    private static bool IsGenericList(Type t, out Type itemType)
    {
        itemType = typeof(object);
        if (!t.IsGenericType) return false;
        if (t.GetGenericTypeDefinition() != typeof(List<>)) return false;
        itemType = t.GetGenericArguments()[0];
        return true;
    }

    private static bool IsCustomSerializable(Type t)
        => Attribute.IsDefined(t, typeof(SerializableAttribute)) && !t.IsInterface && !t.IsAbstract;
}