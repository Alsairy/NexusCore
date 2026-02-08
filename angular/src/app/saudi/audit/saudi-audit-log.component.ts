import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  AuditService,
  SaudiAuditLogDto,
  GetSaudiAuditListInput,
} from './services/audit.service';

@Component({
  selector: 'app-saudi-audit-log',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <div class="row align-items-center">
          <div class="col">
            <h5 class="card-title mb-0">{{ '::Saudi:Audit:Title' | abpLocalization }}</h5>
          </div>
          <div class="col-auto">
            <button type="button" class="btn btn-sm btn-outline-success me-2" (click)="exportCsv()">
              <i class="bi bi-download me-1"></i>
              {{ '::Saudi:Audit:ExportCsv' | abpLocalization }}
            </button>
            <button type="button" class="btn btn-sm btn-outline-primary" (click)="loadData()">
              <i class="bi bi-arrow-clockwise me-1"></i>
              {{ '::Saudi:Audit:Refresh' | abpLocalization }}
            </button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <!-- Filters -->
        <div class="row g-2 mb-3">
          <div class="col-md-2">
            <label class="form-label small">{{ '::Saudi:Audit:StartDate' | abpLocalization }}</label>
            <input type="date" class="form-control form-control-sm" [(ngModel)]="filters.startDate" (change)="onFilterChange()" />
          </div>
          <div class="col-md-2">
            <label class="form-label small">{{ '::Saudi:Audit:EndDate' | abpLocalization }}</label>
            <input type="date" class="form-control form-control-sm" [(ngModel)]="filters.endDate" (change)="onFilterChange()" />
          </div>
          <div class="col-md-2">
            <label class="form-label small">{{ '::Saudi:Audit:Module' | abpLocalization }}</label>
            <select class="form-select form-select-sm" [(ngModel)]="filters.module" (change)="onFilterChange()">
              <option value="">{{ '::Saudi:Audit:AllModules' | abpLocalization }}</option>
              <option value="Zatca">ZATCA</option>
              <option value="Nafath">Nafath</option>
              <option value="Workflow">Workflows</option>
            </select>
          </div>
          <div class="col-md-2">
            <label class="form-label small">{{ '::Saudi:Audit:EntityId' | abpLocalization }}</label>
            <input type="text" class="form-control form-control-sm" [(ngModel)]="filters.entityId"
              placeholder="Entity ID..." (keyup.enter)="onFilterChange()" />
          </div>
          <div class="col-md-2">
            <label class="form-label small">{{ '::Saudi:Audit:MinDuration' | abpLocalization }}</label>
            <input type="number" class="form-control form-control-sm" [(ngModel)]="filters.minDuration"
              placeholder="ms" (keyup.enter)="onFilterChange()" />
          </div>
          <div class="col-md-2 d-flex align-items-end">
            <button type="button" class="btn btn-sm btn-secondary w-100" (click)="clearFilters()">
              <i class="bi bi-x-circle me-1"></i>
              {{ '::Saudi:Audit:ClearFilters' | abpLocalization }}
            </button>
          </div>
        </div>

        <!-- Loading -->
        <div *ngIf="loading" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <!-- Results Table -->
        <div class="table-responsive" *ngIf="!loading">
          <table class="table table-sm table-hover align-middle">
            <thead class="table-light">
              <tr>
                <th style="width: 40px;"></th>
                <th class="sortable" (click)="sort('ExecutionTime')">
                  {{ '::Saudi:Audit:Timestamp' | abpLocalization }}
                  <i *ngIf="filters.sorting === 'ExecutionTime ASC'" class="bi bi-sort-up"></i>
                  <i *ngIf="filters.sorting === 'ExecutionTime DESC'" class="bi bi-sort-down"></i>
                </th>
                <th>{{ '::Saudi:Audit:User' | abpLocalization }}</th>
                <th>{{ '::Saudi:Audit:Action' | abpLocalization }}</th>
                <th>{{ '::Saudi:Audit:HttpMethod' | abpLocalization }}</th>
                <th>{{ '::Saudi:Audit:Url' | abpLocalization }}</th>
                <th class="sortable" (click)="sort('ExecutionDuration')">
                  {{ '::Saudi:Audit:Duration' | abpLocalization }}
                  <i *ngIf="filters.sorting === 'ExecutionDuration ASC'" class="bi bi-sort-up"></i>
                  <i *ngIf="filters.sorting === 'ExecutionDuration DESC'" class="bi bi-sort-down"></i>
                </th>
                <th>{{ '::Saudi:Audit:Status' | abpLocalization }}</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let log of auditLogs"
                  [class.table-danger]="log.hasException"
                  style="cursor: pointer;"
                  (click)="toggleDetail(log)">
                <td>
                  <i class="bi" [class.bi-chevron-right]="expandedId !== log.id" [class.bi-chevron-down]="expandedId === log.id"></i>
                </td>
                <td>
                  <small>{{ formatDate(log.executionTime) }}</small>
                </td>
                <td>{{ log.userName || '—' }}</td>
                <td>
                  <small class="text-muted">{{ log.serviceName }}</small>
                  <br *ngIf="log.methodName" />
                  <small *ngIf="log.methodName">{{ log.methodName }}</small>
                </td>
                <td>
                  <span class="badge" [class]="getHttpMethodBadge(log.httpMethod)">
                    {{ log.httpMethod }}
                  </span>
                </td>
                <td>
                  <small class="text-truncate d-inline-block" style="max-width: 200px;" [title]="log.url">
                    {{ shortenUrl(log.url) }}
                  </small>
                </td>
                <td>
                  <span [class]="getDurationClass(log.executionDuration)">
                    {{ log.executionDuration }}ms
                  </span>
                </td>
                <td>
                  <span class="badge" [class]="getStatusBadge(log.httpStatusCode)">
                    {{ log.httpStatusCode || '—' }}
                  </span>
                </td>
              </tr>

              <!-- Expanded Detail Row -->
              <tr *ngIf="expandedId && expandedLog">
                <td colspan="8" class="bg-light p-3">
                  <div class="row">
                    <!-- Request Info -->
                    <div class="col-md-6">
                      <h6 class="text-muted mb-2">{{ '::Saudi:Audit:RequestDetails' | abpLocalization }}</h6>
                      <dl class="row mb-0">
                        <dt class="col-sm-4 small">IP Address</dt>
                        <dd class="col-sm-8 small">{{ expandedLog.clientIpAddress || '—' }}</dd>
                        <dt class="col-sm-4 small">Browser</dt>
                        <dd class="col-sm-8 small text-truncate" [title]="expandedLog.browserInfo">{{ expandedLog.browserInfo || '—' }}</dd>
                      </dl>
                      <div *ngIf="expandedLog.hasException" class="alert alert-danger mt-2 p-2">
                        <small><strong>Exception:</strong> {{ expandedLog.exceptionMessage }}</small>
                      </div>
                    </div>

                    <!-- Entity Changes -->
                    <div class="col-md-6">
                      <h6 class="text-muted mb-2">{{ '::Saudi:Audit:EntityChanges' | abpLocalization }}</h6>
                      <div *ngIf="expandedLog.entityChanges.length === 0" class="text-muted small">
                        {{ '::Saudi:Audit:NoEntityChanges' | abpLocalization }}
                      </div>
                      <div *ngFor="let change of expandedLog.entityChanges" class="mb-2">
                        <small>
                          <span class="badge" [class]="getChangeTypeBadge(change.changeType)">{{ change.changeType }}</span>
                          {{ change.entityTypeFullName }} #{{ change.entityId }}
                        </small>
                        <table class="table table-sm table-bordered mt-1 mb-0" *ngIf="change.propertyChanges.length > 0">
                          <thead>
                            <tr class="table-light">
                              <th class="small py-1">Property</th>
                              <th class="small py-1">Before</th>
                              <th class="small py-1">After</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr *ngFor="let prop of change.propertyChanges">
                              <td class="small py-1">{{ prop.propertyName }}</td>
                              <td class="small py-1 text-danger">{{ prop.originalValue || '—' }}</td>
                              <td class="small py-1 text-success">{{ prop.newValue || '—' }}</td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                    </div>
                  </div>
                </td>
              </tr>

              <!-- Empty State -->
              <tr *ngIf="auditLogs.length === 0">
                <td colspan="8" class="text-center text-muted py-4">
                  {{ '::Saudi:Audit:NoResults' | abpLocalization }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="d-flex justify-content-between align-items-center mt-3" *ngIf="totalCount > 0">
          <small class="text-muted">
            {{ '::Saudi:Audit:Showing' | abpLocalization }}
            {{ filters.skipCount! + 1 }}–{{ Math.min(filters.skipCount! + pageSize, totalCount) }}
            {{ '::Saudi:Audit:Of' | abpLocalization }} {{ totalCount }}
          </small>
          <nav>
            <ul class="pagination pagination-sm mb-0">
              <li class="page-item" [class.disabled]="filters.skipCount === 0">
                <button class="page-link" (click)="goToPage(currentPage - 1)">
                  <i class="bi bi-chevron-left"></i>
                </button>
              </li>
              <li class="page-item" *ngFor="let page of visiblePages" [class.active]="page === currentPage">
                <button class="page-link" (click)="goToPage(page)">{{ page + 1 }}</button>
              </li>
              <li class="page-item" [class.disabled]="(currentPage + 1) * pageSize >= totalCount">
                <button class="page-link" (click)="goToPage(currentPage + 1)">
                  <i class="bi bi-chevron-right"></i>
                </button>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .sortable {
        cursor: pointer;
        user-select: none;
      }
      .sortable:hover {
        background-color: #f8f9fa;
      }
    `,
  ],
})
export class SaudiAuditLogComponent implements OnInit {
  Math = Math;
  auditLogs: SaudiAuditLogDto[] = [];
  totalCount = 0;
  loading = true;
  expandedId: string | null = null;
  expandedLog: SaudiAuditLogDto | null = null;
  pageSize = 25;

  filters: GetSaudiAuditListInput = {
    skipCount: 0,
    maxResultCount: 25,
    sorting: 'ExecutionTime DESC',
    startDate: '',
    endDate: '',
    module: '',
    entityId: '',
    minDuration: undefined,
  };

  constructor(private auditService: AuditService) {}

  ngOnInit() {
    this.loadData();
  }

  get currentPage(): number {
    return Math.floor((this.filters.skipCount || 0) / this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  get visiblePages(): number[] {
    const pages: number[] = [];
    const start = Math.max(0, this.currentPage - 2);
    const end = Math.min(this.totalPages, start + 5);
    for (let i = start; i < end; i++) {
      pages.push(i);
    }
    return pages;
  }

  loadData() {
    this.loading = true;
    const input: GetSaudiAuditListInput = {
      ...this.filters,
      startDate: this.filters.startDate || undefined,
      endDate: this.filters.endDate || undefined,
      module: this.filters.module || undefined,
      entityId: this.filters.entityId || undefined,
    };

    this.auditService
      .getList(input)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.auditLogs = result.items;
        this.totalCount = result.totalCount;
        this.expandedId = null;
        this.expandedLog = null;
      });
  }

  onFilterChange() {
    this.filters.skipCount = 0;
    this.loadData();
  }

  clearFilters() {
    this.filters = {
      skipCount: 0,
      maxResultCount: this.pageSize,
      sorting: 'ExecutionTime DESC',
      startDate: '',
      endDate: '',
      module: '',
      entityId: '',
      minDuration: undefined,
    };
    this.loadData();
  }

  sort(field: string) {
    if (this.filters.sorting === `${field} ASC`) {
      this.filters.sorting = `${field} DESC`;
    } else {
      this.filters.sorting = `${field} ASC`;
    }
    this.filters.skipCount = 0;
    this.loadData();
  }

  goToPage(page: number) {
    if (page < 0 || page >= this.totalPages) return;
    this.filters.skipCount = page * this.pageSize;
    this.loadData();
  }

  toggleDetail(log: SaudiAuditLogDto) {
    if (this.expandedId === log.id) {
      this.expandedId = null;
      this.expandedLog = null;
    } else {
      this.expandedId = log.id;
      this.expandedLog = log;
    }
  }

  exportCsv() {
    const headers = ['Timestamp', 'User', 'HTTP Method', 'URL', 'Duration (ms)', 'Status Code', 'Has Error'];
    const rows = this.auditLogs.map(log => [
      log.executionTime,
      log.userName || '',
      log.httpMethod || '',
      log.url || '',
      log.executionDuration.toString(),
      log.httpStatusCode?.toString() || '',
      log.hasException ? 'Yes' : 'No',
    ]);

    const csv = [headers, ...rows].map(row => row.map(cell => `"${cell}"`).join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `saudi-audit-log-${new Date().toISOString().slice(0, 10)}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleString();
  }

  shortenUrl(url: string | null): string {
    if (!url) return '—';
    return url.replace('/api/app/', '/');
  }

  getHttpMethodBadge(method: string | null): string {
    switch (method) {
      case 'GET': return 'bg-primary';
      case 'POST': return 'bg-success';
      case 'PUT': return 'bg-warning text-dark';
      case 'DELETE': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }

  getDurationClass(duration: number): string {
    if (duration <= 200) return 'text-success';
    if (duration <= 1000) return 'text-warning';
    return 'text-danger fw-bold';
  }

  getStatusBadge(statusCode: number | null): string {
    if (!statusCode) return 'bg-secondary';
    if (statusCode < 300) return 'bg-success';
    if (statusCode < 400) return 'bg-info';
    if (statusCode < 500) return 'bg-warning text-dark';
    return 'bg-danger';
  }

  getChangeTypeBadge(changeType: string): string {
    switch (changeType) {
      case 'Created': return 'bg-success';
      case 'Updated': return 'bg-primary';
      case 'Deleted': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }
}
