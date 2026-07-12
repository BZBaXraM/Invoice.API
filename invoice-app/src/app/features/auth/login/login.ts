import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher';
import { PasswordInputComponent } from '../../../shared/components/password-input/password-input';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, PasswordInputComponent, LanguageSwitcherComponent, TranslatePipe],
  templateUrl: './login.html',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);
  protected readonly localization = inject(LocalizationService);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    usernameOrEmail: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSucceeded) {
          this.router.navigateByUrl('/dashboard');
        } else {
          this.errorMessage.set(res.message || this.localization.translate('auth.login.loginError'));
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('auth.login.loginFallbackError')));
      },
    });
  }
}
