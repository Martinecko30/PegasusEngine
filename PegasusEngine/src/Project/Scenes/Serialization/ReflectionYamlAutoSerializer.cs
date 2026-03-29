using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using PegasusEngine.Objects;

namespace PegasusEngine.Project.Scenes.Serialization;

public sealed class ReflectionYamlAutoSerializer
{
    public Dictionary<string, object?> SerializeObjectGraph(object obj)
    {
        if (obj is null) throw new ArgumentNullException(nameof(obj));

        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        var type = obj.GetType();

        foreach (var field in GetSerializableFields(type))
        {
            var value = field.GetValue(obj);
            result[field.Name] = SerializeValue(value, field.FieldType);
        }

        return result;
    }

    private IEnumerable<FieldInfo> GetSerializableFields(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Includes inherited fields too.
        var fields = GetAllFields(type, flags);

        foreach (var f in fields)
        {
            if (f.IsStatic) continue;
            if (f.IsLiteral) continue;          // const
            if (f.IsInitOnly) continue;         // readonly
            if (Attribute.IsDefined(f, typeof(NonSerializedAttribute))) continue;

            bool isPublic = f.IsPublic;
            bool hasSerializeField = Attribute.IsDefined(f, typeof(SerializeFieldAttribute));

            if (!isPublic && !hasSerializeField) continue;

            // Must be of a type we can serialize
            if (!CanSerializeType(f.FieldType)) continue;

            yield return f;
        }
    }

    private static IEnumerable<FieldInfo> GetAllFields(Type t, BindingFlags flags)
    {
        for (var cur = t; cur != null; cur = cur.BaseType)
        {
            foreach (var f in cur.GetFields(flags))
                yield return f;
        }
    }

    private object? SerializeValue(object? value, Type declaredType)
    {
        if (value is null) return null;

        var runtimeType = value.GetType();

        if (value is EngineObject engineObject)
            return (ulong)engineObject.Guid;

        // Primitives / simple scalars
        if (IsScalar(runtimeType))
            return value;

        // Enums
        if (runtimeType.IsEnum)
            return value.ToString();

        // Arrays
        if (runtimeType.IsArray)
        {
            var arr = (Array)value;
            var elemType = runtimeType.GetElementType()!;
            var list = new List<object?>(arr.Length);
            foreach (var item in arr)
                list.Add(SerializeValue(item, elemType));
            return list;
        }

        // List<T>
        if (IsGenericList(runtimeType, out var itemType))
        {
            var listObj = (IEnumerable)value;
            var list = new List<object?>();
            foreach (var item in listObj)
                list.Add(SerializeValue(item, itemType));
            return list;
        }

        // Custom [Serializable] class/struct
        if (IsCustomSerializable(runtimeType))
        {
            return SerializeObjectGraph(value);
        }

        // Anything else: skip (or throw, your call)
        return null;
    }

    private bool CanSerializeType(Type t)
    {
        if (IsScalar(t)) return true;
        if (t.IsEnum) return true;
        
        if (typeof(EngineObject).IsAssignableFrom(t))
            return true;

        if (t.IsArray)
            return CanSerializeType(t.GetElementType()!);

        if (IsGenericList(t, out var itemType))
            return CanSerializeType(itemType);

        if (IsCustomSerializable(t))
            return true;

        return false;
    }

    private static bool IsScalar(Type t)
    {
        // Expand this list if you want (DateTime, TimeSpan, etc.)
        return t.IsPrimitive
               || t == typeof(string)
               || t == typeof(decimal);
    }

    private static bool IsGenericList(Type t, out Type itemType)
    {
        itemType = typeof(object);

        if (!t.IsGenericType) return false;
        if (t.GetGenericTypeDefinition() != typeof(List<>)) return false;

        itemType = t.GetGenericArguments()[0];
        return true;
    }

    private static bool IsCustomSerializable(Type t)
    {
        if (t.IsAbstract) return false; // “Custom non abstract classes...”
        if (t.IsInterface) return false;

        // Structs are never abstract; classes can be.
        bool hasSerializable = Attribute.IsDefined(t, typeof(SerializableAttribute));
        return hasSerializable;
    }
}