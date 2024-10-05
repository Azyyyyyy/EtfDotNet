using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using EtfDotNet.Attributes;
using EtfDotNet.Extensions;

namespace EtfDotNet;

public class MemberMetadata
{
    private readonly PropertyInfo? _propertyInfo;
    private readonly FieldInfo? _fieldInfo;
    private readonly object _value;
    private readonly EtfSerializerOptions _serializerOptions;
    private string? _name;

    public MemberMetadata(PropertyInfo? propertyInfo, object value, EtfSerializerOptions serializerOptions)
    {
        _propertyInfo = propertyInfo;
        _value = value;
        _serializerOptions = serializerOptions;
    }
        
    public MemberMetadata(FieldInfo? fieldInfo, object value, EtfSerializerOptions serializerOptions)
    {
        _fieldInfo = fieldInfo;
        _value = value;
        _serializerOptions = serializerOptions;
    }

    public T? GetCustomAttribute<T>()
        where T : Attribute
    {
        MemberInfo memberInfo = AsMemberInfo();
        return memberInfo.GetCustomAttribute<T>();
    }
        
    public object? GetValue()
    {
        if (_propertyInfo != null)
        {
            return _propertyInfo.GetValue(_value);
        }
            
        return _fieldInfo!.GetValue(_value);
    }

    private MemberInfo AsMemberInfo() => _propertyInfo ?? (MemberInfo?)_fieldInfo ?? throw new InvalidOperationException();
    
    public string Name => _name ??= EtfSerializer.GetName(AsMemberInfo(), _serializerOptions);
}

public static partial class EtfSerializer
{
    private const DynamicallyAccessedMemberTypes DeserializeDynamicallyAccessedMemberTypes =
        DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces;

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static EtfContainer Serialize<T>(T value)
    {
        return Serialize(value, EtfSerializerOptions.Default);
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static EtfContainer Serialize<T>(T? value, EtfSerializerOptions serializerOptions)
    {
        return Serialize(value, serializerOptions, 256);
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    private static EtfContainer Serialize<T>(T? value, EtfSerializerOptions serializerOptions, int depthLimit)
    {
        if (depthLimit < 0)
        {
            throw new Exception("Exceeded recursion limit");
        }

        if (value == null)
        {
            return EtfContainer.FromAtom("nil");
        }

        if (value is EtfContainer container)
        {
            return container;
        }

        if (value is Enum)
        {
            object realValue = Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
            return Serialize(realValue, serializerOptions, depthLimit);
        }

        if (value is JsonProperty)
        {
            Debug.Fail("Should never get here");
        }
        if (value is JsonNode jsonNode)
        {
            return jsonNode.ToEtf(serializerOptions);
        }
        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Array)
            { 
                return EtfList.From(jsonElement.EnumerateArray(), serializerOptions);
            }
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var map = new EtfMap();
                foreach (JsonProperty innerJsonProperty in jsonElement.EnumerateObject())
                {
                    map.Add(GetStringContainer(innerJsonProperty.Name, serializerOptions), Serialize(innerJsonProperty.Value, serializerOptions, depthLimit - 1));
                }
                
                return map;
            }

            var jsonValue = JsonValue.Create(jsonElement);
            return jsonValue.ToEtf(serializerOptions);
        }

        switch (value)
        {
            case bool vBool:
                return EtfContainer.FromAtom(vBool ? "true" : "false");
            case int vInt:
                return SerializeNumber(vInt);
            case BigInteger vBigInt:
                return vBigInt;
            case string vStr:
                return GetStringContainer(vStr, serializerOptions);
            case float vFloat:
                return vFloat;
            case byte vByte:
                return vByte;
            case double vDouble:
                return vDouble;
            case long vLong:
                return SerializeNumber(vLong);
            case ulong vUlong:
                return (BigInteger)vUlong;
            case IConvertible:
                return JsonSerializer.Serialize(value, value.GetType(), serializerOptions.JsonSerializerOptions).Trim(['"']);
        }

        if (value is IDictionary vDict)
        {
            var map = new EtfMap();
            foreach (DictionaryEntry entry in vDict)
            {
                map.Add((Serialize(entry.Key, serializerOptions, depthLimit - 1), Serialize(entry.Value, serializerOptions, depthLimit - 1)));
            }

            return map;
        }

