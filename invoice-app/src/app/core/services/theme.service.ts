import { isPlatformBrowser } from '@angular/common';
import { Injectable, PLATFORM_ID, effect, inject, signal } from '@angular/core';
import { DEFAULT_THEME, ThemeMode } from '../models/theme.model';
import { ThemeStorageService } from './theme-storage.service';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storage = inject(ThemeStorageService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  readonly theme = signal<ThemeMode>(this.resolveInitialTheme());

  constructor() {
    effect(() => {
      const theme = this.theme();
      if (this.isBrowser) {
        document.documentElement.setAttribute('data-theme', theme);
      }
    });
  }

  setTheme(theme: ThemeMode): void {
    this.storage.setTheme(theme);
    this.theme.set(theme);
  }

  toggle(): void {
    this.setTheme(this.theme() === 'dark' ? 'light' : 'dark');
  }

  private resolveInitialTheme(): ThemeMode {
    const stored = this.storage.getTheme();
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }
    if (this.isBrowser && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }
    return DEFAULT_THEME;
  }
}
