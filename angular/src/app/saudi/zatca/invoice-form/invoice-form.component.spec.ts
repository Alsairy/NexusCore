import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { of, EMPTY } from 'rxjs';

import { InvoiceFormComponent } from './invoice-form.component';
import { InvoiceService } from '../services/invoice.service';
import { SellerService } from '../services/seller.service';
import {
  createMockInvoiceService,
  createMockSellerService,
  setupDefaultMockReturns,
} from '../../testing/mock-services';
import { createMockSeller, createMockInvoice } from '../../testing/saudi-test-helpers';

describe('InvoiceFormComponent', () => {
  let component: InvoiceFormComponent;
  let fixture: ComponentFixture<InvoiceFormComponent>;
  let mockInvoiceService: jasmine.SpyObj<any>;
  let mockSellerService: jasmine.SpyObj<any>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    mockInvoiceService = createMockInvoiceService();
    mockSellerService = createMockSellerService();
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    setupDefaultMockReturns({
      invoiceService: mockInvoiceService,
      sellerService: mockSellerService,
    });

    await TestBed.configureTestingModule({
      imports: [InvoiceFormComponent, ReactiveFormsModule],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(InvoiceFormComponent, {
        set: {
          providers: [
            { provide: InvoiceService, useValue: mockInvoiceService },
            { provide: SellerService, useValue: mockSellerService },
            { provide: Router, useValue: mockRouter },
            {
              provide: ActivatedRoute,
              useValue: { params: of({}) },
            },
          ],
          imports: [ReactiveFormsModule],
          schemas: [NO_ERRORS_SCHEMA],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(InvoiceFormComponent);
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
    expect(component.sellers[0].id).toBe('seller-001');
  });

  it('should have invalid form when empty', () => {
    fixture.detectChanges();
    // Reset the form to clear defaults
    component.invoiceForm.patchValue({
      sellerId: '',
      invoiceType: '',
      issueDate: '',
      buyerName: '',
    });
    expect(component.invoiceForm.valid).toBeFalse();
  });

  it('should add a line on init', () => {
    fixture.detectChanges();
    // ngOnInit calls addLine() which adds one line
    expect(component.lines.length).toBe(1);
  });

  it('should add line when addLine is called', () => {
    fixture.detectChanges();
    expect(component.lines.length).toBe(1);

    component.addLine();
    expect(component.lines.length).toBe(2);

    component.addLine();
    expect(component.lines.length).toBe(3);
  });

  it('should remove line when removeLine is called but not the last one', () => {
    fixture.detectChanges();
    // Start with 1 line from init
    component.addLine(); // now 2
    expect(component.lines.length).toBe(2);

    component.removeLine(1);
    expect(component.lines.length).toBe(1);

    // Should not remove the last remaining line
    component.removeLine(0);
    expect(component.lines.length).toBe(1);
  });

  it('should calculate line totals correctly', () => {
    fixture.detectChanges();
    const line = component.lines.at(0);
    line.patchValue({ quantity: 5, unitPrice: 200, taxPercent: 15 });

    component.calculateLineTotal(0);

    // subtotal = 5 * 200 = 1000
    // taxAmount = 1000 * 15 / 100 = 150
    // lineTotal = 1000 + 150 = 1150
    expect(line.get('taxAmount')?.value).toBe(150);
    expect(line.get('lineTotal')?.value).toBe(1150);
  });

  it('should calculate grand totals correctly', () => {
    fixture.detectChanges();
    // Set up line 1
    const line1 = component.lines.at(0);
    line1.patchValue({ quantity: 2, unitPrice: 500, taxPercent: 15 });
    component.calculateLineTotal(0);

    // Add line 2
    component.addLine();
    const line2 = component.lines.at(1);
    line2.patchValue({ quantity: 3, unitPrice: 100, taxPercent: 15 });
    component.calculateLineTotal(1);

    // Line 1: subtotal=1000, tax=150, total=1150
    // Line 2: subtotal=300, tax=45, total=345
    // Grand: subtotal=1300, totalTax=195, grandTotal=1495
    expect(component.subtotal).toBe(1300);
    expect(component.totalTax).toBe(195);
    expect(component.grandTotal).toBe(1495);
  });

  it('should call create on submit for new invoice', () => {
    fixture.detectChanges();

    // Fill in the required fields
    component.invoiceForm.patchValue({
      sellerId: 'seller-001',
      invoiceType: '0',
      issueDate: '2024-06-15',
      buyerName: 'Test Buyer',
    });

    // Set up line data
    const line = component.lines.at(0);
    line.patchValue({
      itemName: 'Test Item',
      quantity: 2,
      unitPrice: 500,
      taxPercent: 15,
    });

    component.onSubmit();

    expect(mockInvoiceService.create).toHaveBeenCalled();
    expect(mockInvoiceService.update).not.toHaveBeenCalled();
  });

  it('should mark controls as touched on invalid submit', () => {
    fixture.detectChanges();

    // Ensure form is invalid (clear required fields)
    component.invoiceForm.patchValue({
      sellerId: '',
      invoiceType: '',
      issueDate: '',
      buyerName: '',
    });

    component.onSubmit();

    expect(component.invoiceForm.get('sellerId')?.touched).toBeTrue();
    expect(component.invoiceForm.get('buyerName')?.touched).toBeTrue();
    expect(mockInvoiceService.create).not.toHaveBeenCalled();
  });
});
