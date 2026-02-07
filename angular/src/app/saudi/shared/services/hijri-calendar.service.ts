import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface HijriDateDto {
  year: number;
  month: number;
  day: number;
  monthName?: string;
  dayOfWeekName?: string;
  formatted?: string;
  gregorianDate: string;
}

export interface HijriMonthInfoDto {
  year: number;
  month: number;
  monthName?: string;
  daysInMonth: number;
  firstDayGregorian: string;
  lastDayGregorian: string;
}

@Injectable({ providedIn: 'root' })
export class HijriCalendarService {
  private readonly apiUrl = '/api/app/hijri-calendar';

  constructor(private restService: RestService) {}

  getToday(): Observable<HijriDateDto> {
    return this.restService.request<void, HijriDateDto>({
      method: 'GET',
      url: `${this.apiUrl}/today`,
    });
  }

  convertToHijri(gregorianDate: string): Observable<HijriDateDto> {
    return this.restService.request<any, HijriDateDto>({
      method: 'POST',
      url: `${this.apiUrl}/convert-to-hijri`,
      body: { gregorianDate },
    });
  }

  convertToGregorian(year: number, month: number, day: number): Observable<HijriDateDto> {
    return this.restService.request<any, HijriDateDto>({
      method: 'POST',
      url: `${this.apiUrl}/convert-to-gregorian`,
      body: { year, month, day },
    });
  }

  getMonthInfo(year: number, month: number): Observable<HijriMonthInfoDto> {
    return this.restService.request<any, HijriMonthInfoDto>({
      method: 'POST',
      url: `${this.apiUrl}/month-info`,
      body: { year, month },
    });
  }
}
