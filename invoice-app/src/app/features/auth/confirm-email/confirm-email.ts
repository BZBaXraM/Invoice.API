import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-confirm-email',
  imports: [ReactiveFormsModule, RouterLink, LanguageSwitcherComponent, TranslatePipe],
  templateUrl: './confirm-email.html',
})
export class ConfirmEmailComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notifications = inject(NotificationService);
  protected readonly localization = inject(LocalizationService);

  protected readonly submitting = signal(false);
  protected readonly resending = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly email = signal(this.route.snapshot.queryParamMap.get('email') ?? '');

  protected readonly form = this.fb.nonNullable.group({
    code: ['', [Validators.required]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.auth.confirmEmailCode(this.form.getRawValue()).subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSucceeded) {
          this.notifications.success(this.localization.translate('auth.confirmEmail.confirmSuccess'));
          this.router.navigateByUrl('/login');
        } else {
          this.errorMessage.set(res.message || this.localization.translate('auth.confirmEmail.confirmError'));
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('auth.confirmEmail.confirmError')));
      },
    });
  }

  protected resend(): void {
    const email = this.email();
    if (!email) {
      this.errorMessage.set(this.localization.translate('auth.confirmEmail.resendMissingEmail'));
      return;
    }

    this.resending.set(true);
    this.auth.requestConfirmationCode({ email }).subscribe({
      next: () => {
        this.resending.set(false);
        this.notifications.success(this.localization.translate('auth.confirmEmail.resendSuccess'));
      },
      error: (err) => {
        this.resending.set(false);
        this.errorMessage.set(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('common.errors.generic')));
      },
    });
  }
}
