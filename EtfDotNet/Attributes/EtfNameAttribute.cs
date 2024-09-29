namespace EtfDotNet.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class EtfNameAttribute : Attribute
{
    public EtfNameAttribute(string serializedName)
    {
        SerializedName = serializedName;
    }

    public string SerializedName { get; set; }
}