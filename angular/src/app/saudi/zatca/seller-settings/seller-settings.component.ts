import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  SellerService,
  ZatcaSellerDto,
  CreateUpdateZatcaSellerDto,
} from '../services/seller.service';

@Component({
  selector: 'app-seller-settings',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <div class="row">
          <div class="col col-md-6">
            <h5 class="card-title">{{ '::Saudi:Zatca:SellerSettings' | abpLocalization }}</h5>
          </div>
          <div class="col col-md-6 text-end">
            <button type="button" class="btn btn-primary" (click)="showForm = true" *ngIf="!showForm">
              <i class="bi bi-plus-circle"></i>
              {{ '::Saudi:Zatca:AddSeller' | abpLocalization }}
            </button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <!-- Loading -->
        <div *ngIf="loading" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <!-- Seller Form -->
        <div class="card mb-4" *ngIf="showForm">
          <div class="card-header bg-light">
            <h6 class="mb-0">
              {{ isEditMode ? ('::Saudi:Zatca:EditSeller' | abpLocalization) : ('::Saudi:Zatca:AddSeller' | abpLocalization) }}
            </h6>
          </div>
          <div class="card-body">
            <form [formGroup]="sellerForm" (ngSubmit)="onSubmit()">
              <div class="row mb-3">
                <div class="col-md-6">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:NameEnglish' | abpLocalization }}
                  </label>
                  <input type="text" class="form-control" formControlName="sellerNameEn" />
                </div>
                <div class="col-md-6">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:NameArabic' | abpLocalization }}
                    <span class="text-danger">*</span>
                  </label>
                  <input type="text" class="form-control" formControlName="sellerNameAr" dir="rtl" />
                  <div
                    class="text-danger mt-1"
                    *ngIf="sellerForm.get('sellerNameAr')?.invalid && sellerForm.get('sellerNameAr')?.touched"
                  >
                    {{ '::Saudi:Zatca:NameArabicRequired' | abpLocalization }}
                  </div>
                </div>
              </div>

              <div class="row mb-3">
                <div class="col-md-6">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:VatNumber' | abpLocalization }}
                    <span class="text-danger">*</span>
                  </label>
                  <input
                    type="text"
                    class="form-control"
                    formControlName="vatRegistrationNumber"
                    placeholder="300000000000003"
                    maxlength="15"
                  />
                  <div
                    class="text-danger mt-1"
                    *ngIf="sellerForm.get('vatRegistrationNumber')?.invalid && sellerForm.get('vatRegistrationNumber')?.touched"
                  >
                    {{ '::Saudi:Zatca:VatNumberRequired' | abpLocalization }}
                  </div>
                </div>
                <div class="col-md-6">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:CrNumber' | abpLocalization }}
                  </label>
                  <input
                    type="text"
                    class="form-control"
                    formControlName="commercialRegistrationNumber"
                    placeholder="1010000000"
                    maxlength="10"
                  />
                </div>
              </div>

              <div class="row mb-3">
                <div class="col-md-6">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:Street' | abpLocalization }}
                  </label>
                  <input type="text" class="form-control" formControlName="street" />
                </div>
                <div class="col-md-6">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:BuildingNumber' | abpLocalization }}
                  </label>
                  <input type="text" class="form-control" formControlName="buildingNumber" />
                </div>
              </div>

              <div class="row mb-3">
                <div class="col-md-3">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:City' | abpLocalization }}
                  </label>
                  <input type="text" class="form-control" formControlName="city" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:District' | abpLocalization }}
                  </label>
                  <input type="text" class="form-control" formControlName="district" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:PostalCode' | abpLocalization }}
                  </label>
                  <input type="text" class="form-control" formControlName="postalCode" maxlength="5" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">
                    {{ '::Saudi:Zatca:Country' | abpLocalization }}
                  </label>
                  <input type="text" class="form-control" formControlName="countryCode" readonly />
                </div>
              </div>

              <div class="row mb-3">
                <div class="col-md-12">
                  <div class="form-check">
                    <input
                      type="checkbox"
                      class="form-check-input"
                      id="isDefault"
                      formControlName="isDefault"
                    />
                    <label class="form-check-label" for="isDefault">
                      {{ '::Saudi:Zatca:SetAsDefaultSeller' | abpLocalization }}
                    </label>
                  </div>
                </div>
              </div>

              <div class="row">
                <div class="col">
                  <button type="button" class="btn btn-secondary me-2" (click)="cancelForm()">
                    <i class="bi bi-x-circle"></i>
                    {{ '::Saudi:Zatca:Cancel' | abpLocalization }}
                  </button>
                  <button type="submit" class="btn btn-primary" [disabled]="sellerForm.invalid || isSubmitting">
                    <i class="bi bi-check-circle"></i>
                    {{ '::Saudi:Zatca:Save' | abpLocalization }}
                    <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm ms-2"></span>
                  </button>
                </div>
              </div>
            </form>
          </div>
        </div>

        <!-- Sellers Table -->
        <div class="table-responsive" *ngIf="!loading">
          <table class="table table-striped table-hover">
            <thead>
              <tr>
                <th>{{ '::Saudi:Zatca:NameEnglish' | abpLocalization }}</th>
                <th>{{ '::Saudi:Zatca:NameArabic' | abpLocalization }}</th>
                <th>{{ '::Saudi:Zatca:VatNumber' | abpLocalization }}</th>
                <th>{{ '::Saudi:Zatca:CrNumber' | abpLocalization }}</th>
                <th>{{ '::Saudi:Zatca:City' | abpLocalization }}</th>
                <th class="text-center">{{ '::Saudi:Zatca:Default' | abpLocalization }}</th>
                <th class="text-center">{{ '::Saudi:Zatca:Actions' | abpLocalization }}</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let seller of sellers">
                <td>{{ seller.sellerNameEn }}</td>
                <td dir="rtl">{{ seller.sellerNameAr }}</td>
                <td>{{ seller.vatRegistrationNumber }}</td>
                <td>{{ seller.commercialRegistrationNumber }}</td>
                <td>{{ seller.city }}</td>
                <td class="text-center">
                  <span class="badge bg-success" *ngIf="seller.isDefault">
                    {{ '::Saudi:Zatca:Default' | abpLocalization }}
                  </span>
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-secondary"
                    *ngIf="!seller.isDefault"
                    (click)="setAsDefault(seller.id)"
                  >
                    {{ '::Saudi:Zatca:SetDefault' | abpLocalization }}
                  </button>
                </td>
                <td class="text-center">
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-primary me-1"
                    (click)="editSeller(seller)"
                  >
                    <i class="bi bi-pencil"></i>
                    {{ '::Saudi:Zatca:Edit' | abpLocalization }}
                  </button>
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-danger"
                    (click)="deleteSeller(seller)"
                    [disabled]="seller.isDefault"
                  >
                    <i class="bi bi-trash"></i>
                    {{ '::Saudi:Zatca:Delete' | abpLocalization }}
                  </button>
                </td>
              </tr>
              <tr *ngIf="sellers.length === 0">
                <td colspan="7" class="text-center text-muted py-4">
                  {{ '::Saudi:Zatca:NoSellersFound' | abpLocalization }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  styles: [],
})
export class SellerSettingsComponent implements OnInit {
  sellers: ZatcaSellerDto[] = [];
  sellerForm: FormGroup;
  showForm = false;
  isEditMode = false;
  editingSellerId: string | null = null;
  isSubmitting = false;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private sellerService: SellerService
  ) {
    this.sellerForm = this.fb.group({
      sellerNameAr: ['', Validators.required],
      sellerNameEn: [''],
      vatRegistrationNumber: ['', [Validators.required, Validators.pattern(/^\d{15}$/)]],
      commercialRegistrationNumber: [''],
      street: [''],
      buildingNumber: [''],
      city: [''],
      district: [''],
      postalCode: [''],
      countryCode: ['SA'],
      isDefault: [false],
    });
  }

  ngOnInit() {
    this.loadSellers();
  }

  loadSellers() {
    this.loading = true;
    this.sellerService
      .getList({ skipCount: 0, maxResultCount: 100 })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(result => {
        this.sellers = result.items;
      });
  }

  editSeller(seller: ZatcaSellerDto) {
    this.isEditMode = true;
    this.editingSellerId = seller.id;
    this.showForm = true;

    this.sellerForm.patchValue({
      sellerNameAr: seller.sellerNameAr,
      sellerNameEn: seller.sellerNameEn || '',
      vatRegistrationNumber: seller.vatRegistrationNumber,
      commercialRegistrationNumber: seller.commercialRegistrationNumber || '',
      street: seller.street || '',
      buildingNumber: seller.buildingNumber || '',
      city: seller.city || '',
      district: seller.district || '',
      postalCode: seller.postalCode || '',
      countryCode: seller.countryCode || 'SA',
      isDefault: seller.isDefault,
    });
  }

  deleteSeller(seller: ZatcaSellerDto) {
    if (seller.isDefault) return;

    this.sellerService.delete(seller.id).subscribe(() => {
      this.loadSellers();
    });
  }

  setAsDefault(id: string) {
    this.sellerService.setDefault(id).subscribe(() => {
      this.loadSellers();
    });
  }

  onSubmit() {
    if (this.sellerForm.invalid) {
      Object.keys(this.sellerForm.controls).forEach(key => {
        this.sellerForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isSubmitting = true;
    const formValue = this.sellerForm.value as CreateUpdateZatcaSellerDto;

    const request$ = this.isEditMode && this.editingSellerId
      ? this.sellerService.update(this.editingSellerId, formValue)
      : this.sellerService.create(formValue);

    request$
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe(() => {
        this.cancelForm();
        this.loadSellers();
      });
  }

  cancelForm() {
    this.showForm = false;
    this.isEditMode = false;
    this.editingSellerId = null;
    this.sellerForm.reset({
      countryCode: 'SA',
      isDefault: false,
    });
  }
}
