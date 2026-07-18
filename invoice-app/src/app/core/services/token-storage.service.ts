import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

const ACCESS_TOKEN_KEY = 'invoice_access_token';
const REFRESH_TOKEN_KEY = 'invoice_refresh_token';
const REFRESH_EXPIRY_KEY = 'invoice_refresh_token_expiry';
const REMEMBER_KEY = 'invoice_remember_me';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  getAccessToken(): string | null {
    return this.read(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return this.read(REFRESH_TOKEN_KEY);
  }

  getRefreshTokenExpiry(): string | null {
    return this.read(REFRESH_EXPIRY_KEY);
  }

  isRemembered(): boolean {
    return this.isBrowser && localStorage.getItem(REMEMBER_KEY) === '1';
  }

  setTokens(accessToken: string, refreshToken: string, refreshTokenExpireTime: string, remember: boolean): void {
    if (!this.isBrowser) {
      return;
    }
    this.clearTokenKeys();
    localStorage.setItem(REMEMBER_KEY, remember ? '1' : '0');
    const store = remember ? localStorage : sessionStorage;
    store.setItem(ACCESS_TOKEN_KEY, accessToken);
    store.setItem(REFRESH_TOKEN_KEY, refreshToken);
    store.setItem(REFRESH_EXPIRY_KEY, refreshTokenExpireTime);
  }

  clear(): void {
    if (!this.isBrowser) {
      return;
    }
    this.clearTokenKeys();
    localStorage.removeItem(REMEMBER_KEY);
  }

  private read(key: string): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem(key) ?? sessionStorage.getItem(key);
  }

  private clearTokenKeys(): void {
    for (const key of [ACCESS_TOKEN_KEY, REFRESH_TOKEN_KEY, REFRESH_EXPIRY_KEY]) {
      localStorage.removeItem(key);
      sessionStorage.removeItem(key);
    }
  }
}
