import { Injectable } from '@angular/core';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { ZatcaInvoiceDto, InvoiceTypeLabels, InvoiceStatusLabels } from './invoice.service';

@Injectable({ providedIn: 'root' })
export class InvoicePdfService {
  generate(invoice: ZatcaInvoiceDto): void {
    const doc = new jsPDF();
    const pageWidth = doc.internal.pageSize.getWidth();
    let y = 15;

    // Title
    doc.setFontSize(18);
    doc.text('ZATCA Invoice', pageWidth / 2, y, { align: 'center' });
    y += 8;

    doc.setFontSize(10);
    doc.text(`Invoice #${invoice.invoiceNumber}`, pageWidth / 2, y, { align: 'center' });
    y += 5;

    const statusText = InvoiceStatusLabels[invoice.status] || 'Unknown';
    const typeText = InvoiceTypeLabels[invoice.invoiceType] || 'Unknown';
    doc.text(`${typeText} | ${statusText}`, pageWidth / 2, y, { align: 'center' });
    y += 12;

    // Seller / Buyer info side by side
    doc.setFontSize(11);
    doc.setFont('helvetica', 'bold');
    doc.text('Seller', 14, y);
    doc.text('Buyer', pageWidth / 2 + 5, y);
    y += 6;

    doc.setFontSize(9);
    doc.setFont('helvetica', 'normal');
    doc.text(`ID: ${invoice.sellerId}`, 14, y);
    doc.text(`Name: ${invoice.buyerName || 'N/A'}`, pageWidth / 2 + 5, y);
    y += 5;

    doc.text('', 14, y);
    doc.text(`VAT: ${invoice.buyerVatNumber || 'N/A'}`, pageWidth / 2 + 5, y);
    y += 10;

    // Invoice details
    doc.setFontSize(9);
    const detailsLeft = [
      ['Issue Date:', this.formatDate(invoice.issueDate)],
      ['Hijri Date:', invoice.issueDateHijri || 'N/A'],
      ['Currency:', invoice.currencyCode],
    ];

    for (const [label, value] of detailsLeft) {
      doc.setFont('helvetica', 'bold');
      doc.text(label, 14, y);
      doc.setFont('helvetica', 'normal');
      doc.text(value, 50, y);
      y += 5;
    }

    if (invoice.zatcaRequestId) {
      doc.setFont('helvetica', 'bold');
      doc.text('ZATCA Request ID:', 14, y);
      doc.setFont('helvetica', 'normal');
      doc.text(invoice.zatcaRequestId, 55, y);
      y += 5;
    }

    y += 5;

    // Line items table
    const tableBody = invoice.lines.map(line => [
      line.itemName,
      this.formatNum(line.quantity),
      this.formatNum(line.unitPrice),
      `${line.taxPercent}%`,
      this.formatNum(line.vatAmount),
      this.formatNum(line.totalAmount),
    ]);

    autoTable(doc, {
      startY: y,
      head: [['Item', 'Qty', 'Unit Price', 'Tax %', 'Tax Amount', 'Total']],
      body: tableBody,
      theme: 'grid',
      headStyles: { fillColor: [41, 128, 185], fontSize: 9 },
      bodyStyles: { fontSize: 8 },
      columnStyles: {
        0: { cellWidth: 55 },
        1: { halign: 'right', cellWidth: 20 },
        2: { halign: 'right', cellWidth: 25 },
        3: { halign: 'right', cellWidth: 20 },
        4: { halign: 'right', cellWidth: 25 },
        5: { halign: 'right', cellWidth: 25 },
      },
      margin: { left: 14, right: 14 },
    });

    y = (doc as any).lastAutoTable.finalY + 10;

    // Totals
    const totalsX = pageWidth - 14;
    doc.setFontSize(10);

    doc.setFont('helvetica', 'normal');
    doc.text(`Subtotal: ${this.formatNum(invoice.subTotal)} ${invoice.currencyCode}`, totalsX, y, {
      align: 'right',
    });
    y += 6;

    doc.text(`VAT: ${this.formatNum(invoice.vatAmount)} ${invoice.currencyCode}`, totalsX, y, {
      align: 'right',
    });
    y += 6;

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(12);
    doc.text(
      `Grand Total: ${this.formatNum(invoice.grandTotal)} ${invoice.currencyCode}`,
      totalsX,
      y,
      { align: 'right' }
    );
    y += 12;

    // QR Code image
    if (invoice.qrCode) {
      try {
        doc.addImage(
          `data:image/png;base64,${invoice.qrCode}`,
          'PNG',
          14,
          y,
          40,
          40
        );
      } catch {
        // QR image failed to embed — skip
      }
    }

    // Save
    doc.save(`invoice-${invoice.invoiceNumber}.pdf`);
  }

  private formatNum(value: number): string {
    return value.toFixed(2);
  }

  private formatDate(iso: string): string {
    try {
      return new Date(iso).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      });
    } catch {
      return iso;
    }
  }
}
