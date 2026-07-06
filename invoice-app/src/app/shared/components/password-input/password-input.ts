import { Component, forwardRef, inject, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { LocalizationService } from '../../../core/services/localization.service';

@Component({
  selector: 'app-password-input',
  imports: [],
  template: `
    <div class="password-field">
      <input
        [id]="inputId()"
        class="input"
        [type]="visible() ? 'text' : 'password'"
        [attr.autocomplete]="autocomplete()"
        [value]="value"
        [disabled]="disabled()"
        (input)="onInput($event)"
        (blur)="onTouched()"
      />
      <button
        type="button"
        class="password-field__toggle"
        [attr.aria-label]="visible() ? localization.translate('passwordInput.hide') : localization.translate('passwordInput.show')"
        [attr.aria-pressed]="visible()"
        (click)="toggle()"
      >
        @if (visible()) {
          <!-- eye-off -->
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
            <path
              d="M3 3l18 18M10.58 10.58a2 2 0 0 0 2.83 2.83M9.88 5.09A9.77 9.77 0 0 1 12 5c5 0 9 4.5 10 7-.36 1.02-1.13 2.31-2.27 3.53M6.4 6.6C4.14 8.03 2.5 10 2 12c1 2.5 5 7 10 7 1.31 0 2.55-.29 3.67-.79"
            />
          </svg>
        } @else {
          <!-- eye -->
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M2 12s4-7 10-7 10 7 10 7-4 7-10 7-10-7-10-7Z" />
            <circle cx="12" cy="12" r="3" />
          </svg>
        }
      </button>
    </div>
  `,
  styles: `
    .password-field {
      position: relative;
      display: flex;
    }

    .password-field .input {
      padding-right: 2.5rem;
    }

    .password-field__toggle {
      position: absolute;
      right: 0.5rem;
      top: 50%;
      transform: translateY(-50%);
      background: none;
      border: none;
      padding: 0.25rem;
      color: var(--text-tertiary);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-sm);
    }

    .password-field__toggle:hover {
      color: var(--text-secondary);
    }

    .password-field__toggle:focus-visible {
      outline: none;
      box-shadow: 0 0 0 3px var(--color-accent-100);
    }
  `,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PasswordInputComponent),
      multi: true,
    },
  ],
})
export class PasswordInputComponent implements ControlValueAccessor {
  protected readonly localization = inject(LocalizationService);
  readonly inputId = input<string>('', { alias: 'id' });
  readonly autocomplete = input<string>('current-password');

  protected readonly visible = signal(false);
  protected readonly disabled = signal(false);
  protected value = '';

  private onChange: (value: string) => void = () => {};
  protected onTouched: () => void = () => {};

  protected toggle(): void {
    this.visible.update((v) => !v);
  }

  protected onInput(event: Event): void {
    this.value = (event.target as HTMLInputElement).value;
    this.onChange(this.value);
  }

  writeValue(value: string): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }
}
