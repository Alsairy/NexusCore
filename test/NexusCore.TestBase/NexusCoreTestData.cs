using System;

namespace NexusCore;

public static class NexusCoreTestData
{
    // ZatcaSeller
    public static readonly Guid SellerId = Guid.Parse("a1b2c3d4-1111-1111-1111-000000000001");
    public const string SellerNameAr = "شركة الاختبار للتجارة";
    public const string SellerNameEn = "Test Trading Company";
    public const string SellerVatNumber = "300000000000003";

    // ZatcaCertificate
    public static readonly Guid CertificateId = Guid.Parse("a1b2c3d4-2222-2222-2222-000000000001");

    // ZatcaInvoices
    public static readonly Guid InvoiceDraftId = Guid.Parse("a1b2c3d4-3333-3333-3333-000000000001");
    public static readonly Guid InvoiceValidatedId = Guid.Parse("a1b2c3d4-3333-3333-3333-000000000002");
    public static readonly Guid InvoiceClearedId = Guid.Parse("a1b2c3d4-3333-3333-3333-000000000003");

    public const string InvoiceDraftNumber = "INV-TEST-001";
    public const string InvoiceValidatedNumber = "INV-TEST-002";
    public const string InvoiceClearedNumber = "INV-TEST-003";

    // NafathAuthRequests
    public static readonly Guid NafathCompletedRequestId = Guid.Parse("a1b2c3d4-4444-4444-4444-000000000001");
    public static readonly Guid NafathExpiredRequestId = Guid.Parse("a1b2c3d4-4444-4444-4444-000000000002");
    public const string NafathCompletedTransactionId = "NAFATH-TXN-COMPLETED-001";
    public const string NafathExpiredTransactionId = "NAFATH-TXN-EXPIRED-001";
    public const string NafathNationalId = "1234567890";

    // NafathUserLink
    public static readonly Guid NafathUserLinkId = Guid.Parse("a1b2c3d4-5555-5555-5555-000000000001");
    public static readonly Guid NafathLinkedUserId = Guid.Parse("a1b2c3d4-5555-5555-5555-000000000099");

    // ApprovalTask
    public static readonly Guid ApprovalTaskPendingId = Guid.Parse("a1b2c3d4-6666-6666-6666-000000000001");
    public static readonly Guid ApprovalTaskApprovedId = Guid.Parse("a1b2c3d4-6666-6666-6666-000000000002");
    public static readonly Guid ApprovalTaskAssigneeUserId = Guid.Parse("a1b2c3d4-6666-6666-6666-000000000099");

    // ApprovalDelegation
    public static readonly Guid DelegationId = Guid.Parse("a1b2c3d4-7777-7777-7777-000000000001");
    public static readonly Guid DelegatorUserId = Guid.Parse("a1b2c3d4-7777-7777-7777-000000000098");
    public static readonly Guid DelegateUserId = Guid.Parse("a1b2c3d4-7777-7777-7777-000000000099");
}
