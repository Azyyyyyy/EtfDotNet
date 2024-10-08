﻿using System.Security.Cryptography;
using EtfDotNet.Types;
using Xunit;

namespace EtfDotNet.Tests;

public class EtfMemoryTests
{
    [Fact]
    public void ReadPositionTest()
    {
        byte[] rand = RandomNumberGenerator.GetBytes(1 << 13);
        EtfMemory mem = EtfMemory.FromArray(rand);
        var buf = new byte[4096];
        mem.ReadExactly(buf);
        mem.ReadExactly(buf);
        Assert.Throws<IndexOutOfRangeException>(() => mem.ReadExactly(new byte[10]));
        Assert.Equal(-1, mem.ReadByte());
        Assert.True(buf.SequenceEqual(new ArraySegment<byte>(rand, 4096, 4096)));
    }
    
    [Fact]
    public void ReadStreamPositionTest()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(1 << 13);
        using var rand = new MemoryStream(bytes);
        EtfMemory mem = EtfMemory.FromStream(rand);
        var buf = new byte[4096];
        mem.ReadExactly(buf);
        Assert.True(buf.SequenceEqual(new ArraySegment<byte>(bytes, 0, 4096)));
        for (var i = 0; i < 4096; i++)
        {
            buf[i] = (byte)mem.ReadByte();
        }
        Assert.Throws<IndexOutOfRangeException>(() => mem.ReadExactly(new byte[10]));
        Assert.Equal(-1, mem.ReadByte());
        Assert.True(buf.SequenceEqual(new ArraySegment<byte>(bytes, 4096, 4096)));
    }
    
    [Fact]
    public void ReadSliceStreamTest()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(1 << 13);
        using var rand = new MemoryStream(bytes);
        EtfMemory mem = EtfMemory.FromStream(rand);
        Assert.Throws<InvalidOperationException>(() => mem.ReadSlice(4096));
    }
    
    [Fact]
    public void WriteTest()
    {
        byte[] src = RandomNumberGenerator.GetBytes(1 << 13);
        var dest = new byte[1 << 13];
        EtfMemory mem = EtfMemory.FromArray(dest);
        mem.Write(src);
        Assert.Throws<IndexOutOfRangeException>(() => mem.WriteByte(1));
        Assert.Throws<IndexOutOfRangeException>(() => mem.Write(new byte[10]));
        Assert.True(dest.SequenceEqual(src));
    }
    
    [Fact]
    public void WriteStreamTest()
    {
        byte[] src = RandomNumberGenerator.GetBytes(1 << 13);
        using var dest = new MemoryStream(1 << 13);
        EtfMemory mem = EtfMemory.FromStream(dest);
        mem.Write(src);
        mem.WriteByte(1);
        byte[] output = dest.ToArray();
        Assert.False(output.SequenceEqual(src));
        Assert.True(output.Length == src.Length + 1);
        Assert.True(output[^1] == 1);
        Assert.True(output[..^1].SequenceEqual(src));
    }
    
    [Fact]
    public void ReadContainerTest()
    {
        using var data = new MemoryStream(EtfFormat.Pack("hello"));
        using EtfContainer unpacked = EtfFormat.Unpack(EtfMemory.FromStream(data));
        string result = unpacked;
        Assert.Equal("hello", result);
    }
}