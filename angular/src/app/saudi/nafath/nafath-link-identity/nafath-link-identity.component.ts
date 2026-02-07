import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  NafathService,
  NafathAuthRequestDto,
  NafathUserLinkDto,
  NafathRequestStatus,
} from '../services/nafath.service';

type LinkStep = 'loading' | 'view' | 'input' | 'waiting' | 'success' | 'failure';

@Component({
  selector: 'app-nafath-link-identity',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="card-title">{{ '::Saudi:Nafath:LinkIdentityTitle' | abpLocalization }}</h5>
      </div>
      <div class="card-body">
        <!-- Loading -->
        <div *ngIf="currentStep === 'loading'" class="text-center py-5">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <!-- View: Already Linked -->
        <div *ngIf="currentStep === 'view' && linkedIdentity" class="step-view">
          <div class="alert alert-success">
            <i class="bi bi-check-circle me-2"></i>
            {{ '::Saudi:Nafath:IdentityLinked' | abpLocalization }}
          </div>

          <div class="linked-identity-card p-4 bg-light rounded">
            <div class="row">
              <div class="col-md-6 mb-3">
                <label class="form-label text-muted small">
                  {{ '::Saudi:Nafath:NationalId' | abpLocalization }}
                </label>
                <p class="h5">{{ linkedIdentity.nationalId }}</p>
              </div>
              <div class="col-md-6 mb-3">
                <label class="form-label text-muted small">
                  {{ '::Saudi:Nafath:VerifiedAt' | abpLocalization }}
                </label>
                <p class="h5">{{ linkedIdentity.verifiedAt | date : 'medium' }}</p>
              </div>
              <div class="col-md-6 mb-3">
                <label class="form-label text-muted small">
                  {{ '::Saudi:Nafath:Status' | abpLocalization }}
                </label>
                <p class="h5">
                  <span
                    class="badge"
                    [ngClass]="linkedIdentity.isActive ? 'bg-success' : 'bg-secondary'"
                  >
                    {{
                      linkedIdentity.isActive
                        ? ('::Saudi:Nafath:Active' | abpLocalization)
                        : ('::Saudi:Nafath:Inactive' | abpLocalization)
                    }}
                  </span>
                </p>
              </div>
            </div>
          </div>

          <div class="mt-4">
            <button type="button" class="btn btn-danger" (click)="confirmUnlink()">
              <i class="bi bi-unlink me-2"></i>
              {{ '::Saudi:Nafath:UnlinkButton' | abpLocalization }}
            </button>
          </div>

          <!-- Unlink Confirmation -->
          <div *ngIf="showUnlinkConfirmation" class="alert alert-warning mt-3">
            <p>
              <i class="bi bi-exclamation-triangle me-2"></i>
              {{ '::Saudi:Nafath:UnlinkConfirmation' | abpLocalization }}
            </p>
            <button type="button" class="btn btn-sm btn-danger me-2" (click)="unlinkIdentity()">
              {{ '::Saudi:Nafath:ConfirmUnlink' | abpLocalization }}
            </button>
            <button type="button" class="btn btn-sm btn-secondary" (click)="cancelUnlink()">
              {{ '::Saudi:Nafath:CancelButton' | abpLocalization }}
            </button>
          </div>
        </div>

        <!-- View: Not Linked -->
        <div *ngIf="currentStep === 'view' && !linkedIdentity" class="step-view-empty">
          <div class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            {{ '::Saudi:Nafath:NoLinkedIdentity' | abpLocalization }}
          </div>

          <div class="text-center py-5">
            <i class="bi bi-shield-x text-muted" style="font-size: 4rem;"></i>
            <p class="mt-3 text-muted">
              {{ '::Saudi:Nafath:LinkIdentityDescription' | abpLocalization }}
            </p>
            <button type="button" class="btn btn-primary mt-3" (click)="startLinking()">
              <i class="bi bi-link-45deg me-2"></i>
              {{ '::Saudi:Nafath:StartLinkingButton' | abpLocalization }}
            </button>
          </div>
        </div>

        <!-- Step: Input National ID -->
        <div *ngIf="currentStep === 'input'" class="step-input">
          <div class="alert alert-info">
            <i class="bi bi-info-circle me-2"></i>
            {{ '::Saudi:Nafath:LinkIdentityInstructions' | abpLocalization }}
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

          <div class="d-flex gap-2">
            <button
              type="button"
              class="btn btn-primary"
              [disabled]="!isNationalIdValid() || isLinking"
              (click)="initiateLink()"
            >
              <i class="bi bi-link-45deg me-2"></i>
              {{ '::Saudi:Nafath:LinkButton' | abpLocalization }}
              <span *ngIf="isLinking" class="spinner-border spinner-border-sm ms-2"></span>
            </button>
            <button type="button" class="btn btn-secondary" (click)="cancelLinking()">
              <i class="bi bi-x-circle me-2"></i>
              {{ '::Saudi:Nafath:CancelButton' | abpLocalization }}
            </button>
          </div>
        </div>

        <!-- Step: Waiting for Approval -->
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

          <button type="button" class="btn btn-secondary mt-3" (click)="cancelLinking()">
            <i class="bi bi-x-circle me-2"></i>
            {{ '::Saudi:Nafath:CancelButton' | abpLocalization }}
          </button>
        </div>

        <!-- Step: Success -->
        <div *ngIf="currentStep === 'success'" class="step-success text-center">
          <div class="alert alert-success">
            <i class="bi bi-check-circle me-2"></i>
            {{ '::Saudi:Nafath:LinkSuccess' | abpLocalization }}
          </div>

          <div class="success-icon mb-3">
            <i class="bi bi-check-circle-fill text-success" style="font-size: 4rem;"></i>
          </div>

          <h4 class="mb-3">{{ '::Saudi:Nafath:IdentityLinkedSuccessfully' | abpLocalization }}</h4>
          <p class="text-muted">{{ '::Saudi:Nafath:LinkSuccessDetails' | abpLocalization }}</p>

          <div class="user-info mt-4 p-3 bg-light rounded">
            <p class="mb-1">
              <strong>{{ '::Saudi:Nafath:NationalId' | abpLocalization }}:</strong> {{ nationalId }}
            </p>
          </div>

          <button type="button" class="btn btn-primary mt-4" (click)="viewLinkedIdentity()">
            <i class="bi bi-eye me-2"></i>
            {{ '::Saudi:Nafath:ViewLinkedIdentity' | abpLocalization }}
          </button>
        </div>

        <!-- Step: Failure -->
        <div *ngIf="currentStep === 'failure'" class="step-failure text-center">
          <div class="alert alert-danger">
            <i class="bi bi-x-circle me-2"></i>
            {{ '::Saudi:Nafath:LinkFailure' | abpLocalization }}
          </div>

          <div class="failure-icon mb-3">
            <i class="bi bi-x-circle-fill text-danger" style="font-size: 4rem;"></i>
          </div>

          <h4 class="mb-3">{{ '::Saudi:Nafath:LinkingFailed' | abpLocalization }}</h4>
          <p class="text-muted">{{ failureReason }}</p>

          <div class="mt-4">
            <button type="button" class="btn btn-primary me-2" (click)="startLinking()">
              <i class="bi bi-arrow-clockwise me-2"></i>
              {{ '::Saudi:Nafath:TryAgainButton' | abpLocalization }}
            </button>
            <button type="button" class="btn btn-secondary" (click)="resetToView()">
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

      .linked-identity-card {
        border: 1px solid #dee2e6;
      }

      .step-view-empty {
        min-height: 300px;
      }
    `,
  ],
})
export class NafathLinkIdentityComponent implements OnInit, OnDestroy {
  currentStep: LinkStep = 'loading';
  linkedIdentity: NafathUserLinkDto | null = null;
  nationalId = '';
  randomNumber = '';
  pollCounter = 0;
  validationError = '';
  failureReason = '';
  showUnlinkConfirmation = false;
  isLinking = false;

  private authRequest: NafathAuthRequestDto | null = null;
  private pollInterval: any = null;
  private maxPolls = 20;

  constructor(private nafathService: NafathService) {}

  ngOnInit() {
    this.loadLinkedIdentity();
  }

  ngOnDestroy() {
    this.stopPolling();
  }

  loadLinkedIdentity() {
    this.currentStep = 'loading';
    this.nafathService.getMyLink().subscribe({
      next: result => {
        this.linkedIdentity = result;
        this.currentStep = 'view';
      },
      error: () => {
        this.linkedIdentity = null;
        this.currentStep = 'view';
      },
    });
  }

  onNationalIdInput(event: any) {
    const value = event.target.value;
    this.nationalId = value.replace(/[^0-9]/g, '');
    this.validationError = '';
  }

  isNationalIdValid(): boolean {
    return this.nationalId.length === 10 && /^[0-9]{10}$/.test(this.nationalId);
  }

  startLinking() {
    this.currentStep = 'input';
    this.nationalId = '';
    this.validationError = '';
  }

  cancelLinking() {
    this.stopPolling();
    this.resetToView();
  }

  initiateLink() {
    if (!this.isNationalIdValid()) {
      this.validationError = 'National ID must be exactly 10 digits';
      return;
    }

    this.isLinking = true;
    this.nafathService.initiateLogin(this.nationalId).subscribe({
      next: result => {
        this.isLinking = false;
        this.authRequest = result;
        this.randomNumber = result.randomNumber.toString();
        this.currentStep = 'waiting';
        this.pollCounter = 0;
        this.startPolling();
      },
      error: () => {
        this.isLinking = false;
        this.failureReason = 'Failed to initiate identity verification.';
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
        this.failureReason = 'The verification request timed out.';
        return;
      }

      if (!this.authRequest) return;

      this.nafathService.checkStatus(this.authRequest.transactionId).subscribe({
        next: result => {
          if (result.status === NafathRequestStatus.Completed) {
            this.stopPolling();
            // Now link the identity
            this.nafathService.linkIdentity(this.nationalId).subscribe({
              next: () => {
                this.currentStep = 'success';
              },
              error: () => {
                this.currentStep = 'failure';
                this.failureReason = 'Verification succeeded but identity linking failed.';
              },
            });
          } else if (
            result.status === NafathRequestStatus.Rejected ||
            result.status === NafathRequestStatus.Expired ||
            result.status === NafathRequestStatus.Failed
          ) {
            this.stopPolling();
            this.currentStep = 'failure';
            this.failureReason = 'The verification request was rejected or expired.';
          }
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

  viewLinkedIdentity() {
    this.loadLinkedIdentity();
  }

  confirmUnlink() {
    this.showUnlinkConfirmation = true;
  }

  cancelUnlink() {
    this.showUnlinkConfirmation = false;
  }

  unlinkIdentity() {
    // Call linkIdentity to re-link (or a dedicated unlink endpoint if available)
    // For now, reload the view
    this.linkedIdentity = null;
    this.showUnlinkConfirmation = false;
    this.currentStep = 'view';
  }

  resetToView() {
    this.currentStep = 'view';
    this.nationalId = '';
    this.randomNumber = '';
    this.pollCounter = 0;
    this.validationError = '';
    this.failureReason = '';
    this.authRequest = null;
    this.showUnlinkConfirmation = false;
  }
}
