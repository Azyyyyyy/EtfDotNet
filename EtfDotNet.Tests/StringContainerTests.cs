using System.Text;
using EtfDotNet.Extensions;
using EtfDotNet.Types;
using Xunit;

namespace EtfDotNet.Tests;

public class StringContainerTests
{
    [Fact]
    public void EtfToStringTest()
    {
        using EtfContainer str = EtfDecoder.DecodeType(EtfMemory.FromArray(new byte[]{(byte) EtfConstants.StringExt, 0, 4, (byte) 't', (byte) 'e', (byte) 's', (byte) 't'}));
        Assert.Equal(EtfConstants.StringExt, str.Type);
        Assert.Equal("test", str);
    }

    [Fact]
    public void StringToEtfTest()
    {
        string testStr = string.Join("", Enumerable.Repeat('a', 10000));
        using var str = (EtfContainer) testStr;
        Assert.Equal(EtfConstants.StringExt, str.Type);
        Assert.Equal(testStr, str);
        Assert.Equal(3 + testStr.Length, EtfEncoder.CalculateTypeSize(str));
        var arr = new byte[3 + testStr.Length];
        EtfEncoder.EncodeType(str, EtfMemory.FromArray(arr));
        var narr = new byte[3 + testStr.Length];
        EtfMemory mem = EtfMemory.FromArray(narr);
        mem.WriteByte((byte)EtfConstants.StringExt);
        mem.WriteUShort((ushort)testStr.Length);
        mem.Write(Encoding.Latin1.GetBytes(testStr));
        Assert.True(arr.SequenceEqual(narr));
    }

    [Fact]
    public void ContainerToStringTest()
    {
        string testStr = string.Join("", Enumerable.Repeat('a', 10000));
        using EtfContainer val = testStr;
        Assert.Equal(EtfConstants.StringExt, val.Type);
        string got = val;
        Assert.Equal(testStr, got);
    }

    [Fact]
    public void StringLengthTest()
    {
        string str = string.Join("", Enumerable.Repeat('a', 100000));
        Assert.Throws<EtfException>(() => (EtfContainer)str);
    }
}