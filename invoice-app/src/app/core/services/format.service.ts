import { Injectable, computed, inject } from '@angular/core';
import { LOCALE_MAP } from '../models/localization.model';
import { LocalizationService } from './localization.service';

@Injectable({ providedIn: 'root' })
export class FormatService {
  private readonly localization = inject(LocalizationService);
  private readonly locale = computed(() => LOCALE_MAP[this.localization.currentLang()]);

  currency(amount: number, currency = 'AZN'): string {
    const formatted = new Intl.NumberFormat(this.locale(), {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount);
    return `${formatted} ${currency}`;
  }

  date(value: string | Date | null | undefined): string {
    if (!value) {
      return '—';
    }
    const date = typeof value === 'string' ? new Date(value) : value;
    return new Intl.DateTimeFormat(this.locale(), { year: 'numeric', month: 'short', day: '2-digit' }).format(date);
  }

  dateTime(value: string | Date | null | undefined): string {
    if (!value) {
      return '—';
    }
    const date = typeof value === 'string' ? new Date(value) : value;
    return new Intl.DateTimeFormat(this.locale(), {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date);
  }
}
