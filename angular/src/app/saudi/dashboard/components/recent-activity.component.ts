import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LocalizationModule } from '@abp/ng.core';
import { RecentActivityDto } from '../services/dashboard.service';

@Component({
  selector: 'app-recent-activity',
  standalone: true,
  imports: [CommonModule, RouterModule, LocalizationModule],
  template: `
    <div class="card border-0 shadow-sm">
      <div class="card-header bg-transparent border-0">
        <h6 class="card-title mb-0">{{ '::Saudi:Dashboard:RecentActivity' | abpLocalization }}</h6>
      </div>
      <div class="card-body p-0">
        <div
          *ngIf="!activities || activities.length === 0"
          class="text-center text-muted py-4"
        >
          {{ '::Saudi:Dashboard:NoRecentActivity' | abpLocalization }}
        </div>

        <div class="list-group list-group-flush" *ngIf="activities && activities.length > 0">
          <div
            *ngFor="let activity of activities"
            class="list-group-item list-group-item-action d-flex align-items-start px-3 py-2"
          >
            <div class="me-3 mt-1">
              <i
                [class]="getActivityIcon(activity.activityType)"
                [style.color]="getActivityColor(activity.activityType)"
              ></i>
            </div>
            <div class="flex-grow-1">
              <div class="small">{{ activity.description }}</div>
              <div class="text-muted" style="font-size: 0.75rem;">
                {{ formatTimestamp(activity.timestamp) }}
              </div>
            </div>
            <div *ngIf="activity.entityType === 'ZatcaInvoice' && activity.entityId">
              <a
                [routerLink]="['/saudi/zatca/invoices', activity.entityId]"
                class="btn btn-sm btn-outline-primary"
              >
                <i class="bi bi-eye"></i>
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .list-group-item:hover {
        background-color: #f8f9fa;
      }
    `,
  ],
})
export class RecentActivityComponent {
  @Input() activities: RecentActivityDto[] = [];

  getActivityIcon(type: string): string {
    switch (type) {
      case 'Invoice':
        return 'bi bi-receipt';
      case 'Approval':
        return 'bi bi-check2-square';
      case 'Nafath':
        return 'bi bi-shield-check';
      default:
        return 'bi bi-activity';
    }
  }

  getActivityColor(type: string): string {
    switch (type) {
      case 'Invoice':
        return '#0d6efd';
      case 'Approval':
        return '#198754';
      case 'Nafath':
        return '#6f42c1';
      default:
        return '#6c757d';
    }
  }

  formatTimestamp(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
  }
}
