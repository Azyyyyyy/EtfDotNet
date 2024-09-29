namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public static implicit operator EtfContainer(byte[] v)
    {
        return AsContainer(v, EtfConstants.BinaryExt);
    }
    public static implicit operator EtfContainer(ArraySegment<byte> v)
    {
        return AsContainer(v, EtfConstants.BinaryExt);
    }
    public static implicit operator ArraySegment<byte>(EtfContainer v)
    {
        v.EnforceType(EtfConstants.BinaryExt);
        return v.ContainedData;
    }
}