        if (value is IEnumerable vEnu)
        {
            var list = new EtfList();
            foreach (object? item in vEnu)
            {
                list.Add(Serialize(item, serializerOptions, depthLimit - 1));
            }

            return list.Count > 0 ? list : Serialize<object>(null, serializerOptions, depthLimit - 1);
        }

        if (value is ITuple vTup)
        {
            var tup = new EtfTuple((uint)vTup.Length);
            for (var i = 0; i < vTup.Length; i++)
            {
                tup[i] = Serialize(vTup[i], serializerOptions, depthLimit - 1);
            }
            return tup;
        }

        var etfMap = new EtfMap();
        Type type = value.GetType();
        PropertyInfo[] props = type.GetProperties();
        FieldInfo[] fields = serializerOptions.IncludeFields ? type.GetFields() : [];
        
        var membersMetadata = new MemberMetadata[props.Length + fields.Length];
        for (int i = 0; i < props.Length; i++)
        {
            membersMetadata[i] = new MemberMetadata(props[i], value, serializerOptions);
        }
        for (int i = 0; i < fields.Length; i++)
        {
            membersMetadata[i + props.Length] = new MemberMetadata(fields[i], value, serializerOptions);
        }

        foreach (MemberMetadata? memberMetadata in membersMetadata.OrderBy(x => x.Name))
        {
            (EtfContainer, EtfContainer)? val = SerializeMember(memberMetadata, serializerOptions, depthLimit);
            if(val.HasValue) etfMap.Add(val.Value);
        }

        return etfMap;
    }

    private static EtfContainer SerializeNumber<T>(T value)
        where T : INumber<T>
    {
        if (value is int or long)
        {
            if (value >= T.CreateChecked(0) && value <= T.CreateChecked(255))
            {
                return byte.CreateChecked(value);
            }
            if (value >= T.CreateChecked(-2147483648) && value <= T.CreateChecked(2147483647))
            {
                return int.CreateChecked(value);
            }

            if (value > T.CreateChecked(0))
            {
                return ulong.CreateChecked(value);
            }

            return long.CreateChecked(value);
        }

        throw new Exception("Unsupported type");
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    private static (EtfContainer, EtfContainer)? SerializeMember(MemberMetadata info, EtfSerializerOptions serializerOptions, int depthLimit)
    {
        object? value = info.GetValue();

        if (info.GetCustomAttribute<EtfIgnoreAttribute>() is not null)
        {
            return null;
        }
        if (serializerOptions.CheckJsonAttributes)
        {
            var jsonIgnore = info.GetCustomAttribute<JsonIgnoreAttribute>();
            if (jsonIgnore is not null)
            {
                switch (jsonIgnore.Condition)
                {
                    case JsonIgnoreCondition.Never:
                        break;
                    case JsonIgnoreCondition.Always:
                        return null;
                    case JsonIgnoreCondition.WhenWritingDefault:
                        if (value == default)
                        {
                            return null;
                        }
                        break;
                    case JsonIgnoreCondition.WhenWritingNull:
                        if (value == null)
                        {
                            return null;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        string name = info.Name;
        return (GetStringContainer(name, serializerOptions), Serialize(value, serializerOptions, depthLimit - 1));
    }

    internal static string GetName(MemberInfo info, EtfSerializerOptions serializerOptions)
    {
        var etfName = info.GetCustomAttribute<EtfNameAttribute>();
        if (etfName is not null)
        {
            return etfName.SerializedName;
        }
        if (serializerOptions.CheckJsonAttributes)
        {
            var jsonName = info.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonName is not null)
            {
                return jsonName.Name;
            }
        }

        return info.Name;
    }

    internal static EtfContainer GetStringContainer(string name, EtfSerializerOptions serializerOptions)
    {
        return serializerOptions.StringOption switch
        {
            StringOption.String => name,
            StringOption.Binary => Encoding.Unicode.GetBytes(name),
            _ => throw new ArgumentOutOfRangeException(nameof(serializerOptions), serializerOptions.KeyOption, "Unknown key name option")
        };
    }
}