namespace EtfDotNet.Extensions;

public static class EtfContainerExtensions
{
    public static JsonNode? ToJson(this EtfContainer type)
    {
        if (type.Type == EtfConstants.MapExt)
        {
            var obj = new JsonObject();
            foreach ((EtfContainer k, EtfContainer v) in type.AsMap())
            {
                JsonNode? key = ToJson(k);
                var keyName = key!.AsValue().GetValue<string>();
                obj[keyName] = ToJson(v);
            }
            return obj;
        }
        if (type.Type == EtfConstants.ListExt)
        {
            var arr = new JsonArray();
            foreach (EtfContainer v in type.AsList())
            {
                arr.Add(ToJson(v));
            }
            return arr;
        }
        if (type.Type is EtfConstants.SmallTupleExt or EtfConstants.LargeTupleExt)
        {
            var arr = new JsonArray();
            foreach (EtfContainer v in type.AsTuple())
            {
                arr.Add(ToJson(v));
            }
            return arr;
        }
        if (type.Type == EtfConstants.AtomExt)
        {
            string atom = type.ToAtom();
            return atom switch {
                "false" => JsonValue.Create(false),
                "true" => JsonValue.Create(true),
                "nil" => null,
                _ => JsonValue.Create(atom)
            };
        }

        return type.Type switch
        {
            EtfConstants.IntegerExt => JsonValue.Create((int)type),
            EtfConstants.NewFloatExt => JsonValue.Create((double)type),
            EtfConstants.StringExt => JsonValue.Create((string)type),
            EtfConstants.SmallBigExt => JsonValue.Create(((BigInteger)type).ToString()),
            EtfConstants.BinaryExt => JsonValue.Create(Encoding.Latin1.GetString(type.ContainedData)),
            EtfConstants.SmallIntegerExt => JsonValue.Create((byte)type),
            EtfConstants.NilExt => new JsonArray(),
            _ => throw new EtfException($"Unknown EtfType: {type.Type}")
        };
    }
}