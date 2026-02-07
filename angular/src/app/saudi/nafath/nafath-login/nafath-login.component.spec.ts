import { ComponentFixture, TestBed, fakeAsync, tick, discardPeriodicTasks } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';

import { NafathLoginComponent } from './nafath-login.component';
import { NafathService, NafathRequestStatus } from '../services/nafath.service';
import { createMockNafathService, setupDefaultMockReturns } from '../../testing/mock-services';
import { createMockNafathAuthRequest } from '../../testing/saudi-test-helpers';

describe('NafathLoginComponent', () => {
  let component: NafathLoginComponent;
  let fixture: ComponentFixture<NafathLoginComponent>;
  let mockNafathService: jasmine.SpyObj<NafathService>;

  beforeEach(async () => {
    mockNafathService = createMockNafathService() as jasmine.SpyObj<NafathService>;
    setupDefaultMockReturns({ nafathService: mockNafathService });

    await TestBed.configureTestingModule({
      imports: [NafathLoginComponent, FormsModule],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(NafathLoginComponent, {
        set: {
          providers: [{ provide: NafathService, useValue: mockNafathService }],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(NafathLoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    component.stopPolling();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should start on input step', () => {
    expect(component.currentStep).toBe('input');
  });

  it('should validate national ID must be 10 digits', () => {
    component.nationalId = '123456789';
    expect(component.isNationalIdValid()).toBeFalse();

    component.nationalId = '12345678901';
    expect(component.isNationalIdValid()).toBeFalse();

    component.nationalId = '1234567890';
    expect(component.isNationalIdValid()).toBeTrue();
  });

  it('should reject non-numeric national ID', () => {
    component.nationalId = '12345abcde';
    expect(component.isNationalIdValid()).toBeFalse();

    component.nationalId = 'abcdefghij';
    expect(component.isNationalIdValid()).toBeFalse();

    component.nationalId = '12345 6789';
    expect(component.isNationalIdValid()).toBeFalse();
  });

  it('should call initiateLogin on nafathService', () => {
    const mockAuth = createMockNafathAuthRequest({ randomNumber: 55 });
    mockNafathService.initiateLogin.and.returnValue(of(mockAuth));

    component.nationalId = '1234567890';
    component.initiateLogin();

    expect(mockNafathService.initiateLogin).toHaveBeenCalledWith('1234567890');
  });

  it('should transition to waiting step after successful initiation', () => {
    const mockAuth = createMockNafathAuthRequest({ randomNumber: 42 });
    mockNafathService.initiateLogin.and.returnValue(of(mockAuth));

    component.nationalId = '1234567890';
    component.initiateLogin();

    expect(component.currentStep).toBe('waiting');
    expect(component.isInitiating).toBeFalse();
  });

  it('should display random number in waiting step', () => {
    const mockAuth = createMockNafathAuthRequest({ randomNumber: 77 });
    mockNafathService.initiateLogin.and.returnValue(of(mockAuth));

    component.nationalId = '1234567890';
    component.initiateLogin();

    expect(component.randomNumber).toBe('77');
    expect(component.currentStep).toBe('waiting');
  });

  it('should transition to success on completed status', fakeAsync(() => {
    const mockAuth = createMockNafathAuthRequest({ randomNumber: 42 });
    mockNafathService.initiateLogin.and.returnValue(of(mockAuth));

    component.nationalId = '1234567890';
    component.initiateLogin();

    const completedAuth = createMockNafathAuthRequest({
      status: NafathRequestStatus.Completed,
      randomNumber: 42,
    });
    mockNafathService.checkStatus.and.returnValue(of(completedAuth));

    tick(3000);

    expect(component.currentStep).toBe('success');
    discardPeriodicTasks();
  }));

  it('should transition to failure on rejected status', fakeAsync(() => {
    const mockAuth = createMockNafathAuthRequest({ randomNumber: 42 });
    mockNafathService.initiateLogin.and.returnValue(of(mockAuth));

    component.nationalId = '1234567890';
    component.initiateLogin();

    const rejectedAuth = createMockNafathAuthRequest({
      status: NafathRequestStatus.Rejected,
      randomNumber: 42,
    });
    mockNafathService.checkStatus.and.returnValue(of(rejectedAuth));

    tick(3000);

    expect(component.currentStep).toBe('failure');
    discardPeriodicTasks();
  }));

  it('should reset all state on reset()', () => {
    component.currentStep = 'failure';
    component.nationalId = '1234567890';
    component.randomNumber = '42';
    component.pollCounter = 5;
    component.validationError = 'some error';
    component.failureReason = 'some reason';

    component.reset();

    expect(component.currentStep).toBe('input');
    expect(component.nationalId).toBe('');
    expect(component.randomNumber).toBe('');
    expect(component.pollCounter).toBe(0);
    expect(component.validationError).toBe('');
    expect(component.failureReason).toBe('');
    expect(component.authRequest).toBeNull();
  });

  it('should stop polling on destroy', fakeAsync(() => {
    const mockAuth = createMockNafathAuthRequest({ randomNumber: 42 });
    mockNafathService.initiateLogin.and.returnValue(of(mockAuth));

    component.nationalId = '1234567890';
    component.initiateLogin();

    spyOn(component, 'stopPolling').and.callThrough();
    component.ngOnDestroy();

    expect(component.stopPolling).toHaveBeenCalled();
    discardPeriodicTasks();
  }));
});
