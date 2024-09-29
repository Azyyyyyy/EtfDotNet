using System.Text.Json;
using AutoFixture;
using EtfDotNet.Attributes;
using EtfDotNet.Extensions;
using EtfDotNet.Types;

namespace EtfDotNet.Example;

public class SecondExample
{
    public static void RunExample()
    {
        //var res = EtfSerializer.Deserialize<bool>(EtfContainer.FromAtom("true"));

        var fixture = new Fixture();
        var obj = fixture.Create<CustomClass2>();

        EtfContainer serialized = EtfSerializer.Serialize(obj);

        Console.WriteLine(serialized.ToJson());

        var deserialized = EtfSerializer.Deserialize<CustomClass2>(serialized);

        Console.WriteLine(JsonSerializer.Serialize(deserialized, new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        }));
    }
}

class CustomClass2
{
    public string Test;

    public Dictionary<string, long> NiceDictionary { get; set; }
    public Dictionary<string, int> NicerDictionary { get; set; }
    public Dictionary<string, DateTime> NicestDictionary { get; set; }
    public Dictionary<string, byte[]> BetterDictionary { get; set; }
    [EtfName("betterDictionary")] public int Notevenadictionary { get; set; }

    public Dictionary<string, (int, long, bool, string, int, int, int, int, int, int, int, int, int, int, int, int,
        int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int
        , int, int, int, int)> BestDictionary { get; set; }

    [EtfName("other")] public string V;
    [EtfIgnore] public string Nope = "not set";
}