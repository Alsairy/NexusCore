import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  ApprovalService,
  ApprovalTaskDto,
  ApprovalStatus,
  ApprovalStatusLabels,
} from '../services/approval.service';

@Component({
  selector: 'app-approval-inbox',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="card-title">{{ '::Saudi:Workflows:ApprovalInbox' | abpLocalization }}</h5>
      </div>
      <div class="card-body">
        <!-- Filter Section -->
        <div class="row mb-3">
          <div class="col-md-4">
            <label for="statusFilter" class="form-label">
              {{ '::Saudi:Workflows:FilterByStatus' | abpLocalization }}
            </label>
            <select
              id="statusFilter"
              class="form-select"
              [(ngModel)]="selectedStatus"
              (change)="loadTasks()"
            >
              <option value="">{{ '::Saudi:Workflows:AllStatuses' | abpLocalization }}</option>
              <option *ngFor="let s of statusOptions" [value]="s.value">
                {{ '::Saudi:Workflows:' + s.label | abpLocalization }}
              </option>
            </select>
          </div>
        </div>

        <!-- Loading -->
        <div *ngIf="loading" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <!-- Tasks Table -->
        <div *ngIf="!loading && tasks.length > 0" class="table-responsive">
          <table class="table table-striped table-hover">
            <thead>
              <tr>
                <th>{{ '::Saudi:Workflows:TaskName' | abpLocalization }}</th>
                <th>{{ '::Saudi:Workflows:EntityType' | abpLocalization }}</th>
                <th>{{ '::Saudi:Workflows:DueDate' | abpLocalization }}</th>
                <th>{{ '::Saudi:Workflows:Status' | abpLocalization }}</th>
                <th class="text-end">{{ '::Saudi:Workflows:Actions' | abpLocalization }}</th>
              </tr>
            </thead>
            <tbody>
              <ng-container *ngFor="let task of tasks">
                <tr>
                  <td>{{ task.taskName }}</td>
                  <td>{{ task.entityType }}</td>
                  <td>{{ task.dueDate | date : 'short' }}</td>
                  <td>
                    <span class="badge" [ngClass]="getStatusBadgeClass(task.status)">
                      {{ '::Saudi:Workflows:' + getStatusLabel(task.status) | abpLocalization }}
                    </span>
                  </td>
                  <td class="text-end">
                    <div class="btn-group btn-group-sm" *ngIf="task.status === pendingStatus">
                      <button
                        type="button"
                        class="btn btn-outline-success"
                        (click)="toggleComment(task)"
                        [title]="'::Saudi:Workflows:Approve' | abpLocalization"
                      >
                        <i class="bi bi-check-circle me-1"></i>
                        {{ '::Saudi:Workflows:Approve' | abpLocalization }}
                      </button>
                      <button
                        type="button"
                        class="btn btn-outline-danger"
                        (click)="toggleComment(task)"
                        [title]="'::Saudi:Workflows:Reject' | abpLocalization"
                      >
                        <i class="bi bi-x-circle me-1"></i>
                        {{ '::Saudi:Workflows:Reject' | abpLocalization }}
                      </button>
                    </div>
                    <span *ngIf="task.status !== pendingStatus" class="text-muted small">
                      {{ '::Saudi:Workflows:NoActionsAvailable' | abpLocalization }}
                    </span>
                  </td>
                </tr>
                <!-- Comment Row -->
                <tr *ngIf="expandedTaskId === task.id && task.status === pendingStatus">
                  <td colspan="5">
                    <div class="card border-0 bg-light">
                      <div class="card-body">
                        <div class="mb-3">
                          <label class="form-label">
                            {{ '::Saudi:Workflows:CommentOptional' | abpLocalization }}
                          </label>
                          <textarea
                            class="form-control"
                            rows="3"
                            [(ngModel)]="actionComment"
                            [placeholder]="'::Saudi:Workflows:EnterComment' | abpLocalization"
                          ></textarea>
                        </div>
                        <div class="d-flex gap-2">
                          <button
                            type="button"
                            class="btn btn-success btn-sm"
                            [disabled]="isProcessing"
                            (click)="approveTask(task)"
                          >
                            <i class="bi bi-check-circle me-1"></i>
                            {{ '::Saudi:Workflows:ConfirmApproval' | abpLocalization }}
                            <span *ngIf="isProcessing" class="spinner-border spinner-border-sm ms-1"></span>
                          </button>
                          <button
                            type="button"
                            class="btn btn-danger btn-sm"
                            [disabled]="isProcessing"
                            (click)="rejectTask(task)"
                          >
                            <i class="bi bi-x-circle me-1"></i>
                            {{ '::Saudi:Workflows:ConfirmRejection' | abpLocalization }}
                            <span *ngIf="isProcessing" class="spinner-border spinner-border-sm ms-1"></span>
                          </button>
                          <button
                            type="button"
                            class="btn btn-secondary btn-sm"
                            (click)="toggleComment(task)"
                          >
                            {{ '::Saudi:Workflows:Cancel' | abpLocalization }}
                          </button>
                        </div>
                      </div>
                    </div>
                  </td>
                </tr>
              </ng-container>
            </tbody>
          </table>
        </div>

        <!-- No Tasks Message -->
        <div *ngIf="!loading && tasks.length === 0" class="alert alert-info text-center">
          <i class="bi bi-inbox me-2"></i>
          {{ '::Saudi:Workflows:NoPendingApprovals' | abpLocalization }}
        </div>

        <!-- Pagination -->
        <div class="row mt-3" *ngIf="totalCount > pageSize">
          <div class="col">
            <nav>
              <ul class="pagination justify-content-center">
                <li class="page-item" [class.disabled]="currentPage === 1">
                  <button class="page-link" (click)="goToPage(currentPage - 1)" [disabled]="currentPage === 1">
                    {{ '::Saudi:Workflows:Previous' | abpLocalization }}
                  </button>
                </li>
                <li class="page-item disabled">
                  <span class="page-link">
                    {{ currentPage }} / {{ totalPages }}
                  </span>
                </li>
                <li class="page-item" [class.disabled]="currentPage >= totalPages">
                  <button class="page-link" (click)="goToPage(currentPage + 1)" [disabled]="currentPage >= totalPages">
                    {{ '::Saudi:Workflows:Next' | abpLocalization }}
                  </button>
                </li>
              </ul>
            </nav>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .table-responsive {
        overflow-x: auto;
      }

      .btn-group-sm .btn {
        white-space: nowrap;
      }

      .badge {
        font-size: 0.85em;
        padding: 0.35em 0.65em;
      }

      .card-body .card {
        margin-top: 0.5rem;
      }

      .d-flex.gap-2 {
        gap: 0.5rem;
      }
    `,
  ],
})
export class ApprovalInboxComponent implements OnInit {
  tasks: ApprovalTaskDto[] = [];
  totalCount = 0;
  selectedStatus = '';
  expandedTaskId: string | null = null;
  actionComment = '';
  loading = false;
  isProcessing = false;
  currentPage = 1;
  pageSize = 10;

  pendingStatus = ApprovalStatus.Pending;

  statusOptions = [
    { value: ApprovalStatus.Pending, label: 'Pending' },
    { value: ApprovalStatus.Approved, label: 'Approved' },
    { value: ApprovalStatus.Rejected, label: 'Rejected' },
    { value: ApprovalStatus.Escalated, label: 'Escalated' },
    { value: ApprovalStatus.Delegated, label: 'Delegated' },
  ];

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  constructor(private approvalService: ApprovalService) {}

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.loading = true;
    this.approvalService
      .getMyTasks({
        status: this.selectedStatus !== '' ? Number(this.selectedStatus) : undefined,
        skipCount: (this.currentPage - 1) * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.tasks = result.items;
        this.totalCount = result.totalCount;
      });
  }

  getStatusLabel(status: number): string {
    return ApprovalStatusLabels[status] || 'Unknown';
  }

  getStatusBadgeClass(status: number): string {
    const classes: Record<number, string> = {
      [ApprovalStatus.Pending]: 'bg-warning',
      [ApprovalStatus.Approved]: 'bg-success',
      [ApprovalStatus.Rejected]: 'bg-danger',
      [ApprovalStatus.Escalated]: 'bg-info',
      [ApprovalStatus.Delegated]: 'bg-secondary',
    };
    return classes[status] || 'bg-secondary';
  }

  toggleComment(task: ApprovalTaskDto) {
    if (this.expandedTaskId === task.id) {
      this.expandedTaskId = null;
      this.actionComment = '';
    } else {
      this.expandedTaskId = task.id;
      this.actionComment = '';
    }
  }

  approveTask(task: ApprovalTaskDto) {
    this.isProcessing = true;
    this.approvalService
      .approve({ taskId: task.id, comment: this.actionComment || undefined })
      .pipe(finalize(() => (this.isProcessing = false)))
      .subscribe(() => {
        this.expandedTaskId = null;
        this.actionComment = '';
        this.loadTasks();
      });
  }

  rejectTask(task: ApprovalTaskDto) {
    this.isProcessing = true;
    this.approvalService
      .reject({ taskId: task.id, comment: this.actionComment || undefined })
      .pipe(finalize(() => (this.isProcessing = false)))
      .subscribe(() => {
        this.expandedTaskId = null;
        this.actionComment = '';
        this.loadTasks();
      });
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadTasks();
    }
  }
}
