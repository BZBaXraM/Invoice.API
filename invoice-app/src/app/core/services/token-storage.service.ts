import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

const ACCESS_TOKEN_KEY = 'invoice_access_token';
const REFRESH_TOKEN_KEY = 'invoice_refresh_token';
const REFRESH_EXPIRY_KEY = 'invoice_refresh_token_expiry';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  getAccessToken(): string | null {
    return this.isBrowser ? localStorage.getItem(ACCESS_TOKEN_KEY) : null;
  }

  getRefreshToken(): string | null {
    return this.isBrowser ? localStorage.getItem(REFRESH_TOKEN_KEY) : null;
  }

  getRefreshTokenExpiry(): string | null {
    return this.isBrowser ? localStorage.getItem(REFRESH_EXPIRY_KEY) : null;
  }

  setTokens(accessToken: string, refreshToken: string, refreshTokenExpireTime: string): void {
    if (!this.isBrowser) {
      return;
    }
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(REFRESH_EXPIRY_KEY, refreshTokenExpireTime);
  }

  clear(): void {
    if (!this.isBrowser) {
      return;
    }
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(REFRESH_EXPIRY_KEY);
  }
}
