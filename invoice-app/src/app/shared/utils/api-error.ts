import { HttpErrorResponse } from '@angular/common/http';
import { ResponseModel } from '../../core/models/common.model';

export function extractApiError(error: unknown, fallback = 'Что-то пошло не так. Попробуйте снова.'): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as ResponseModel | undefined;
    if (body?.errors?.length) {
      return body.errors.join(' ');
    }
    if (body?.message) {
      return body.message;
    }
  }
  return fallback;
}
