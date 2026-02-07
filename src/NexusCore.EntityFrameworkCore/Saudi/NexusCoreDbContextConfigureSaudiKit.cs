using Microsoft.EntityFrameworkCore;
using NexusCore.Saudi;
using NexusCore.Saudi.Nafath;
using NexusCore.Saudi.Workflows;
using NexusCore.Saudi.Zatca;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace NexusCore.EntityFrameworkCore;

public static class NexusCoreDbContextConfigureSaudiKit
{
    public static void ConfigureSaudiKit(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // --- ZATCA Seller ---
        builder.Entity<ZatcaSeller>(b =>
        {
            b.ToTable(SaudiConsts.DbTablePrefix + "ZatcaSellers", SaudiConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.SellerNameAr).IsRequired().HasMaxLength(ZatcaConsts.MaxSellerNameLength);
            b.Property(x => x.SellerNameEn).HasMaxLength(ZatcaConsts.MaxSellerNameLength);
            b.Property(x => x.VatRegistrationNumber).IsRequired().HasMaxLength(ZatcaConsts.MaxVatRegistrationNumberLength);
            b.Property(x => x.CommercialRegistrationNumber).HasMaxLength(ZatcaConsts.MaxCommercialRegistrationNumberLength);
            b.Property(x => x.Street).HasMaxLength(ZatcaConsts.MaxAddressFieldLength);
            b.Property(x => x.BuildingNumber).HasMaxLength(ZatcaConsts.MaxAddressFieldLength);
            b.Property(x => x.City).HasMaxLength(ZatcaConsts.MaxAddressFieldLength);
            b.Property(x => x.District).HasMaxLength(ZatcaConsts.MaxAddressFieldLength);
            b.Property(x => x.PostalCode).HasMaxLength(ZatcaConsts.MaxPostalCodeLength);
            b.Property(x => x.CountryCode).HasMaxLength(ZatcaConsts.MaxCountryCodeLength);

            b.HasIndex(x => new { x.TenantId, x.VatRegistrationNumber });
        });

        // --- ZATCA Certificate ---
        builder.Entity<ZatcaCertificate>(b =>
        {
            b.ToTable(SaudiConsts.DbTablePrefix + "ZatcaCertificates", SaudiConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.SerialNumber).HasMaxLength(ZatcaConsts.MaxSerialNumberLength);
            b.Property(x => x.CertificateContent).IsRequired();
            b.Property(x => x.PrivateKeyEncrypted).IsRequired();

            b.HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.SellerId, x.IsActive });
        });

        // --- ZATCA Invoice ---
        builder.Entity<ZatcaInvoice>(b =>
        {
            b.ToTable(SaudiConsts.DbTablePrefix + "ZatcaInvoices", SaudiConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(ZatcaConsts.MaxInvoiceNumberLength);
            b.Property(x => x.BuyerName).HasMaxLength(ZatcaConsts.MaxBuyerNameLength);
            b.Property(x => x.BuyerVatNumber).HasMaxLength(ZatcaConsts.MaxVatRegistrationNumberLength);
            b.Property(x => x.IssueDateHijri).HasMaxLength(20);
            b.Property(x => x.CurrencyCode).HasMaxLength(ZatcaConsts.MaxCurrencyCodeLength);
            b.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            b.Property(x => x.VatAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");

            b.HasOne(x => x.Seller).WithMany().HasForeignKey(x => x.SellerId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.InvoiceNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status });
        });

        // --- ZATCA Invoice Line ---
        builder.Entity<ZatcaInvoiceLine>(b =>
        {
            b.ToTable(SaudiConsts.DbTablePrefix + "ZatcaInvoiceLines", SaudiConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ItemName).IsRequired().HasMaxLength(ZatcaConsts.MaxItemNameLength);
            b.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
            b.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
            b.Property(x => x.TaxCategoryCode).HasMaxLength(ZatcaConsts.MaxTaxCategoryCodeLength);
            b.Property(x => x.TaxPercent).HasColumnType("decimal(5,2)");
            b.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.VatAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        });

        // --- Nafath Auth Request ---
        builder.Entity<NafathAuthRequest>(b =>
        {
            b.ToTable(SaudiConsts.DbTablePrefix + "NafathAuthRequests", SaudiConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.TransactionId).IsRequired().HasMaxLength(NafathConsts.MaxTransactionIdLength);
            b.Property(x => x.NationalId).IsRequired().HasMaxLength(NafathConsts.NationalIdLength);

            b.HasIndex(x => new { x.TenantId, x.TransactionId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status });
        });

        // --- Nafath User Link ---
        builder.Entity<NafathUserLink>(b =>
        {
            b.ToTable(SaudiConsts.DbTablePrefix + "NafathUserLinks", SaudiConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.NationalId).IsRequired().HasMaxLength(NafathConsts.NationalIdLength);

            b.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.NationalId }).IsUnique();
        });

        // --- Approval Delegation ---
        builder.Entity<ApprovalDelegation>(b =>
        {
            b.ToTable(SaudiConsts.DbTablePrefix + "ApprovalDelegations", SaudiConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Reason).HasMaxLength(SaudiConsts.MaxDescriptionLength);

            b.HasIndex(x => new { x.TenantId, x.DelegatorUserId, x.IsActive });
            b.HasIndex(x => new { x.TenantId, x.DelegateUserId, x.IsActive });
        });

        // --- Approval Task ---
        builder.Entity<ApprovalTask>(b =>
        {
            b.ToTable(SaudiConsts.DbTablePrefix + "ApprovalTasks", SaudiConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.WorkflowInstanceId).IsRequired().HasMaxLength(SaudiConsts.MaxCodeLength);
            b.Property(x => x.TaskName).IsRequired().HasMaxLength(SaudiConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(SaudiConsts.MaxDescriptionLength);
            b.Property(x => x.AssignedToRoleName).HasMaxLength(SaudiConsts.MaxNameLength);
            b.Property(x => x.Comment).HasMaxLength(SaudiConsts.MaxDescriptionLength);
            b.Property(x => x.EntityType).HasMaxLength(SaudiConsts.MaxNameLength);
            b.Property(x => x.EntityId).HasMaxLength(SaudiConsts.MaxCodeLength);

            b.HasIndex(x => new { x.TenantId, x.AssignedToUserId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.WorkflowInstanceId });
            b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
            b.HasIndex(x => new { x.TenantId, x.Status, x.DueDate });
        });
    }
}
