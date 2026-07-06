import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NotificationService } from '../../core/services/notification.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { PasswordInputComponent } from '../../shared/components/password-input/password-input';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { extractApiError } from '../../shared/utils/api-error';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, PasswordInputComponent, TranslatePipe],
  templateUrl: './profile.html',
})
export class ProfileComponent {
  private readonly fb = inject(FormBuilder);
  protected readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly router = inject(Router);
  protected readonly localization = inject(LocalizationService);

  protected readonly loading = signal(true);
  protected readonly savingProfile = signal(false);
  protected readonly profileError = signal<string | null>(null);
  protected readonly changingPassword = signal(false);
  protected readonly passwordError = signal<string | null>(null);
  protected readonly deleting = signal(false);

  protected readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    username: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    address: [''],
    phoneNumber: [''],
  });

  protected readonly passwordForm = this.fb.nonNullable.group({
    oldPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
  });

  constructor() {
    this.auth.getProfile().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSucceeded && res.data) {
          this.profileForm.patchValue({
            firstName: res.data.firstName,
            lastName: res.data.lastName,
            username: res.data.username,
            email: res.data.email,
            address: res.data.address ?? '',
            phoneNumber: res.data.phoneNumber ?? '',
          });
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.profileForm.patchValue({ email: this.auth.claims().email ?? '' });
        this.notifications.error(extractApiError(err, this.localization.translate('profile.loadError')));
      },
    });
  }

  protected saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.savingProfile.set(true);
    this.profileError.set(null);
    const raw = this.profileForm.getRawValue();

    this.auth
      .updateProfile({ ...raw, address: raw.address || null, phoneNumber: raw.phoneNumber || null })
      .subscribe({
        next: (res) => {
          this.savingProfile.set(false);
          if (res.isSucceeded) {
            this.notifications.success(this.localization.translate('profile.saveSuccess'));
          } else {
            this.profileError.set(res.message || this.localization.translate('profile.saveError'));
          }
        },
        error: (err) => {
          this.savingProfile.set(false);
          this.profileError.set(extractApiError(err));
        },
      });
  }

  protected changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.changingPassword.set(true);
    this.passwordError.set(null);

    this.auth.changePassword(this.passwordForm.getRawValue()).subscribe({
      next: (res) => {
        this.changingPassword.set(false);
        if (res.isSucceeded) {
          this.notifications.success(this.localization.translate('profile.passwordSuccess'));
          this.passwordForm.reset({ oldPassword: '', newPassword: '' });
          this.auth.clearSession();
          this.router.navigateByUrl('/login');
        } else {
          this.passwordError.set(res.message || this.localization.translate('profile.passwordError'));
        }
      },
      error: (err) => {
        this.changingPassword.set(false);
        this.passwordError.set(extractApiError(err));
      },
    });
  }

  protected async deleteAccount(): Promise<void> {
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('profile.deleteButton'),
      message: this.localization.translate('profile.deleteConfirm.message'),
      confirmLabel: this.localization.translate('profile.deleteButton'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.deleting.set(true);
    this.auth.deleteProfile().subscribe({
      next: () => {
        this.deleting.set(false);
        this.notifications.info(this.localization.translate('profile.deleteSuccess'));
        this.router.navigateByUrl('/login');
      },
      error: (err) => {
        this.deleting.set(false);
        this.notifications.error(extractApiError(err, this.localization.translate('profile.deleteError')));
      },
    });
  }
}
