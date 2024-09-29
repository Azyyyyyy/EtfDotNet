using EtfDotNet.Types;
using Xunit;

namespace EtfDotNet.Tests;

public class UnknownTypeTest
{
    private readonly EtfContainer _unknownType = EtfContainer.AsContainer(ArraySegment<byte>.Empty, (EtfConstants) 255);
    
    [Fact]
    public void UnpackInvalidTypeTest()
    {
        Assert.Throws<EtfException>(() => EtfDecoder.DecodeType(EtfMemory.FromArray(new byte[] { 255 })));
    }
    
    [Fact]
    public void PackUnknownTypeTest()
    {
        Assert.Throws<EtfException>(() => EtfFormat.Pack(_unknownType));
    }
    
    [Fact]
    public void EncodeUnknownTypeTest()
    {
        Assert.Throws<EtfException>(() => EtfEncoder.EncodeType(_unknownType, EtfMemory.FromArray(new byte[8])));
    }
    
    [Fact]
    public void GetLengthOfUnknownTypeTest()
    {
        Assert.Throws<EtfException>(() => EtfFormat.GetPackedSize(_unknownType));
    }
    
}