import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

const LANGUAGE_KEY = 'invoice_language';

@Injectable({ providedIn: 'root' })
export class LanguageStorageService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  getLanguage(): string | null {
    return this.isBrowser ? localStorage.getItem(LANGUAGE_KEY) : null;
  }

  setLanguage(lang: string): void {
    if (!this.isBrowser) {
      return;
    }
    localStorage.setItem(LANGUAGE_KEY, lang);
  }
}
