import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CustomerResponse } from '../../../core/models/customer.model';
import {
  DISCOUNT_TYPES,
  DiscountType,
  InvoiceTotals,
  computeInvoiceTotals,
  round2,
} from '../../../core/models/invoice.model';
import { RECURRENCE_FREQUENCIES, RecurrenceFrequency } from '../../../core/models/recurring.model';
import { CustomerService } from '../../../core/services/customer.service';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RecurringService } from '../../../core/services/recurring.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';
import { toDateInputValue } from '../../../shared/utils/format';

@Component({
  selector: 'app-recurring-form',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './recurring-form.html',
})
export class RecurringFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly recurringService = inject(RecurringService);
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);

  protected readonly templateId = this.route.snapshot.paramMap.get('id');
  protected readonly isEdit = !!this.templateId;
  protected readonly loading = signal(this.isEdit);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly customers = signal<CustomerResponse[]>([]);

  protected readonly frequencies = RECURRENCE_FREQUENCIES;
  protected readonly discountTypes = DISCOUNT_TYPES;
  protected readonly DiscountType = DiscountType;

  protected readonly form = this.fb.nonNullable.group({
    customerId: ['', [Validators.required]],
    frequency: [RecurrenceFrequency.Monthly, [Validators.required]],
    nextRunDate: ['', [Validators.required]],
    endDate: [''],
    dueInDays: [14, [Validators.required, Validators.min(0), Validators.max(365)]],
    vatRate: [0, [Validators.min(0), Validators.max(100)]],
    discountType: [DiscountType.None],
    discountValue: [0, [Validators.min(0)]],
    comment: [''],
    rows: this.fb.array([this.createRow()]),
  });

  protected readonly rowsFormArray = this.form.controls.rows;

  protected readonly totals = signal<InvoiceTotals>({
    subtotal: 0,
    discountAmount: 0,
    vatAmount: 0,
    total: 0,
  });

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

    if (this.templateId) {
      this.recurringService.getById(this.templateId).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.isSucceeded && res.data) {
            const template = res.data;
            this.rowsFormArray.clear();
            template.rows.forEach((row) =>
              this.rowsFormArray.push(
                this.createRow({ service: row.service, quantity: row.quantity, rate: row.rate }),
              ),
            );
            this.form.patchValue({
              customerId: template.customerId,
              frequency: template.frequency,
              nextRunDate: toDateInputValue(template.nextRunDate),
              endDate: template.endDate ? toDateInputValue(template.endDate) : '',
              dueInDays: template.dueInDays,
              vatRate: template.vatRate,
              discountType: template.discountType,
              discountValue: template.discountValue,
              comment: template.comment ?? '',
            });
            this.recomputeTotal();
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.notifications.error(
            extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('recurring.form.loadError')),
          );
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
    return round2((row.quantity || 0) * (row.rate || 0));
  }

  private recomputeTotal(): void {
    const raw = this.form.getRawValue();
    this.totals.set(
      computeInvoiceTotals(raw.rows, raw.discountType, raw.discountValue, raw.vatRate),
    );
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
      frequency: raw.frequency,
      nextRunDate: new Date(raw.nextRunDate).toISOString(),
      endDate: raw.endDate ? new Date(raw.endDate).toISOString() : null,
      dueInDays: raw.dueInDays,
      vatRate: raw.vatRate || 0,
      discountType: raw.discountType,
      discountValue: raw.discountType === DiscountType.None ? 0 : raw.discountValue || 0,
      comment: raw.comment || null,
      rows: raw.rows.map((r) => ({ service: r.service, quantity: r.quantity, rate: r.rate })),
    };

    const request$ = this.templateId
      ? this.recurringService.update(this.templateId, payload)
      : this.recurringService.create(payload);

    request$.subscribe({
      next: (res) => {
        this.submitting.set(false);
        if (res.isSucceeded && res.data) {
          this.notifications.success(
            this.localization.translate(this.isEdit ? 'recurring.form.updateSuccess' : 'recurring.form.createSuccess'),
          );
          this.router.navigateByUrl('/recurring');
        } else {
          this.errorMessage.set(res.message || this.localization.translate('recurring.form.saveError'));
        }
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMessage.set(extractApiError(err, (k) => this.localization.translate(k)));
      },
    });
  }
}
