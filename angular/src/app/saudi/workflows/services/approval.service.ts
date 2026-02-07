import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { PagedResultDto } from '../../zatca/services/invoice.service';

export interface ApprovalTaskDto {
  id: string;
  tenantId?: string;
  workflowInstanceId: string;
  taskName: string;
  description?: string;
  assignedToUserId: string;
  assignedToRoleName?: string;
  status: number;
  comment?: string;
  dueDate?: string;
  completedAt?: string;
  completedByUserId?: string;
  entityType?: string;
  entityId?: string;
  creationTime: string;
}

export interface GetApprovalTaskListInput {
  status?: number;
  dateFrom?: string;
  dateTo?: string;
  sorting?: string;
  skipCount: number;
  maxResultCount: number;
}

export interface ApproveRejectInput {
  taskId: string;
  comment?: string;
}

export enum ApprovalStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Escalated = 3,
  Delegated = 4,
}

export const ApprovalStatusLabels: Record<number, string> = {
  [ApprovalStatus.Pending]: 'Pending',
  [ApprovalStatus.Approved]: 'Approved',
  [ApprovalStatus.Rejected]: 'Rejected',
  [ApprovalStatus.Escalated]: 'Escalated',
  [ApprovalStatus.Delegated]: 'Delegated',
};

@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private readonly apiUrl = '/api/app/approval-inbox';

  constructor(private restService: RestService) {}

  getMyTasks(input: GetApprovalTaskListInput): Observable<PagedResultDto<ApprovalTaskDto>> {
    return this.restService.request<void, PagedResultDto<ApprovalTaskDto>>({
      method: 'GET',
      url: `${this.apiUrl}/my-tasks`,
      params: input as any,
    });
  }

  approve(input: ApproveRejectInput): Observable<ApprovalTaskDto> {
    return this.restService.request<ApproveRejectInput, ApprovalTaskDto>({
      method: 'POST',
      url: `${this.apiUrl}/approve`,
      body: input,
    });
  }

  reject(input: ApproveRejectInput): Observable<ApprovalTaskDto> {
    return this.restService.request<ApproveRejectInput, ApprovalTaskDto>({
      method: 'POST',
      url: `${this.apiUrl}/reject`,
      body: input,
    });
  }
}
