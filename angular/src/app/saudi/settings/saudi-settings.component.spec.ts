import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { SaudiSettingsComponent } from './saudi-settings.component';
import { SaudiSettingsService } from '../shared/services/saudi-settings.service';
import { createMockSaudiSettingsService, setupDefaultMockReturns } from '../testing/mock-services';
import { of } from 'rxjs';

describe('SaudiSettingsComponent', () => {
  let component: SaudiSettingsComponent;
  let fixture: ComponentFixture<SaudiSettingsComponent>;
  let mockSettingsService: jasmine.SpyObj<SaudiSettingsService>;

  beforeEach(async () => {
    mockSettingsService = createMockSaudiSettingsService() as jasmine.SpyObj<SaudiSettingsService>;
    setupDefaultMockReturns({ settingsService: mockSettingsService });

    await TestBed.configureTestingModule({
      imports: [SaudiSettingsComponent],
      schemas: [NO_ERRORS_SCHEMA],
    })
      .overrideComponent(SaudiSettingsComponent, {
        set: {
          providers: [{ provide: SaudiSettingsService, useValue: mockSettingsService }],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(SaudiSettingsComponent);
    component = fixture.componentInstance;
  });

  it('should create the component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load ZATCA settings on init', () => {
    fixture.detectChanges();

    expect(mockSettingsService.getZatcaSettings).toHaveBeenCalledTimes(1);
    expect(component.zatcaSettings).toEqual(
      jasmine.objectContaining({ environment: 'Sandbox' })
    );
  });

  it('should load Nafath settings', () => {
    fixture.detectChanges();

    expect(mockSettingsService.getNafathSettings).toHaveBeenCalledTimes(1);
    expect(component.nafathSettings).toEqual(
      jasmine.objectContaining({ appId: 'test-app' })
    );
  });

  it('should save ZATCA settings', () => {
    fixture.detectChanges();

    component.zatcaSettings = {
      environment: 'Production',
      apiBaseUrl: 'https://gw-fatoora.zatca.gov.sa',
    };

    component.saveZatcaSettings();

    expect(mockSettingsService.updateZatcaSettings).toHaveBeenCalledWith(
      jasmine.objectContaining({ environment: 'Production' })
    );
    expect(component.isSaving).toBeFalse();
    expect(component.saveSuccess).toBe('zatca');
  });

  it('should save Nafath settings', () => {
    fixture.detectChanges();

    component.nafathSettings = {
      appId: 'updated-app',
      apiBaseUrl: 'https://nafath.api.elm.sa',
    };

    component.saveNafathSettings();

    expect(mockSettingsService.updateNafathSettings).toHaveBeenCalledWith(
      jasmine.objectContaining({ appId: 'updated-app' })
    );
    expect(component.isSaving).toBeFalse();
    expect(component.saveSuccess).toBe('nafath');
  });
});
