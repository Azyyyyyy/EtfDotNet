namespace EtfDotNet;

public class EtfSerializerOptions
{
    public static readonly EtfSerializerOptions Default = new EtfSerializerOptions();
    
    public KeyOption KeyOption { get; set; } = KeyOption.Atom;
    public StringOption StringOption { get; set; } = StringOption.Binary;
    
    public bool CheckJsonAttributes { get; set; } = false;
    
    public bool IncludeFields { get; set; } = true;

    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
}

public enum StringOption
{
    Binary,
    String
}

public enum KeyOption
{
    Binary,
    String,
    Atom
}