import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LocalizationService } from '../../../core/services/localization.service';
import { LanguageCode, SUPPORTED_LANGUAGES } from '../../../core/models/localization.model';

@Component({
  selector: 'app-language-switcher',
  imports: [FormsModule],
  template: `
    <select
      class="select"
      style="max-width: 9rem"
      [ngModel]="localization.currentLang()"
      (ngModelChange)="onChange($event)"
    >
      @for (lang of languages; track lang.code) {
        <option [value]="lang.code">{{ lang.label }}</option>
      }
    </select>
  `,
})
export class LanguageSwitcherComponent {
  protected readonly localization = inject(LocalizationService);
  protected readonly languages = SUPPORTED_LANGUAGES;

  protected onChange(lang: LanguageCode): void {
    this.localization.setLanguage(lang);
  }
}
