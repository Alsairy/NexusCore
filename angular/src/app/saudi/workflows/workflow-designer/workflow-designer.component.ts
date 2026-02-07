import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationModule } from '@abp/ng.core';

@Component({
  selector: 'app-workflow-designer',
  standalone: true,
  imports: [CommonModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="card-title">{{ '::Saudi:Workflows:WorkflowDesigner' | abpLocalization }}</h5>
      </div>
      <div class="card-body">
        <!-- Info Alert -->
        <div class="alert alert-info mb-4">
          <div class="d-flex align-items-start">
            <i class="bi bi-info-circle me-3" style="font-size: 1.5rem;"></i>
            <div>
              <h6 class="alert-heading">
                {{ '::Saudi:Workflows:ElsaStudioIntegration' | abpLocalization }}
              </h6>
              <p class="mb-0">
                {{ '::Saudi:Workflows:ElsaStudioDescription' | abpLocalization }}
              </p>
            </div>
          </div>
        </div>

        <!-- Designer Embed Area -->
        <div class="designer-container">
          <!-- Option 1: iframe embed (when Elsa Studio is ready) -->
          <div *ngIf="showIframe; else comingSoon" class="iframe-wrapper">
            <iframe
              [src]="elsaStudioUrl"
              class="elsa-iframe"
              frameborder="0"
              allowfullscreen
            ></iframe>
          </div>

          <!-- Option 2: Coming Soon placeholder -->
          <ng-template #comingSoon>
            <div class="coming-soon-card text-center">
              <div class="icon-container mb-4">
                <i class="bi bi-diagram-3 text-primary" style="font-size: 5rem;"></i>
              </div>
              <h3 class="mb-3">
                {{ '::Saudi:Workflows:DesignerComingSoon' | abpLocalization }}
              </h3>
              <p class="lead text-muted mb-4">
                {{ '::Saudi:Workflows:VisualDesignerNote' | abpLocalization }}
              </p>

              <!-- Features List -->
              <div class="features-list text-start mx-auto" style="max-width: 600px;">
                <h5 class="mb-3">
                  {{ '::Saudi:Workflows:PlannedFeatures' | abpLocalization }}
                </h5>
                <ul class="list-group list-group-flush">
                  <li class="list-group-item">
                    <i class="bi bi-check-circle-fill text-success me-2"></i>
                    {{ '::Saudi:Workflows:Feature1' | abpLocalization }}
                  </li>
                  <li class="list-group-item">
                    <i class="bi bi-check-circle-fill text-success me-2"></i>
                    {{ '::Saudi:Workflows:Feature2' | abpLocalization }}
                  </li>
                  <li class="list-group-item">
                    <i class="bi bi-check-circle-fill text-success me-2"></i>
                    {{ '::Saudi:Workflows:Feature3' | abpLocalization }}
                  </li>
                  <li class="list-group-item">
                    <i class="bi bi-check-circle-fill text-success me-2"></i>
                    {{ '::Saudi:Workflows:Feature4' | abpLocalization }}
                  </li>
                  <li class="list-group-item">
                    <i class="bi bi-check-circle-fill text-success me-2"></i>
                    {{ '::Saudi:Workflows:Feature5' | abpLocalization }}
                  </li>
                </ul>
              </div>

              <!-- Integration Note -->
              <div class="alert alert-secondary mt-4 mx-auto" style="max-width: 600px;">
                <p class="mb-0">
                  <i class="bi bi-lightbulb me-2"></i>
                  <strong>{{ '::Saudi:Workflows:IntegrationNote' | abpLocalization }}:</strong>
                  {{ '::Saudi:Workflows:IntegrationDetails' | abpLocalization }}
                </p>
              </div>

              <!-- Button to toggle iframe (for testing when Elsa is deployed) -->
              <button
                type="button"
                class="btn btn-outline-primary mt-3"
                (click)="toggleIframe()"
              >
                <i class="bi bi-box-arrow-up-right me-2"></i>
                {{ '::Saudi:Workflows:TestElsaStudio' | abpLocalization }}
              </button>
            </div>
          </ng-template>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .designer-container {
        min-height: 600px;
        background: #f8f9fa;
        border-radius: 0.5rem;
        padding: 2rem;
      }

      .iframe-wrapper {
        width: 100%;
        height: 600px;
        position: relative;
      }

      .elsa-iframe {
        width: 100%;
        height: 100%;
        border: 1px solid #dee2e6;
        border-radius: 0.5rem;
        background: white;
      }

      .coming-soon-card {
        padding: 3rem 1rem;
      }

      .icon-container {
        margin-bottom: 2rem;
      }

      .features-list {
        text-align: left;
      }

      .list-group-item {
        border-left: none;
        border-right: none;
        padding-left: 0;
        padding-right: 0;
      }

      .list-group-item:first-child {
        border-top: none;
      }

      .list-group-item:last-child {
        border-bottom: none;
      }

      .alert-secondary {
        background-color: #f8f9fa;
        border-color: #dee2e6;
      }
    `,
  ],
})
export class WorkflowDesignerComponent {
  showIframe = false;
  elsaStudioUrl = '/elsa'; // URL where Elsa Studio will be hosted

  toggleIframe() {
    this.showIframe = !this.showIframe;
  }
}
