import { Component, Input, OnChanges, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as QRCode from 'qrcode';

@Component({
  selector: 'app-zatca-qr-code',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="qr-code-container text-center">
      <canvas #qrCanvas *ngIf="data"></canvas>
      <p class="text-muted small mt-1" *ngIf="!data">
        No QR code available
      </p>
    </div>
  `,
  styles: [
    `
      .qr-code-container canvas {
        max-width: 200px;
        height: auto;
      }
    `,
  ],
})
export class ZatcaQrCodeComponent implements OnChanges, AfterViewInit {
  @Input() data: string | null = null;
  @Input() size = 200;
  @ViewChild('qrCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;

  private viewReady = false;

  ngAfterViewInit() {
    this.viewReady = true;
    this.renderQr();
  }

  ngOnChanges() {
    if (this.viewReady) {
      this.renderQr();
    }
  }

  private renderQr() {
    if (!this.data || !this.canvasRef?.nativeElement) return;

    QRCode.toCanvas(this.canvasRef.nativeElement, this.data, {
      width: this.size,
      margin: 2,
      errorCorrectionLevel: 'M',
    });
  }
}
