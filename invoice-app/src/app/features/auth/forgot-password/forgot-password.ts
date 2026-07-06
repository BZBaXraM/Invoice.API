import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { LanguageSwitcherComponent } from '../../../shared/components/language-switcher/language-switcher';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, LanguageSwitcherComponent, TranslatePipe],
  templateUrl: './forgot-password.html',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly localization = inject(LocalizationService);

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    const email = this.form.getRawValue().email;

    this.auth.forgetPassword({ email }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.router.navigate(['/reset-password'], { queryParams: { email } });
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(extractApiError(err, this.localization.translate('common.errors.generic')));
      },
    });
  }
}
