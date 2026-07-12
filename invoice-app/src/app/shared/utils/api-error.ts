import { HttpErrorResponse } from '@angular/common/http';
import { ResponseModel } from '../../core/models/common.model';

/**
 * The server returns translation keys (e.g. `validation.email.invalid`) in
 * `errors`/`message`; pass a translate fn to localize them. Non-key strings
 * pass through `translate` unchanged (LocalizationService falls back to the
 * raw key), so untranslated legacy messages degrade gracefully.
 */
export function extractApiError(
  error: unknown,
  translate: (key: string) => string = (key) => key,
  fallback = 'Что-то пошло не так. Попробуйте снова.',
): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as ResponseModel | undefined;
    if (body?.errors?.length) {
      return body.errors.map(translate).join(' ');
    }
    if (body?.message) {
      return translate(body.message);
    }
  }
  return fallback;
}
