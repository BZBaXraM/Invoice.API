import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CustomerService } from '../../../core/services/customer.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-customer-form',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './customer-form.html',
})
export class CustomerFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly localization = inject(LocalizationService);

  protected readonly customerId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = !!this.customerId;
  protected readonly loading = signal(this.isEdit);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    address: [''],
    phoneNumber: [''],
  });

  constructor() {
    if (this.customerId) {
      this.customerService.getById(this.customerId).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.isSucceeded && res.data) {
            this.form.patchValue({
              name: res.data.name,
              email: res.data.email,
              address: res.data.address ?? '',
              phoneNumber: res.data.phoneNumber ?? '',
            });
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.notifications.error(extractApiError(err, this.localization.translate('customers.form.loadError')));
        },
      });
    }
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    const raw = this.form.getRawValue();
    const payload = { ...raw, address: raw.address || null, phoneNumber: raw.phoneNumber || null };

    const request$ = this.customerId
      ? this.customerService.update(this.customerId, payload)
      : this.customerService.create(payload);

    request$.subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSucceeded && res.data) {
          this.notifications.success(
            this.localization.translate(this.isEdit ? 'customers.form.updateSuccess' : 'customers.form.createSuccess'),
          );
          this.router.navigate(['/customers', res.data.id]);
        } else {
          this.errorMessage.set(res.message || this.localization.translate('customers.form.saveError'));
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(extractApiError(err));
      },
    });
  }
}
