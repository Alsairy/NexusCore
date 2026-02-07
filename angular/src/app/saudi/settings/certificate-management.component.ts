import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  SellerService,
  ZatcaSellerDto,
} from '../zatca/services/seller.service';
import {
  CertificateService,
  ZatcaCertificateDto,
  CreateZatcaCertificateDto,
  ZatcaEnvironment,
} from '../zatca/services/certificate.service';

@Component({
  selector: 'app-certificate-management',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="card-title mb-0">
          {{ '::Saudi:Settings:Certificates:Title' | abpLocalization }}
        </h5>
      </div>
      <div class="card-body">
        <!-- Seller Selector -->
        <div class="row mb-4">
          <div class="col-md-6">
            <label for="sellerSelect" class="form-label">
              {{ '::Saudi:Settings:Certificates:SelectSeller' | abpLocalization }}
            </label>
            <select
              id="sellerSelect"
              class="form-select"
              [(ngModel)]="selectedSellerId"
              (change)="onSellerChange()"
            >
              <option value="">{{ '::Saudi:Settings:Certificates:ChooseSeller' | abpLocalization }}</option>
              <option *ngFor="let seller of sellers" [value]="seller.id">
                {{ seller.sellerNameAr }}
                <span *ngIf="seller.sellerNameEn"> ({{ seller.sellerNameEn }})</span>
                <span *ngIf="seller.isDefault"> - Default</span>
              </option>
            </select>
          </div>
        </div>

        <!-- Loading -->
        <div *ngIf="loadingCerts" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <div *ngIf="selectedSellerId && !loadingCerts">
          <!-- Add Certificate Button -->
          <button
            type="button"
            class="btn btn-primary btn-sm mb-3"
            (click)="toggleForm()"
          >
            <i class="bi bi-plus-circle me-1"></i>
            {{ '::Saudi:Settings:Certificates:Add' | abpLocalization }}
          </button>

          <!-- Add Certificate Form -->
          <div *ngIf="showForm" class="card border-primary mb-4">
            <div class="card-header bg-primary text-white">
              <h6 class="mb-0">
                {{ '::Saudi:Settings:Certificates:NewCertificate' | abpLocalization }}
              </h6>
            </div>
            <div class="card-body">
              <form #certForm="ngForm">
                <div class="row">
                  <div class="col-md-4 mb-3">
                    <label for="certEnv" class="form-label">
                      {{ '::Saudi:Settings:Certificates:Environment' | abpLocalization }}
                      <span class="text-danger">*</span>
                    </label>
                    <select
                      id="certEnv"
                      class="form-select"
                      [(ngModel)]="newCertificate.environment"
                      name="environment"
                      required
                    >
                      <option [value]="envSandbox">Sandbox</option>
                      <option [value]="envSimulation">Simulation</option>
                      <option [value]="envProduction">Production</option>
                    </select>
                  </div>

                  <div class="col-md-4 mb-3">
                    <label for="certCsid" class="form-label">
                      CSID
                      <span class="text-danger">*</span>
                    </label>
                    <input
                      type="text"
                      id="certCsid"
                      class="form-control"
                      [(ngModel)]="newCertificate.csid"
                      name="csid"
                      required
                    />
                  </div>

                  <div class="col-md-4 mb-3">
                    <label for="certSecret" class="form-label">
                      {{ '::Saudi:Settings:Certificates:Secret' | abpLocalization }}
                      <span class="text-danger">*</span>
                    </label>
                    <input
                      type="password"
                      id="certSecret"
                      class="form-control"
                      [(ngModel)]="newCertificate.secret"
                      name="secret"
                      required
                    />
                  </div>

                  <div class="col-md-6 mb-3">
                    <label for="certPem" class="form-label">
                      {{ '::Saudi:Settings:Certificates:CertificatePem' | abpLocalization }}
                    </label>
                    <textarea
                      id="certPem"
                      class="form-control font-monospace"
                      rows="4"
                      [(ngModel)]="newCertificate.certificatePem"
                      name="certificatePem"
                      placeholder="-----BEGIN CERTIFICATE-----"
                    ></textarea>
                  </div>

                  <div class="col-md-6 mb-3">
                    <label for="keyPem" class="form-label">
                      {{ '::Saudi:Settings:Certificates:PrivateKeyPem' | abpLocalization }}
                    </label>
                    <textarea
                      id="keyPem"
                      class="form-control font-monospace"
                      rows="4"
                      [(ngModel)]="newCertificate.privateKeyPem"
                      name="privateKeyPem"
                      placeholder="-----BEGIN EC PRIVATE KEY-----"
                    ></textarea>
                  </div>

                  <div class="col-md-4 mb-3">
                    <label for="issuedAt" class="form-label">
                      {{ '::Saudi:Settings:Certificates:IssuedAt' | abpLocalization }}
                      <span class="text-danger">*</span>
                    </label>
                    <input
                      type="date"
                      id="issuedAt"
                      class="form-control"
                      [(ngModel)]="issuedAtDate"
                      name="issuedAt"
                      required
                    />
                  </div>

                  <div class="col-md-4 mb-3">
                    <label for="expiresAt" class="form-label">
                      {{ '::Saudi:Settings:Certificates:ExpiresAt' | abpLocalization }}
                    </label>
                    <input
                      type="date"
                      id="expiresAt"
                      class="form-control"
                      [(ngModel)]="expiresAtDate"
                      name="expiresAt"
                    />
                  </div>

                  <div class="col-md-4 mb-3 d-flex align-items-end">
                    <div class="form-check">
                      <input
                        type="checkbox"
                        id="certActive"
                        class="form-check-input"
                        [(ngModel)]="newCertificate.isActive"
                        name="isActive"
                      />
                      <label for="certActive" class="form-check-label">
                        {{ '::Saudi:Settings:Certificates:ActivateImmediately' | abpLocalization }}
                      </label>
                    </div>
                  </div>
                </div>

                <div class="d-flex gap-2">
                  <button
                    type="button"
                    class="btn btn-primary"
                    [disabled]="!certForm.form.valid || isSubmitting"
                    (click)="createCertificate()"
                  >
                    <i class="bi bi-save me-1"></i>
                    {{ '::Saudi:Settings:Certificates:Create' | abpLocalization }}
                    <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm ms-1"></span>
                  </button>
                  <button type="button" class="btn btn-secondary" (click)="toggleForm()">
                    {{ '::Saudi:Settings:Certificates:Cancel' | abpLocalization }}
                  </button>
                </div>
              </form>
            </div>
          </div>

          <!-- Certificates Table -->
          <div *ngIf="certificates.length > 0" class="table-responsive">
            <table class="table table-striped table-hover">
              <thead>
                <tr>
                  <th>{{ '::Saudi:Settings:Certificates:Environment' | abpLocalization }}</th>
                  <th>CSID</th>
                  <th>{{ '::Saudi:Settings:Certificates:IssuedAt' | abpLocalization }}</th>
                  <th>{{ '::Saudi:Settings:Certificates:ExpiresAt' | abpLocalization }}</th>
                  <th>{{ '::Saudi:Settings:Certificates:Status' | abpLocalization }}</th>
                  <th class="text-end">{{ '::Saudi:Settings:Certificates:Actions' | abpLocalization }}</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let cert of certificates">
                  <td>{{ getEnvironmentLabel(cert.environment) }}</td>
                  <td>
                    <span class="text-truncate d-inline-block" style="max-width: 150px;">
                      {{ cert.csid }}
                    </span>
                  </td>
                  <td>{{ cert.issuedAt | date : 'mediumDate' }}</td>
                  <td>
                    <span *ngIf="cert.expiresAt">{{ cert.expiresAt | date : 'mediumDate' }}</span>
                    <span *ngIf="!cert.expiresAt" class="text-muted">N/A</span>
                  </td>
                  <td>
                    <span
                      class="badge"
                      [ngClass]="cert.isActive ? 'bg-success' : 'bg-secondary'"
                    >
                      {{ cert.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="text-end">
                    <button
                      *ngIf="!cert.isActive"
                      type="button"
                      class="btn btn-sm btn-outline-success me-1"
                      (click)="activateCertificate(cert.id)"
                    >
                      <i class="bi bi-check-circle me-1"></i>
                      {{ '::Saudi:Settings:Certificates:Activate' | abpLocalization }}
                    </button>
                    <button
                      *ngIf="cert.isActive"
                      type="button"
                      class="btn btn-sm btn-outline-warning me-1"
                      (click)="deactivateCertificate(cert.id)"
                    >
                      <i class="bi bi-pause-circle me-1"></i>
                      {{ '::Saudi:Settings:Certificates:Deactivate' | abpLocalization }}
                    </button>
                    <button
                      type="button"
                      class="btn btn-sm btn-outline-danger"
                      (click)="deleteCertificate(cert.id)"
                    >
                      <i class="bi bi-trash me-1"></i>
                      {{ '::Saudi:Settings:Certificates:Delete' | abpLocalization }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- No Certificates -->
          <div *ngIf="!loadingCerts && certificates.length === 0 && !showForm" class="alert alert-info text-center">
            <i class="bi bi-shield-x me-2"></i>
            {{ '::Saudi:Settings:Certificates:NoCertificates' | abpLocalization }}
          </div>
        </div>

        <!-- No Seller Selected -->
        <div *ngIf="!selectedSellerId && !loadingSellers" class="alert alert-info text-center">
          <i class="bi bi-info-circle me-2"></i>
          {{ '::Saudi:Settings:Certificates:SelectSellerFirst' | abpLocalization }}
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .font-monospace {
        font-family: 'Courier New', monospace;
        font-size: 0.85rem;
      }

      .text-truncate {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
    `,
  ],
})
export class CertificateManagementComponent implements OnInit {
  sellers: ZatcaSellerDto[] = [];
  certificates: ZatcaCertificateDto[] = [];
  selectedSellerId = '';
  showForm = false;
  loadingSellers = false;
  loadingCerts = false;
  isSubmitting = false;

  issuedAtDate = '';
  expiresAtDate = '';

  envSandbox = ZatcaEnvironment.Sandbox;
  envSimulation = ZatcaEnvironment.Simulation;
  envProduction = ZatcaEnvironment.Production;

  newCertificate: CreateZatcaCertificateDto = {
    sellerId: '',
    environment: ZatcaEnvironment.Sandbox,
    csid: '',
    secret: '',
    issuedAt: '',
    isActive: false,
  };

  constructor(
    private sellerService: SellerService,
    private certificateService: CertificateService
  ) {}

  ngOnInit() {
    this.loadSellers();
  }

  loadSellers() {
    this.loadingSellers = true;
    this.sellerService
      .getList({ skipCount: 0, maxResultCount: 100 })
      .pipe(finalize(() => (this.loadingSellers = false)))
      .subscribe(result => {
        this.sellers = result.items;
      });
  }

  onSellerChange() {
    if (this.selectedSellerId) {
      this.loadCertificates();
    } else {
      this.certificates = [];
    }
    this.showForm = false;
  }

  loadCertificates() {
    this.loadingCerts = true;
    this.certificateService
      .getList(this.selectedSellerId, { skipCount: 0, maxResultCount: 100 })
      .pipe(finalize(() => (this.loadingCerts = false)))
      .subscribe(result => {
        this.certificates = result.items;
      });
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (this.showForm) {
      this.resetForm();
    }
  }

  resetForm() {
    const today = new Date().toISOString().split('T')[0];
    this.issuedAtDate = today;
    this.expiresAtDate = '';
    this.newCertificate = {
      sellerId: this.selectedSellerId,
      environment: ZatcaEnvironment.Sandbox,
      csid: '',
      secret: '',
      issuedAt: today,
      isActive: false,
    };
  }

  createCertificate() {
    this.isSubmitting = true;
    this.newCertificate.sellerId = this.selectedSellerId;
    this.newCertificate.issuedAt = this.issuedAtDate;
    this.newCertificate.expiresAt = this.expiresAtDate || undefined;

    this.certificateService
      .create(this.newCertificate)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe(() => {
        this.showForm = false;
        this.loadCertificates();
      });
  }

  activateCertificate(id: string) {
    this.certificateService.activate(id).subscribe(() => {
      this.loadCertificates();
    });
  }

  deactivateCertificate(id: string) {
    this.certificateService.deactivate(id).subscribe(() => {
      this.loadCertificates();
    });
  }

  deleteCertificate(id: string) {
    this.certificateService.delete(id).subscribe(() => {
      this.loadCertificates();
    });
  }

  getEnvironmentLabel(env: number): string {
    const labels: Record<number, string> = {
      [ZatcaEnvironment.Sandbox]: 'Sandbox',
      [ZatcaEnvironment.Simulation]: 'Simulation',
      [ZatcaEnvironment.Production]: 'Production',
    };
    return labels[env] || 'Unknown';
  }
}
