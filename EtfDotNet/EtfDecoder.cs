using EtfDotNet.Extensions;

namespace EtfDotNet;

public static class EtfDecoder
{
    public static EtfContainer DecodeType(EtfMemory input)
    {
        EtfConstants typeId = input.ReadConstant();
        switch (typeId)
        {
            case EtfConstants.NewFloatExt:
                return input.ReadContainer(8, typeId);
            case EtfConstants.SmallIntegerExt:
                return input.ReadContainer(1, typeId);
            case EtfConstants.IntegerExt:
                return input.ReadContainer(4, typeId);
            case EtfConstants.SmallTupleExt:
                return DecodeTuple(input, typeId, (uint) input.ReadByte());
            case EtfConstants.LargeTupleExt:
                return DecodeTuple(input, typeId, input.ReadUInt());
            case EtfConstants.NilExt:
                return EtfContainer.Nil;
            case EtfConstants.ListExt:
                return DecodeList(input);
            case EtfConstants.MapExt:
                return DecodeMap(input);
            case EtfConstants.BinaryExt:
            {
                uint len = input.ReadUInt();
                return input.ReadContainer((int)len, typeId);
            }
            case EtfConstants.SmallBigExt:
            {
                int len = input.ReadByte();
                return input.ReadContainer(len + 1, typeId);
            }
            case EtfConstants.StringExt:
            {
                ushort len = input.ReadUShort();
                return input.ReadContainer(len, typeId);
            }
            case EtfConstants.AtomExt:
            {
                ushort len = input.ReadUShort();
                return input.ReadContainer(len, typeId);
            }
            case EtfConstants.SmallAtomExt:
            {
                int len = input.ReadByte();
                return input.ReadContainer(len, EtfConstants.AtomExt);
            }
            case EtfConstants.VersionNumber:
            default:
                throw new EtfException($"Unknown type '{typeId}'");
        }
    }

    public static EtfContainer DecodeTuple(EtfMemory input, EtfConstants typeId, uint length)
    {
        var tuple = new EtfTuple(length);
        for (var i = 0u; i < length; i++)
        {
            tuple[i] = DecodeType(input);
        }
        return EtfContainer.AsContainer(tuple, typeId);
    }

    public static EtfContainer DecodeList(EtfMemory input)
    {
        uint length = input.ReadUInt();
        var list = new EtfList();
        for (var i = 0u; i < length; i++)
        {
            list.Add(DecodeType(input));
        }
        if (input.ReadConstant() != EtfConstants.NilExt)
        {
            throw new EtfException("Expected NilExt");
        }
        return EtfContainer.AsContainer(list, EtfConstants.ListExt);
    }

    public static EtfContainer DecodeMap(EtfMemory input)
    {
        uint kvLength = input.ReadUInt();
        var map = new EtfMap();
        for (var i = 0u; i < kvLength; i++)
        {
            EtfContainer key = DecodeType(input);
            EtfContainer value = DecodeType(input);
            map.Add((key, value));
        }
        return EtfContainer.AsContainer(map, EtfConstants.MapExt);
    }
}