using EtfDotNet.Extensions;

namespace EtfDotNet.Types;

public partial record struct EtfContainer : IDisposable
{
    internal IEtfComplex? ComplexData;
    private bool _returnToPool;

    public static readonly EtfContainer Nil = AsContainer(ArraySegment<byte>.Empty, EtfConstants.NilExt);
    public ArraySegment<byte> ContainedData;
    public EtfConstants Type;

    public static EtfContainer Make(int length, EtfConstants type)
    {
        return new EtfContainer
        {
            ContainedData = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(length), 0, length),
            Type = type,
            _returnToPool = true
        };
    }

    public static EtfContainer AsContainer(ArraySegment<byte> data, EtfConstants type)
    {
        var container = new EtfContainer
        {
            Type = type,
            ContainedData = data,
            _returnToPool = false
        };
        return container;
    }

    public static EtfContainer AsContainer(IEtfComplex complexData, EtfConstants type)
    {
        var container = new EtfContainer
        {
            Type = type,
            ComplexData = complexData,
            _returnToPool = false,
        };
        return container;
    }
    
    public readonly void Dispose()
    {
        ComplexData?.Dispose();
        if (!_returnToPool) return;
        ContainedData.ReturnShared();
    }

    [Pure]
    public int GetSize()
    {
        return ComplexData?.GetSize() ?? ContainedData.Count;
    }

    [Pure]
    public int GetSerializedSize()
    {
        return EtfEncoder.CalculateTypeSize(this);
    }

    [Pure]
    public ArraySegment<byte> Serialize(out bool shouldReturnToSharedPool)
    {
        if (ComplexData is not null)
        {
            shouldReturnToSharedPool = true;
            int size = ComplexData.GetSerializedSize();
            var arr = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(size), 0, size);
            EtfMemory mem = EtfMemory.FromArray(arr);
            ComplexData.Serialize(mem);
            return arr;
        }

        shouldReturnToSharedPool = false;
        return ContainedData;
    }

    private readonly void EnforceType(EtfConstants type)
    {
        if (Type != type)
            throw new InvalidCastException($"The EtfContainer is of type {Type} and not {type}");
    }
}