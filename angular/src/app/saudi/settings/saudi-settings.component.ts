import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationModule } from '@abp/ng.core';
import { finalize } from 'rxjs';
import {
  SaudiSettingsService,
  ZatcaSettingsDto,
  NafathSettingsDto,
} from '../shared/services/saudi-settings.service';

type SettingsTab = 'zatca' | 'nafath';

@Component({
  selector: 'app-saudi-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, LocalizationModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="card-title">{{ '::Saudi:Settings:Title' | abpLocalization }}</h5>
      </div>
      <div class="card-body">
        <!-- Tabs -->
        <ul class="nav nav-tabs mb-4">
          <li class="nav-item">
            <button
              class="nav-link"
              [class.active]="activeTab === 'zatca'"
              (click)="activeTab = 'zatca'"
            >
              <i class="bi bi-receipt me-1"></i>
              {{ '::Saudi:Settings:ZatcaTab' | abpLocalization }}
            </button>
          </li>
          <li class="nav-item">
            <button
              class="nav-link"
              [class.active]="activeTab === 'nafath'"
              (click)="activeTab = 'nafath'"
            >
              <i class="bi bi-shield-check me-1"></i>
              {{ '::Saudi:Settings:NafathTab' | abpLocalization }}
            </button>
          </li>
        </ul>

        <!-- Loading -->
        <div *ngIf="loading" class="text-center py-4">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>

        <!-- ZATCA Settings -->
        <div *ngIf="!loading && activeTab === 'zatca'">
          <form #zatcaForm="ngForm">
            <div class="row">
              <div class="col-md-6 mb-3">
                <label for="zatcaEnv" class="form-label">
                  {{ '::Saudi:Settings:Zatca:Environment' | abpLocalization }}
                </label>
                <select
                  id="zatcaEnv"
                  class="form-select"
                  [(ngModel)]="zatcaSettings.environment"
                  name="environment"
                >
                  <option value="Sandbox">Sandbox</option>
                  <option value="Simulation">Simulation</option>
                  <option value="Production">Production</option>
                </select>
              </div>

              <div class="col-md-6 mb-3">
                <label for="zatcaApiBaseUrl" class="form-label">
                  {{ '::Saudi:Settings:Zatca:ApiBaseUrl' | abpLocalization }}
                </label>
                <input
                  type="url"
                  id="zatcaApiBaseUrl"
                  class="form-control"
                  [(ngModel)]="zatcaSettings.apiBaseUrl"
                  name="apiBaseUrl"
                  placeholder="https://gw-fatoora.zatca.gov.sa"
                />
              </div>

              <div class="col-md-6 mb-3">
                <label for="complianceCsid" class="form-label">
                  {{ '::Saudi:Settings:Zatca:ComplianceCsid' | abpLocalization }}
                </label>
                <input
                  type="text"
                  id="complianceCsid"
                  class="form-control"
                  [(ngModel)]="zatcaSettings.complianceCsid"
                  name="complianceCsid"
                />
              </div>

              <div class="col-md-6 mb-3">
                <label for="productionCsid" class="form-label">
                  {{ '::Saudi:Settings:Zatca:ProductionCsid' | abpLocalization }}
                </label>
                <input
                  type="text"
                  id="productionCsid"
                  class="form-control"
                  [(ngModel)]="zatcaSettings.productionCsid"
                  name="productionCsid"
                />
              </div>

              <div class="col-md-6 mb-3">
                <label for="zatcaSecret" class="form-label">
                  {{ '::Saudi:Settings:Zatca:Secret' | abpLocalization }}
                </label>
                <div class="input-group">
                  <input
                    [type]="showZatcaSecret ? 'text' : 'password'"
                    id="zatcaSecret"
                    class="form-control"
                    [(ngModel)]="zatcaSettings.secret"
                    name="secret"
                  />
                  <button
                    type="button"
                    class="btn btn-outline-secondary"
                    (click)="showZatcaSecret = !showZatcaSecret"
                  >
                    <i class="bi" [ngClass]="showZatcaSecret ? 'bi-eye-slash' : 'bi-eye'"></i>
                  </button>
                </div>
              </div>
            </div>

            <button
              type="button"
              class="btn btn-primary"
              [disabled]="isSaving"
              (click)="saveZatcaSettings()"
            >
              <i class="bi bi-save me-1"></i>
              {{ '::Saudi:Settings:Save' | abpLocalization }}
              <span *ngIf="isSaving" class="spinner-border spinner-border-sm ms-1"></span>
            </button>

            <span *ngIf="saveSuccess === 'zatca'" class="text-success ms-3">
              <i class="bi bi-check-circle me-1"></i>
              {{ '::Saudi:Settings:Saved' | abpLocalization }}
            </span>
          </form>
        </div>

        <!-- Nafath Settings -->
        <div *ngIf="!loading && activeTab === 'nafath'">
          <form #nafathForm="ngForm">
            <div class="row">
              <div class="col-md-6 mb-3">
                <label for="nafathAppId" class="form-label">
                  {{ '::Saudi:Settings:Nafath:AppId' | abpLocalization }}
                </label>
                <input
                  type="text"
                  id="nafathAppId"
                  class="form-control"
                  [(ngModel)]="nafathSettings.appId"
                  name="appId"
                />
              </div>

              <div class="col-md-6 mb-3">
                <label for="nafathAppKey" class="form-label">
                  {{ '::Saudi:Settings:Nafath:AppKey' | abpLocalization }}
                </label>
                <div class="input-group">
                  <input
                    [type]="showNafathKey ? 'text' : 'password'"
                    id="nafathAppKey"
                    class="form-control"
                    [(ngModel)]="nafathSettings.appKey"
                    name="appKey"
                  />
                  <button
                    type="button"
                    class="btn btn-outline-secondary"
                    (click)="showNafathKey = !showNafathKey"
                  >
                    <i class="bi" [ngClass]="showNafathKey ? 'bi-eye-slash' : 'bi-eye'"></i>
                  </button>
                </div>
              </div>

              <div class="col-md-6 mb-3">
                <label for="nafathApiBaseUrl" class="form-label">
                  {{ '::Saudi:Settings:Nafath:ApiBaseUrl' | abpLocalization }}
                </label>
                <input
                  type="url"
                  id="nafathApiBaseUrl"
                  class="form-control"
                  [(ngModel)]="nafathSettings.apiBaseUrl"
                  name="nafathApiBaseUrl"
                  placeholder="https://nafath.api.elm.sa"
                />
              </div>

              <div class="col-md-6 mb-3">
                <label for="nafathCallbackUrl" class="form-label">
                  {{ '::Saudi:Settings:Nafath:CallbackUrl' | abpLocalization }}
                </label>
                <input
                  type="url"
                  id="nafathCallbackUrl"
                  class="form-control"
                  [(ngModel)]="nafathSettings.callbackUrl"
                  name="callbackUrl"
                />
              </div>

              <div class="col-md-6 mb-3">
                <label for="nafathTimeout" class="form-label">
                  {{ '::Saudi:Settings:Nafath:TimeoutSeconds' | abpLocalization }}
                </label>
                <input
                  type="number"
                  id="nafathTimeout"
                  class="form-control"
                  [(ngModel)]="nafathSettings.timeoutSeconds"
                  name="timeoutSeconds"
                  min="30"
                  max="300"
                />
              </div>
            </div>

            <button
              type="button"
              class="btn btn-primary"
              [disabled]="isSaving"
              (click)="saveNafathSettings()"
            >
              <i class="bi bi-save me-1"></i>
              {{ '::Saudi:Settings:Save' | abpLocalization }}
              <span *ngIf="isSaving" class="spinner-border spinner-border-sm ms-1"></span>
            </button>

            <span *ngIf="saveSuccess === 'nafath'" class="text-success ms-3">
              <i class="bi bi-check-circle me-1"></i>
              {{ '::Saudi:Settings:Saved' | abpLocalization }}
            </span>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .nav-tabs .nav-link {
        cursor: pointer;
      }
    `,
  ],
})
export class SaudiSettingsComponent implements OnInit {
  activeTab: SettingsTab = 'zatca';
  loading = true;
  isSaving = false;
  saveSuccess: SettingsTab | null = null;
  showZatcaSecret = false;
  showNafathKey = false;

  zatcaSettings: ZatcaSettingsDto = {};
  nafathSettings: NafathSettingsDto = {};

  constructor(private settingsService: SaudiSettingsService) {}

  ngOnInit() {
    this.loadSettings();
  }

  loadSettings() {
    this.loading = true;
    let loaded = 0;
    const done = () => {
      loaded++;
      if (loaded >= 2) this.loading = false;
    };

    this.settingsService.getZatcaSettings().subscribe({
      next: result => {
        this.zatcaSettings = result || {};
        done();
      },
      error: () => done(),
    });

    this.settingsService.getNafathSettings().subscribe({
      next: result => {
        this.nafathSettings = result || {};
        done();
      },
      error: () => done(),
    });
  }

  saveZatcaSettings() {
    this.isSaving = true;
    this.saveSuccess = null;
    this.settingsService
      .updateZatcaSettings(this.zatcaSettings)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe(() => {
        this.saveSuccess = 'zatca';
        setTimeout(() => {
          if (this.saveSuccess === 'zatca') this.saveSuccess = null;
        }, 3000);
      });
  }

  saveNafathSettings() {
    this.isSaving = true;
    this.saveSuccess = null;
    this.settingsService
      .updateNafathSettings(this.nafathSettings)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe(() => {
        this.saveSuccess = 'nafath';
        setTimeout(() => {
          if (this.saveSuccess === 'nafath') this.saveSuccess = null;
        }, 3000);
      });
  }
}
