import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationModule } from '@abp/ng.core';

@Component({
  selector: 'app-invoice-stats-card',
  standalone: true,
  imports: [CommonModule, LocalizationModule],
  template: `
    <div class="card h-100 border-0 shadow-sm">
      <div class="card-body">
        <div class="d-flex align-items-center">
          <div
            class="rounded-circle d-flex align-items-center justify-content-center me-3"
            [style.width.px]="48"
            [style.height.px]="48"
            [style.background-color]="bgColor"
          >
            <i [class]="'bi ' + icon + ' text-white'" style="font-size: 1.3rem;"></i>
          </div>
          <div>
            <div class="text-muted small">{{ label }}</div>
            <div class="h4 mb-0 fw-bold">{{ formattedValue }}</div>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class InvoiceStatsCardComponent {
  @Input() label = '';
  @Input() value: number = 0;
  @Input() icon = 'bi-receipt';
  @Input() bgColor = '#0d6efd';
  @Input() isCurrency = false;

  get formattedValue(): string {
    if (this.isCurrency) {
      return new Intl.NumberFormat('en-SA', {
        style: 'currency',
        currency: 'SAR',
        minimumFractionDigits: 2,
      }).format(this.value);
    }
    return new Intl.NumberFormat('en-SA').format(this.value);
  }
}
