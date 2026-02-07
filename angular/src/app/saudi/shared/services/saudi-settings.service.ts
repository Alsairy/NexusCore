import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface ZatcaSettingsDto {
  environment?: string;
  apiBaseUrl?: string;
  complianceCsid?: string;
  productionCsid?: string;
  secret?: string;
}

export interface NafathSettingsDto {
  appId?: string;
  appKey?: string;
  apiBaseUrl?: string;
  callbackUrl?: string;
  timeoutSeconds?: number;
}

@Injectable({ providedIn: 'root' })
export class SaudiSettingsService {
  private readonly apiUrl = '/api/app/saudi-settings';

  constructor(private restService: RestService) {}

  getZatcaSettings(): Observable<ZatcaSettingsDto> {
    return this.restService.request<void, ZatcaSettingsDto>({
      method: 'GET',
      url: `${this.apiUrl}/zatca`,
    });
  }

  updateZatcaSettings(input: ZatcaSettingsDto): Observable<void> {
    return this.restService.request<ZatcaSettingsDto, void>({
      method: 'PUT',
      url: `${this.apiUrl}/zatca`,
      body: input,
    });
  }

  getNafathSettings(): Observable<NafathSettingsDto> {
    return this.restService.request<void, NafathSettingsDto>({
      method: 'GET',
      url: `${this.apiUrl}/nafath`,
    });
  }

  updateNafathSettings(input: NafathSettingsDto): Observable<void> {
    return this.restService.request<NafathSettingsDto, void>({
      method: 'PUT',
      url: `${this.apiUrl}/nafath`,
      body: input,
    });
  }
}
