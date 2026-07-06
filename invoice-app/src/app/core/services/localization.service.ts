import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DEFAULT_LANGUAGE, LanguageCode, TranslationsResponse } from '../models/localization.model';
import { ResponseModelData } from '../models/common.model';
import { LanguageStorageService } from './language-storage.service';

@Injectable({ providedIn: 'root' })
export class LocalizationService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(LanguageStorageService);
  private readonly baseUrl = `${environment.apiUrl}/translations`;

  readonly currentLang = signal<LanguageCode>((this.storage.getLanguage() as LanguageCode) ?? DEFAULT_LANGUAGE);
  readonly translations = signal<Record<string, string>>({});

  load(lang: LanguageCode): Observable<ResponseModelData<TranslationsResponse>> {
    return this.http.get<ResponseModelData<TranslationsResponse>>(`${this.baseUrl}?lang=${lang}`).pipe(
      tap((res) => {
        if (res.data) {
          this.translations.set(res.data.translations);
          this.currentLang.set(res.data.language as LanguageCode);
        }
      }),
    );
  }

  setLanguage(lang: LanguageCode): void {
    this.storage.setLanguage(lang);
    this.load(lang).subscribe();
  }

  translate(key: string, params?: Record<string, string | number>): string {
    const template = this.translations()[key] ?? key;
    if (!params) {
      return template;
    }
    return Object.entries(params).reduce(
      (text, [param, value]) => text.replaceAll(`{{${param}}}`, String(value)),
      template,
    );
  }
}
