using System.Diagnostics.CodeAnalysis;
using EtfDotNet.Extensions;

namespace EtfDotNet.Types;

public class EtfList : List<EtfContainer>, IEtfComplex
{
    public EtfList() { }
    // ReSharper disable once MemberCanBePrivate.Global
    public EtfList(IEnumerable<EtfContainer> collection) : base(collection) { }
    public EtfList(int capacity) : base(capacity) { }
    
    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static EtfContainer From<T>(IEnumerable<T> collection)
    {
        return new EtfList(collection.Select(EtfSerializer.Serialize));
    }

    [RequiresUnreferencedCode("<Pending>")]
    [RequiresDynamicCode("<Pending>")]
    public static EtfContainer From<T>(IEnumerable<T> collection, EtfSerializerOptions serializerOptions)
    {
        return new EtfList(collection.Select(x => EtfSerializer.Serialize(x, serializerOptions)));
    }
    
    public int GetSize()
    {
        var size = 5; // uint length + 1 EtfConstant
        foreach (EtfContainer container in this)
        {
            size += container.GetSize();
        }
        return size;
    }
    
    public int GetSerializedSize()
    {
        var size = 5; // uint length (count) + 1 EtfConstant
        foreach (EtfContainer container in this)
        {
            int calculatedSize = EtfEncoder.CalculateTypeSize(container);
            size += calculatedSize;
        }
        return size;
    }

    public void Serialize(EtfMemory memory)
    {
        memory.WriteUInt((uint)Count);
        foreach (EtfContainer container in this)
        {
            EtfEncoder.EncodeType(container, memory);
        }
        memory.WriteConstant(EtfConstants.NilExt);
    }

    public void Dispose()
    {
        foreach (EtfContainer container in this)
        {
            container.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }
}