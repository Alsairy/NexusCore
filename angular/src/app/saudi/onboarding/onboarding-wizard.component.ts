import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import { OnboardingService, OnboardingStatusDto, OnboardingStep } from './services/onboarding.service';

interface WizardStep {
  step: OnboardingStep;
  titleKey: string;
  descriptionKey: string;
  icon: string;
  routerLink: string[];
  completed: boolean;
  optional: boolean;
}

@Component({
  selector: 'app-onboarding-wizard',
  standalone: true,
  imports: [CommonModule, RouterModule, LocalizationModule],
  template: `
    <!-- Loading -->
    <div *ngIf="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div *ngIf="!loading && status">
      <!-- Completed State -->
      <div *ngIf="status.isComplete" class="card border-success">
        <div class="card-body text-center py-5">
          <div class="mb-4">
            <i class="bi bi-check-circle-fill text-success" style="font-size: 4rem;"></i>
          </div>
          <h3 class="text-success">{{ '::Saudi:Onboarding:Complete:Title' | abpLocalization }}</h3>
          <p class="text-muted mb-4">{{ '::Saudi:Onboarding:Complete:Description' | abpLocalization }}</p>
          <a routerLink="/saudi/zatca/invoices" class="btn btn-primary me-2">
            <i class="bi bi-receipt me-1"></i>
            {{ '::Saudi:Onboarding:Complete:GoToDashboard' | abpLocalization }}
          </a>
          <button type="button" class="btn btn-outline-secondary" (click)="resetOnboarding()">
            <i class="bi bi-arrow-counterclockwise me-1"></i>
            {{ '::Saudi:Onboarding:Reset' | abpLocalization }}
          </button>
        </div>
      </div>

      <!-- Wizard State -->
      <div *ngIf="!status.isComplete">
        <div class="card">
          <div class="card-header">
            <div class="row align-items-center">
              <div class="col">
                <h5 class="card-title mb-0">{{ '::Saudi:Onboarding:Title' | abpLocalization }}</h5>
                <small class="text-muted">{{ '::Saudi:Onboarding:Subtitle' | abpLocalization }}</small>
              </div>
              <div class="col-auto">
                <button
                  type="button"
                  class="btn btn-sm btn-outline-danger"
                  (click)="resetOnboarding()"
                  *ngIf="status.completedSteps > 0"
                >
                  <i class="bi bi-arrow-counterclockwise me-1"></i>
                  {{ '::Saudi:Onboarding:Reset' | abpLocalization }}
                </button>
              </div>
            </div>
          </div>

          <div class="card-body">
            <!-- Progress Bar -->
            <div class="mb-4">
              <div class="d-flex justify-content-between mb-1">
                <small class="text-muted">{{ '::Saudi:Onboarding:Progress' | abpLocalization }}</small>
                <small class="text-muted">{{ status.completedSteps }} / {{ status.totalSteps }}</small>
              </div>
              <div class="progress" style="height: 8px;">
                <div
                  class="progress-bar bg-success"
                  role="progressbar"
                  [style.width.%]="(status.completedSteps / status.totalSteps) * 100"
                ></div>
              </div>
            </div>

            <!-- Steps -->
            <div class="list-group">
              <div
                *ngFor="let wizardStep of wizardSteps; let i = index"
                class="list-group-item list-group-item-action"
                [class.active]="activeStepIndex === i && !wizardStep.completed"
                [class.list-group-item-success]="wizardStep.completed"
                (click)="setActiveStep(i)"
                style="cursor: pointer;"
              >
                <div class="d-flex align-items-center">
                  <!-- Step Number / Check -->
                  <div class="me-3">
                    <div
                      class="rounded-circle d-flex align-items-center justify-content-center"
                      [style.width.px]="40"
                      [style.height.px]="40"
                      [class.bg-success]="wizardStep.completed"
                      [class.bg-primary]="activeStepIndex === i && !wizardStep.completed"
                      [class.bg-light]="activeStepIndex !== i && !wizardStep.completed"
                      [class.text-white]="wizardStep.completed || activeStepIndex === i"
                    >
                      <i *ngIf="wizardStep.completed" class="bi bi-check-lg"></i>
                      <i *ngIf="!wizardStep.completed" [class]="'bi ' + wizardStep.icon"></i>
                    </div>
                  </div>

                  <!-- Step Content -->
                  <div class="flex-grow-1">
                    <div class="d-flex justify-content-between align-items-center">
                      <h6 class="mb-0">
                        {{ wizardStep.titleKey | abpLocalization }}
                        <span *ngIf="wizardStep.optional" class="badge bg-secondary ms-2" style="font-size: 0.7em;">
                          {{ '::Saudi:Onboarding:Skip' | abpLocalization }}
                        </span>
                      </h6>
                      <span *ngIf="wizardStep.completed" class="badge bg-success">
                        <i class="bi bi-check me-1"></i>
                      </span>
                    </div>
                    <small [class.text-white-50]="activeStepIndex === i && !wizardStep.completed" [class.text-muted]="activeStepIndex !== i || wizardStep.completed">
                      {{ wizardStep.descriptionKey | abpLocalization }}
                    </small>
                  </div>
                </div>

                <!-- Expanded Step Actions -->
                <div *ngIf="activeStepIndex === i && !wizardStep.completed" class="mt-3 ms-5">
                  <a [routerLink]="wizardStep.routerLink" class="btn btn-sm btn-light me-2">
                    <i class="bi bi-box-arrow-up-right me-1"></i>
                    {{ wizardStep.titleKey | abpLocalization }}
                  </a>
                  <button
                    type="button"
                    class="btn btn-sm btn-outline-light me-2"
                    [disabled]="isCompleting"
                    (click)="completeStep(wizardStep, $event)"
                  >
                    <i class="bi bi-check-circle me-1"></i>
                    {{ '::Saudi:Onboarding:MarkComplete' | abpLocalization }}
                    <span *ngIf="isCompleting" class="spinner-border spinner-border-sm ms-1"></span>
                  </button>
                  <button
                    *ngIf="wizardStep.optional"
                    type="button"
                    class="btn btn-sm btn-outline-light"
                    (click)="skipStep(i, $event)"
                  >
                    {{ '::Saudi:Onboarding:Skip' | abpLocalization }}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .list-group-item.active {
        border-color: var(--bs-primary);
      }
      .progress {
        border-radius: 4px;
      }
    `,
  ],
})
export class OnboardingWizardComponent implements OnInit {
  status: OnboardingStatusDto | null = null;
  loading = true;
  isCompleting = false;
  activeStepIndex = 0;

