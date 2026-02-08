import { Directive, ElementRef, OnDestroy, OnInit, Renderer2 } from '@angular/core';
import { LocalizationService } from '@abp/ng.core';
import { Subscription } from 'rxjs';

/**
 * Standalone directive that auto-applies `dir="rtl"` and `lang="ar"`
 * when the active locale is Arabic. Listens to ABP's LocalizationService
 * for language changes and updates the host element accordingly.
 *
 * Usage:
 *   <div appSaudiRtl>...</div>
 *
 * Or on the Saudi module root container to apply RTL to the entire module.
 */
@Directive({
  selector: '[appSaudiRtl]',
  standalone: true,
})
export class SaudiRtlDirective implements OnInit, OnDestroy {
  private subscription?: Subscription;

  constructor(
    private el: ElementRef<HTMLElement>,
    private renderer: Renderer2,
    private localizationService: LocalizationService
  ) {}

  ngOnInit(): void {
    // Apply initial direction
    this.applyDirection(this.localizationService.currentLang);

    // Listen for language changes
    this.subscription = this.localizationService.languageChange$.subscribe(({ payload }) => {
      this.applyDirection(payload);
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private applyDirection(lang: string): void {
    const isRtl = lang === 'ar' || lang.startsWith('ar-');

    if (isRtl) {
      this.renderer.setAttribute(this.el.nativeElement, 'dir', 'rtl');
      this.renderer.setAttribute(this.el.nativeElement, 'lang', 'ar');
      this.renderer.addClass(this.el.nativeElement, 'saudi-rtl');
    } else {
      this.renderer.setAttribute(this.el.nativeElement, 'dir', 'ltr');
      this.renderer.setAttribute(this.el.nativeElement, 'lang', lang || 'en');
      this.renderer.removeClass(this.el.nativeElement, 'saudi-rtl');
    }
  }
}
