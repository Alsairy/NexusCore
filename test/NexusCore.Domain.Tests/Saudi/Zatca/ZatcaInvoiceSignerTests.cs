using NexusCore.Saudi.Zatca;
using Shouldly;
using Xunit;

namespace NexusCore.Saudi.Tests.Zatca;

public class ZatcaInvoiceSignerTests
{
    private readonly ZatcaInvoiceSigner _signer = new();

    [Fact]
    public void ComputeInvoiceHash_Should_Return_Consistent_Hash()
    {
        var xml = "<Invoice><ID>INV-001</ID></Invoice>";

        var hash1 = _signer.ComputeInvoiceHash(xml);
        var hash2 = _signer.ComputeInvoiceHash(xml);

        hash1.ShouldNotBeNullOrWhiteSpace();
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void ComputeInvoiceHash_Different_Input_Should_Produce_Different_Hash()
    {
        var xml1 = "<Invoice><ID>INV-001</ID></Invoice>";
        var xml2 = "<Invoice><ID>INV-002</ID></Invoice>";

        var hash1 = _signer.ComputeInvoiceHash(xml1);
        var hash2 = _signer.ComputeInvoiceHash(xml2);

        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void ComputeInvoiceHash_Should_Return_Valid_Base64()
    {
        var xml = "<Invoice><ID>INV-001</ID></Invoice>";

        var hash = _signer.ComputeInvoiceHash(xml);

        // Should be valid Base64
        var bytes = System.Convert.FromBase64String(hash);
        bytes.Length.ShouldBe(32); // SHA-256 produces 32 bytes
    }
}
