using System.Collections.Generic;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// DTO representing the result of submitting an invoice to ZATCA
/// </summary>
public class ZatcaSubmitResultDto
{
    /// <summary>
    /// ZATCA request identifier
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Status after submission (Accepted, Rejected, Warning)
    /// </summary>
    public ZatcaInvoiceStatus Status { get; set; }

    /// <summary>
    /// Warnings returned by ZATCA (if any)
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Errors returned by ZATCA (if any)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// QR code returned by ZATCA (Base64 encoded)
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// Indicates if submission was successful
    /// </summary>
    public bool IsSuccess => Status == ZatcaInvoiceStatus.Cleared || Status == ZatcaInvoiceStatus.Reported;
}
