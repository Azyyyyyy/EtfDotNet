using EtfDotNet.Extensions;

namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public static implicit operator EtfContainer(double v)
    {
        EtfContainer container = Make(8, EtfConstants.NewFloatExt);
        container.ContainedData.WriteULong(BitConverter.DoubleToUInt64Bits(v));
        return container;
    }
    
    public static implicit operator double(EtfContainer v)
    {
        v.EnforceType(EtfConstants.NewFloatExt);
        return BitConverter.UInt64BitsToDouble(v.ContainedData.ReadULong());
    }
}