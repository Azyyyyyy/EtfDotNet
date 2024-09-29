namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public readonly EtfList AsList()
    {
        EnforceType(EtfConstants.ListExt);
        return (EtfList)ComplexData!;
    }
    public readonly EtfMap AsMap()
    {
        EnforceType(EtfConstants.MapExt);
        return (EtfMap)ComplexData!;
    }
    
    public static implicit operator EtfContainer(EtfList list)
    {
        return AsContainer(list, EtfConstants.ListExt);
    }
    public static implicit operator EtfContainer(EtfMap map)
    {
        return AsContainer(map, EtfConstants.MapExt);
    }
}