using System;
using System.Threading.Tasks;
using NexusCore.Saudi.Zatca;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace NexusCore.Saudi;

/// <summary>
/// Seeds demo Saudi Kit data for new tenants.
/// Runs automatically when a tenant is created or data seed is triggered.
/// </summary>
public class SaudiKitDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<ZatcaSeller, Guid> _sellerRepository;
    private readonly IRepository<ZatcaInvoice, Guid> _invoiceRepository;
    private readonly IRepository<ZatcaInvoiceLine, Guid> _invoiceLineRepository;

    public SaudiKitDataSeederContributor(
        ICurrentTenant currentTenant,
        IRepository<ZatcaSeller, Guid> sellerRepository,
        IRepository<ZatcaInvoice, Guid> invoiceRepository,
        IRepository<ZatcaInvoiceLine, Guid> invoiceLineRepository)
    {
        _currentTenant = currentTenant;
        _sellerRepository = sellerRepository;
        _invoiceRepository = invoiceRepository;
        _invoiceLineRepository = invoiceLineRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            if (await _sellerRepository.GetCountAsync() > 0)
            {
                return; // Already seeded
            }

            await SeedDemoSellerAsync();
        }
    }

    private async Task SeedDemoSellerAsync()
    {
        var sellerId = Guid.NewGuid();
        var seller = new ZatcaSeller(
            sellerId,
            "شركة نموذجية للتجارة",
            "300000000000003")
        {
            SellerNameEn = "Demo Trading Company",
            City = "الرياض",
            District = "العليا",
            Street = "شارع العليا العام",
            BuildingNumber = "1000",
            PostalCode = "12211",
            CountryCode = "SA",
            IsDefault = true
        };
        await _sellerRepository.InsertAsync(seller);

        // Sample draft invoice
        var invoiceId = Guid.NewGuid();
        var invoice = new ZatcaInvoice(
            invoiceId,
            sellerId,
            "INV-DEMO-001",
            ZatcaInvoiceType.Simplified,
            DateTime.UtcNow)
        {
            BuyerName = "عميل تجريبي",
            CurrencyCode = "SAR"
        };
        await _invoiceRepository.InsertAsync(invoice);

        var line1 = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoiceId, "خدمة استشارية", 1, 500m);
        var line2 = new ZatcaInvoiceLine(
            Guid.NewGuid(), invoiceId, "تطوير برمجيات", 2, 1000m);
        await _invoiceLineRepository.InsertAsync(line1);
        await _invoiceLineRepository.InsertAsync(line2);

        invoice.SubTotal = line1.NetAmount + line2.NetAmount;
        invoice.VatAmount = line1.VatAmount + line2.VatAmount;
        invoice.GrandTotal = line1.TotalAmount + line2.TotalAmount;
        await _invoiceRepository.UpdateAsync(invoice);
    }
}
