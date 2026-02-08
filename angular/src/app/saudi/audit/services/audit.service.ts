import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface SaudiAuditLogDto {
  id: string;
  tenantId: string | null;
  userId: string | null;
  userName: string | null;
  serviceName: string | null;
  methodName: string | null;
  executionTime: string;
  executionDuration: number;
  httpMethod: string | null;
  url: string | null;
  httpStatusCode: number | null;
  clientIpAddress: string | null;
  browserInfo: string | null;
  hasException: boolean;
  exceptionMessage: string | null;
  actions: SaudiAuditLogActionDto[];
  entityChanges: SaudiEntityChangeDto[];
}

export interface SaudiAuditLogActionDto {
  serviceName: string | null;
  methodName: string | null;
  parameters: string | null;
  executionTime: string;
  executionDuration: number;
}

export interface SaudiEntityChangeDto {
  id: string;
  entityTypeFullName: string | null;
  entityId: string | null;
  changeType: string;
  changeTime: string;
  propertyChanges: SaudiPropertyChangeDto[];
}

export interface SaudiPropertyChangeDto {
  propertyName: string | null;
  originalValue: string | null;
  newValue: string | null;
}

export interface PagedResult<T> {
  totalCount: number;
  items: T[];
}

export interface GetSaudiAuditListInput {
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
  startDate?: string;
  endDate?: string;
  userId?: string;
  module?: string;
  entityType?: string;
  entityId?: string;
  minDuration?: number;
}

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly apiUrl = '/api/app/saudi-audit';

  constructor(private restService: RestService) {}

  getList(input: GetSaudiAuditListInput): Observable<PagedResult<SaudiAuditLogDto>> {
    const params: Record<string, string> = {};
    if (input.skipCount != null) params['SkipCount'] = input.skipCount.toString();
    if (input.maxResultCount != null) params['MaxResultCount'] = input.maxResultCount.toString();
    if (input.sorting) params['Sorting'] = input.sorting;
    if (input.startDate) params['StartDate'] = input.startDate;
    if (input.endDate) params['EndDate'] = input.endDate;
    if (input.userId) params['UserId'] = input.userId;
    if (input.module) params['Module'] = input.module;
    if (input.entityType) params['EntityType'] = input.entityType;
    if (input.entityId) params['EntityId'] = input.entityId;
    if (input.minDuration != null) params['MinDuration'] = input.minDuration.toString();

    return this.restService.request<void, PagedResult<SaudiAuditLogDto>>({
      method: 'GET',
      url: this.apiUrl,
      params,
    });
  }

  get(id: string): Observable<SaudiAuditLogDto> {
    return this.restService.request<void, SaudiAuditLogDto>({
      method: 'GET',
      url: `${this.apiUrl}/${id}`,
    });
  }

  getEntityHistory(entityType: string, entityId: string): Observable<SaudiEntityChangeDto[]> {
    return this.restService.request<void, SaudiEntityChangeDto[]>({
      method: 'GET',
      url: `${this.apiUrl}/entity-history`,
      params: { entityType, entityId },
    });
  }
}
