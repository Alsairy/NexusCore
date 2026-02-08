using System;
using System.IO;
using System.Text;
using NexusCore.Saudi.Zatca;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

/// <summary>
/// Tests that verify TLV (Tag-Length-Value) encoding in QR codes
/// matches ZATCA Phase 2 specification. All 5 required tags must
/// be present and correctly encoded.
/// </summary>
public class ZatcaQrComplianceTests
{
    private readonly ZatcaQrGenerator _qrGenerator = new();

    [Fact]
    public void QrCode_Should_Produce_Valid_Base64()
    {
        var result = _qrGenerator.GenerateQrCode(
            "شركة اختبار",
            "300000000000003",
            new DateTime(2024, 6, 15, 10, 30, 0),
            1150.00m,
            150.00m);

        result.ShouldNotBeNullOrWhiteSpace();

        // Should decode without exception
        var bytes = Convert.FromBase64String(result);
        bytes.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void QrCode_Should_Contain_All_Five_Required_Tags()
    {
        var sellerName = "شركة الاختبار";
        var vatNumber = "300000000000003";
        var timestamp = new DateTime(2024, 6, 15, 10, 30, 0);
        var total = 1150.00m;
        var vatTotal = 150.00m;

        var result = _qrGenerator.GenerateQrCode(
            sellerName, vatNumber, timestamp, total, vatTotal);

        var bytes = Convert.FromBase64String(result);
        using var stream = new MemoryStream(bytes);

        var tags = ParseTlvTags(stream);

        // Verify all 5 tags present
        tags.Length.ShouldBe(5);

        // Tag 1: Seller Name
        tags[0].Tag.ShouldBe(1);
        tags[0].Value.ShouldBe(sellerName);

        // Tag 2: VAT Registration Number
        tags[1].Tag.ShouldBe(2);
        tags[1].Value.ShouldBe(vatNumber);

        // Tag 3: Timestamp (ISO 8601, uses current culture's calendar)
        tags[2].Tag.ShouldBe(3);
        var expectedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ");
        tags[2].Value.ShouldBe(expectedTimestamp);

        // Tag 4: Invoice Total
        tags[3].Tag.ShouldBe(4);
        tags[3].Value.ShouldBe("1150.00");

        // Tag 5: VAT Total
        tags[4].Tag.ShouldBe(5);
        tags[4].Value.ShouldBe("150.00");
    }

    [Fact]
    public void QrCode_Should_Handle_Arabic_UTF8_In_Seller_Name()
    {
        var arabicName = "شركة الاتصالات السعودية";
        var result = _qrGenerator.GenerateQrCode(
            arabicName,
            "300000000000003",
            DateTime.UtcNow,
            100m,
            15m);

        var bytes = Convert.FromBase64String(result);
        using var stream = new MemoryStream(bytes);

        var tags = ParseTlvTags(stream);
        tags[0].Tag.ShouldBe(1);
        tags[0].Value.ShouldBe(arabicName);
    }

    [Fact]
    public void QrCode_Should_Produce_Different_Output_For_Different_Inputs()
    {
        var qr1 = _qrGenerator.GenerateQrCode(
            "شركة أ", "300000000000003",
            DateTime.UtcNow, 1000m, 150m);

        var qr2 = _qrGenerator.GenerateQrCode(
            "شركة ب", "300000000000010",
            DateTime.UtcNow, 2000m, 300m);

        qr1.ShouldNotBe(qr2);
    }

    [Fact]
    public void QrCode_Same_Input_Should_Produce_Same_Output()
    {
        var timestamp = new DateTime(2024, 1, 1, 12, 0, 0);

        var qr1 = _qrGenerator.GenerateQrCode(
            "شركة", "300000000000003",
            timestamp, 100m, 15m);

        var qr2 = _qrGenerator.GenerateQrCode(
            "شركة", "300000000000003",
            timestamp, 100m, 15m);

        qr1.ShouldBe(qr2);
    }

    [Fact]
    public void QrCode_Tags_Should_Be_In_Sequential_Order()
    {
        var result = _qrGenerator.GenerateQrCode(
            "Test", "300000000000003",
            DateTime.UtcNow, 100m, 15m);

        var bytes = Convert.FromBase64String(result);
        using var stream = new MemoryStream(bytes);

        var tags = ParseTlvTags(stream);

        for (int i = 0; i < tags.Length; i++)
        {
            tags[i].Tag.ShouldBe(i + 1);
        }
    }

    #region TLV Parser

    private record TlvEntry(int Tag, string Value);

    private static TlvEntry[] ParseTlvTags(MemoryStream stream)
    {
        var entries = new System.Collections.Generic.List<TlvEntry>();
        stream.Position = 0;

        while (stream.Position < stream.Length)
        {
            var tag = stream.ReadByte();
            if (tag == -1) break;

            var length = stream.ReadByte();
            if (length == -1) break;

            var valueBytes = new byte[length];
            var bytesRead = stream.Read(valueBytes, 0, length);
            if (bytesRead != length) break;

            var value = Encoding.UTF8.GetString(valueBytes);
            entries.Add(new TlvEntry(tag, value));
        }

        return entries.ToArray();
    }

    #endregion
}
