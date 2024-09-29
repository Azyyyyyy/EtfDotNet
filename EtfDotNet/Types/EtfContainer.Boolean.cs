namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public static implicit operator EtfContainer(bool val)
    {
        return FromAtom(val ? "true" : "false");
    }
    
    public static implicit operator bool(EtfContainer v)
    {
        v.EnforceType(EtfConstants.AtomExt);
        string atomicValue = v.ToAtom();
        if (atomicValue != "true" && atomicValue != "false")
        {
            throw new InvalidCastException($"Cannot convert atom of value \"{atomicValue}\" to a bool");
        }
        return atomicValue == "true";
    }
    
    public static implicit operator bool?(EtfContainer v)
    {
        v.EnforceType(EtfConstants.AtomExt);
        string atomicValue = v.ToAtom();
        if (atomicValue != "true" && atomicValue != "false")
        {
            return null;
        }
        return atomicValue == "true";
    }
}