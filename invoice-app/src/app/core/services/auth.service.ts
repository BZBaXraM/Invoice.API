import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ChangePasswordRequest,
  ConfirmEmailCodeRequest,
  ForgetPasswordRequest,
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
  RegisterRequest,
  RequestConfirmationCodeRequest,
  ResetPasswordRequest,
  UpdateProfileRequest,
  UserResponse,
} from '../models/account.model';
import { ResponseModel, ResponseModelData } from '../models/common.model';
import { TokenStorageService } from './token-storage.service';

interface JwtClaims {
  userId: string | null;
  email: string | null;
  role: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly baseUrl = `${environment.apiUrl}/account`;

  private readonly accessTokenSignal = signal<string | null>(this.tokenStorage.getAccessToken());
  readonly isAuthenticated = computed(() => !!this.accessTokenSignal());
  readonly currentUser = signal<UserResponse | null>(null);

  readonly claims = computed<JwtClaims>(() => this.decode(this.accessTokenSignal()));
  readonly isAdmin = computed(() => this.claims().role === 'Admin');

  register(request: RegisterRequest): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.baseUrl}/register`, request);
  }

  confirmEmailCode(request: ConfirmEmailCodeRequest): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.baseUrl}/confirm-email-code`, request);
  }

  requestConfirmationCode(request: RequestConfirmationCodeRequest): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.baseUrl}/request-confirmation-code`, request);
  }

  login(request: LoginRequest): Observable<ResponseModelData<LoginResponse>> {
    return this.http
      .post<ResponseModelData<LoginResponse>>(`${this.baseUrl}/login`, request)
      .pipe(tap((res) => this.applyLoginResponse(res)));
  }

  refreshToken(): Observable<ResponseModelData<LoginResponse>> {
    const request: RefreshTokenRequest = { refreshToken: this.tokenStorage.getRefreshToken() ?? '' };
    return this.http
      .post<ResponseModelData<LoginResponse>>(`${this.baseUrl}/refresh-token`, request)
      .pipe(tap((res) => this.applyLoginResponse(res)));
  }

  logout(): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.baseUrl}/logout`, {}).pipe(tap(() => this.clearSession()));
  }

  forgetPassword(request: ForgetPasswordRequest): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.baseUrl}/forget-password`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<ResponseModel> {
    return this.http.post<ResponseModel>(`${this.baseUrl}/reset-password`, request);
  }

  getProfile(): Observable<ResponseModelData<UserResponse>> {
    return this.http
      .get<ResponseModelData<UserResponse>>(`${this.baseUrl}/profile`)
      .pipe(tap((res) => res.data && this.currentUser.set(res.data)));
  }

  updateProfile(request: UpdateProfileRequest): Observable<ResponseModelData<UserResponse>> {
    return this.http
      .put<ResponseModelData<UserResponse>>(`${this.baseUrl}/profile`, request)
      .pipe(tap((res) => res.data && this.currentUser.set(res.data)));
  }

  changePassword(request: ChangePasswordRequest): Observable<ResponseModel> {
    return this.http.put<ResponseModel>(`${this.baseUrl}/profile/change-password`, request);
  }

  deleteProfile(): Observable<ResponseModel> {
    return this.http.delete<ResponseModel>(`${this.baseUrl}/profile`).pipe(tap(() => this.clearSession()));
  }

  getAccessToken(): string | null {
    return this.accessTokenSignal();
  }

  getRefreshToken(): string | null {
    return this.tokenStorage.getRefreshToken();
  }

  applyTokens(accessToken: string, refreshToken: string, refreshTokenExpireTime: string): void {
    this.tokenStorage.setTokens(accessToken, refreshToken, refreshTokenExpireTime);
    this.accessTokenSignal.set(accessToken);
  }

  clearSession(): void {
    this.tokenStorage.clear();
    this.accessTokenSignal.set(null);
    this.currentUser.set(null);
  }

  private applyLoginResponse(res: ResponseModelData<LoginResponse>): void {
    if (res.isSucceeded && res.data) {
      this.applyTokens(res.data.accessToken, res.data.refreshToken, res.data.refreshTokenExpireTime);
    }
  }

  private decode(token: string | null): JwtClaims {
    if (!token) {
      return { userId: null, email: null, role: null };
    }
    try {
      const payload = token.split('.')[1];
      const json = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
      return {
        userId: json['nameid'] ?? json['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? null,
        email: json['unique_name'] ?? json['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? null,
        role: json['role'] ?? json['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null,
      };
    } catch {
      return { userId: null, email: null, role: null };
    }
  }
}
