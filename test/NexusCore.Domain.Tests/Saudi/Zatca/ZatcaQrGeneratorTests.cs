using System;
using System.IO;
using System.Text;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

public class ZatcaQrGeneratorTests
{
    private readonly ZatcaQrGenerator _qrGenerator = new();

    [Fact]
    public void Should_Produce_Valid_Base64()
    {
        var result = _qrGenerator.GenerateQrCode(
            "شركة الاختبار",
            "300000000000003",
            new DateTime(2024, 6, 15, 12, 0, 0),
            1150.00m,
            150.00m);

        result.ShouldNotBeNullOrWhiteSpace();

        // Should be valid Base64
        var decoded = Convert.FromBase64String(result);
        decoded.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Should_Contain_All_Five_Tags()
    {
        var result = _qrGenerator.GenerateQrCode(
            "شركة الاختبار",
            "300000000000003",
            new DateTime(2024, 6, 15, 12, 0, 0),
            1150.00m,
            150.00m);

        var decoded = Convert.FromBase64String(result);
        using var stream = new MemoryStream(decoded);

        var tagsFound = new bool[6]; // tags 1-5

        while (stream.Position < stream.Length)
        {
            var tag = stream.ReadByte();
            var length = stream.ReadByte();
            var value = new byte[length];
            stream.Read(value, 0, length);

            tag.ShouldBeGreaterThanOrEqualTo(1);
            tag.ShouldBeLessThanOrEqualTo(5);
            tagsFound[tag] = true;
        }

        for (int i = 1; i <= 5; i++)
        {
            tagsFound[i].ShouldBeTrue($"Tag {i} not found in QR code");
        }
    }

    [Fact]
    public void Should_Encode_Arabic_In_Utf8()
    {
        var sellerName = "شركة الاختبار للتجارة";

        var result = _qrGenerator.GenerateQrCode(
            sellerName,
            "300000000000003",
            new DateTime(2024, 6, 15, 12, 0, 0),
            1150.00m,
            150.00m);

        var decoded = Convert.FromBase64String(result);
        using var stream = new MemoryStream(decoded);

        // Read Tag 1 (Seller Name)
        var tag = stream.ReadByte();
        tag.ShouldBe(1);

        var length = stream.ReadByte();
        var value = new byte[length];
        stream.Read(value, 0, length);

        var decodedName = Encoding.UTF8.GetString(value);
        decodedName.ShouldBe(sellerName);
    }

    [Fact]
    public void Should_Format_Amounts_As_Two_Decimal_Places()
    {
        var result = _qrGenerator.GenerateQrCode(
            "Test",
            "300000000000003",
            new DateTime(2024, 6, 15, 12, 0, 0),
            1150.50m,
            150.50m);

        var decoded = Convert.FromBase64String(result);
        using var stream = new MemoryStream(decoded);

        // Skip to Tag 4 (Invoice Total)
        for (int i = 0; i < 3; i++)
        {
            stream.ReadByte(); // tag
            var len = stream.ReadByte();
            stream.Position += len;
        }

        var tag4 = stream.ReadByte();
        tag4.ShouldBe(4);
        var tag4Len = stream.ReadByte();
        var tag4Value = new byte[tag4Len];
        stream.Read(tag4Value, 0, tag4Len);
        Encoding.UTF8.GetString(tag4Value).ShouldBe("1150.50");
    }
}
