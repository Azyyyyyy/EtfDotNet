namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public static implicit operator EtfContainer(string v)
    {
        int count = Encoding.Latin1.GetByteCount(v);
        if (count > 65535)
        {
            throw new EtfException("The given string is longer than 65535. LIST_EXT is not yet implemented.");
        }
        EtfContainer container = Make(count, EtfConstants.StringExt);
        Encoding.Latin1.GetBytes(v, container.ContainedData);
        return container;
    }
    
    public static implicit operator string(EtfContainer v)
    {
        v.EnforceType(EtfConstants.StringExt);
        return Encoding.Latin1.GetString(v.ContainedData);
    }
}