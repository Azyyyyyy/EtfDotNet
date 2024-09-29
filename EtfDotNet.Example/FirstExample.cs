using EtfDotNet.Attributes;
using EtfDotNet.Extensions;
using EtfDotNet.Types;

namespace EtfDotNet.Example;

public class FirstExample
{
    public static void RunExample()
    {
        Console.WriteLine(EtfSerializer.Deserialize<long>(345));

        var test = new EtfMap
        {
            { "its not a property", "abc" }
        };
        var tuple = new EtfTuple(16);
        tuple[0] = "a";
        tuple[1] = "b";
        tuple[2] = "c";
        tuple[3] = "d";
        tuple[4] = "e";
        tuple[5] = "f";
        tuple[6] = "g";
        tuple[7] = "h";
        tuple[8] = "i";
        tuple[9] = "j";
        tuple[10] = "k";
        tuple[11] = "l";
        tuple[12] = "m";
        tuple[13] = "n";
        tuple[14] = "o";
        tuple[15] = "p";
        //var test2 = (EtfContainer)tuple;
        var t = EtfSerializer
            .Deserialize<(string, string, string, string, string, string, string, string, string, string, string, string
                , string, string, string, string)>(test);
        Console.WriteLine($"{t} / {t.GetType()}");

        var map = new EtfMap();
        map.Add("test", "abc");
        map.Add("other", "def");
        map.Add("v", "ghi");
        map.Add("nope", "jkl");
        var custom = EtfSerializer.Deserialize<IDictionary<string, string>>(map);
        Console.WriteLine("-----");
        Console.WriteLine(custom.GetType());
        Console.WriteLine(custom["test"]);
        Console.WriteLine(custom["v"]);
        Console.WriteLine(custom["nope"]);
        Console.WriteLine("=====");
        IEnumerator<KeyValuePair<string, string>> e = custom.GetEnumerator();
        while (e.MoveNext())
        {
            Console.WriteLine($"{e.Current.Key} : {e.Current.Value}");
        }

        Console.WriteLine("-----");

        //Console.WriteLine(clz.ThisIsAField);

        EtfContainer customClassContainer = EtfSerializer.Serialize(new CustomClass());
        Console.WriteLine(customClassContainer.ToJson());
    }
}

class CustomClass
{
    public string Test;
    [EtfName("other")] public string V;
    [EtfIgnore] public string Nope = "not set";
}