import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

const THEME_KEY = 'invoice_theme';

@Injectable({ providedIn: 'root' })
export class ThemeStorageService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  getTheme(): string | null {
    return this.isBrowser ? localStorage.getItem(THEME_KEY) : null;
  }

  setTheme(theme: string): void {
    if (!this.isBrowser) {
      return;
    }
    localStorage.setItem(THEME_KEY, theme);
  }
}
