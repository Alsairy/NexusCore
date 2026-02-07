using System;
using System.Threading.Tasks;
using NexusCore.Saudi.Nafath;
using NexusCore.Saudi.Workflows;
using NexusCore.Saudi.Zatca;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace NexusCore;

public class NexusCoreTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<ZatcaSeller, Guid> _sellerRepository;
    private readonly IRepository<ZatcaCertificate, Guid> _certificateRepository;
    private readonly IRepository<ZatcaInvoice, Guid> _invoiceRepository;
    private readonly IRepository<ZatcaInvoiceLine, Guid> _invoiceLineRepository;
    private readonly IRepository<NafathAuthRequest, Guid> _nafathAuthRequestRepository;
    private readonly IRepository<NafathUserLink, Guid> _nafathUserLinkRepository;
    private readonly IRepository<ApprovalTask, Guid> _approvalTaskRepository;
    private readonly IRepository<ApprovalDelegation, Guid> _approvalDelegationRepository;

    public NexusCoreTestDataSeedContributor(
        ICurrentTenant currentTenant,
        IRepository<ZatcaSeller, Guid> sellerRepository,
        IRepository<ZatcaCertificate, Guid> certificateRepository,
        IRepository<ZatcaInvoice, Guid> invoiceRepository,
        IRepository<ZatcaInvoiceLine, Guid> invoiceLineRepository,
        IRepository<NafathAuthRequest, Guid> nafathAuthRequestRepository,
        IRepository<NafathUserLink, Guid> nafathUserLinkRepository,
        IRepository<ApprovalTask, Guid> approvalTaskRepository,
        IRepository<ApprovalDelegation, Guid> approvalDelegationRepository)
    {
        _currentTenant = currentTenant;
        _sellerRepository = sellerRepository;
        _certificateRepository = certificateRepository;
        _invoiceRepository = invoiceRepository;
        _invoiceLineRepository = invoiceLineRepository;
        _nafathAuthRequestRepository = nafathAuthRequestRepository;
        _nafathUserLinkRepository = nafathUserLinkRepository;
        _approvalTaskRepository = approvalTaskRepository;
        _approvalDelegationRepository = approvalDelegationRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await SeedZatcaDataAsync();
            await SeedNafathDataAsync();
            await SeedWorkflowDataAsync();
        }
    }

    private async Task SeedZatcaDataAsync()
    {
        // Seller
        var seller = new ZatcaSeller(
            NexusCoreTestData.SellerId,
            NexusCoreTestData.SellerNameAr,
            NexusCoreTestData.SellerVatNumber)
        {
            SellerNameEn = NexusCoreTestData.SellerNameEn,
            City = "الرياض",
            District = "العليا",
            Street = "شارع الأمير محمد بن عبدالعزيز",
            BuildingNumber = "1234",
            PostalCode = "12345",
            CountryCode = "SA",
            IsDefault = true
        };
        await _sellerRepository.InsertAsync(seller);

        // Certificate
        var certificate = new ZatcaCertificate(
            NexusCoreTestData.CertificateId,
            NexusCoreTestData.SellerId,
            "test-csid-compliance-001",
            "test-secret-001",
            ZatcaEnvironment.Sandbox)
        {
            IssuedAt = new DateTime(2024, 1, 1),
            ExpiresAt = new DateTime(2025, 12, 31),
            IsActive = true
        };
        await _certificateRepository.InsertAsync(certificate);

        // Invoice 1 - Draft
        var invoiceDraft = new ZatcaInvoice(
            NexusCoreTestData.InvoiceDraftId,
            NexusCoreTestData.SellerId,
            NexusCoreTestData.InvoiceDraftNumber,
            ZatcaInvoiceType.Simplified,
            new DateTime(2024, 6, 15))
        {
            BuyerName = "محمد أحمد",
            CurrencyCode = "SAR"
        };
        await _invoiceRepository.InsertAsync(invoiceDraft);

        var line1 = new ZatcaInvoiceLine(Guid.NewGuid(), invoiceDraft.Id, "خدمة استشارية", 1, 1000m);
        var line2 = new ZatcaInvoiceLine(Guid.NewGuid(), invoiceDraft.Id, "تدريب فني", 2, 500m);
        await _invoiceLineRepository.InsertAsync(line1);
        await _invoiceLineRepository.InsertAsync(line2);

        invoiceDraft.SubTotal = line1.NetAmount + line2.NetAmount;
        invoiceDraft.VatAmount = line1.VatAmount + line2.VatAmount;
        invoiceDraft.GrandTotal = line1.TotalAmount + line2.TotalAmount;
        await _invoiceRepository.UpdateAsync(invoiceDraft);

        // Invoice 2 - Validated
        var invoiceValidated = new ZatcaInvoice(
            NexusCoreTestData.InvoiceValidatedId,
            NexusCoreTestData.SellerId,
            NexusCoreTestData.InvoiceValidatedNumber,
            ZatcaInvoiceType.Standard,
            new DateTime(2024, 7, 1))
        {
            BuyerName = "شركة الاتصالات السعودية",
            BuyerVatNumber = "300000000000010",
            CurrencyCode = "SAR",
            Status = ZatcaInvoiceStatus.Validated
        };
        await _invoiceRepository.InsertAsync(invoiceValidated);

        var line3 = new ZatcaInvoiceLine(Guid.NewGuid(), invoiceValidated.Id, "ترخيص برمجيات", 1, 5000m);
        var line4 = new ZatcaInvoiceLine(Guid.NewGuid(), invoiceValidated.Id, "دعم فني سنوي", 1, 2000m);
        await _invoiceLineRepository.InsertAsync(line3);
        await _invoiceLineRepository.InsertAsync(line4);

        invoiceValidated.SubTotal = line3.NetAmount + line4.NetAmount;
        invoiceValidated.VatAmount = line3.VatAmount + line4.VatAmount;
        invoiceValidated.GrandTotal = line3.TotalAmount + line4.TotalAmount;
        await _invoiceRepository.UpdateAsync(invoiceValidated);

        // Invoice 3 - Cleared
        var invoiceCleared = new ZatcaInvoice(
            NexusCoreTestData.InvoiceClearedId,
            NexusCoreTestData.SellerId,
            NexusCoreTestData.InvoiceClearedNumber,
            ZatcaInvoiceType.Standard,
            new DateTime(2024, 5, 1))
        {
            BuyerName = "شركة أرامكو السعودية",
            BuyerVatNumber = "300000000000020",
            CurrencyCode = "SAR",
            Status = ZatcaInvoiceStatus.Cleared,
            QrCode = "dGVzdC1xci1jb2Rl",
            InvoiceHash = "test-hash-cleared"
        };
        await _invoiceRepository.InsertAsync(invoiceCleared);

        var line5 = new ZatcaInvoiceLine(Guid.NewGuid(), invoiceCleared.Id, "خدمات تقنية", 3, 3000m);
        var line6 = new ZatcaInvoiceLine(Guid.NewGuid(), invoiceCleared.Id, "استشارات أمنية", 1, 8000m);
        await _invoiceLineRepository.InsertAsync(line5);
        await _invoiceLineRepository.InsertAsync(line6);

        invoiceCleared.SubTotal = line5.NetAmount + line6.NetAmount;
        invoiceCleared.VatAmount = line5.VatAmount + line6.VatAmount;
        invoiceCleared.GrandTotal = line5.TotalAmount + line6.TotalAmount;
        await _invoiceRepository.UpdateAsync(invoiceCleared);
    }

    private async Task SeedNafathDataAsync()
    {
        // Completed auth request
        var completedRequest = new NafathAuthRequest(
            NexusCoreTestData.NafathCompletedRequestId,
            NexusCoreTestData.NafathCompletedTransactionId,
            NexusCoreTestData.NafathNationalId,
            42,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddMinutes(-58));
        completedRequest.MarkCompleted(NexusCoreTestData.NafathLinkedUserId);
        await _nafathAuthRequestRepository.InsertAsync(completedRequest);

        // Expired auth request
        var expiredRequest = new NafathAuthRequest(
            NexusCoreTestData.NafathExpiredRequestId,
            NexusCoreTestData.NafathExpiredTransactionId,
            NexusCoreTestData.NafathNationalId,
            73,
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow.AddHours(-2).AddMinutes(2));
        expiredRequest.MarkExpired();
        await _nafathAuthRequestRepository.InsertAsync(expiredRequest);

        // User link
        var userLink = new NafathUserLink(
            NexusCoreTestData.NafathUserLinkId,
            NexusCoreTestData.NafathLinkedUserId,
            NexusCoreTestData.NafathNationalId,
            DateTime.UtcNow.AddHours(-1));
        await _nafathUserLinkRepository.InsertAsync(userLink);
    }

    private async Task SeedWorkflowDataAsync()
    {
        // Pending approval task
        var pendingTask = new ApprovalTask(
            NexusCoreTestData.ApprovalTaskPendingId,
            "WF-INSTANCE-001",
            "Manager Approval",
            NexusCoreTestData.ApprovalTaskAssigneeUserId,
            description: "Approve invoice INV-TEST-001",
            entityType: "ZatcaInvoice",
            entityId: NexusCoreTestData.InvoiceDraftId.ToString());
        await _approvalTaskRepository.InsertAsync(pendingTask);

        // Approved task
        var approvedTask = new ApprovalTask(
            NexusCoreTestData.ApprovalTaskApprovedId,
            "WF-INSTANCE-002",
            "Finance Review",
            NexusCoreTestData.ApprovalTaskAssigneeUserId,
            description: "Review invoice INV-TEST-003",
            entityType: "ZatcaInvoice",
            entityId: NexusCoreTestData.InvoiceClearedId.ToString());
        approvedTask.Approve(NexusCoreTestData.ApprovalTaskAssigneeUserId, "Looks good");
        await _approvalTaskRepository.InsertAsync(approvedTask);

        // Delegation
        var delegation = new ApprovalDelegation(
            NexusCoreTestData.DelegationId,
            NexusCoreTestData.DelegatorUserId,
            NexusCoreTestData.DelegateUserId,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(30),
            "Annual leave");
        await _approvalDelegationRepository.InsertAsync(delegation);
    }
}
