import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher';
import { PasswordInputComponent } from '../../../shared/components/password-input/password-input';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, PasswordInputComponent, LanguageSwitcherComponent, TranslatePipe],
  templateUrl: './register.html',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly localization = inject(LocalizationService);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    username: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    address: [''],
    phoneNumber: [''],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    const raw = this.form.getRawValue();

    this.auth
      .register({
        ...raw,
        address: raw.address || null,
        phoneNumber: raw.phoneNumber || null,
      })
      .subscribe({
        next: (res) => {
          this.submitting.set(false);
          if (res.isSucceeded) {
            this.router.navigate(['/confirm-email'], { queryParams: { email: raw.email } });
          } else {
            this.errorMessage.set(res.message || this.localization.translate('auth.register.registerError'));
          }
        },
        error: (err) => {
          this.submitting.set(false);
          this.errorMessage.set(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('common.errors.generic')));
        },
      });
  }
}
