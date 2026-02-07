import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { CertificateManagementComponent } from './certificate-management.component';
import { CertificateService } from '../zatca/services/certificate.service';
import { SellerService } from '../zatca/services/seller.service';
import { createMockCertificateService, createMockSellerService, setupDefaultMockReturns } from '../testing/mock-services';
import { of } from 'rxjs';

describe('CertificateManagementComponent', () => {
  let component: CertificateManagementComponent;
  let fixture: ComponentFixture<CertificateManagementComponent>;
  let mockCertificateService: jasmine.SpyObj<CertificateService>;
  let mockSellerService: jasmine.SpyObj<SellerService>;

  beforeEach(async () => {
    mockCertificateService = createMockCertificateService() as jasmine.SpyObj<CertificateService>;
    mockSellerService = createMockSellerService() as jasmine.SpyObj<SellerService>;
    setupDefaultMockReturns({
      certificateService: mockCertificateService,
      sellerService: mockSellerService,
    });

    await TestBed.configureTestingModule({
      imports: [CertificateManagementComponent],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(CertificateManagementComponent, {
        set: {
          providers: [
            { provide: CertificateService, useValue: mockCertificateService },
            { provide: SellerService, useValue: mockSellerService },
          ],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(CertificateManagementComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load sellers on init', () => {
    fixture.detectChanges();

    expect(mockSellerService.getList).toHaveBeenCalledTimes(1);
    expect(component.sellers.length).toBe(1);
    expect(component.sellers[0].sellerNameAr).toBe('شركة اختبار');
  });

  it('should load certificates when seller is selected', () => {
    fixture.detectChanges();

    component.selectedSellerId = 'seller-001';
    component.onSellerChange();

    expect(mockCertificateService.getList).toHaveBeenCalledWith(
      'seller-001',
      jasmine.objectContaining({ skipCount: 0, maxResultCount: 100 })
    );
    expect(component.certificates.length).toBe(1);
  });

  it('should call activate on certificate', () => {
    fixture.detectChanges();

    component.selectedSellerId = 'seller-001';
    component.onSellerChange();

    component.activateCertificate('cert-001');

    expect(mockCertificateService.activate).toHaveBeenCalledWith('cert-001');
  });
});
