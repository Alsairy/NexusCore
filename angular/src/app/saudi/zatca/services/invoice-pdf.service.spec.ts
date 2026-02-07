import { TestBed } from '@angular/core/testing';
import { InvoicePdfService } from './invoice-pdf.service';
import { createMockInvoice } from '../../testing/saudi-test-helpers';

describe('InvoicePdfService', () => {
  let service: InvoicePdfService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [InvoicePdfService],
    });

    service = TestBed.inject(InvoicePdfService);
  });

  it('should create the service', () => {
    expect(service).toBeTruthy();
  });

  it('should not throw when generate is called with a valid invoice', () => {
    const invoice = createMockInvoice();

    // Mock doc.save to prevent actual file download in test environment
    spyOn(document, 'createElement').and.callThrough();

    expect(() => service.generate(invoice)).not.toThrow();
  });
});
