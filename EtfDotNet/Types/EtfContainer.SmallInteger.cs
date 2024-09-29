namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public static implicit operator EtfContainer(byte v)
    {
        EtfContainer container = Make(1, EtfConstants.SmallIntegerExt);
        container.ContainedData[0] = v;
        return container;
    }
    
    public static implicit operator byte(EtfContainer v)
    {
        v.EnforceType(EtfConstants.SmallIntegerExt);
        return v.ContainedData[0];
    }
}