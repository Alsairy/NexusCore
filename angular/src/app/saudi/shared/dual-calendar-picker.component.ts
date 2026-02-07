import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HijriDatePipe } from './hijri-date.pipe';

export interface DualDate {
  gregorian: Date;
  hijriFormatted: string;
}

@Component({
  selector: 'app-dual-calendar-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, HijriDatePipe],
  template: `
    <div class="dual-calendar-picker">
      <div class="row">
        <div class="col-md-6">
          <label class="form-label">{{ gregorianLabel }}</label>
          <input
            type="date"
            class="form-control"
            [ngModel]="gregorianValue"
            (ngModelChange)="onGregorianChange($event)"
            [disabled]="disabled"
          />
        </div>
        <div class="col-md-6">
          <label class="form-label">{{ hijriLabel }}</label>
          <div class="form-control bg-light hijri-display" [class.text-muted]="!selectedDate">
            {{ selectedDate | hijriDate : 'full' : locale }}
            <span *ngIf="!selectedDate" class="text-muted">—</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .hijri-display {
        min-height: 38px;
        display: flex;
        align-items: center;
      }
    `,
  ],
})
export class DualCalendarPickerComponent implements OnInit {
  @Input() selectedDate: Date | null = null;
  @Input() gregorianLabel = 'Gregorian Date';
  @Input() hijriLabel = 'Hijri Date';
  @Input() locale = 'ar';
  @Input() disabled = false;

  @Output() dateChange = new EventEmitter<DualDate>();

  gregorianValue = '';

  ngOnInit() {
    if (this.selectedDate) {
      this.gregorianValue = this.formatDateForInput(this.selectedDate);
    }
  }

  onGregorianChange(value: string) {
    this.gregorianValue = value;
    if (value) {
      this.selectedDate = new Date(value + 'T00:00:00');
      const hijriFormatted = new Intl.DateTimeFormat('ar-SA-u-ca-islamic-umalqura', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      }).format(this.selectedDate);

      this.dateChange.emit({
        gregorian: this.selectedDate,
        hijriFormatted,
      });
    } else {
      this.selectedDate = null;
    }
  }

  private formatDateForInput(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
