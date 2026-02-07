import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ZatcaInvoiceListDto {
  id: string;
  sellerId: string;
  invoiceNumber: string;
  invoiceType: number;
  issueDate: string;
  buyerName?: string;
  buyerVatNumber?: string;
  subTotal: number;
  vatAmount: number;
  grandTotal: number;
  status: number;
  zatcaRequestId?: string;
  creationTime: string;
}

export interface ZatcaInvoiceLineDto {
  id: string;
  invoiceId: string;
  itemName: string;
  quantity: number;
  unitPrice: number;
  taxCategoryCode?: string;
  taxPercent: number;
  netAmount: number;
  vatAmount: number;
  totalAmount: number;
}

export interface ZatcaInvoiceDto extends ZatcaInvoiceListDto {
  issueDateHijri?: string;
  currencyCode: string;
  qrCode?: string;
  xmlContent?: string;
  invoiceHash?: string;
  previousInvoiceHash?: string;
  zatcaWarnings?: string;
  zatcaErrors?: string;
  lines: ZatcaInvoiceLineDto[];
}

export interface CreateUpdateZatcaInvoiceDto {
  sellerId: string;
  invoiceNumber: string;
  invoiceType: number;
  issueDate: string;
  issueDateHijri?: string;
  buyerName?: string;
  buyerVatNumber?: string;
  currencyCode: string;
  lines: CreateUpdateZatcaInvoiceLineDto[];
}

export interface CreateUpdateZatcaInvoiceLineDto {
  itemName: string;
  quantity: number;
  unitPrice: number;
  taxCategoryCode?: string;
  taxPercent: number;
}

export interface GetZatcaInvoiceListInput {
  status?: number;
  invoiceType?: number;
  dateFrom?: string;
  dateTo?: string;
  filter?: string;
  sellerId?: string;
  sorting?: string;
  skipCount: number;
  maxResultCount: number;
}

export interface ZatcaSubmitResultDto {
  requestId: string;
  status: number;
  warnings: string[];
  errors: string[];
  qrCode?: string;
  isSuccess: boolean;
}

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[];
}

// Enum mappings matching backend
export enum ZatcaInvoiceStatus {
  Draft = 0,
  Validated = 1,
  Reported = 2,
  Cleared = 3,
  Rejected = 4,
  Archived = 5,
}

export enum ZatcaInvoiceType {
  Standard = 0,
  Simplified = 1,
  StandardCreditNote = 2,
  StandardDebitNote = 3,
  SimplifiedCreditNote = 4,
  SimplifiedDebitNote = 5,
}

export const InvoiceStatusLabels: Record<number, string> = {
  [ZatcaInvoiceStatus.Draft]: 'Draft',
  [ZatcaInvoiceStatus.Validated]: 'Validated',
  [ZatcaInvoiceStatus.Reported]: 'Reported',
  [ZatcaInvoiceStatus.Cleared]: 'Cleared',
  [ZatcaInvoiceStatus.Rejected]: 'Rejected',
  [ZatcaInvoiceStatus.Archived]: 'Archived',
};

export const InvoiceTypeLabels: Record<number, string> = {
  [ZatcaInvoiceType.Standard]: 'Standard',
  [ZatcaInvoiceType.Simplified]: 'Simplified',
  [ZatcaInvoiceType.StandardCreditNote]: 'Credit',
  [ZatcaInvoiceType.StandardDebitNote]: 'Debit',
  [ZatcaInvoiceType.SimplifiedCreditNote]: 'SimplifiedCredit',
  [ZatcaInvoiceType.SimplifiedDebitNote]: 'SimplifiedDebit',
};

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly apiUrl = '/api/app/zatca-invoice';

  constructor(private restService: RestService) {}

  getList(input: GetZatcaInvoiceListInput): Observable<PagedResultDto<ZatcaInvoiceListDto>> {
    return this.restService.request<void, PagedResultDto<ZatcaInvoiceListDto>>({
      method: 'GET',
      url: this.apiUrl,
      params: input as any,
    });
  }

  get(id: string): Observable<ZatcaInvoiceDto> {
    return this.restService.request<void, ZatcaInvoiceDto>({
      method: 'GET',
      url: `${this.apiUrl}/${id}`,
    });
  }

  create(input: CreateUpdateZatcaInvoiceDto): Observable<ZatcaInvoiceDto> {
    return this.restService.request<CreateUpdateZatcaInvoiceDto, ZatcaInvoiceDto>({
      method: 'POST',
      url: this.apiUrl,
      body: input,
    });
  }

  update(id: string, input: CreateUpdateZatcaInvoiceDto): Observable<ZatcaInvoiceDto> {
    return this.restService.request<CreateUpdateZatcaInvoiceDto, ZatcaInvoiceDto>({
      method: 'PUT',
      url: `${this.apiUrl}/${id}`,
      body: input,
    });
  }

  delete(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'DELETE',
      url: `${this.apiUrl}/${id}`,
    });
  }

  submit(id: string): Observable<ZatcaSubmitResultDto> {
    return this.restService.request<void, ZatcaSubmitResultDto>({
      method: 'POST',
      url: `${this.apiUrl}/${id}/submit`,
    });
  }

  validate(id: string): Observable<ZatcaSubmitResultDto> {
    return this.restService.request<void, ZatcaSubmitResultDto>({
      method: 'POST',
      url: `${this.apiUrl}/${id}/validate`,
    });
  }
}
