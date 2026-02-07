import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';

import { SellerSettingsComponent } from './seller-settings.component';
import { SellerService } from '../services/seller.service';
import {
  createMockSellerService,
  setupDefaultMockReturns,
} from '../../testing/mock-services';
import { createMockSeller } from '../../testing/saudi-test-helpers';

describe('SellerSettingsComponent', () => {
  let component: SellerSettingsComponent;
  let fixture: ComponentFixture<SellerSettingsComponent>;
  let mockSellerService: jasmine.SpyObj<any>;

  beforeEach(async () => {
    mockSellerService = createMockSellerService();
    setupDefaultMockReturns({ sellerService: mockSellerService });

    await TestBed.configureTestingModule({
      imports: [SellerSettingsComponent, ReactiveFormsModule],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(SellerSettingsComponent, {
        set: {
          providers: [
            { provide: SellerService, useValue: mockSellerService },
          ],
          imports: [ReactiveFormsModule],
          schemas: [NO_ERRORS_SCHEMA],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(SellerSettingsComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load sellers on init', () => {
    fixture.detectChanges();
    expect(mockSellerService.getList).toHaveBeenCalled();
    expect(component.sellers.length).toBe(1);
    expect(component.sellers[0].sellerNameAr).toBe('\u0634\u0631\u0643\u0629 \u0627\u062e\u062a\u0628\u0627\u0631');
  });

  it('should show form when showForm is true', () => {
    fixture.detectChanges();
    component.showForm = true;
    fixture.detectChanges();

    const nativeEl: HTMLElement = fixture.nativeElement;
    const formEl = nativeEl.querySelector('form');
    expect(formEl).toBeTruthy();
  });

  it('should validate VAT number format (15 digits)', () => {
    fixture.detectChanges();
    const vatControl = component.sellerForm.get('vatRegistrationNumber');

    // Invalid: too short
    vatControl?.setValue('12345');
    expect(vatControl?.valid).toBeFalse();

    // Invalid: contains letters
    vatControl?.setValue('30000000000000A');
    expect(vatControl?.valid).toBeFalse();

    // Invalid: 16 digits
    vatControl?.setValue('1234567890123456');
    expect(vatControl?.valid).toBeFalse();

    // Valid: exactly 15 digits
    vatControl?.setValue('300000000000003');
    expect(vatControl?.valid).toBeTrue();
  });

  it('should validate Arabic name is required', () => {
    fixture.detectChanges();
    const nameArControl = component.sellerForm.get('sellerNameAr');

    nameArControl?.setValue('');
    expect(nameArControl?.valid).toBeFalse();
    expect(nameArControl?.hasError('required')).toBeTrue();

    nameArControl?.setValue('\u0634\u0631\u0643\u0629 \u062a\u062c\u0631\u064a\u0628\u064a\u0629');
    expect(nameArControl?.valid).toBeTrue();
  });

  it('should call create on submit for new seller', () => {
    fixture.detectChanges();
    component.showForm = true;
    component.isEditMode = false;

    component.sellerForm.patchValue({
      sellerNameAr: '\u0634\u0631\u0643\u0629 \u062c\u062f\u064a\u062f\u0629',
      vatRegistrationNumber: '300000000000003',
      countryCode: 'SA',
      isDefault: false,
    });

    component.onSubmit();

    expect(mockSellerService.create).toHaveBeenCalled();
    expect(mockSellerService.update).not.toHaveBeenCalled();
  });

  it('should call update on submit for edit', () => {
    fixture.detectChanges();
    const seller = createMockSeller();
    component.editSeller(seller);

    // Verify edit mode was set
    expect(component.isEditMode).toBeTrue();
    expect(component.editingSellerId).toBe('seller-001');

    // Form should be valid with seller data
    expect(component.sellerForm.valid).toBeTrue();

    component.onSubmit();

    expect(mockSellerService.update).toHaveBeenCalledWith('seller-001', jasmine.any(Object));
    expect(mockSellerService.create).not.toHaveBeenCalled();
  });

  it('should call delete and reload', () => {
    fixture.detectChanges();
    mockSellerService.getList.calls.reset();

    const seller = createMockSeller({ isDefault: false });
    component.deleteSeller(seller);

    expect(mockSellerService.delete).toHaveBeenCalledWith('seller-001');
    // After delete completes, loadSellers should be called again
    expect(mockSellerService.getList).toHaveBeenCalled();
  });

  it('should call setDefault and reload', () => {
    fixture.detectChanges();
    mockSellerService.getList.calls.reset();

    component.setAsDefault('seller-001');

    expect(mockSellerService.setDefault).toHaveBeenCalledWith('seller-001');
    // After setDefault completes, loadSellers should be called again
    expect(mockSellerService.getList).toHaveBeenCalled();
  });

  it('should reset form on cancel', () => {
    fixture.detectChanges();

    // Set up edit mode with form data
    component.showForm = true;
    component.isEditMode = true;
    component.editingSellerId = 'seller-001';
    component.sellerForm.patchValue({
      sellerNameAr: '\u0634\u0631\u0643\u0629 \u0627\u062e\u062a\u0628\u0627\u0631',
      vatRegistrationNumber: '300000000000003',
    });

    component.cancelForm();

    expect(component.showForm).toBeFalse();
    expect(component.isEditMode).toBeFalse();
    expect(component.editingSellerId).toBeNull();
    // Form should be reset; countryCode defaults to 'SA', isDefault to false
    expect(component.sellerForm.get('countryCode')?.value).toBe('SA');
    expect(component.sellerForm.get('isDefault')?.value).toBe(false);
    // Other fields should be null/empty after reset
    expect(component.sellerForm.get('sellerNameAr')?.value).toBeFalsy();
  });
});
