using EtfDotNet.Extensions;

namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public static implicit operator EtfContainer(int v)
    {
        EtfContainer container = Make(4, EtfConstants.IntegerExt);
        container.ContainedData.WriteUInt((uint)v);
        return container;
    }
    
    public static implicit operator int(EtfContainer v)
    {
        v.EnforceType(EtfConstants.IntegerExt);
        return (int)v.ContainedData.ReadUInt();
    }
}