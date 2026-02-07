using System;
using System.Diagnostics;

namespace NexusCore.Diagnostics;

public static class SaudiKitActivitySource
{
    public const string SourceName = "NexusCore.SaudiKit";

    public static readonly ActivitySource Source = new(SourceName, "1.0.0");

    public static Activity? StartZatcaInvoiceSubmission(string invoiceId, string invoiceNumber)
    {
        var activity = Source.StartActivity("ZatcaInvoiceSubmission", ActivityKind.Client);
        activity?.SetTag("zatca.invoice.id", invoiceId);
        activity?.SetTag("zatca.invoice.number", invoiceNumber);
        return activity;
    }

    public static Activity? StartZatcaInvoiceValidation(string invoiceId)
    {
        var activity = Source.StartActivity("ZatcaInvoiceValidation", ActivityKind.Internal);
        activity?.SetTag("zatca.invoice.id", invoiceId);
        return activity;
    }

    public static Activity? StartZatcaXmlGeneration(string invoiceId)
    {
        var activity = Source.StartActivity("ZatcaXmlGeneration", ActivityKind.Internal);
        activity?.SetTag("zatca.invoice.id", invoiceId);
        return activity;
    }

    public static Activity? StartNafathAuthentication(string nationalId)
    {
        var activity = Source.StartActivity("NafathAuthentication", ActivityKind.Client);
        activity?.SetTag("nafath.national_id_masked", MaskNationalId(nationalId));
        return activity;
    }

    public static Activity? StartNafathStatusCheck(string transactionId)
    {
        var activity = Source.StartActivity("NafathStatusCheck", ActivityKind.Client);
        activity?.SetTag("nafath.transaction_id", transactionId);
        return activity;
    }

    private static string MaskNationalId(string nationalId)
    {
        if (string.IsNullOrEmpty(nationalId) || nationalId.Length < 4)
            return "****";
        return "******" + nationalId.Substring(nationalId.Length - 4);
    }
}
