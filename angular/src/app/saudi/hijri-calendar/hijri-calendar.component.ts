import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HijriDatePipe } from '../shared/hijri-date.pipe';
import { DualCalendarPickerComponent, DualDate } from '../shared/dual-calendar-picker.component';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import { HijriCalendarService, HijriDateDto } from '../shared/services/hijri-calendar.service';

@Component({
  selector: 'app-hijri-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule, HijriDatePipe, DualCalendarPickerComponent, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <div class="row">
          <div class="col col-md-6">
            <h5 class="card-title">{{ '::Saudi:HijriCalendar:Today' | abpLocalization }}</h5>
          </div>
        </div>
      </div>
      <div class="card-body">
        <!-- Loading -->
        <div *ngIf="loadingToday" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <div class="row mb-4" *ngIf="!loadingToday && todayHijri">
          <div class="col-md-6">
            <div class="card bg-light">
              <div class="card-body text-center">
                <h3 class="mb-1 hijri-today">{{ todayHijri.formatted }}</h3>
                <p class="text-muted mb-0">{{ todayHijri.gregorianDate | date : 'fullDate' }}</p>
              </div>
            </div>
          </div>
        </div>

        <div class="row mb-4" *ngIf="!loadingToday && !todayHijri">
          <div class="col-md-6">
            <div class="card bg-light">
              <div class="card-body text-center">
                <h3 class="mb-1 hijri-today">{{ today | hijriDate : 'full' : 'ar' }}</h3>
                <p class="text-muted mb-0">{{ today | date : 'fullDate' }}</p>
              </div>
            </div>
          </div>
        </div>

        <h5 class="mb-3">{{ '::Saudi:HijriCalendar:Convert' | abpLocalization }}</h5>
        <app-dual-calendar-picker
          [selectedDate]="convertDate"
          [gregorianLabel]="'::Saudi:HijriCalendar:GregorianDate' | abpLocalization"
          [hijriLabel]="'::Saudi:HijriCalendar:HijriDate' | abpLocalization"
          (dateChange)="onDateChange($event)"
        />

        <div class="mt-3" *ngIf="convertedResult">
          <div class="alert alert-info">
            <strong>{{ '::Saudi:HijriCalendar:HijriDate' | abpLocalization }}:</strong>
            {{ convertedResult }}
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .hijri-today {
        font-family: 'Noto Sans Arabic', 'Segoe UI', Tahoma, sans-serif;
        font-size: 1.5rem;
      }
    `,
  ],
})
export class HijriCalendarComponent implements OnInit {
  today = new Date();
  todayHijri: HijriDateDto | null = null;
  loadingToday = false;
  convertDate: Date | null = null;
  convertedResult: string | null = null;

  constructor(private hijriCalendarService: HijriCalendarService) {}

  ngOnInit() {
    this.loadToday();
  }

  loadToday() {
    this.loadingToday = true;
    this.hijriCalendarService
      .getToday()
      .pipe(finalize(() => (this.loadingToday = false)))
      .subscribe({
        next: result => {
          this.todayHijri = result;
        },
        error: () => {
          // Fallback to client-side Intl API via the pipe
          this.todayHijri = null;
        },
      });
  }

  onDateChange(dual: DualDate) {
    this.convertDate = dual.gregorian;

    if (dual.gregorian) {
      const dateStr = dual.gregorian.toISOString().split('T')[0];
      this.hijriCalendarService.convertToHijri(dateStr).subscribe({
        next: result => {
          this.convertedResult = result.formatted || dual.hijriFormatted;
        },
        error: () => {
          // Fallback to client-side conversion
          this.convertedResult = dual.hijriFormatted;
        },
      });
    } else {
      this.convertedResult = dual.hijriFormatted;
    }
  }
}
