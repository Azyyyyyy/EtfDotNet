namespace EtfDotNet.Extensions;

public static class JsonNodeExtensions
{
    public static EtfContainer ToEtf(this JsonNode? type)
    {
        if (type is null)
        {
            return "nil";
        }

        if (type is JsonValue val)
        {
            if (val.TryGetValue(out bool vBool))
            {
                return vBool ? "true" : "false";
            }
            if (val.TryGetValue(out string? vString))
            {
                return Encoding.UTF8.GetBytes(vString);
            }
            if (val.TryGetValue(out double vDouble))
            {
                return vDouble;
            }
            var v = val.GetValue<object>();
            if (long.TryParse(v.ToString(), out long vLong))
            {
                return vLong switch
                {
                    >= byte.MinValue and <= byte.MaxValue => (byte)vLong,
                    >= int.MinValue and <= int.MaxValue => (int)vLong,
                    _ => (BigInteger)vLong
                };
            }
            throw new EtfException($"Unknown Json value type: {v.GetType()}");
        }
        if (type is JsonArray arr)
        {
            var list = new EtfList();
            foreach (JsonNode? value in arr)
            {
                list.Add(ToEtf(value));
            }
            return list;
        }
        if (type is JsonObject obj)
        {
            var map = new EtfMap();
            foreach ((string? k, JsonNode? v) in obj)
            {
                map.Add((k, ToEtf(v)));
            }
            return map;
        }

        throw new EtfException($"Unsupported Json node: {type}");
    }
}