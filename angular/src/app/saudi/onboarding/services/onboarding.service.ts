import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface OnboardingStatusDto {
  id: string;
  zatcaConfigured: boolean;
  sellerCreated: boolean;
  certificateUploaded: boolean;
  firstInvoiceSubmitted: boolean;
  nafathConfigured: boolean;
  isComplete: boolean;
  completedAt: string | null;
  completedSteps: number;
  totalRequiredSteps: number;
  totalSteps: number;
}

export enum OnboardingStep {
  ZatcaConfigured = 1,
  SellerCreated = 2,
  CertificateUploaded = 3,
  FirstInvoiceSubmitted = 4,
  NafathConfigured = 5,
}

@Injectable({ providedIn: 'root' })
export class OnboardingService {
  private readonly apiUrl = '/api/app/onboarding';

  constructor(private restService: RestService) {}

  getStatus(): Observable<OnboardingStatusDto> {
    return this.restService.request<void, OnboardingStatusDto>({
      method: 'GET',
      url: `${this.apiUrl}/status`,
    });
  }

  completeStep(step: OnboardingStep): Observable<OnboardingStatusDto> {
    return this.restService.request<{ step: OnboardingStep }, OnboardingStatusDto>({
      method: 'POST',
      url: `${this.apiUrl}/complete-step`,
      body: { step },
    });
  }

  reset(): Observable<void> {
    return this.restService.request<void, void>({
      method: 'POST',
      url: `${this.apiUrl}/reset`,
    });
  }
}
