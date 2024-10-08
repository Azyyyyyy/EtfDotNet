using EtfDotNet.Extensions;

namespace EtfDotNet.Types;

public class EtfTuple : IReadOnlyList<EtfContainer>, IEtfComplex, ITuple
{
    private static readonly EtfContainer NilAtom = EtfContainer.FromAtom("nil");
    private readonly EtfContainer[] _array;

    public int Count => _array.Length;

    public EtfContainer this[int index] {
        get => _array[index];
        set => _array[index] = value;
    }
    
    object ITuple.this[int index] => _array[index];

    public int Length => _array.Length;

    public EtfContainer this[uint index] {
        get => _array[index];
        set => _array[index] = value;
    }

    public EtfTuple(uint length)
    {
        _array = new EtfContainer[length];
        for (var i = 0u; i < length; i++)
        {
            _array[i] = NilAtom;
        }
    }
    
    public int GetSize()
    {
        int size = _array.Length > 255 ? 4 : 1; // uint length (count)
        foreach (EtfContainer container in _array)
        {
            size += container.GetSize();
        }
        return size;
    }
    
    public int GetSerializedSize()
    {
        int size = _array.Length > 255 ? 4 : 1; // uint length (count)
        foreach (EtfContainer container in _array)
        {
            int calculatedSize = EtfEncoder.CalculateTypeSize(container);
            size += calculatedSize;
        }
        return size;
    }

    public void Serialize(EtfMemory memory)
    {
        if (_array.Length > 255)
        {
            memory.WriteUInt((uint) Count);
        } else
        {
            memory.WriteByte((byte) Count);
        }
        foreach (EtfContainer container in _array)
        {
            EtfEncoder.EncodeType(container, memory);
        }
    }

    public void Dispose()
    {
        foreach (EtfContainer container in _array)
        {
            container.Dispose();
        }
    }

    public IEnumerator<EtfContainer> GetEnumerator()
    {
        return ((IEnumerable<EtfContainer>) _array).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}