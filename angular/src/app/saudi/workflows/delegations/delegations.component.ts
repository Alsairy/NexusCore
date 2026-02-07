import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  DelegationService,
  ApprovalDelegationDto,
  CreateUpdateApprovalDelegationDto,
} from '../services/delegation.service';

@Component({
  selector: 'app-delegations',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="card-title mb-0">
          {{ '::Saudi:Workflows:DelegationManagement' | abpLocalization }}
        </h5>
        <button
          type="button"
          class="btn btn-primary btn-sm"
          (click)="toggleForm()"
        >
          <i class="bi bi-plus-circle me-1"></i>
          {{ '::Saudi:Workflows:CreateDelegation' | abpLocalization }}
        </button>
      </div>
      <div class="card-body">
        <!-- Create Delegation Form -->
        <div *ngIf="showForm" class="card border-primary mb-4">
          <div class="card-header bg-primary text-white">
            <h6 class="mb-0">
              {{ isEditMode
                ? ('::Saudi:Workflows:EditDelegation' | abpLocalization)
                : ('::Saudi:Workflows:NewDelegation' | abpLocalization) }}
            </h6>
          </div>
          <div class="card-body">
            <form #delegationForm="ngForm">
              <div class="row">
                <div class="col-md-6 mb-3">
                  <label for="delegateUserId" class="form-label">
                    {{ '::Saudi:Workflows:DelegateTo' | abpLocalization }}
                    <span class="text-danger">*</span>
                  </label>
                  <input
                    type="text"
                    id="delegateUserId"
                    class="form-control"
                    [(ngModel)]="newDelegation.delegateUserId"
                    name="delegateUserId"
                    required
                    [placeholder]="'::Saudi:Workflows:EnterUserId' | abpLocalization"
                  />
                </div>

                <div class="col-md-3 mb-3">
                  <label for="startDate" class="form-label">
                    {{ '::Saudi:Workflows:StartDate' | abpLocalization }}
                    <span class="text-danger">*</span>
                  </label>
                  <input
                    type="date"
                    id="startDate"
                    class="form-control"
                    [(ngModel)]="newDelegation.startDate"
                    name="startDate"
                    required
                  />
                </div>

                <div class="col-md-3 mb-3">
                  <label for="endDate" class="form-label">
                    {{ '::Saudi:Workflows:EndDate' | abpLocalization }}
                    <span class="text-danger">*</span>
                  </label>
                  <input
                    type="date"
                    id="endDate"
                    class="form-control"
                    [(ngModel)]="newDelegation.endDate"
                    name="endDate"
                    required
                  />
                </div>

                <div class="col-12 mb-3">
                  <label for="reason" class="form-label">
                    {{ '::Saudi:Workflows:Reason' | abpLocalization }}
                  </label>
                  <textarea
                    id="reason"
                    class="form-control"
                    rows="3"
                    [(ngModel)]="newDelegation.reason"
                    name="reason"
                    [placeholder]="'::Saudi:Workflows:ReasonPlaceholder' | abpLocalization"
                  ></textarea>
                </div>
              </div>

              <div class="d-flex gap-2">
                <button
                  type="submit"
                  class="btn btn-primary"
                  [disabled]="!delegationForm.form.valid || isSubmitting"
                  (click)="saveDelegation()"
                >
                  <i class="bi bi-save me-1"></i>
                  {{ '::Saudi:Workflows:Save' | abpLocalization }}
                  <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm ms-1"></span>
                </button>
                <button
                  type="button"
                  class="btn btn-secondary"
                  (click)="toggleForm()"
                >
                  {{ '::Saudi:Workflows:Cancel' | abpLocalization }}
                </button>
              </div>
            </form>
          </div>
        </div>

        <!-- Loading -->
        <div *ngIf="loading" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <!-- Delegations Table -->
        <div *ngIf="!loading && delegations.length > 0" class="table-responsive">
          <table class="table table-striped table-hover">
            <thead>
              <tr>
                <th>{{ '::Saudi:Workflows:DelegateTo' | abpLocalization }}</th>
                <th>{{ '::Saudi:Workflows:StartDate' | abpLocalization }}</th>
                <th>{{ '::Saudi:Workflows:EndDate' | abpLocalization }}</th>
                <th>{{ '::Saudi:Workflows:Status' | abpLocalization }}</th>
                <th>{{ '::Saudi:Workflows:Reason' | abpLocalization }}</th>
                <th class="text-end">{{ '::Saudi:Workflows:Actions' | abpLocalization }}</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let delegation of delegations">
                <td>{{ delegation.delegateUserId }}</td>
                <td>{{ delegation.startDate | date : 'short' }}</td>
                <td>{{ delegation.endDate | date : 'short' }}</td>
                <td>
                  <span
                    class="badge"
                    [ngClass]="delegation.isActive ? 'bg-success' : 'bg-secondary'"
                  >
                    {{
                      delegation.isActive
                        ? ('::Saudi:Workflows:Active' | abpLocalization)
                        : ('::Saudi:Workflows:Inactive' | abpLocalization)
                    }}
                  </span>
                </td>
                <td>
                  <span class="text-truncate d-inline-block" style="max-width: 200px;" [title]="delegation.reason || ''">
                    {{ delegation.reason }}
                  </span>
                </td>
                <td class="text-end">
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-primary me-1"
                    (click)="editDelegation(delegation)"
                  >
                    <i class="bi bi-pencil me-1"></i>
                    {{ '::Saudi:Workflows:Edit' | abpLocalization }}
                  </button>
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-danger"
                    (click)="deleteDelegation(delegation.id)"
                  >
                    <i class="bi bi-trash me-1"></i>
                    {{ '::Saudi:Workflows:Delete' | abpLocalization }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- No Delegations Message -->
        <div *ngIf="!loading && delegations.length === 0" class="alert alert-info text-center">
          <i class="bi bi-people me-2"></i>
          {{ '::Saudi:Workflows:NoDelegations' | abpLocalization }}
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .table-responsive {
        overflow-x: auto;
      }

      .badge {
        font-size: 0.85em;
        padding: 0.35em 0.65em;
      }

      .text-truncate {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }

      .d-flex.gap-2 {
        gap: 0.5rem;
      }

      .card-header .btn {
        white-space: nowrap;
      }

      form .form-label .text-danger {
        margin-left: 0.25rem;
      }
    `,
  ],
})
export class DelegationsComponent implements OnInit {
  delegations: ApprovalDelegationDto[] = [];
  showForm = false;
  isEditMode = false;
  editingId: string | null = null;
  isSubmitting = false;
  loading = false;
  newDelegation: CreateUpdateApprovalDelegationDto = {
    delegateUserId: '',
    startDate: '',
    endDate: '',
    reason: '',
  };

  constructor(private delegationService: DelegationService) {}

  ngOnInit() {
    this.loadDelegations();
    this.setDefaultDates();
  }

  loadDelegations() {
    this.loading = true;
    this.delegationService
      .getList({ skipCount: 0, maxResultCount: 100 })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.delegations = result.items;
      });
  }

  setDefaultDates() {
    const today = new Date();
    const nextWeek = new Date();
    nextWeek.setDate(today.getDate() + 7);

    this.newDelegation.startDate = today.toISOString().split('T')[0];
    this.newDelegation.endDate = nextWeek.toISOString().split('T')[0];
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (this.showForm && !this.isEditMode) {
      this.resetForm();
    }
    if (!this.showForm) {
      this.isEditMode = false;
      this.editingId = null;
    }
  }

  resetForm() {
    this.newDelegation = {
      delegateUserId: '',
      startDate: '',
      endDate: '',
      reason: '',
    };
    this.setDefaultDates();
  }

  editDelegation(delegation: ApprovalDelegationDto) {
    this.isEditMode = true;
    this.editingId = delegation.id;
    this.showForm = true;
    this.newDelegation = {
      delegateUserId: delegation.delegateUserId,
      startDate: delegation.startDate.split('T')[0],
      endDate: delegation.endDate.split('T')[0],
      reason: delegation.reason || '',
    };
  }

  saveDelegation() {
    this.isSubmitting = true;

    const request$ = this.isEditMode && this.editingId
      ? this.delegationService.update(this.editingId, this.newDelegation)
      : this.delegationService.create(this.newDelegation);

    request$
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe(() => {
        this.showForm = false;
        this.isEditMode = false;
        this.editingId = null;
        this.loadDelegations();
      });
  }

  deleteDelegation(id: string) {
    this.delegationService.delete(id).subscribe(() => {
      this.loadDelegations();
    });
  }
}
