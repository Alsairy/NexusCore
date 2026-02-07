import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { PagedResultDto } from './invoice.service';

export interface ZatcaSellerDto {
  id: string;
  sellerNameAr: string;
  sellerNameEn?: string;
  vatRegistrationNumber: string;
  commercialRegistrationNumber?: string;
  street?: string;
  buildingNumber?: string;
  city?: string;
  district?: string;
  postalCode?: string;
  countryCode?: string;
  isDefault: boolean;
  creationTime: string;
}

export interface CreateUpdateZatcaSellerDto {
  sellerNameAr: string;
  sellerNameEn?: string;
  vatRegistrationNumber: string;
  commercialRegistrationNumber?: string;
  street?: string;
  buildingNumber?: string;
  city?: string;
  district?: string;
  postalCode?: string;
  countryCode: string;
  isDefault: boolean;
}

@Injectable({ providedIn: 'root' })
export class SellerService {
  private readonly apiUrl = '/api/app/zatca-seller';

  constructor(private restService: RestService) {}

  getList(input: { sorting?: string; skipCount: number; maxResultCount: number }): Observable<PagedResultDto<ZatcaSellerDto>> {
    return this.restService.request<void, PagedResultDto<ZatcaSellerDto>>({
      method: 'GET',
      url: this.apiUrl,
      params: input as any,
    });
  }

  get(id: string): Observable<ZatcaSellerDto> {
    return this.restService.request<void, ZatcaSellerDto>({
      method: 'GET',
      url: `${this.apiUrl}/${id}`,
    });
  }

  create(input: CreateUpdateZatcaSellerDto): Observable<ZatcaSellerDto> {
    return this.restService.request<CreateUpdateZatcaSellerDto, ZatcaSellerDto>({
      method: 'POST',
      url: this.apiUrl,
      body: input,
    });
  }

  update(id: string, input: CreateUpdateZatcaSellerDto): Observable<ZatcaSellerDto> {
    return this.restService.request<CreateUpdateZatcaSellerDto, ZatcaSellerDto>({
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

  setDefault(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'POST',
      url: `${this.apiUrl}/${id}/set-default`,
    });
  }

  getDefault(): Observable<ZatcaSellerDto | null> {
    return this.restService.request<void, ZatcaSellerDto | null>({
      method: 'GET',
      url: `${this.apiUrl}/default`,
    });
  }
}
