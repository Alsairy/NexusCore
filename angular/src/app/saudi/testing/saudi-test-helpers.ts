import { ZatcaInvoiceListDto, ZatcaInvoiceDto, ZatcaInvoiceLineDto, ZatcaInvoiceStatus, ZatcaInvoiceType } from '../zatca/services/invoice.service';
import { ZatcaSellerDto } from '../zatca/services/seller.service';
import { ZatcaCertificateDto, ZatcaEnvironment } from '../zatca/services/certificate.service';
import { NafathUserLinkDto, NafathAuthRequestDto, NafathRequestStatus } from '../nafath/services/nafath.service';
import { ApprovalTaskDto, ApprovalStatus } from '../workflows/services/approval.service';
import { ApprovalDelegationDto } from '../workflows/services/delegation.service';
import { HijriDateDto } from '../shared/services/hijri-calendar.service';

export function createMockInvoiceList(overrides?: Partial<ZatcaInvoiceListDto>): ZatcaInvoiceListDto {
  return {
    id: 'inv-001',
    sellerId: 'seller-001',
    invoiceNumber: 'INV-2024-0001',
    invoiceType: ZatcaInvoiceType.Standard,
    issueDate: '2024-06-15',
    buyerName: 'Test Buyer',
    buyerVatNumber: '300000000000003',
    subTotal: 1000,
    vatAmount: 150,
    grandTotal: 1150,
    status: ZatcaInvoiceStatus.Draft,
    creationTime: '2024-06-15T10:00:00Z',
    ...overrides,
  };
}

export function createMockInvoiceLine(overrides?: Partial<ZatcaInvoiceLineDto>): ZatcaInvoiceLineDto {
  return {
    id: 'line-001',
    invoiceId: 'inv-001',
    itemName: 'Test Item',
    quantity: 2,
    unitPrice: 500,
    taxCategoryCode: 'S',
    taxPercent: 15,
    netAmount: 1000,
    vatAmount: 150,
    totalAmount: 1150,
    ...overrides,
  };
}

export function createMockInvoice(overrides?: Partial<ZatcaInvoiceDto>): ZatcaInvoiceDto {
  return {
    ...createMockInvoiceList(),
    currencyCode: 'SAR',
    issueDateHijri: '12/12/1445',
    lines: [createMockInvoiceLine()],
    ...overrides,
  };
}

export function createMockSeller(overrides?: Partial<ZatcaSellerDto>): ZatcaSellerDto {
  return {
    id: 'seller-001',
    sellerNameAr: 'شركة اختبار',
    sellerNameEn: 'Test Company',
    vatRegistrationNumber: '300000000000003',
    commercialRegistrationNumber: '1010000000',
    street: 'King Fahd Road',
    buildingNumber: '1234',
    city: 'Riyadh',
    district: 'Al Olaya',
    postalCode: '12345',
    countryCode: 'SA',
    isDefault: true,
    creationTime: '2024-06-01T00:00:00Z',
    ...overrides,
  };
}

export function createMockCertificate(overrides?: Partial<ZatcaCertificateDto>): ZatcaCertificateDto {
  return {
    id: 'cert-001',
    sellerId: 'seller-001',
    environment: ZatcaEnvironment.Sandbox,
    csid: 'test-csid',
    secret: '***',
    isActive: true,
    validFrom: '2024-01-01',
    validTo: '2025-01-01',
    creationTime: '2024-06-01T00:00:00Z',
    ...overrides,
  };
}

export function createMockNafathLink(overrides?: Partial<NafathUserLinkDto>): NafathUserLinkDto {
  return {
    id: 'link-001',
    userId: 'user-001',
    nationalId: '1234567890',
    verifiedAt: '2024-06-15T10:00:00Z',
    isActive: true,
    ...overrides,
  };
}

export function createMockNafathAuthRequest(overrides?: Partial<NafathAuthRequestDto>): NafathAuthRequestDto {
  return {
    id: 'auth-001',
    transactionId: 'txn-abc-123',
    nationalId: '1234567890',
    randomNumber: 42,
    status: NafathRequestStatus.Waiting,
    requestedAt: '2024-06-15T10:00:00Z',
    expiresAt: '2024-06-15T10:01:00Z',
    ...overrides,
  };
}

export function createMockApprovalTask(overrides?: Partial<ApprovalTaskDto>): ApprovalTaskDto {
  return {
    id: 'task-001',
    workflowInstanceId: 'wf-001',
    taskName: 'Approve Invoice',
    description: 'Please review and approve invoice INV-2024-0001',
    assignedToUserId: 'user-001',
    status: ApprovalStatus.Pending,
    entityType: 'ZatcaInvoice',
    entityId: 'inv-001',
    creationTime: '2024-06-15T10:00:00Z',
    ...overrides,
  };
}

export function createMockDelegation(overrides?: Partial<ApprovalDelegationDto>): ApprovalDelegationDto {
  return {
    id: 'del-001',
    delegatorUserId: 'user-001',
    delegateUserId: 'user-002',
    startDate: '2024-06-15T00:00:00Z',
    endDate: '2024-06-22T00:00:00Z',
    isActive: true,
    reason: 'On vacation',
    creationTime: '2024-06-14T00:00:00Z',
    ...overrides,
  };
}

export function createMockHijriDate(overrides?: Partial<HijriDateDto>): HijriDateDto {
  return {
    year: 1446,
    month: 6,
    day: 15,
    monthName: 'جمادى الآخرة',
    dayOfWeekName: 'السبت',
    formatted: '15 جمادى الآخرة 1446',
    gregorianDate: '2024-12-14',
    ...overrides,
  };
}
