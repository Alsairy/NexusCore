import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationModule } from '@abp/ng.core';
import {
  NafathService,
  NafathAuthRequestDto,
  NafathRequestStatus,
} from '../services/nafath.service';

type LoginStep = 'input' | 'waiting' | 'success' | 'failure';

@Component({
  selector: 'app-nafath-login',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="card-title">{{ '::Saudi:Nafath:LoginTitle' | abpLocalization }}</h5>
      </div>
      <div class="card-body">
        <!-- Step 1: Input National ID -->
        <div *ngIf="currentStep === 'input'" class="step-input">
          <div class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            {{ '::Saudi:Nafath:LoginDescription' | abpLocalization }}
          </div>

          <div class="mb-3">
            <label for="nationalId" class="form-label">
              {{ '::Saudi:Nafath:NationalId' | abpLocalization }}
            </label>
            <input
              type="text"
              id="nationalId"
              class="form-control"
              [(ngModel)]="nationalId"
              [placeholder]="'::Saudi:Nafath:NationalIdPlaceholder' | abpLocalization"
              maxlength="10"
              pattern="[0-9]*"
              (input)="onNationalIdInput($event)"
            />
            <div class="form-text">
              {{ '::Saudi:Nafath:NationalIdHint' | abpLocalization }}
            </div>
            <div *ngIf="validationError" class="text-danger mt-2">
              {{ validationError }}
            </div>
          </div>

          <button
            type="button"
            class="btn btn-primary"
            [disabled]="!isNationalIdValid() || isInitiating"
            (click)="initiateLogin()"
          >
            <i class="bi bi-shield-check me-2"></i>
            {{ '::Saudi:Nafath:LoginButton' | abpLocalization }}
            <span *ngIf="isInitiating" class="spinner-border spinner-border-sm ms-2"></span>
          </button>
        </div>

        <!-- Step 2: Waiting for Approval -->
        <div *ngIf="currentStep === 'waiting'" class="step-waiting text-center">
          <div class="alert alert-warning">
            <i class="bi bi-exclamation-triangle me-2"></i>
            {{ '::Saudi:Nafath:WaitingMessage' | abpLocalization }}
          </div>

          <div class="random-number-display mb-4">
            <div class="random-number-label mb-2">
              {{ '::Saudi:Nafath:RandomNumberLabel' | abpLocalization }}
            </div>
            <div class="random-number">{{ randomNumber }}</div>
          </div>

          <div class="instruction-text mb-4">
            <p class="lead">{{ '::Saudi:Nafath:OpenAppInstruction' | abpLocalization }}</p>
            <p class="text-muted">{{ '::Saudi:Nafath:SelectNumberInstruction' | abpLocalization }}</p>
          </div>

          <div class="spinner-container mb-4">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-3 text-muted">
              <i class="bi bi-clock me-2"></i>
              {{ '::Saudi:Nafath:WaitingForApproval' | abpLocalization }}
            </p>
          </div>

          <div class="poll-counter text-muted small">
            {{ '::Saudi:Nafath:CheckingStatus' | abpLocalization }}: {{ pollCounter }}
          </div>

          <button type="button" class="btn btn-secondary mt-3" (click)="cancelLogin()">
            <i class="bi bi-x-circle me-2"></i>
            {{ '::Saudi:Nafath:CancelButton' | abpLocalization }}
          </button>
        </div>

        <!-- Step 3: Success -->
        <div *ngIf="currentStep === 'success'" class="step-success text-center">
          <div class="alert alert-success">
            <i class="bi bi-check-circle me-2"></i>
            {{ '::Saudi:Nafath:LoginSuccess' | abpLocalization }}
          </div>

          <div class="success-icon mb-3">
            <i class="bi bi-check-circle-fill text-success" style="font-size: 4rem;"></i>
          </div>

          <h4 class="mb-3">{{ '::Saudi:Nafath:WelcomeMessage' | abpLocalization }}</h4>
          <p class="text-muted">{{ '::Saudi:Nafath:LoginSuccessDetails' | abpLocalization }}</p>

          <div class="user-info mt-4 p-3 bg-light rounded">
            <p class="mb-1">
              <strong>{{ '::Saudi:Nafath:NationalId' | abpLocalization }}:</strong> {{ nationalId }}
            </p>
            <p class="mb-0" *ngIf="authRequest?.completedAt">
              <strong>{{ '::Saudi:Nafath:LoginTime' | abpLocalization }}:</strong>
              {{ authRequest!.completedAt | date : 'medium' }}
            </p>
          </div>

          <button type="button" class="btn btn-primary mt-4" (click)="reset()">
            <i class="bi bi-arrow-left me-2"></i>
            {{ '::Saudi:Nafath:BackButton' | abpLocalization }}
          </button>
        </div>

        <!-- Step 4: Failure -->
        <div *ngIf="currentStep === 'failure'" class="step-failure text-center">
          <div class="alert alert-danger">
            <i class="bi bi-x-circle me-2"></i>
            {{ '::Saudi:Nafath:LoginFailure' | abpLocalization }}
          </div>

          <div class="failure-icon mb-3">
            <i class="bi bi-x-circle-fill text-danger" style="font-size: 4rem;"></i>
          </div>

          <h4 class="mb-3">{{ '::Saudi:Nafath:AuthenticationFailed' | abpLocalization }}</h4>
          <p class="text-muted">{{ failureReason }}</p>

          <div class="mt-4">
            <button type="button" class="btn btn-primary me-2" (click)="reset()">
              <i class="bi bi-arrow-clockwise me-2"></i>
              {{ '::Saudi:Nafath:TryAgainButton' | abpLocalization }}
            </button>
            <button type="button" class="btn btn-secondary" (click)="reset()">
              <i class="bi bi-arrow-left me-2"></i>
              {{ '::Saudi:Nafath:BackButton' | abpLocalization }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .random-number-display {
        padding: 2rem;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        border-radius: 1rem;
        box-shadow: 0 10px 30px rgba(102, 126, 234, 0.4);
      }

      .random-number-label {
        color: rgba(255, 255, 255, 0.9);
        font-size: 1rem;
        font-weight: 500;
        text-transform: uppercase;
        letter-spacing: 0.05em;
      }

      .random-number {
        color: #ffffff;
        font-size: 4rem;
        font-weight: bold;
        letter-spacing: 0.5rem;
        text-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
        font-family: 'Courier New', monospace;
      }

      .instruction-text {
        max-width: 600px;
        margin: 0 auto;
      }

      .spinner-container {
        padding: 2rem 0;
      }

      .poll-counter {
        font-size: 0.875rem;
        margin-top: 1rem;
      }

      .success-icon,
      .failure-icon {
        margin: 2rem 0;
      }

      .user-info {
        max-width: 500px;
        margin: 0 auto;
        text-align: left;
      }

      .step-input,
      .step-waiting,
      .step-success,
      .step-failure {
        min-height: 300px;
      }
    `,
  ],
})
export class NafathLoginComponent implements OnInit, OnDestroy {
  currentStep: LoginStep = 'input';
  nationalId = '';
  randomNumber = '';
  pollCounter = 0;
  validationError = '';
  failureReason = '';
  isInitiating = false;
  authRequest: NafathAuthRequestDto | null = null;

  private pollInterval: any = null;
  private maxPolls = 20;

  constructor(private nafathService: NafathService) {}

  ngOnInit() {}

  ngOnDestroy() {
    this.stopPolling();
  }

  onNationalIdInput(event: any) {
    const value = event.target.value;
    this.nationalId = value.replace(/[^0-9]/g, '');
    this.validationError = '';
  }

  isNationalIdValid(): boolean {
    return this.nationalId.length === 10 && /^[0-9]{10}$/.test(this.nationalId);
  }

  initiateLogin() {
    if (!this.isNationalIdValid()) {
      this.validationError = 'National ID must be exactly 10 digits';
      return;
    }

    this.isInitiating = true;
    this.nafathService.initiateLogin(this.nationalId).subscribe({
      next: result => {
        this.isInitiating = false;
        this.authRequest = result;
        this.randomNumber = result.randomNumber.toString();
        this.currentStep = 'waiting';
        this.pollCounter = 0;
        this.startPolling();
      },
      error: () => {
        this.isInitiating = false;
        this.failureReason = 'Failed to initiate Nafath authentication.';
        this.currentStep = 'failure';
      },
    });
  }

  startPolling() {
    this.pollInterval = setInterval(() => {
      this.pollCounter++;

      if (this.pollCounter >= this.maxPolls) {
        this.stopPolling();
        this.currentStep = 'failure';
        this.failureReason = 'The authentication request timed out.';
        return;
      }

      if (!this.authRequest) return;

      this.nafathService.checkStatus(this.authRequest.transactionId).subscribe({
        next: result => {
          this.authRequest = result;
          if (result.status === NafathRequestStatus.Completed) {
            this.stopPolling();
            this.currentStep = 'success';
          } else if (
            result.status === NafathRequestStatus.Rejected ||
            result.status === NafathRequestStatus.Expired ||
            result.status === NafathRequestStatus.Failed
          ) {
            this.stopPolling();
            this.currentStep = 'failure';
            this.failureReason = 'The authentication request was rejected or expired.';
          }
        },
        error: () => {
          // Continue polling on transient errors
        },
      });
    }, 3000);
  }

  stopPolling() {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
      this.pollInterval = null;
    }
  }

  cancelLogin() {
    this.stopPolling();
    this.reset();
  }

  reset() {
    this.currentStep = 'input';
    this.nationalId = '';
    this.randomNumber = '';
    this.pollCounter = 0;
    this.validationError = '';
    this.failureReason = '';
    this.authRequest = null;
  }
}
