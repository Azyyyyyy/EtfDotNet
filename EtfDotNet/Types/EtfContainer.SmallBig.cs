namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public static implicit operator EtfContainer(BigInteger big)
    {
        bool sign = big.Sign == decimal.MinusOne;
        byte[] bytes = sign ? (-big).ToByteArray(true) : big.ToByteArray(true);
        if (bytes.Length > 255)
        {
            throw new EtfException("Cannot encode number with more than 255 bytes");
        }
        EtfContainer container = Make(1 + bytes.Length, EtfConstants.SmallBigExt);
        EtfMemory mem = EtfMemory.FromArray(container.ContainedData);
        mem.WriteByte((byte)(sign ? 1 : 0));
        mem.Write(bytes);
        return container;
    }
    
    public static implicit operator BigInteger(EtfContainer v)
    {
        v.EnforceType(EtfConstants.SmallBigExt);
        byte sign = v.ContainedData[0];
        var num = new BigInteger(v.ContainedData.Slice(1), isUnsigned: true);
        if (sign == 1) num = -num;
        return num;
    }
}