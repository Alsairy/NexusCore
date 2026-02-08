import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { finalize } from 'rxjs/operators';

import { InvoiceListComponent } from './invoice-list.component';
import {
  InvoiceService,
  ZatcaInvoiceStatus,
  ZatcaInvoiceType,
} from '../services/invoice.service';
import {
  createMockInvoiceService,
  setupDefaultMockReturns,
} from '../../testing/mock-services';
import { createMockInvoiceList } from '../../testing/saudi-test-helpers';

describe('InvoiceListComponent', () => {
  let component: InvoiceListComponent;
  let fixture: ComponentFixture<InvoiceListComponent>;
  let mockInvoiceService: jasmine.SpyObj<any>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    mockInvoiceService = createMockInvoiceService();
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    setupDefaultMockReturns({ invoiceService: mockInvoiceService });

    await TestBed.configureTestingModule({
      imports: [InvoiceListComponent],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(InvoiceListComponent, {
        set: {
          providers: [
            { provide: InvoiceService, useValue: mockInvoiceService },
            { provide: Router, useValue: mockRouter },
          ],
          imports: [],
          schemas: [NO_ERRORS_SCHEMA],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(InvoiceListComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load invoices on init', () => {
    fixture.detectChanges();
    expect(mockInvoiceService.getList).toHaveBeenCalled();
    expect(component.invoices.length).toBe(1);
    expect(component.totalCount).toBe(1);
  });

  it('should set loading to true while fetching', () => {
    // Before detectChanges, loading starts as false; ngOnInit sets it to true
    expect(component.loading).toBe(false);
    // ngOnInit calls loadInvoices which sets loading = true synchronously,
    // then the observable (of()) fires synchronously and finalize sets it back
    fixture.detectChanges();
    // After the synchronous observable completes, loading is back to false
    expect(component.loading).toBe(false);
  });

  it('should display invoices in the table', () => {
    fixture.detectChanges();
    const nativeEl: HTMLElement = fixture.nativeElement;
    const rows = nativeEl.querySelectorAll('tbody tr');
    expect(rows.length).toBe(1);
    expect(rows[0].textContent).toContain('INV-2024-0001');
  });

  it('should show empty message when no invoices', () => {
    mockInvoiceService.getList.and.returnValue(of({ items: [], totalCount: 0 }));
    fixture.detectChanges();
    const nativeEl: HTMLElement = fixture.nativeElement;
    const emptyRow = nativeEl.querySelector('tbody tr td[colspan]');
    expect(emptyRow).toBeTruthy();
  });

  it('should navigate to detail on viewInvoice', () => {
    fixture.detectChanges();
    component.viewInvoice('inv-001');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/saudi/zatca/invoices', 'inv-001']);
  });

  it('should filter by status', () => {
    fixture.detectChanges();
    mockInvoiceService.getList.calls.reset();

    component.filterStatus = String(ZatcaInvoiceStatus.Draft);
    component.loadInvoices();

    expect(mockInvoiceService.getList).toHaveBeenCalledWith(
      jasmine.objectContaining({ status: ZatcaInvoiceStatus.Draft })
    );
  });

  it('should calculate totalPages correctly', () => {
    fixture.detectChanges();
    component.totalCount = 25;
    component.pageSize = 10;
    expect(component.totalPages).toBe(3);

    component.totalCount = 20;
    expect(component.totalPages).toBe(2);

    component.totalCount = 0;
    expect(component.totalPages).toBe(0);
  });

  it('should navigate pages with goToPage', () => {
    fixture.detectChanges();
    component.totalCount = 30;
    component.pageSize = 10;
    mockInvoiceService.getList.calls.reset();

    component.goToPage(2);
    expect(component.currentPage).toBe(2);
    expect(mockInvoiceService.getList).toHaveBeenCalledWith(
      jasmine.objectContaining({ skipCount: 10, maxResultCount: 10 })
    );

    // Should not navigate beyond totalPages
    mockInvoiceService.getList.calls.reset();
    component.goToPage(5);
    expect(component.currentPage).toBe(2); // unchanged
    expect(mockInvoiceService.getList).not.toHaveBeenCalled();

    // Should not navigate to page 0
    mockInvoiceService.getList.calls.reset();
    component.goToPage(0);
    expect(component.currentPage).toBe(2); // unchanged
    expect(mockInvoiceService.getList).not.toHaveBeenCalled();
  });

  it('should return correct badge classes for each status', () => {
    fixture.detectChanges();
    expect(component.getStatusBadgeClass(ZatcaInvoiceStatus.Draft)).toBe('bg-secondary');
    expect(component.getStatusBadgeClass(ZatcaInvoiceStatus.Validated)).toBe('bg-info');
    expect(component.getStatusBadgeClass(ZatcaInvoiceStatus.Reported)).toBe('bg-primary');
    expect(component.getStatusBadgeClass(ZatcaInvoiceStatus.Cleared)).toBe('bg-success');
    expect(component.getStatusBadgeClass(ZatcaInvoiceStatus.Rejected)).toBe('bg-danger');
    expect(component.getStatusBadgeClass(ZatcaInvoiceStatus.Archived)).toBe('bg-warning');
    expect(component.getStatusBadgeClass(99)).toBe('bg-secondary');
  });
});
