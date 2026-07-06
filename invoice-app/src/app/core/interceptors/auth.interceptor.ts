import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

const PUBLIC_ENDPOINTS = ['/account/login', '/account/refresh-token', '/account/register'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();
  const isPublic = PUBLIC_ENDPOINTS.some((endpoint) => req.url.includes(endpoint));

  if (token && !isPublic) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    });
  }

  return next(req);
};
