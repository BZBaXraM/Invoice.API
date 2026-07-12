import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CustomerService } from '../../../core/services/customer.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { NotificationService } from '../../../core/services/notification.service';
import { CustomerResponse } from '../../../core/models/customer.model';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';
import { toDateInputValue } from '../../../shared/utils/format';

@Component({
  selector: 'app-invoice-form',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './invoice-form.html',
})
export class InvoiceFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly invoiceService = inject(InvoiceService);
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);

  protected readonly invoiceId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = !!this.invoiceId;
  protected readonly loading = signal(this.isEdit);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly customers = signal<CustomerResponse[]>([]);

  protected readonly form = this.fb.nonNullable.group({
    customerId: ['', [Validators.required]],
    startDate: ['', [Validators.required]],
    endDate: ['', [Validators.required]],
    comment: [''],
    rows: this.fb.array([this.createRow()]),
  });

  protected readonly rowsFormArray = this.form.controls.rows;

  protected readonly totalSum = signal(0);

  constructor() {
    this.customerService.getList({ pageNumber: 1, pageSize: 200 }).subscribe({
      next: (res) => {
        if (res.isSucceeded && res.data) {
          this.customers.set(res.data.items.filter((c) => !c.deletedAt));
        }
      },
    });

    this.form.valueChanges.subscribe(() => this.recomputeTotal());
    this.recomputeTotal();

    if (this.invoiceId) {
      this.invoiceService.getById(this.invoiceId).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.isSucceeded && res.data) {
            const invoice = res.data;
            this.rowsFormArray.clear();
            invoice.rows.forEach((row) =>
              this.rowsFormArray.push(
                this.createRow({ service: row.service, quantity: row.quantity, rate: row.rate }),
              ),
            );
            this.form.patchValue({
              customerId: invoice.customerId,
              startDate: toDateInputValue(invoice.startDate),
              endDate: toDateInputValue(invoice.endDate),
              comment: invoice.comment ?? '',
            });
            this.recomputeTotal();
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.form.loadError')));
        },
      });
    }
  }

  private createRow(initial?: { service: string; quantity: number; rate: number }) {
    return this.fb.nonNullable.group({
      service: [initial?.service ?? '', [Validators.required]],
      quantity: [initial?.quantity ?? 1, [Validators.required, Validators.min(0.01)]],
      rate: [initial?.rate ?? 0, [Validators.required, Validators.min(0)]],
    });
  }

  protected addRow(): void {
    this.rowsFormArray.push(this.createRow());
  }

  protected removeRow(index: number): void {
    if (this.rowsFormArray.length > 1) {
      this.rowsFormArray.removeAt(index);
    }
  }

  protected rowSum(index: number): number {
    const row = this.rowsFormArray.at(index).getRawValue();
    return (row.quantity || 0) * (row.rate || 0);
  }

  private recomputeTotal(): void {
    const total = this.rowsFormArray.controls.reduce((sum, control) => {
      const value = control.getRawValue();
      return sum + (value.quantity || 0) * (value.rate || 0);
    }, 0);
    this.totalSum.set(total);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    const raw = this.form.getRawValue();
    const payload = {
      customerId: raw.customerId,
      startDate: new Date(raw.startDate).toISOString(),
      endDate: new Date(raw.endDate).toISOString(),
      comment: raw.comment || null,
      rows: raw.rows.map((r) => ({ service: r.service, quantity: r.quantity, rate: r.rate })),
    };

    const request$ = this.invoiceId
      ? this.invoiceService.update(this.invoiceId, payload)
      : this.invoiceService.create(payload);

    request$.subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSucceeded && res.data) {
          this.notifications.success(
            this.localization.translate(this.isEdit ? 'invoices.form.updateSuccess' : 'invoices.form.createSuccess'),
          );
          this.router.navigate(['/invoices', res.data.id]);
        } else {
          this.errorMessage.set(res.message || this.localization.translate('invoices.form.saveError'));
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(extractApiError(err, (k) => this.localization.translate(k)));
      },
    });
  }
}
