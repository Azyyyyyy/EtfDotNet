using EtfDotNet.Extensions;

namespace EtfDotNet.Types;

public class EtfMap : List<(EtfContainer, EtfContainer)>, IEtfComplex
{
    [Pure]
    public int GetSize()
    {
        var size = 4; // uint length (count)
        foreach ((EtfContainer, EtfContainer) container in this)
        {
            size += container.Item1.GetSize();
            size += container.Item2.GetSize();
        }
        return size;
    }

    public int GetSerializedSize()
    {
        var size = 4; // uint length (count)
        foreach ((EtfContainer, EtfContainer) container in this)
        {
            size += EtfEncoder.CalculateTypeSize(container.Item1);
            size += EtfEncoder.CalculateTypeSize(container.Item2);
        }
        return size;
    }

    public void Serialize(EtfMemory memory)
    {
        memory.WriteUInt((uint)Count);
        foreach ((EtfContainer, EtfContainer) container in this)
        {
            EtfEncoder.EncodeType(container.Item1, memory);
            EtfEncoder.EncodeType(container.Item2, memory);
        }
    }
    
    public void Dispose()
    {
        foreach ((EtfContainer, EtfContainer) container in this)
        {
            container.Item1.Dispose();
            container.Item2.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }
    
    public void Add(EtfContainer key, EtfContainer value) => Add((key, value));
}