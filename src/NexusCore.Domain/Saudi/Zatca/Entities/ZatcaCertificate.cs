using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi.Zatca;

public class ZatcaCertificate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid SellerId { get; set; }
    public virtual ZatcaSeller Seller { get; set; } = null!;

    public string CertificateContent { get; set; } = null!;
    public string PrivateKeyEncrypted { get; set; } = null!;

    public string? SerialNumber { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public ZatcaEnvironment Environment { get; set; }
    public bool IsActive { get; set; }

    protected ZatcaCertificate() { }

    public ZatcaCertificate(
        Guid id,
        Guid sellerId,
        string certificateContent,
        string privateKeyEncrypted,
        ZatcaEnvironment environment,
        Guid? tenantId = null)
        : base(id)
    {
        SellerId = sellerId;
        CertificateContent = certificateContent;
        PrivateKeyEncrypted = privateKeyEncrypted;
        Environment = environment;
        TenantId = tenantId;
    }
}
