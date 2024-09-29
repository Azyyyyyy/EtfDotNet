namespace EtfDotNet.Types;

public partial record struct EtfContainer
{
    public readonly EtfTuple AsTuple()
    {
        if (Type == EtfConstants.LargeTupleExt)
        {
            return (EtfTuple) ComplexData!;
        }
        EnforceType(EtfConstants.SmallTupleExt);
        return (EtfTuple)ComplexData!;
    }
    
    public static implicit operator EtfContainer(EtfTuple tuple)
    {
        if (tuple.Count > 255)
        {
            return AsContainer(tuple, EtfConstants.LargeTupleExt);
        }
        return AsContainer(tuple, EtfConstants.SmallTupleExt);
    }
}