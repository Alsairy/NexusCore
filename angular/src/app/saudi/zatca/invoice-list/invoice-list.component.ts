import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  InvoiceService,
  ZatcaInvoiceListDto,
  ZatcaInvoiceStatus,
  ZatcaInvoiceType,
  InvoiceStatusLabels,
  InvoiceTypeLabels,
} from '../services/invoice.service';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <div class="row">
          <div class="col col-md-6">
            <h5 class="card-title">{{ '::Saudi:Zatca:Invoices' | abpLocalization }}</h5>
          </div>
          <div class="col col-md-6 text-end">
            <button
              type="button"
              class="btn btn-primary"
              [routerLink]="['/saudi/zatca/invoices/create']"
            >
              <i class="bi bi-plus-circle"></i>
              {{ '::Saudi:Zatca:CreateInvoice' | abpLocalization }}
            </button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <!-- Filters -->
        <div class="row mb-3">
          <div class="col-md-4">
            <label class="form-label">{{ '::Saudi:Zatca:Status' | abpLocalization }}</label>
            <select class="form-select" [(ngModel)]="filterStatus" (ngModelChange)="loadInvoices()">
              <option value="">{{ '::Saudi:Zatca:AllStatuses' | abpLocalization }}</option>
              <option *ngFor="let s of statusOptions" [value]="s.value">
                {{ '::Saudi:Zatca:' + s.label | abpLocalization }}
              </option>
            </select>
          </div>
          <div class="col-md-4">
            <label class="form-label">{{ '::Saudi:Zatca:FromDate' | abpLocalization }}</label>
            <input
              type="date"
              class="form-control"
              [(ngModel)]="filterFromDate"
              (ngModelChange)="loadInvoices()"
            />
          </div>
          <div class="col-md-4">
            <label class="form-label">{{ '::Saudi:Zatca:ToDate' | abpLocalization }}</label>
            <input
              type="date"
              class="form-control"
              [(ngModel)]="filterToDate"
              (ngModelChange)="loadInvoices()"
            />
          </div>
        </div>

        <!-- Loading -->
        <div *ngIf="loading" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <!-- Invoice Table -->
        <div class="table-responsive" *ngIf="!loading">
          <table class="table table-striped table-hover">
            <thead>
              <tr>
                <th>{{ '::Saudi:Zatca:InvoiceNumber' | abpLocalization }}</th>
                <th>{{ '::Saudi:Zatca:Type' | abpLocalization }}</th>
                <th>{{ '::Saudi:Zatca:Status' | abpLocalization }}</th>
                <th>{{ '::Saudi:Zatca:IssueDate' | abpLocalization }}</th>
                <th>{{ '::Saudi:Zatca:BuyerName' | abpLocalization }}</th>
                <th class="text-end">{{ '::Saudi:Zatca:GrandTotal' | abpLocalization }}</th>
                <th class="text-center">{{ '::Saudi:Zatca:Actions' | abpLocalization }}</th>
              </tr>
            </thead>
            <tbody>
              <tr
                *ngFor="let invoice of invoices"
                class="cursor-pointer"
                (click)="viewInvoice(invoice.id)"
              >
                <td>{{ invoice.invoiceNumber }}</td>
                <td>{{ '::Saudi:Zatca:' + getTypeLabel(invoice.invoiceType) | abpLocalization }}</td>
                <td>
                  <span class="badge" [ngClass]="getStatusBadgeClass(invoice.status)">
                    {{ '::Saudi:Zatca:' + getStatusLabel(invoice.status) | abpLocalization }}
                  </span>
                </td>
                <td>{{ invoice.issueDate | date : 'short' }}</td>
                <td>{{ invoice.buyerName }}</td>
                <td class="text-end">{{ invoice.grandTotal | number : '1.2-2' }} SAR</td>
                <td class="text-center" (click)="$event.stopPropagation()">
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-primary"
                    [routerLink]="['/saudi/zatca/invoices', invoice.id]"
                  >
                    <i class="bi bi-eye"></i>
                    {{ '::Saudi:Zatca:View' | abpLocalization }}
                  </button>
                </td>
              </tr>
              <tr *ngIf="invoices.length === 0">
                <td colspan="7" class="text-center text-muted py-4">
                  {{ '::Saudi:Zatca:NoInvoicesFound' | abpLocalization }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="row mt-3" *ngIf="totalCount > 0">
          <div class="col">
            <nav>
              <ul class="pagination justify-content-center">
                <li class="page-item" [class.disabled]="currentPage === 1">
                  <button class="page-link" (click)="goToPage(currentPage - 1)" [disabled]="currentPage === 1">
                    {{ '::Saudi:Zatca:Previous' | abpLocalization }}
                  </button>
                </li>
                <li class="page-item disabled">
                  <span class="page-link">
                    {{ '::Saudi:Zatca:Page' | abpLocalization }} {{ currentPage }} / {{ totalPages }}
                  </span>
                </li>
                <li class="page-item" [class.disabled]="currentPage >= totalPages">
                  <button class="page-link" (click)="goToPage(currentPage + 1)" [disabled]="currentPage >= totalPages">
                    {{ '::Saudi:Zatca:Next' | abpLocalization }}
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
      .cursor-pointer {
        cursor: pointer;
      }

      .cursor-pointer:hover {
        background-color: rgba(0, 0, 0, 0.03);
      }
    `,
  ],
})
export class InvoiceListComponent implements OnInit {
  invoices: ZatcaInvoiceListDto[] = [];
  totalCount = 0;
  filterStatus = '';
  filterFromDate = '';
  filterToDate = '';
  currentPage = 1;
  pageSize = 10;
  loading = false;

  statusOptions = [
    { value: ZatcaInvoiceStatus.Draft, label: 'Draft' },
    { value: ZatcaInvoiceStatus.Validated, label: 'Validated' },
    { value: ZatcaInvoiceStatus.Reported, label: 'Reported' },
    { value: ZatcaInvoiceStatus.Cleared, label: 'Cleared' },
    { value: ZatcaInvoiceStatus.Rejected, label: 'Rejected' },
    { value: ZatcaInvoiceStatus.Archived, label: 'Archived' },
  ];

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  constructor(
    private router: Router,
    private invoiceService: InvoiceService
  ) {}

  ngOnInit() {
    this.loadInvoices();
  }

  loadInvoices() {
    this.loading = true;
    this.invoiceService
      .getList({
        status: this.filterStatus !== '' ? Number(this.filterStatus) : undefined,
        dateFrom: this.filterFromDate || undefined,
        dateTo: this.filterToDate || undefined,
        skipCount: (this.currentPage - 1) * this.pageSize,
        maxResultCount: this.pageSize,
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.invoices = result.items;
        this.totalCount = result.totalCount;
      });
  }

  getStatusLabel(status: number): string {
    return InvoiceStatusLabels[status] || 'Unknown';
  }

  getTypeLabel(type: number): string {
    return InvoiceTypeLabels[type] || 'Unknown';
  }

  getStatusBadgeClass(status: number): string {
    const statusClasses: Record<number, string> = {
      [ZatcaInvoiceStatus.Draft]: 'bg-secondary',
      [ZatcaInvoiceStatus.Validated]: 'bg-info',
      [ZatcaInvoiceStatus.Reported]: 'bg-primary',
      [ZatcaInvoiceStatus.Cleared]: 'bg-success',
      [ZatcaInvoiceStatus.Rejected]: 'bg-danger',
      [ZatcaInvoiceStatus.Archived]: 'bg-warning',
    };
    return statusClasses[status] || 'bg-secondary';
  }

  viewInvoice(id: string) {
    this.router.navigate(['/saudi/zatca/invoices', id]);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadInvoices();
    }
  }
}
