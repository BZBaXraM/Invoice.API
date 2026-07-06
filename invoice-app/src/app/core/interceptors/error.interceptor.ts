import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { LocalizationService } from '../services/localization.service';
import { NotificationService } from '../services/notification.service';

let isRefreshing = false;
const refreshedToken$ = new BehaviorSubject<string | null>(null);

const REFRESH_EXEMPT = ['/account/login', '/account/refresh-token', '/account/register'];

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const notifications = inject(NotificationService);
  const router = inject(Router);
  const localization = inject(LocalizationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      const isExempt = REFRESH_EXEMPT.some((endpoint) => req.url.includes(endpoint));

      if (error.status === 401 && !isExempt && authService.getRefreshToken()) {
        if (!isRefreshing) {
          isRefreshing = true;
          refreshedToken$.next(null);

          return authService.refreshToken().pipe(
            switchMap((res) => {
              isRefreshing = false;
              const token = res.data?.accessToken ?? null;
              refreshedToken$.next(token);
              if (!token) {
                authService.clearSession();
                router.navigateByUrl('/login');
                return throwError(() => error);
              }
              return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
            }),
            catchError((refreshError) => {
              isRefreshing = false;
              authService.clearSession();
              router.navigateByUrl('/login');
              return throwError(() => refreshError);
            }),
          );
        }

        return refreshedToken$.pipe(
          filter((token) => token !== null),
          take(1),
          switchMap((token) => next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }))),
        );
      }

      if (error.status === 0) {
        notifications.error(localization.translate('errors.networkDown'));
      } else if (error.status >= 500) {
        notifications.error(localization.translate('errors.serverError'));
      }

      return throwError(() => error);
    }),
  );
};
