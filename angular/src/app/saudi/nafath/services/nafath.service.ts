import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface NafathAuthRequestDto {
  id: string;
  transactionId: string;
  nationalId: string;
  randomNumber: number;
  status: number;
  requestedAt: string;
  expiresAt: string;
  completedAt?: string;
  userId?: string;
}

export interface NafathUserLinkDto {
  id: string;
  userId: string;
  nationalId: string;
  verifiedAt: string;
  isActive: boolean;
}

export enum NafathRequestStatus {
  Pending = 0,
  Waiting = 1,
  Completed = 2,
  Rejected = 3,
  Expired = 4,
  Failed = 5,
}

@Injectable({ providedIn: 'root' })
export class NafathService {
  private readonly apiUrl = '/api/app/nafath';

  constructor(private restService: RestService) {}

  initiateLogin(nationalId: string): Observable<NafathAuthRequestDto> {
    return this.restService.request<any, NafathAuthRequestDto>({
      method: 'POST',
      url: `${this.apiUrl}/initiate-login`,
      body: { nationalId },
    });
  }

  checkStatus(transactionId: string): Observable<NafathAuthRequestDto> {
    return this.restService.request<any, NafathAuthRequestDto>({
      method: 'POST',
      url: `${this.apiUrl}/check-status`,
      body: { transactionId },
    });
  }

  linkIdentity(nationalId: string): Observable<NafathUserLinkDto> {
    return this.restService.request<any, NafathUserLinkDto>({
      method: 'POST',
      url: `${this.apiUrl}/link-identity`,
      body: { nationalId },
    });
  }

  getMyLink(): Observable<NafathUserLinkDto | null> {
    return this.restService.request<void, NafathUserLinkDto | null>({
      method: 'GET',
      url: `${this.apiUrl}/my-link`,
    });
  }
}
