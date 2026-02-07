import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { HijriCalendarComponent } from './hijri-calendar.component';
import { HijriCalendarService } from '../shared/services/hijri-calendar.service';
import { createMockHijriCalendarService, setupDefaultMockReturns } from '../testing/mock-services';
import { createMockHijriDate } from '../testing/saudi-test-helpers';
import { of, throwError } from 'rxjs';

describe('HijriCalendarComponent', () => {
  let component: HijriCalendarComponent;
  let fixture: ComponentFixture<HijriCalendarComponent>;
  let mockHijriCalendarService: jasmine.SpyObj<HijriCalendarService>;

  beforeEach(async () => {
    mockHijriCalendarService = createMockHijriCalendarService() as jasmine.SpyObj<HijriCalendarService>;
    setupDefaultMockReturns({ hijriCalendarService: mockHijriCalendarService });

    await TestBed.configureTestingModule({
      imports: [HijriCalendarComponent],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(HijriCalendarComponent, {
        set: {
          providers: [{ provide: HijriCalendarService, useValue: mockHijriCalendarService }],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(HijriCalendarComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load today\'s Hijri date on init', () => {
    fixture.detectChanges();
    expect(mockHijriCalendarService.getToday).toHaveBeenCalledTimes(1);
  });

  it('should set todayHijri from service response', () => {
    const mockDate = createMockHijriDate({ day: 20, formatted: '20 جمادى الآخرة 1446' });
    mockHijriCalendarService.getToday.and.returnValue(of(mockDate));

    fixture.detectChanges();

    expect(component.todayHijri).toEqual(mockDate);
    expect(component.loadingToday).toBeFalse();
  });

  it('should fall back to null on error', () => {
    mockHijriCalendarService.getToday.and.returnValue(throwError(() => new Error('API error')));

    fixture.detectChanges();

    expect(component.todayHijri).toBeNull();
    expect(component.loadingToday).toBeFalse();
  });

  it('should convert date when onDateChange is called', () => {
    const mockConverted = createMockHijriDate({ formatted: '1 رجب 1446' });
    mockHijriCalendarService.convertToHijri.and.returnValue(of(mockConverted));

    fixture.detectChanges();

    const testDate = new Date('2024-12-30');
    const dualDate = { gregorian: testDate, hijriFormatted: 'fallback text' };

    component.onDateChange(dualDate);

    expect(mockHijriCalendarService.convertToHijri).toHaveBeenCalledWith('2024-12-30');
    expect(component.convertedResult).toBe('1 رجب 1446');
  });

  it('should use client-side fallback when API convert fails', () => {
    mockHijriCalendarService.convertToHijri.and.returnValue(throwError(() => new Error('API error')));

    fixture.detectChanges();

    const testDate = new Date('2024-12-30');
    const dualDate = { gregorian: testDate, hijriFormatted: 'client-side hijri' };

    component.onDateChange(dualDate);

    expect(component.convertedResult).toBe('client-side hijri');
  });
});
