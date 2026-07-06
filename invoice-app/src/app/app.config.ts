import { isPlatformBrowser } from '@angular/common';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, PLATFORM_ID, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { catchError, firstValueFrom, of } from 'rxjs';

import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { LocalizationService } from './core/services/localization.service';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor, errorInterceptor])),
    provideAppInitializer(() => {
      // Translations are only fetched in the browser: `apiUrl` is a relative
      // path ('/api', proxied by nginx), which has no origin to resolve
      // against during server-side prerendering — and prerendering happens at
      // build time anyway, with no live backend to call. The server-rendered
      // auth pages fall back to raw translation keys until the client fetches
      // and hydrates, same as an unreachable-backend fallback would look.
      if (!isPlatformBrowser(inject(PLATFORM_ID))) {
        return Promise.resolve();
      }
      const localization = inject(LocalizationService);
      return firstValueFrom(localization.load(localization.currentLang()).pipe(catchError(() => of(null))));
    }),
  ],
};
