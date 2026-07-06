import { Pipe, PipeTransform, inject } from '@angular/core';
import { LocalizationService } from '../../core/services/localization.service';

@Pipe({ name: 'translate', pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly localization = inject(LocalizationService);

  transform(key: string, params?: Record<string, string | number>): string {
    return this.localization.translate(key, params);
  }
}
