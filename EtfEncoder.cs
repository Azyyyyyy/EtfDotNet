using System.Text;
using EtfDotNet.Types;

namespace EtfDotNet;

internal static class EtfEncoder
{
    public static void EncodeType(Stream output, EtfType type)
    {
        if (type is EtfSmallInteger smallInteger)
        {
            output.WriteConstant(EtfConstants.SmallIntegerExt);
            output.WriteByte(smallInteger.Value);
            return;
        }
        if (type is EtfInteger integer)
        {
            output.WriteConstant(EtfConstants.IntegerExt);
            output.WriteUInt((uint) integer.Value);
            return;
        }
        if (type is EtfString str)
        {
            output.WriteConstant(EtfConstants.StringExt);
            var bytes = Encoding.Latin1.GetBytes(str.Value);
            if (bytes.Length > ushort.MaxValue)
            {
                throw new EtfException($"Cannot encode EtfString longer than {ushort.MaxValue} bytes");
            }
            output.WriteUShort((ushort) bytes.Length);
            output.Write(bytes);
            return;
        }
        if (type is EtfAtom atom)
        {
            output.WriteConstant(EtfConstants.AtomExt);
            EncodeAtom(output, atom);
            return;
        }
        if (type is EtfList list)
        {
            if (list.Count != 0)
            {
                output.WriteConstant(EtfConstants.ListExt);
                EncodeList(output, list);
                return;
            }
            output.WriteConstant(EtfConstants.NilExt);
            return;
        }
        if (type is EtfBinary binary)
        {
            output.WriteConstant(EtfConstants.BinaryExt);
            EncodeBinary(output, binary);
            return;
        }
        if (type is EtfBig big)
        {
            var sign = big.Number.Sign == decimal.Zero;
            var bytes = sign ? (-big.Number).ToByteArray() : big.Number.ToByteArray();
            if (bytes.Length > 255)
            {
                throw new EtfException("Cannot encode number with more than 255 bytes");
            }
            output.WriteConstant(EtfConstants.SmallBigExt);
            output.WriteByte((byte) bytes.Length);
            output.WriteByte((byte) (sign ? 1 : 0));
            output.Write(bytes);
            return;
        }
        if (type is EtfMap map)
        {
            output.WriteConstant(EtfConstants.MapExt);
            EncodeMap(output, map);
            return;
        }
        throw new EtfException($"Unknown type {type}");
    }

    private static void EncodeAtom(Stream output, EtfAtom atom)
    {
        var bytes = Encoding.Latin1.GetBytes(atom.Name);
        if (bytes.Length > 255)
        {
            throw new EtfException("Currently cannot encode atom with >255 bytes");
        }
        output.WriteUShort((ushort) bytes.Length);
        output.Write(bytes);
    }
    
    private static void EncodeList(Stream output, EtfList list)
    {
        output.WriteUInt((uint) list.Count);
        foreach(var value in list)
        {
            EncodeType(output, value);
        }
        output.WriteConstant(EtfConstants.NilExt);
    }
    
    private static void EncodeBinary(Stream output, EtfBinary binary)
    {
        var bytes = binary.Bytes;
        output.WriteUInt((uint) bytes.Count);
        for (var i = 0; i < bytes.Count; i++)
        {
            output.WriteByte(bytes[i]);
        }
    }
    
    private static void EncodeMap(Stream output, EtfMap map)
    {
        var entries = map.Entries();
        output.WriteUInt((uint) entries.Count);
        foreach(var (k, v) in entries)
        {
            EncodeType(output, k);
            EncodeType(output, v);
        }
    }
    
}