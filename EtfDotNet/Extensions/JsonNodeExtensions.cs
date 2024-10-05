namespace EtfDotNet.Extensions;

public static class JsonNodeExtensions
{
    public static EtfContainer ToEtf(this JsonNode? type, EtfSerializerOptions options)
    {
        if (type is null)
        {
            return EtfContainer.Nil;
        }

        if (type is JsonValue val)
        {
            JsonValueKind kind = val.GetValueKind();
            switch (kind)
            {
                case JsonValueKind.String:
                    return Encoding.UTF8.GetBytes(val.GetValue<string>());
                case JsonValueKind.Number:
                    break;
                case JsonValueKind.True:
                    return EtfContainer.FromAtom("true");
                case JsonValueKind.False:
                    return EtfContainer.FromAtom("false");
                case JsonValueKind.Null:
                    return EtfContainer.Nil;
                //Already handled below
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    break;
                case JsonValueKind.Undefined:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (val.TryGetValue(out double vDouble))
            {
                if (vDouble is >= byte.MinValue and <= byte.MaxValue)
                    return (byte)vDouble;
                if (vDouble is >= int.MinValue and <= int.MaxValue)
                    return (int)vDouble;

                return vDouble;
            }

            string v = val.GetValue<object>().ToString() ?? "";
            if (long.TryParse(v, out long vLong))
            {
                if (vLong is >= byte.MinValue and <= byte.MaxValue)
                    return (byte)vLong;
                if (vLong is >= int.MinValue and <= int.MaxValue)
                    return (int)vLong;

                return (BigInteger)vLong;
            }
            throw new EtfException($"Unknown Json value type: {v.GetType()}");
        }
        if (type is JsonArray arr)
        {
            var list = new EtfList();
            foreach (JsonNode? value in arr)
            {
                list.Add(value.ToEtf(options));
            }
            return list;
        }
        if (type is JsonObject obj)
        {
            var map = new EtfMap();
            foreach ((string? k, JsonNode? v) in obj)
            {
                EtfContainer nameContainer = EtfSerializer.GetStringContainer(k, options);
                map.Add((nameContainer, v.ToEtf(options)));
            }
            return map;
        }

        throw new EtfException($"Unsupported Json node: {type}");
    }
}