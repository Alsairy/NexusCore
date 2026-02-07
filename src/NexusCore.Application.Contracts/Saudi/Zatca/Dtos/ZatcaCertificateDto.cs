using System;
using System.ComponentModel.DataAnnotations;

namespace NexusCore.Saudi.Zatca;

/// <summary>
/// Full DTO for ZATCA Certificate entity
/// </summary>
public class ZatcaCertificateDto
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public ZatcaEnvironment Environment { get; set; }

    public string Csid { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public string? CertificatePem { get; set; }

    public string? PrivateKeyPem { get; set; }

    public DateTime IssuedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }
}

/// <summary>
/// DTO for creating a ZATCA Certificate
/// </summary>
public class CreateZatcaCertificateDto
{
    [Required]
    public Guid SellerId { get; set; }

    [Required]
    public ZatcaEnvironment Environment { get; set; }

    [Required]
    [StringLength(1000)]
    public string Csid { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Secret { get; set; } = string.Empty;

    public string? CertificatePem { get; set; }

    public string? PrivateKeyPem { get; set; }

    public DateTime IssuedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
}
