using System.Diagnostics.Metrics;

namespace NexusCore.Diagnostics;

public sealed class SaudiKitMetrics
{
    public const string MeterName = "NexusCore.SaudiKit";

    private readonly Counter<long> _invoicesSubmitted;
    private readonly Counter<long> _invoicesCleared;
    private readonly Counter<long> _invoicesRejected;
    private readonly Counter<long> _nafathAuthInitiated;
    private readonly Counter<long> _nafathAuthCompleted;
    private readonly Histogram<double> _xmlGenerationDuration;
    private readonly Histogram<double> _zatcaApiCallDuration;
    private readonly Histogram<double> _nafathApiCallDuration;

    public SaudiKitMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _invoicesSubmitted = meter.CreateCounter<long>(
            "saudikit.invoices.submitted",
            unit: "{invoice}",
            description: "Number of invoices submitted to ZATCA");

        _invoicesCleared = meter.CreateCounter<long>(
            "saudikit.invoices.cleared",
            unit: "{invoice}",
            description: "Number of invoices cleared by ZATCA");

        _invoicesRejected = meter.CreateCounter<long>(
            "saudikit.invoices.rejected",
            unit: "{invoice}",
            description: "Number of invoices rejected by ZATCA");

        _nafathAuthInitiated = meter.CreateCounter<long>(
            "saudikit.nafath.auth.initiated",
            unit: "{request}",
            description: "Number of Nafath authentication requests initiated");

        _nafathAuthCompleted = meter.CreateCounter<long>(
            "saudikit.nafath.auth.completed",
            unit: "{request}",
            description: "Number of Nafath authentication requests completed");

        _xmlGenerationDuration = meter.CreateHistogram<double>(
            "saudikit.xml.generation.duration",
            unit: "ms",
            description: "Duration of UBL XML generation");

        _zatcaApiCallDuration = meter.CreateHistogram<double>(
            "saudikit.zatca.api.duration",
            unit: "ms",
            description: "Duration of ZATCA API calls");

        _nafathApiCallDuration = meter.CreateHistogram<double>(
            "saudikit.nafath.api.duration",
            unit: "ms",
            description: "Duration of Nafath API calls");
    }

    public void InvoiceSubmitted() => _invoicesSubmitted.Add(1);
    public void InvoiceCleared() => _invoicesCleared.Add(1);
    public void InvoiceRejected() => _invoicesRejected.Add(1);
    public void NafathAuthInitiated() => _nafathAuthInitiated.Add(1);
    public void NafathAuthCompleted() => _nafathAuthCompleted.Add(1);
    public void RecordXmlGenerationDuration(double milliseconds) => _xmlGenerationDuration.Record(milliseconds);
    public void RecordZatcaApiCallDuration(double milliseconds) => _zatcaApiCallDuration.Record(milliseconds);
    public void RecordNafathApiCallDuration(double milliseconds) => _nafathApiCallDuration.Record(milliseconds);
}
