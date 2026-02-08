import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  DashboardService,
  DashboardDto,
  RecentActivityDto,
} from './services/dashboard.service';
import { InvoiceStatsCardComponent } from './components/invoice-stats-card.component';
import { MonthlyChartComponent } from './components/monthly-chart.component';
import { RecentActivityComponent } from './components/recent-activity.component';

@Component({
  selector: 'app-saudi-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    LocalizationModule,
    InvoiceStatsCardComponent,
    MonthlyChartComponent,
    RecentActivityComponent,
  ],
  template: `
    <div class="container-fluid">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h4 class="mb-0">{{ '::Saudi:Dashboard:Title' | abpLocalization }}</h4>
          <small class="text-muted">{{ '::Saudi:Dashboard:Subtitle' | abpLocalization }}</small>
        </div>
        <button type="button" class="btn btn-outline-primary btn-sm" (click)="refresh()" [disabled]="loading">
          <i class="bi bi-arrow-clockwise me-1" [class.spin]="loading"></i>
          {{ '::Saudi:Dashboard:Refresh' | abpLocalization }}
        </button>
      </div>

      <!-- Loading -->
      <div *ngIf="loading" class="text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>

      <div *ngIf="!loading && dashboard">
        <!-- Stats Cards Row -->
        <div class="row g-3 mb-4">
          <div class="col-md-3">
            <app-invoice-stats-card
              [label]="'::Saudi:Dashboard:TotalInvoices' | abpLocalization"
              [value]="dashboard.totalInvoices"
              icon="bi-receipt"
              bgColor="#0d6efd"
            />
          </div>
          <div class="col-md-3">
            <app-invoice-stats-card
              [label]="'::Saudi:Dashboard:ClearedInvoices' | abpLocalization"
              [value]="dashboard.submittedInvoices"
              icon="bi-check-circle"
              bgColor="#198754"
            />
          </div>
          <div class="col-md-3">
            <app-invoice-stats-card
              [label]="'::Saudi:Dashboard:RejectedInvoices' | abpLocalization"
              [value]="dashboard.rejectedInvoices"
              icon="bi-x-circle"
              bgColor="#dc3545"
            />
          </div>
          <div class="col-md-3">
            <app-invoice-stats-card
              [label]="'::Saudi:Dashboard:TotalRevenue' | abpLocalization"
              [value]="dashboard.totalRevenue"
              icon="bi-currency-dollar"
              bgColor="#0dcaf0"
              [isCurrency]="true"
            />
          </div>
        </div>

        <!-- Monthly Chart -->
        <div class="row mb-4">
          <div class="col-12">
            <app-monthly-chart [stats]="dashboard.monthlyStats" />
          </div>
        </div>

        <!-- Second Row: Nafath + Approvals + Health -->
        <div class="row g-3 mb-4">
          <!-- Nafath Stats -->
          <div class="col-md-4">
            <div class="card border-0 shadow-sm h-100">
              <div class="card-header bg-transparent border-0">
                <h6 class="card-title mb-0">
                  <i class="bi bi-shield-check me-1 text-purple"></i>
                  {{ '::Saudi:Dashboard:NafathStats' | abpLocalization }}
                </h6>
              </div>
              <div class="card-body">
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted">{{ '::Saudi:Dashboard:TotalAuths' | abpLocalization }}</span>
                  <strong>{{ dashboard.totalNafathAuths }}</strong>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted">{{ '::Saudi:Dashboard:Completed' | abpLocalization }}</span>
                  <strong class="text-success">
                    {{ dashboard.completedNafathAuths }}
                    <small *ngIf="dashboard.totalNafathAuths > 0" class="text-muted">
                      ({{ ((dashboard.completedNafathAuths / dashboard.totalNafathAuths) * 100) | number:'1.0-0' }}%)
                    </small>
                  </strong>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted">{{ '::Saudi:Dashboard:Expired' | abpLocalization }}</span>
                  <strong class="text-warning">{{ dashboard.expiredNafathAuths }}</strong>
                </div>
                <div class="d-flex justify-content-between">
                  <span class="text-muted">{{ '::Saudi:Dashboard:LinkedUsers' | abpLocalization }}</span>
                  <strong>{{ dashboard.linkedUsers }}</strong>
                </div>
              </div>
            </div>
          </div>

          <!-- Approval Stats -->
          <div class="col-md-4">
            <div class="card border-0 shadow-sm h-100">
              <div class="card-header bg-transparent border-0">
                <h6 class="card-title mb-0">
                  <i class="bi bi-check2-square me-1 text-success"></i>
                  {{ '::Saudi:Dashboard:ApprovalStats' | abpLocalization }}
                </h6>
              </div>
              <div class="card-body">
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted">{{ '::Saudi:Dashboard:PendingApprovals' | abpLocalization }}</span>
                  <strong class="text-primary">{{ dashboard.pendingApprovals }}</strong>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted">{{ '::Saudi:Dashboard:ApprovedThisMonth' | abpLocalization }}</span>
                  <strong class="text-success">{{ dashboard.approvedThisMonth }}</strong>
                </div>
                <div class="d-flex justify-content-between">
                  <span class="text-muted">{{ '::Saudi:Dashboard:RejectedThisMonth' | abpLocalization }}</span>
                  <strong class="text-danger">{{ dashboard.rejectedThisMonth }}</strong>
                </div>
              </div>
            </div>
          </div>

          <!-- System Health -->
          <div class="col-md-4">
            <div class="card border-0 shadow-sm h-100">
              <div class="card-header bg-transparent border-0">
                <h6 class="card-title mb-0">
                  <i class="bi bi-heart-pulse me-1 text-danger"></i>
                  {{ '::Saudi:Dashboard:SystemHealth' | abpLocalization }}
                </h6>
              </div>
              <div class="card-body">
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted">{{ '::Saudi:Dashboard:ZatcaApi' | abpLocalization }}</span>
                  <span [class]="getHealthBadgeClass(dashboard.zatcaApiStatus)">
                    {{ dashboard.zatcaApiStatus }}
                  </span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted">{{ '::Saudi:Dashboard:NafathApi' | abpLocalization }}</span>
                  <span [class]="getHealthBadgeClass(dashboard.nafathApiStatus)">
                    {{ dashboard.nafathApiStatus }}
                  </span>
                </div>
                <div class="d-flex justify-content-between" *ngIf="dashboard.lastZatcaSubmission">
                  <span class="text-muted">{{ '::Saudi:Dashboard:LastSubmission' | abpLocalization }}</span>
                  <small class="text-muted">{{ formatTimestamp(dashboard.lastZatcaSubmission) }}</small>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Activity -->
        <div class="row">
          <div class="col-12">
            <app-recent-activity [activities]="recentActivities" />
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .text-purple {
        color: #6f42c1;
      }
      .spin {
        animation: spin 1s linear infinite;
      }
      @keyframes spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
      }
    `,
  ],
})
export class SaudiDashboardComponent implements OnInit {
  dashboard: DashboardDto | null = null;
  recentActivities: RecentActivityDto[] = [];
  loading = true;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit() {
    this.loadData();
  }

  refresh() {
    this.loadData();
  }

  getHealthBadgeClass(status: string): string {
    switch (status) {
      case 'Healthy':
        return 'badge bg-success';
      case 'Degraded':
        return 'badge bg-warning text-dark';
      case 'Unhealthy':
        return 'badge bg-danger';
      default:
        return 'badge bg-secondary';
    }
  }

  formatTimestamp(timestamp: string | null): string {
    if (!timestamp) return '';
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    return date.toLocaleDateString();
  }

  private loadData() {
    this.loading = true;
    let loaded = 0;
    const done = () => {
      loaded++;
      if (loaded >= 2) this.loading = false;
    };

    this.dashboardService.getDashboard().subscribe({
      next: result => {
        this.dashboard = result;
        done();
      },
      error: () => done(),
    });

    this.dashboardService.getRecentActivity(10).subscribe({
      next: result => {
        this.recentActivities = result;
        done();
      },
      error: () => done(),
    });
  }
}
