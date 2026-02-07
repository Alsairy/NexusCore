import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { PagedResultDto } from './invoice.service';

export interface ZatcaCertificateDto {
  id: string;
  sellerId: string;
  environment: number;
  csid: string;
  secret: string;
  certificatePem?: string;
  privateKeyPem?: string;
  issuedAt: string;
  expiresAt?: string;
  isActive: boolean;
  creationTime: string;
}

export interface CreateZatcaCertificateDto {
  sellerId: string;
  environment: number;
  csid: string;
  secret: string;
  certificatePem?: string;
  privateKeyPem?: string;
  issuedAt: string;
  expiresAt?: string;
  isActive: boolean;
}

export enum ZatcaEnvironment {
  Sandbox = 0,
  Simulation = 1,
  Production = 2,
}

@Injectable({ providedIn: 'root' })
export class CertificateService {
  private readonly apiUrl = '/api/app/zatca-certificate';

  constructor(private restService: RestService) {}

  getList(
    sellerId: string,
    input: { sorting?: string; skipCount: number; maxResultCount: number }
  ): Observable<PagedResultDto<ZatcaCertificateDto>> {
    return this.restService.request<void, PagedResultDto<ZatcaCertificateDto>>({
      method: 'GET',
      url: this.apiUrl,
      params: { sellerId, ...input } as any,
    });
  }

  get(id: string): Observable<ZatcaCertificateDto> {
    return this.restService.request<void, ZatcaCertificateDto>({
      method: 'GET',
      url: `${this.apiUrl}/${id}`,
    });
  }

  create(input: CreateZatcaCertificateDto): Observable<ZatcaCertificateDto> {
    return this.restService.request<CreateZatcaCertificateDto, ZatcaCertificateDto>({
      method: 'POST',
      url: this.apiUrl,
      body: input,
    });
  }

  delete(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'DELETE',
      url: `${this.apiUrl}/${id}`,
    });
  }

  activate(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'POST',
      url: `${this.apiUrl}/${id}/activate`,
    });
  }

  deactivate(id: string): Observable<void> {
    return this.restService.request<void, void>({
      method: 'POST',
      url: `${this.apiUrl}/${id}/deactivate`,
    });
  }
}
