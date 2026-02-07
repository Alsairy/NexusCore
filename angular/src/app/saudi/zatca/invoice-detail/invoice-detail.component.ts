import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  InvoiceService,
  ZatcaInvoiceDto,
  ZatcaInvoiceStatus,
  InvoiceStatusLabels,
  InvoiceTypeLabels,
} from '../services/invoice.service';
import { InvoicePdfService } from '../services/invoice-pdf.service';
import { ZatcaQrCodeComponent } from '../components/zatca-qr-code.component';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, LocalizationModule, ZatcaQrCodeComponent],
  template: `
    <div class="card" *ngIf="invoice">
      <div class="card-header">
        <div class="row">
          <div class="col">
            <h5 class="card-title">
              {{ '::Saudi:Zatca:InvoiceDetails' | abpLocalization }}
              <span class="badge ms-2" [ngClass]="getStatusBadgeClass(invoice.status)">
                {{ '::Saudi:Zatca:' + getStatusLabel(invoice.status) | abpLocalization }}
              </span>
            </h5>
          </div>
          <div class="col text-end">
            <button type="button" class="btn btn-secondary me-2" [routerLink]="['/saudi/zatca/invoices']">
              <i class="bi bi-arrow-left"></i>
              {{ '::Saudi:Zatca:BackToList' | abpLocalization }}
            </button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <!-- Invoice Header -->
        <div class="row mb-4">
          <div class="col-md-6">
            <div class="card">
              <div class="card-header bg-light">
                <h6 class="mb-0">{{ '::Saudi:Zatca:SellerInformation' | abpLocalization }}</h6>
              </div>
              <div class="card-body">
                <p class="mb-1">
                  <strong>{{ '::Saudi:Zatca:Name' | abpLocalization }}:</strong>
                  {{ invoice.sellerId }}
                </p>
              </div>
            </div>
          </div>

          <div class="col-md-6">
            <div class="card">
              <div class="card-header bg-light">
                <h6 class="mb-0">{{ '::Saudi:Zatca:BuyerInformation' | abpLocalization }}</h6>
              </div>
              <div class="card-body">
                <p class="mb-1">
                  <strong>{{ '::Saudi:Zatca:Name' | abpLocalization }}:</strong>
                  {{ invoice.buyerName }}
                </p>
                <p class="mb-0" *ngIf="invoice.buyerVatNumber">
                  <strong>{{ '::Saudi:Zatca:VatNumber' | abpLocalization }}:</strong>
                  {{ invoice.buyerVatNumber }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <div class="row mb-4">
          <div class="col-md-6">
            <table class="table table-sm">
              <tr>
                <th style="width: 40%">{{ '::Saudi:Zatca:InvoiceNumber' | abpLocalization }}:</th>
                <td>{{ invoice.invoiceNumber }}</td>
              </tr>
              <tr>
                <th>{{ '::Saudi:Zatca:Type' | abpLocalization }}:</th>
                <td>{{ '::Saudi:Zatca:' + getTypeLabel(invoice.invoiceType) | abpLocalization }}</td>
              </tr>
              <tr>
                <th>{{ '::Saudi:Zatca:IssueDate' | abpLocalization }}:</th>
                <td>
                  {{ invoice.issueDate | date : 'medium' }}
                  <br *ngIf="invoice.issueDateHijri" />
                  <small class="text-muted" *ngIf="invoice.issueDateHijri">{{ invoice.issueDateHijri }}</small>
                </td>
              </tr>
              <tr>
                <th>{{ '::Saudi:Zatca:Currency' | abpLocalization }}:</th>
                <td>{{ invoice.currencyCode }}</td>
              </tr>
            </table>
          </div>

          <div class="col-md-6 text-center" *ngIf="invoice.qrCode">
            <div class="card">
              <div class="card-header bg-light">
                <h6 class="mb-0">{{ '::Saudi:Zatca:QRCode' | abpLocalization }}</h6>
              </div>
              <div class="card-body">
                <app-zatca-qr-code [data]="invoice.qrCode" [size]="200" />
              </div>
            </div>
          </div>
        </div>

        <!-- Invoice Lines -->
        <h6 class="mb-3">{{ '::Saudi:Zatca:InvoiceLines' | abpLocalization }}</h6>
        <div class="table-responsive mb-4">
          <table class="table table-bordered">
            <thead class="table-light">
              <tr>
                <th>{{ '::Saudi:Zatca:ItemName' | abpLocalization }}</th>
                <th class="text-end">{{ '::Saudi:Zatca:Quantity' | abpLocalization }}</th>
                <th class="text-end">{{ '::Saudi:Zatca:UnitPrice' | abpLocalization }}</th>
                <th class="text-end">{{ '::Saudi:Zatca:TaxPercent' | abpLocalization }}</th>
                <th class="text-end">{{ '::Saudi:Zatca:TaxAmount' | abpLocalization }}</th>
                <th class="text-end">{{ '::Saudi:Zatca:LineTotal' | abpLocalization }}</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let line of invoice.lines">
                <td>{{ line.itemName }}</td>
                <td class="text-end">{{ line.quantity | number : '1.2-2' }}</td>
                <td class="text-end">{{ line.unitPrice | number : '1.2-2' }}</td>
                <td class="text-end">{{ line.taxPercent }}%</td>
                <td class="text-end">{{ line.vatAmount | number : '1.2-2' }}</td>
                <td class="text-end">{{ line.totalAmount | number : '1.2-2' }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Totals -->
        <div class="row mb-4">
          <div class="col-md-6 ms-auto">
            <div class="card bg-light">
              <div class="card-body">
                <div class="row mb-2">
                  <div class="col-6 text-end">
                    <strong>{{ '::Saudi:Zatca:Subtotal' | abpLocalization }}:</strong>
                  </div>
                  <div class="col-6 text-end">{{ invoice.subTotal | number : '1.2-2' }} {{ invoice.currencyCode }}</div>
                </div>
                <div class="row mb-2">
                  <div class="col-6 text-end">
                    <strong>{{ '::Saudi:Zatca:TotalTax' | abpLocalization }}:</strong>
                  </div>
                  <div class="col-6 text-end">{{ invoice.vatAmount | number : '1.2-2' }} {{ invoice.currencyCode }}</div>
                </div>
                <hr />
                <div class="row">
                  <div class="col-6 text-end">
                    <strong class="fs-5">{{ '::Saudi:Zatca:GrandTotal' | abpLocalization }}:</strong>
                  </div>
                  <div class="col-6 text-end">
                    <strong class="fs-5">{{ invoice.grandTotal | number : '1.2-2' }} {{ invoice.currencyCode }}</strong>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- ZATCA Response -->
        <div class="card bg-light mb-4" *ngIf="invoice.zatcaRequestId">
          <div class="card-header">
            <h6 class="mb-0">{{ '::Saudi:Zatca:ZatcaResponse' | abpLocalization }}</h6>
          </div>
          <div class="card-body">
            <p class="mb-2">
              <strong>{{ '::Saudi:Zatca:RequestId' | abpLocalization }}:</strong>
              {{ invoice.zatcaRequestId }}
            </p>

            <div *ngIf="invoice.zatcaWarnings" class="alert alert-warning mb-2">
              <strong>{{ '::Saudi:Zatca:Warnings' | abpLocalization }}:</strong>
              <p class="mb-0 mt-1">{{ invoice.zatcaWarnings }}</p>
            </div>

            <div *ngIf="invoice.zatcaErrors" class="alert alert-danger mb-0">
              <strong>{{ '::Saudi:Zatca:Errors' | abpLocalization }}:</strong>
              <p class="mb-0 mt-1">{{ invoice.zatcaErrors }}</p>
            </div>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="row">
          <div class="col">
            <button
              type="button"
              class="btn btn-primary me-2"
              *ngIf="invoice.status === statusDraft"
              [routerLink]="['/saudi/zatca/invoices', invoice.id, 'edit']"
            >
              <i class="bi bi-pencil"></i>
              {{ '::Saudi:Zatca:Edit' | abpLocalization }}
            </button>

            <button
              type="button"
              class="btn btn-success me-2"
              *ngIf="invoice.status === statusDraft || invoice.status === statusValidated"
              (click)="submitToZatca()"
              [disabled]="isSubmitting"
            >
              <i class="bi bi-send"></i>
              {{ '::Saudi:Zatca:SubmitToZatca' | abpLocalization }}
              <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm ms-2"></span>
            </button>

            <button type="button" class="btn btn-outline-secondary me-2" (click)="downloadPdf()">
              <i class="bi bi-file-pdf"></i>
              {{ '::Saudi:Zatca:DownloadPdf' | abpLocalization }}
            </button>

            <button type="button" class="btn btn-outline-secondary" (click)="printInvoice()">
              <i class="bi bi-printer"></i>
              {{ '::Saudi:Zatca:Print' | abpLocalization }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card" *ngIf="!invoice && !loading">
      <div class="card-body text-center py-5">
        <i class="bi bi-exclamation-circle fs-1 text-muted"></i>
        <p class="mt-3">{{ '::Saudi:Zatca:InvoiceNotFound' | abpLocalization }}</p>
        <button type="button" class="btn btn-primary" [routerLink]="['/saudi/zatca/invoices']">
          {{ '::Saudi:Zatca:BackToList' | abpLocalization }}
        </button>
      </div>
    </div>

    <div class="card" *ngIf="loading">
      <div class="card-body text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">{{ '::Saudi:Zatca:Loading' | abpLocalization }}</span>
        </div>
      </div>
    </div>
  `,
  styles: [],
})
export class InvoiceDetailComponent implements OnInit {
  invoice: ZatcaInvoiceDto | null = null;
  loading = true;
  isSubmitting = false;

  statusDraft = ZatcaInvoiceStatus.Draft;
  statusValidated = ZatcaInvoiceStatus.Validated;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private invoiceService: InvoiceService,
    private pdfService: InvoicePdfService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.loadInvoice(id);
      }
    });
  }

  loadInvoice(id: string) {
    this.loading = true;
    this.invoiceService
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: result => {
          this.invoice = result;
        },
        error: () => {
          this.invoice = null;
        },
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

  submitToZatca() {
    if (!this.invoice) return;

    this.isSubmitting = true;
    this.invoiceService
      .submit(this.invoice.id)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: result => {
          // Reload to get updated status
          this.loadInvoice(this.invoice!.id);
        },
        error: () => {
          // Error handled by ABP interceptor
        },
      });
  }

  downloadPdf() {
    if (this.invoice) {
      this.pdfService.generate(this.invoice);
    }
  }

  printInvoice() {
    window.print();
  }
}
