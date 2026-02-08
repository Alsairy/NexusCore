import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface DashboardDto {
  totalInvoices: number;
  draftInvoices: number;
  submittedInvoices: number;
  rejectedInvoices: number;
  totalRevenue: number;
  totalVat: number;
  monthlyStats: MonthlyInvoiceStatsDto[];
  totalNafathAuths: number;
  completedNafathAuths: number;
  expiredNafathAuths: number;
  linkedUsers: number;
  pendingApprovals: number;
  approvedThisMonth: number;
  rejectedThisMonth: number;
  zatcaApiStatus: string;
  nafathApiStatus: string;
  lastZatcaSubmission: string | null;
}

export interface MonthlyInvoiceStatsDto {
  year: number;
  month: number;
  monthName: string;
  invoiceCount: number;
  revenue: number;
  vat: number;
}

export interface RecentActivityDto {
  timestamp: string;
  activityType: string;
  description: string;
  entityId: string | null;
  entityType: string | null;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly apiUrl = '/api/app/saudi-dashboard';

  constructor(private restService: RestService) {}

  getDashboard(): Observable<DashboardDto> {
    return this.restService.request<void, DashboardDto>({
      method: 'GET',
      url: `${this.apiUrl}/dashboard`,
    });
  }

  getMonthlyStats(year?: number): Observable<MonthlyInvoiceStatsDto[]> {
    return this.restService.request<void, MonthlyInvoiceStatsDto[]>({
      method: 'GET',
      url: `${this.apiUrl}/monthly-stats`,
      params: year ? { year: year.toString() } : {},
    });
  }

  getRecentActivity(count: number = 10): Observable<RecentActivityDto[]> {
    return this.restService.request<void, RecentActivityDto[]>({
      method: 'GET',
      url: `${this.apiUrl}/recent-activity`,
      params: { count: count.toString() },
    });
  }
}
