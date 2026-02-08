import { of } from 'rxjs';
import {
  createMockInvoiceList,
  createMockInvoice,
  createMockSeller,
  createMockCertificate,
  createMockNafathLink,
  createMockNafathAuthRequest,
  createMockApprovalTask,
  createMockDelegation,
  createMockHijriDate,
} from './saudi-test-helpers';

export function createMockInvoiceService() {
  return jasmine.createSpyObj('InvoiceService', [
    'getList', 'get', 'create', 'update', 'delete', 'submit', 'validate',
  ], {});
}

export function createMockSellerService() {
  return jasmine.createSpyObj('SellerService', [
    'getList', 'get', 'create', 'update', 'delete', 'setDefault', 'getDefault',
  ]);
}

export function createMockCertificateService() {
  return jasmine.createSpyObj('CertificateService', [
    'getList', 'get', 'create', 'delete', 'activate', 'deactivate',
  ]);
}

export function createMockNafathService() {
  return jasmine.createSpyObj('NafathService', [
    'initiateLogin', 'checkStatus', 'linkIdentity', 'getMyLink',
  ]);
}

export function createMockApprovalService() {
  return jasmine.createSpyObj('ApprovalService', [
    'getMyTasks', 'approve', 'reject',
  ]);
}

export function createMockDelegationService() {
  return jasmine.createSpyObj('DelegationService', [
    'getList', 'get', 'create', 'update', 'delete',
  ]);
}

export function createMockHijriCalendarService() {
  return jasmine.createSpyObj('HijriCalendarService', [
    'getToday', 'convertToHijri', 'convertToGregorian', 'getMonthInfo',
  ]);
}

export function createMockSaudiSettingsService() {
  return jasmine.createSpyObj('SaudiSettingsService', [
    'getZatcaSettings', 'updateZatcaSettings', 'getNafathSettings', 'updateNafathSettings',
  ]);
}

export function createMockInvoicePdfService() {
  return jasmine.createSpyObj('InvoicePdfService', ['generate']);
}

export function setupDefaultMockReturns(mocks: {
  invoiceService?: any;
  sellerService?: any;
  certificateService?: any;
  nafathService?: any;
  approvalService?: any;
  delegationService?: any;
  hijriCalendarService?: any;
  settingsService?: any;
}) {
  if (mocks.invoiceService) {
    mocks.invoiceService.getList.and.returnValue(of({ items: [createMockInvoiceList()], totalCount: 1 }));
    mocks.invoiceService.get.and.returnValue(of(createMockInvoice()));
    mocks.invoiceService.create.and.returnValue(of(createMockInvoice()));
    mocks.invoiceService.update.and.returnValue(of(createMockInvoice()));
    mocks.invoiceService.delete.and.returnValue(of(void 0));
    mocks.invoiceService.submit.and.returnValue(of({ requestId: 'req-1', status: 3, warnings: [], errors: [], isSuccess: true }));
    mocks.invoiceService.validate.and.returnValue(of({ requestId: 'req-1', status: 1, warnings: [], errors: [], isSuccess: true }));
  }
  if (mocks.sellerService) {
    mocks.sellerService.getList.and.returnValue(of({ items: [createMockSeller()], totalCount: 1 }));
    mocks.sellerService.get.and.returnValue(of(createMockSeller()));
    mocks.sellerService.create.and.returnValue(of(createMockSeller()));
    mocks.sellerService.update.and.returnValue(of(createMockSeller()));
    mocks.sellerService.delete.and.returnValue(of(void 0));
    mocks.sellerService.setDefault.and.returnValue(of(void 0));
    mocks.sellerService.getDefault.and.returnValue(of(createMockSeller()));
  }
  if (mocks.certificateService) {
    mocks.certificateService.getList.and.returnValue(of({ items: [createMockCertificate()], totalCount: 1 }));
    mocks.certificateService.get.and.returnValue(of(createMockCertificate()));
    mocks.certificateService.create.and.returnValue(of(createMockCertificate()));
    mocks.certificateService.delete.and.returnValue(of(void 0));
    mocks.certificateService.activate.and.returnValue(of(void 0));
    mocks.certificateService.deactivate.and.returnValue(of(void 0));
  }
  if (mocks.nafathService) {
    mocks.nafathService.initiateLogin.and.returnValue(of(createMockNafathAuthRequest()));
    mocks.nafathService.checkStatus.and.returnValue(of(createMockNafathAuthRequest()));
    mocks.nafathService.linkIdentity.and.returnValue(of(createMockNafathLink()));
    mocks.nafathService.getMyLink.and.returnValue(of(createMockNafathLink()));
  }
  if (mocks.approvalService) {
    mocks.approvalService.getMyTasks.and.returnValue(of({ items: [createMockApprovalTask()], totalCount: 1 }));
    mocks.approvalService.approve.and.returnValue(of(createMockApprovalTask({ status: 1 })));
    mocks.approvalService.reject.and.returnValue(of(createMockApprovalTask({ status: 2 })));
  }
  if (mocks.delegationService) {
    mocks.delegationService.getList.and.returnValue(of({ items: [createMockDelegation()], totalCount: 1 }));
    mocks.delegationService.get.and.returnValue(of(createMockDelegation()));
    mocks.delegationService.create.and.returnValue(of(createMockDelegation()));
    mocks.delegationService.update.and.returnValue(of(createMockDelegation()));
    mocks.delegationService.delete.and.returnValue(of(void 0));
  }
  if (mocks.hijriCalendarService) {
    mocks.hijriCalendarService.getToday.and.returnValue(of(createMockHijriDate()));
    mocks.hijriCalendarService.convertToHijri.and.returnValue(of(createMockHijriDate()));
    mocks.hijriCalendarService.convertToGregorian.and.returnValue(of(createMockHijriDate()));
    mocks.hijriCalendarService.getMonthInfo.and.returnValue(of({ year: 1446, month: 6, monthName: 'جمادى الآخرة', daysInMonth: 30, firstDayGregorian: '2024-11-15', lastDayGregorian: '2024-12-14' }));
  }
  if (mocks.settingsService) {
    mocks.settingsService.getZatcaSettings.and.returnValue(of({ environment: 'Sandbox', apiBaseUrl: 'https://sandbox.zatca.gov.sa' }));
    mocks.settingsService.updateZatcaSettings.and.returnValue(of(void 0));
    mocks.settingsService.getNafathSettings.and.returnValue(of({ appId: 'test-app', apiBaseUrl: 'https://nafath.api.elm.sa' }));
    mocks.settingsService.updateNafathSettings.and.returnValue(of(void 0));
  }
}
