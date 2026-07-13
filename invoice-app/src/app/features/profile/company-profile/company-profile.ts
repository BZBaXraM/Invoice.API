import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CompanyImageKind } from '../../../core/models/company-profile.model';
import { CompanyProfileService } from '../../../core/services/company-profile.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-company-profile',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './company-profile.html',
})
export class CompanyProfileComponent {
  private readonly fb = inject(FormBuilder);
  private readonly companyProfile = inject(CompanyProfileService);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly localization = inject(LocalizationService);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly profileExists = signal(false);

  protected readonly logoUrl = signal<string | null>(null);
  protected readonly signatureUrl = signal<string | null>(null);
  protected readonly uploadingLogo = signal(false);
  protected readonly uploadingSignature = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    companyName: ['', [Validators.required, Validators.maxLength(200)]],
    voen: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    bankName: [''],
    bankVoen: ['', [Validators.pattern(/^\d{10}$/)]],
    iban: [''],
    bankAccount: [''],
    swiftCode: [''],
  });

  constructor() {
    this.destroyRef.onDestroy(() => {
      this.revoke(this.logoUrl());
      this.revoke(this.signatureUrl());
    });

    this.companyProfile.get().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSucceeded && res.data) {
          this.profileExists.set(true);
          this.form.patchValue({
            companyName: res.data.companyName,
            voen: res.data.voen,
            bankName: res.data.bankName ?? '',
            bankVoen: res.data.bankVoen ?? '',
            iban: res.data.iban ?? '',
            bankAccount: res.data.bankAccount ?? '',
            swiftCode: res.data.swiftCode ?? '',
          });
          if (res.data.hasLogo) {
            this.loadImage('logo');
          }
          if (res.data.hasSignature) {
            this.loadImage('signature');
          }
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.notifications.error(
          extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('companyProfile.loadError')),
        );
      },
    });
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);
    const raw = this.form.getRawValue();

    this.companyProfile
      .upsert({
        companyName: raw.companyName,
        voen: raw.voen,
        bankName: raw.bankName || null,
        bankVoen: raw.bankVoen || null,
        iban: raw.iban || null,
        bankAccount: raw.bankAccount || null,
        swiftCode: raw.swiftCode || null,
      })
      .subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.isSucceeded) {
            this.profileExists.set(true);
            this.notifications.success(this.localization.translate('companyProfile.saveSuccess'));
          } else {
            this.errorMessage.set(res.message || this.localization.translate('companyProfile.saveError'));
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.errorMessage.set(extractApiError(err, (k) => this.localization.translate(k)));
        },
      });
  }

  protected onFileSelected(kind: CompanyImageKind, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    if (!['image/png', 'image/jpeg'].includes(file.type)) {
      this.notifications.error(this.localization.translate('companyProfile.error.imageUnsupportedType'));
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this.notifications.error(this.localization.translate('companyProfile.error.imageTooLarge'));
      return;
    }

    const uploading = kind === 'logo' ? this.uploadingLogo : this.uploadingSignature;
    uploading.set(true);
    this.companyProfile.uploadImage(kind, file).subscribe({
      next: (res) => {
        uploading.set(false);
        if (res.isSucceeded) {
          this.notifications.success(this.localization.translate('companyProfile.imageUploaded'));
          this.loadImage(kind);
        } else {
          this.notifications.error(this.localization.translate(res.message));
        }
      },
      error: (err) => {
        uploading.set(false);
        this.notifications.error(
          extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('companyProfile.uploadError')),
        );
      },
    });
  }

  protected async deleteImage(kind: CompanyImageKind): Promise<void> {
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('companyProfile.deleteImageConfirm.title'),
      message: this.localization.translate('companyProfile.deleteImageConfirm.message'),
      confirmLabel: this.localization.translate('common.actions.delete'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.companyProfile.deleteImage(kind).subscribe({
      next: () => {
        this.setImageUrl(kind, null);
        this.notifications.success(this.localization.translate('companyProfile.imageDeleted'));
      },
      error: (err) =>
        this.notifications.error(extractApiError(err, (k) => this.localization.translate(k))),
    });
  }

  private loadImage(kind: CompanyImageKind): void {
    this.companyProfile.getImageBlob(kind).subscribe({
      next: (blob) => this.setImageUrl(kind, URL.createObjectURL(blob)),
      error: () => this.setImageUrl(kind, null),
    });
  }

  private setImageUrl(kind: CompanyImageKind, url: string | null): void {
    const target = kind === 'logo' ? this.logoUrl : this.signatureUrl;
    this.revoke(target());
    target.set(url);
  }

  private revoke(url: string | null): void {
    if (url) {
      URL.revokeObjectURL(url);
    }
  }
}
