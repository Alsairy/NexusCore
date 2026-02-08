import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ZatcaQrCodeComponent } from './zatca-qr-code.component';

describe('ZatcaQrCodeComponent', () => {
  let component: ZatcaQrCodeComponent;
  let fixture: ComponentFixture<ZatcaQrCodeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ZatcaQrCodeComponent],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ZatcaQrCodeComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should not render when data is null', () => {
    component.data = null;
    fixture.detectChanges();

    const canvas = fixture.nativeElement.querySelector('canvas');
    expect(canvas).toBeNull();
  });

  it('should have default size of 200', () => {
    expect(component.size).toBe(200);
  });

  it('should accept size input', () => {
    component.size = 300;
    fixture.detectChanges();

    expect(component.size).toBe(300);
  });
});
