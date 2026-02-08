import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationModule } from '@abp/ng.core';
import { MonthlyInvoiceStatsDto } from '../services/dashboard.service';

@Component({
  selector: 'app-monthly-chart',
  standalone: true,
  imports: [CommonModule, LocalizationModule],
  template: `
    <div class="card border-0 shadow-sm">
      <div class="card-header bg-transparent border-0">
        <h6 class="card-title mb-0">{{ '::Saudi:Dashboard:MonthlyVolume' | abpLocalization }}</h6>
      </div>
      <div class="card-body">
        <div *ngIf="!stats || stats.length === 0" class="text-center text-muted py-4">
          {{ '::Saudi:Dashboard:NoData' | abpLocalization }}
        </div>

        <div *ngIf="stats && stats.length > 0">
          <!-- SVG Bar Chart -->
          <svg [attr.viewBox]="'0 0 ' + chartWidth + ' ' + chartHeight" class="w-100" style="max-height: 250px;">
            <!-- Bars -->
            <g *ngFor="let bar of bars; let i = index">
              <!-- Bar -->
              <rect
                [attr.x]="bar.x"
                [attr.y]="bar.y"
                [attr.width]="barWidth"
                [attr.height]="bar.height"
                [attr.fill]="bar.revenue > 0 ? '#198754' : '#0d6efd'"
                rx="3"
                ry="3"
              >
                <title>{{ bar.label }}: {{ bar.count }} invoices, {{ bar.revenue | number:'1.2-2' }} SAR</title>
              </rect>

              <!-- Count label on top of bar -->
              <text
                *ngIf="bar.count > 0"
                [attr.x]="bar.x + barWidth / 2"
                [attr.y]="bar.y - 4"
                text-anchor="middle"
                fill="#6c757d"
                font-size="11"
              >
                {{ bar.count }}
              </text>

              <!-- Month label -->
              <text
                [attr.x]="bar.x + barWidth / 2"
                [attr.y]="chartHeight - 4"
                text-anchor="middle"
                fill="#6c757d"
                font-size="11"
              >
                {{ bar.label }}
              </text>
            </g>

            <!-- Baseline -->
            <line
              x1="0"
              [attr.y1]="chartHeight - 20"
              [attr.x1]="chartWidth"
              [attr.y2]="chartHeight - 20"
              stroke="#dee2e6"
              stroke-width="1"
            />
          </svg>

          <!-- Legend -->
          <div class="d-flex justify-content-center gap-4 mt-2">
            <small class="text-muted">
              <span class="badge me-1" style="background-color: #0d6efd;">&nbsp;</span>
              {{ '::Saudi:Dashboard:InvoiceCount' | abpLocalization }}
            </small>
            <small class="text-muted">
              <span class="badge me-1" style="background-color: #198754;">&nbsp;</span>
              {{ '::Saudi:Dashboard:WithRevenue' | abpLocalization }}
            </small>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class MonthlyChartComponent implements OnChanges {
  @Input() stats: MonthlyInvoiceStatsDto[] = [];

  chartWidth = 600;
  chartHeight = 220;
  barWidth = 36;
  bars: { x: number; y: number; height: number; label: string; count: number; revenue: number }[] = [];

  ngOnChanges() {
    this.buildBars();
  }

  private buildBars() {
    if (!this.stats || this.stats.length === 0) {
      this.bars = [];
      return;
    }

    const maxCount = Math.max(...this.stats.map(s => s.invoiceCount), 1);
    const availableHeight = this.chartHeight - 40; // top/bottom padding
    const gap = (this.chartWidth - this.stats.length * this.barWidth) / (this.stats.length + 1);

    this.bars = this.stats.map((s, i) => {
      const barHeight = (s.invoiceCount / maxCount) * availableHeight;
      return {
        x: gap + i * (this.barWidth + gap),
        y: this.chartHeight - 20 - barHeight,
        height: Math.max(barHeight, s.invoiceCount > 0 ? 2 : 0),
        label: s.monthName,
        count: s.invoiceCount,
        revenue: s.revenue,
      };
    });
  }
}
