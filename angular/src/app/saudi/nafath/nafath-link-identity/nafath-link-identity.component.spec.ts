import { ComponentFixture, TestBed, fakeAsync, tick, discardPeriodicTasks } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';

import { NafathLinkIdentityComponent } from './nafath-link-identity.component';
import { NafathService } from '../services/nafath.service';
import { createMockNafathService, setupDefaultMockReturns } from '../../testing/mock-services';
import { createMockNafathLink, createMockNafathAuthRequest } from '../../testing/saudi-test-helpers';

describe('NafathLinkIdentityComponent', () => {
  let component: NafathLinkIdentityComponent;
  let fixture: ComponentFixture<NafathLinkIdentityComponent>;
  let mockNafathService: jasmine.SpyObj<NafathService>;

  beforeEach(async () => {
    mockNafathService = createMockNafathService() as jasmine.SpyObj<NafathService>;
    setupDefaultMockReturns({ nafathService: mockNafathService });

    await TestBed.configureTestingModule({
      imports: [NafathLinkIdentityComponent, FormsModule],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(NafathLinkIdentityComponent, {
        set: {
          providers: [{ provide: NafathService, useValue: mockNafathService }],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(NafathLinkIdentityComponent);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    component.stopPolling();
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load linked identity on init', () => {
    const mockLink = createMockNafathLink();
    mockNafathService.getMyLink.and.returnValue(of(mockLink));

    fixture.detectChanges();

    expect(mockNafathService.getMyLink).toHaveBeenCalled();
    expect(component.linkedIdentity).toEqual(mockLink);
    expect(component.currentStep).toBe('view');
  });

  it('should show linked identity when link exists', () => {
    const mockLink = createMockNafathLink({ nationalId: '1234567890', isActive: true });
    mockNafathService.getMyLink.and.returnValue(of(mockLink));

    fixture.detectChanges();

    expect(component.linkedIdentity).toBeTruthy();
    expect(component.linkedIdentity!.nationalId).toBe('1234567890');
    expect(component.currentStep).toBe('view');
  });

  it('should show "no link" view when no link exists', () => {
    mockNafathService.getMyLink.and.returnValue(throwError(() => new Error('Not found')));

    fixture.detectChanges();

    expect(component.linkedIdentity).toBeNull();
    expect(component.currentStep).toBe('view');
  });

  it('should transition to input step on startLinking', () => {
    mockNafathService.getMyLink.and.returnValue(throwError(() => new Error('Not found')));
    fixture.detectChanges();

    component.startLinking();

    expect(component.currentStep).toBe('input');
    expect(component.nationalId).toBe('');
    expect(component.validationError).toBe('');
  });

  it('should validate national ID', () => {
    fixture.detectChanges();

    component.nationalId = '123456789';
    expect(component.isNationalIdValid()).toBeFalse();

    component.nationalId = 'abcdefghij';
    expect(component.isNationalIdValid()).toBeFalse();

    component.nationalId = '1234567890';
    expect(component.isNationalIdValid()).toBeTrue();
  });

  it('should show unlink confirmation on confirmUnlink', () => {
    const mockLink = createMockNafathLink();
    mockNafathService.getMyLink.and.returnValue(of(mockLink));
    fixture.detectChanges();

    expect(component.showUnlinkConfirmation).toBeFalse();

    component.confirmUnlink();

    expect(component.showUnlinkConfirmation).toBeTrue();
  });

  it('should hide confirmation on cancelUnlink', () => {
    const mockLink = createMockNafathLink();
    mockNafathService.getMyLink.and.returnValue(of(mockLink));
    fixture.detectChanges();

    component.confirmUnlink();
    expect(component.showUnlinkConfirmation).toBeTrue();

    component.cancelUnlink();
    expect(component.showUnlinkConfirmation).toBeFalse();
  });
});
