using System.Diagnostics.CodeAnalysis;

namespace EtfDotNet;

public class EtfMemory
{
    private readonly ArraySegment<byte>? _arraySource;
    private readonly Stream? _streamSource;
    private int _position;

    [MemberNotNullWhen(false, nameof(_arraySource))]
    [MemberNotNullWhen(true, nameof(_streamSource))]
    private bool IsStreamed { get; }
    
    private EtfMemory(ArraySegment<byte> arraySource)
    {
        _arraySource = arraySource;
        IsStreamed = false;
    }

    private EtfMemory(Stream streamSource)
    {
        _streamSource = streamSource;
        IsStreamed = true;
    }

    public static EtfMemory FromArray(ArraySegment<byte> source)
    {
        return new EtfMemory(source);
    }
    public static EtfMemory FromStream(Stream source)
    {
        return new EtfMemory(source);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public int Read(ArraySegment<byte> destination)
    {
        if (IsStreamed)
        {
            return _streamSource.Read(destination);
        }

        if (_position + destination.Count > _arraySource.Value.Count)
        {
            return _arraySource.Value.Count - _position;
        }
        
        Buffer.BlockCopy(_arraySource.Value.Array!, _position + _arraySource.Value.Offset, destination.Array!, destination.Offset, destination.Count);
        _position += destination.Count;
        return destination.Count;
    }

    public ArraySegment<byte> ReadSlice(int length)
    {
        if (IsStreamed)
        {
            throw new InvalidOperationException("This operation cannot be performed on a streamed memory segment");
        }

        ArraySegment<byte> slice = _arraySource.Value.Slice(_position, length);
        _position += length;
        return slice;
    }

    public EtfContainer ReadContainer(int length, EtfConstants type)
    {
        if (IsStreamed)
        {
            var container = EtfContainer.Make(length, type);
            ReadExactly(container.ContainedData);
            return container;
        }

        return EtfContainer.AsContainer(ReadSlice(length), type);
    }

    public void ReadExactly(ArraySegment<byte> destination)
    {
        var offset = 0;
        while (offset < destination.Count)
        {
            int readCount = Read(destination.Slice(destination.Offset + offset, destination.Count - offset));
            if (readCount == 0)
                throw new IndexOutOfRangeException("The specified data length will exceed the bounded capacity of the memory segment.");
            offset += readCount;
        }
    }

    public void Write(ArraySegment<byte> data)
    {
        if (IsStreamed)
        {
            _streamSource.Write(data);
            return;
        }

        if (_position + data.Count > _arraySource.Value.Count)
        {
            throw new IndexOutOfRangeException(
                "The specified data length will exceed the bounded capacity of the memory segment.");
        }
        Buffer.BlockCopy(data.Array!, data.Offset, _arraySource.Value.Array!, _position + _arraySource.Value.Offset, data.Count);
        _position += data.Count;
    }

    public int ReadByte()
    {
        if (IsStreamed)
        {
            return _streamSource.ReadByte();
        }

        if (_position + 1 > _arraySource.Value.Count)
        {
            return -1;
        }

        return _arraySource.Value[_position++];
    }

    public void WriteByte(byte b)
    {
        if (IsStreamed)
        {
            _streamSource.WriteByte(b);
            return;
        }

        if (_position + 1 > _arraySource.Value.Count)
        {
            throw new IndexOutOfRangeException(
                "The specified data length will exceed the bounded capacity of the memory segment.");
        }
            
        _arraySource.Value.Array![_position++ + _arraySource.Value.Offset] = b;
    }
}