  wizardSteps: WizardStep[] = [
    {
      step: OnboardingStep.ZatcaConfigured,
      titleKey: '::Saudi:Onboarding:Step1:Title',
      descriptionKey: '::Saudi:Onboarding:Step1:Description',
      icon: 'bi-gear',
      routerLink: ['/saudi/settings'],
      completed: false,
      optional: false,
    },
    {
      step: OnboardingStep.SellerCreated,
      titleKey: '::Saudi:Onboarding:Step2:Title',
      descriptionKey: '::Saudi:Onboarding:Step2:Description',
      icon: 'bi-building',
      routerLink: ['/saudi/zatca/sellers'],
      completed: false,
      optional: false,
    },
    {
      step: OnboardingStep.CertificateUploaded,
      titleKey: '::Saudi:Onboarding:Step3:Title',
      descriptionKey: '::Saudi:Onboarding:Step3:Description',
      icon: 'bi-file-earmark-lock',
      routerLink: ['/saudi/settings/certificates'],
      completed: false,
      optional: false,
    },
    {
      step: OnboardingStep.FirstInvoiceSubmitted,
      titleKey: '::Saudi:Onboarding:Step4:Title',
      descriptionKey: '::Saudi:Onboarding:Step4:Description',
      icon: 'bi-receipt',
      routerLink: ['/saudi/zatca/invoices/create'],
      completed: false,
      optional: false,
    },
    {
      step: OnboardingStep.NafathConfigured,
      titleKey: '::Saudi:Onboarding:Step5:Title',
      descriptionKey: '::Saudi:Onboarding:Step5:Description',
      icon: 'bi-shield-check',
      routerLink: ['/saudi/settings'],
      completed: false,
      optional: true,
    },
  ];

  constructor(private onboardingService: OnboardingService) {}

  ngOnInit() {
    this.loadStatus();
  }

  loadStatus() {
    this.loading = true;
    this.onboardingService
      .getStatus()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe(status => {
        this.status = status;
        this.syncStepStates(status);
        this.setFirstIncompleteStep();
      });
  }

  setActiveStep(index: number) {
    this.activeStepIndex = index;
  }

  completeStep(wizardStep: WizardStep, event: Event) {
    event.stopPropagation();
    this.isCompleting = true;
    this.onboardingService
      .completeStep(wizardStep.step)
      .pipe(finalize(() => (this.isCompleting = false)))
      .subscribe(status => {
        this.status = status;
        this.syncStepStates(status);
        this.setFirstIncompleteStep();
      });
  }

  skipStep(index: number, event: Event) {
    event.stopPropagation();
    // Move to next step without completing
    if (index < this.wizardSteps.length - 1) {
      this.activeStepIndex = index + 1;
    }
  }

  resetOnboarding() {
    this.onboardingService.reset().subscribe(() => {
      this.loadStatus();
    });
  }

  private syncStepStates(status: OnboardingStatusDto) {
    this.wizardSteps[0].completed = status.zatcaConfigured;
    this.wizardSteps[1].completed = status.sellerCreated;
    this.wizardSteps[2].completed = status.certificateUploaded;
    this.wizardSteps[3].completed = status.firstInvoiceSubmitted;
    this.wizardSteps[4].completed = status.nafathConfigured;
  }

  private setFirstIncompleteStep() {
    const index = this.wizardSteps.findIndex(s => !s.completed);
    this.activeStepIndex = index >= 0 ? index : 0;
  }
}
