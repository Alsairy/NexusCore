import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { PagedResultDto } from '../../zatca/services/invoice.service';

export interface ApprovalDelegationDto {
  id: string;
  tenantId?: string;
  delegatorUserId: string;
  delegateUserId: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  reason?: string;
  creationTime: string;
}

export interface CreateUpdateApprovalDelegationDto {
  delegateUserId: string;
  startDate: string;
  endDate: string;
  reason?: string;
}

@Injectable({ providedIn: 'root' })
export class DelegationService {
  private readonly apiUrl = '/api/app/approval-delegation';

  constructor(private restService: RestService) {}

  getList(input: { sorting?: string; skipCount: number; maxResultCount: number }): Observable<PagedResultDto<ApprovalDelegationDto>> {
    return this.restService.request<void, PagedResultDto<ApprovalDelegationDto>>({
      method: 'GET',
      url: this.apiUrl,
      params: input as any,
    });
  }

  get(id: string): Observable<ApprovalDelegationDto> {
    return this.restService.request<void, ApprovalDelegationDto>({
      method: 'GET',
      url: `${this.apiUrl}/${id}`,
    });
  }

  create(input: CreateUpdateApprovalDelegationDto): Observable<ApprovalDelegationDto> {
    return this.restService.request<CreateUpdateApprovalDelegationDto, ApprovalDelegationDto>({
      method: 'POST',
      url: this.apiUrl,
      body: input,
    });
  }

  update(id: string, input: CreateUpdateApprovalDelegationDto): Observable<ApprovalDelegationDto> {
    return this.restService.request<CreateUpdateApprovalDelegationDto, ApprovalDelegationDto>({
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
}
