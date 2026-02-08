using System;
using System.Collections.Generic;

namespace NexusCore.Saudi.Zatca;

public class ZatcaInvoiceValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public string GetErrorSummary()
    {
        return IsValid
            ? "Validation passed."
            : string.Join(Environment.NewLine, Errors);
    }
}
