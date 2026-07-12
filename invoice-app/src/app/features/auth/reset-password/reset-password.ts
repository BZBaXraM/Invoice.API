import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher';
import { PasswordInputComponent } from '../../../shared/components/password-input/password-input';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-reset-password',
  imports: [ReactiveFormsModule, RouterLink, PasswordInputComponent, LanguageSwitcherComponent, TranslatePipe],
  templateUrl: './reset-password.html',
})
export class ResetPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notifications = inject(NotificationService);
  protected readonly localization = inject(LocalizationService);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    email: [this.route.snapshot.queryParamMap.get('email') ?? '', [Validators.required, Validators.email]],
    code: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.auth.resetPassword(this.form.getRawValue()).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSucceeded) {
          this.notifications.success(this.localization.translate('auth.resetPassword.resetSuccess'));
          this.router.navigateByUrl('/login');
        } else {
          this.errorMessage.set(res.message || this.localization.translate('auth.resetPassword.resetError'));
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('common.errors.generic')));
      },
    });
  }
}
