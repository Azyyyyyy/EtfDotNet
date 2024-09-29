using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using EtfDotNet.Attributes;

namespace EtfDotNet;

public static class EtfSerializer
{
    private const DynamicallyAccessedMemberTypes DeserializeDynamicallyAccessedMemberTypes =
        DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces;

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static EtfContainer Serialize<T>(T value)
    {
        return Serialize(value, JsonSerializerOptions.Default);
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "<Pending>")]
    public static EtfContainer Serialize<T>(T value, JsonSerializerContext serializerContext)
    {
        return Serialize(value, serializerContext.Options);
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static EtfContainer Serialize<T>(T value, JsonSerializerOptions serializerOptions)
    {
        if (value is null)
        {
            return EtfContainer.FromAtom("nil");
        }

        switch (value)
        {
            case bool vBool:
                return vBool ? EtfContainer.FromAtom("true") : EtfContainer.FromAtom("false");
            case int vInt:
                return vInt;
            case BigInteger vBigi:
                return vBigi;
            case string vStr:
                return vStr;
            case byte vByte:
                return vByte;
            case double vDbl:
                return vDbl;
            case long vLong:
                return (BigInteger)vLong;
            case ulong vUlong:
                return (BigInteger)vUlong;
            case IConvertible:
                return JsonSerializer.Serialize(value, serializerOptions).Trim(['"']);
        }

        Type type = value.GetType();
        if (type.IsGenericType)
        {
            if (value is IDictionary vDict)
            {
                var map = new EtfMap();
                foreach (DictionaryEntry entry in vDict)
                {
                    map.Add((Serialize(entry.Key, serializerOptions), Serialize(entry.Value, serializerOptions)));
                }

                return map;
            }
            if (value is IEnumerable vEnu)
            {
                var list = new EtfList();
                foreach (object? item in vEnu)
                {
                    list.Add(Serialize(item, serializerOptions));
                }
                return list;
            }
        }

        if (value is ITuple vTup)
        {
            var tup = new EtfTuple((uint)vTup.Length);
            for (var i = 0; i < vTup.Length; i++)
            {
                tup[i] = Serialize(vTup[i], serializerOptions);
            }
            return tup;
        }

        // complex object map type
        var etfMap = new EtfMap();
        PropertyInfo[] props = type.GetProperties();
        FieldInfo[] fields = type.GetFields();
        foreach (PropertyInfo? propertyInfo in props)
        {
            (EtfContainer, EtfContainer)? val = SerializeMember(propertyInfo, propertyInfo.GetValue(value), serializerOptions);
            if(val.HasValue) etfMap.Add(val.Value);
        }
        foreach (FieldInfo? fieldInfo in fields)
        {
            (EtfContainer, EtfContainer)? val = SerializeMember(fieldInfo, fieldInfo.GetValue(value), serializerOptions);
            if(val.HasValue) etfMap.Add(val.Value);
        }

        return etfMap;
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    private static (EtfContainer, EtfContainer)? SerializeMember(MemberInfo info, object? value, JsonSerializerOptions serializerOptions)
    {
        if (info.GetCustomAttribute<EtfIgnoreAttribute>() is not null) return null;
        string name = info.Name;
        var etfName = info.GetCustomAttribute<EtfNameAttribute>();
        if (etfName is not null)
        {
            name = etfName.SerializedName;
        }

        return (EtfContainer.FromAtom(name), Serialize(value, serializerOptions));
    }

    private static string? GetMappedMemberName(MemberInfo info)
    {
        if (info.GetCustomAttribute<EtfIgnoreAttribute>() is not null) return null;
        string name = info.Name;
        var etfName = info.GetCustomAttribute<EtfNameAttribute>();
        if (etfName is not null)
        {
            name = etfName.SerializedName;
        }
        return name;
    }
    
    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static T? Deserialize<[DynamicallyAccessedMembers(DeserializeDynamicallyAccessedMemberTypes)] T>(EtfContainer container)
    {
        return (T?) Deserialize(container, typeof(T), JsonSerializerOptions.Default);
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "<Pending>")]
    public static T? Deserialize<[DynamicallyAccessedMembers(DeserializeDynamicallyAccessedMemberTypes)] T>(EtfContainer container, JsonSerializerContext serializerContext)
    {
        return (T?) Deserialize(container, typeof(T), serializerContext.Options);
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static T? Deserialize<[DynamicallyAccessedMembers(DeserializeDynamicallyAccessedMemberTypes)] T>(EtfContainer container, JsonSerializerOptions serializerOptions)
    {
        return (T?) Deserialize(container, typeof(T), serializerOptions);
    }
    
    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static object? Deserialize(EtfContainer container, [DynamicallyAccessedMembers(DeserializeDynamicallyAccessedMemberTypes)] Type t)
    {
        return Deserialize(container, t, JsonSerializerOptions.Default);
    }
    
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "<Pending>")]
    public static object? Deserialize(EtfContainer container, [DynamicallyAccessedMembers(DeserializeDynamicallyAccessedMemberTypes)] Type t, JsonSerializerContext serializerContext)
    {
        return Deserialize(container, t, serializerContext.Options);
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static object? Deserialize(EtfContainer container, [DynamicallyAccessedMembers(DeserializeDynamicallyAccessedMemberTypes)] Type t, JsonSerializerOptions serializerOptions)
    {
        if (container.Type == EtfConstants.AtomExt)
        {
            string name = container.ToAtom();
            return name switch {
                "true" => true,
                "false" => false,
                "nil" => null,
                _ => name.To(t)
            };
        }
        if (container.Type == EtfConstants.BinaryExt)
        {
            if (typeof(string).IsAssignableFrom(t))
            {
                return Encoding.UTF8.GetString((ArraySegment<byte>) container);
            }
            if (typeof(byte[]).IsAssignableFrom(t))
            {
                return ((ArraySegment<byte>) container).ToArray();
            }
            if (typeof(ArraySegment<byte>).IsAssignableFrom(t))
            {
                return (ArraySegment<byte>) container;
            }
            if (t.IsAssignableTo(typeof(IConvertible)) && t != typeof(string))
            {
                return JsonSerializer.Deserialize(container, t, serializerOptions);
            }
            throw new EtfException($"Cannot convert BinaryExt to {t}");
        }
        if (container.Type == EtfConstants.StringExt)
        {
            if (t.IsAssignableTo(typeof(IConvertible)) && t != typeof(string))
            {
                return JsonSerializer.Deserialize('"' + (string)container + '"', t, serializerOptions);
            }
            return ((string) container).To(t);
        }
        if (container.Type == EtfConstants.SmallIntegerExt)
        {
            return ((byte) container).To(t);
        }
        if (container.Type == EtfConstants.IntegerExt)
        {
            return ((int) container).To(t);
        }
        if (container.Type == EtfConstants.SmallBigExt)
        {
            return ((BigInteger) container).To(t);
        }
        if (container.Type == EtfConstants.NewFloatExt)
        {
            return ((double) container).To(t);
        }
        if (container.Type is EtfConstants.SmallTupleExt or EtfConstants.LargeTupleExt)
        {
            return ToTuple(container.AsTuple(), t, serializerOptions);
        }

        if (container.Type == EtfConstants.NilExt)
        {
            if (t.IsArray)
            {
                return Array.CreateInstance(t.GetElementType()!, 0);
            }

            Type[]? args = GetEnumerableType(t);

            if (args == null)
            {
                throw new EtfException($"Expected type {t} to implement IEnumerable<>");
            }
            
            if (args.Length != 1)
            {
                throw new EtfException($"Expected one generic type argument for type {t}");
            }

            Type enuType = typeof(IEnumerable<>).MakeGenericType(args);
            Type listType = typeof(List<>).MakeGenericType(args);
            if (!t.IsAssignableTo(enuType) && t != enuType)
            {
                throw new EtfException("Mismatched type, cannot assign NilExt to non enumerable type");
            }

            if (listType.IsAssignableTo(t))
            {
                return Activator.CreateInstance(listType);
            }
            return Activator.CreateInstance(t);
        }
        
        if (container.Type == EtfConstants.ListExt)
        {
            EtfList data = container.AsList();
            if (t.IsArray)
            {
                var arr = Array.CreateInstance(t.GetElementType()!, data.Count);
                for (var i = 0; i < data.Count; i++)
                {
                    arr.SetValue(Deserialize(data[i], t.GetElementType(), serializerOptions), i);
                }

                return arr;
            }

            Type[]? args = GetEnumerableType(t);

            if (args == null)
            {
                throw new EtfException($"Expected type {t} to implement IEnumerable<>");
            }
            
            if (args.Length != 1)
            {
                throw new EtfException($"Expected one generic type argument for type {t}");
            }
            
            var mappedData = Array.CreateInstance(args[0], data.Count);
            for (var i = 0; i < data.Count; i++)
            {
                mappedData.SetValue(Deserialize(data[i], args[0], serializerOptions), i);
            }

            Type enuType = typeof(IEnumerable<>).MakeGenericType(args);
            Type listType = typeof(List<>).MakeGenericType(args);
            if (!t.IsAssignableTo(enuType) && t != enuType)
            {
                throw new EtfException("Mismatched type, cannot assign NilExt to non enumerable type");
            }

            return Activator.CreateInstance(listType.IsAssignableTo(t) ? listType : t, mappedData);

        }

        if (container.Type == EtfConstants.MapExt)
        {
            EtfMap map = container.AsMap();
            Type[]? arg = GetDictionaryType(t);
            if (t.IsGenericType && arg is not null)
            {
                if (typeof(Dictionary<,>).IsAssignableTo(t))
                {
                    t = typeof(Dictionary<,>).MakeGenericType(arg);
                }

                Type keyType = arg[0];
                Type valueType = arg[1];
                var dict = (IDictionary?) Activator.CreateInstance(t);
                foreach ((EtfContainer etfKey, EtfContainer etfValue) in map)
                {
                    object? key = Deserialize(etfKey, keyType, serializerOptions);
                    if (key == null)
                    {
                        throw new EtfException("Key is null");
                    }
                    dict[key] = Deserialize(etfValue, valueType, serializerOptions);
                }
                return dict;
            }

            var etfMembers = new Dictionary<string, EtfContainer>();
            foreach ((EtfContainer etfKey, EtfContainer etfValue) in map)
            {
                if (etfKey.Type != EtfConstants.BinaryExt && etfKey.Type != EtfConstants.StringExt)
                {
                    throw new EtfException("Mismatched type, cannot deserialize non-string map keys into an object");
                }

                var key = (string)etfKey;
                if (!etfMembers.TryAdd(key, etfValue))
                {
                    throw new EtfException("Invalid type, cannot deserialize a map containing duplicate keys into an object");
                }
            }

            var members = new Dictionary<string, MemberInfo>();
            PropertyInfo[] props = t.GetProperties();
            FieldInfo[] fields = t.GetFields();
            foreach (PropertyInfo? propertyInfo in props)
            {
                string? val = GetMappedMemberName(propertyInfo);
                if(val is null) continue;
                members[val] = propertyInfo;
            }
            foreach (FieldInfo? fieldInfo in fields)
            {
                string? val = GetMappedMemberName(fieldInfo);
                if(val is null) continue;
                members[val] = fieldInfo;
            }

            // assign values to members
            object? obj = Activator.CreateInstance(t);
            foreach ((string? key, EtfContainer value) in etfMembers)
            {
                if (!members.TryGetValue(key, out MemberInfo? info))
                {
                    throw new EtfException($"Mismatched member, cannot map etf-serialized key {key} to a member of the type {t}. No matching member found.");
                }

                switch (info)
                {
                    case FieldInfo fi:
                        fi.SetValue(obj, Deserialize(value, fi.FieldType, serializerOptions));
                        break;
                    case PropertyInfo pi:
                        pi.SetValue(obj, Deserialize(value, pi.PropertyType, serializerOptions));
                        break;
                }
            }

            return obj;
        }
        throw new EtfException($"Deserializing {container.Type} is not implemented, report this bug.");
    }

    private static Type[]? GetEnumerableType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments();
        }
        return type.GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            ?.GetGenericArguments();
    }

    private static Type[]? GetDictionaryType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
        {
            return type.GetGenericArguments();
        }
        return type.GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            ?.GetGenericArguments();
    }

    private static long GetTupleLength(Type t)
    {
        if (!typeof(ITuple).IsAssignableFrom(t)) return 1;
        long amt = t.GenericTypeArguments.LongLength;
        if (amt == 8)
        {
            return 7 + GetTupleLength(t.GenericTypeArguments[7]);
        }
        return amt;
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    private static ITuple? CreateTuple([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type t, object?[] values, JsonSerializerOptions serializerOptions)
    {
        Type[] typeArguments = t.GenericTypeArguments;
        
        object?[] vals;
        if (values.Length >= 8)
        {
            vals = values[..8];
            vals[7] = CreateTuple(typeArguments[7], values[7..], serializerOptions);
        }
        else
        {
            vals = values;
        }

        return (ITuple?)Activator.CreateInstance(t, vals);
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    private static ITuple? ToTuple(EtfTuple tuple, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type t, JsonSerializerOptions serializerOptions)
    {
        if (!typeof(ITuple).IsAssignableFrom(t))
        {
            throw new EtfException($"Cannot convert EtfTuple to {t}, expected an ITuple or a subclass of it");
        }

        long len = GetTupleLength(t);
        if (len != tuple.Length)
        {
            throw new EtfException($"Tuple lengths are not the same, expected {tuple.Length} got {len}");
        }

        var values = new object?[tuple.Length];
        for (var i = 0; i < values.Length; i++)
        {
            int genericIndex = i;
            Type generic = t.GenericTypeArguments[genericIndex < 8 ? genericIndex : 7];
            while (genericIndex >= 7)
            {
                genericIndex -= 7;
                generic = generic.GenericTypeArguments[genericIndex < 8 ? genericIndex : 7];
            }
            values[i] = Deserialize(tuple[i], generic, serializerOptions);
        }

        return CreateTuple(t, values, serializerOptions);
    }
}