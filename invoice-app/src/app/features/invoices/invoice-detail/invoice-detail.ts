import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CustomerService } from '../../../core/services/customer.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RealtimeService } from '../../../core/services/realtime.service';
import { CustomerResponse } from '../../../core/models/customer.model';
import {
  DiscountType,
  INVOICE_STATUSES,
  InvoiceResponse,
  InvoiceStatus,
  PaymentResponse,
} from '../../../core/models/invoice.model';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

const PAYABLE_STATUSES: InvoiceStatus[] = [
  InvoiceStatus.Sent,
  InvoiceStatus.Received,
  InvoiceStatus.PartiallyPaid,
  InvoiceStatus.Overdue,
];

@Component({
  selector: 'app-invoice-detail',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './invoice-detail.html',
})
export class InvoiceDetailComponent {
  private readonly fb = inject(FormBuilder);
  private readonly invoiceService = inject(InvoiceService);
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly realtime = inject(RealtimeService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);

  private readonly invoiceId = this.route.snapshot.paramMap.get('id')!;

  protected readonly loading = signal(true);
  protected readonly invoice = signal<InvoiceResponse | null>(null);
  protected readonly customer = signal<CustomerResponse | null>(null);
  protected readonly exporting = signal(false);
  protected readonly updatingStatus = signal(false);
  protected readonly statuses = INVOICE_STATUSES;
  protected readonly DiscountType = DiscountType;

  protected readonly savingPayment = signal(false);
  protected readonly canRecordPayment = computed(() => {
    const invoice = this.invoice();
    return !!invoice && PAYABLE_STATUSES.includes(invoice.status) && invoice.balanceDue > 0;
  });

  protected readonly paymentForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    paymentDate: [new Date().toISOString().slice(0, 10), [Validators.required]],
    note: [''],
  });

  constructor() {
    this.load();

    this.realtime.invoiceUpdated$
      .pipe(takeUntilDestroyed())
      .subscribe((invoice) => invoice.id === this.invoiceId && this.load());

    this.realtime.invoiceStatusChanged$
      .pipe(takeUntilDestroyed())
      .subscribe((event) => event.invoiceId === this.invoiceId && this.load());

    this.realtime.invoiceArchived$
      .pipe(takeUntilDestroyed())
      .subscribe((event) => event.invoiceId === this.invoiceId && this.load());

    this.realtime.invoiceDeleted$.pipe(takeUntilDestroyed()).subscribe((event) => {
      if (event.invoiceId === this.invoiceId) {
        this.notifications.info(this.localization.translate('invoices.detail.deletedNotice'));
        this.router.navigateByUrl('/invoices');
      }
    });
  }

  private load(): void {
    this.invoiceService.getById(this.invoiceId).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSucceeded && res.data) {
          this.invoice.set(res.data);
          this.customerService.getById(res.data.customerId).subscribe({
            next: (customerRes) => {
              if (customerRes.isSucceeded && customerRes.data) {
                this.customer.set(customerRes.data);
              }
            },
          });
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.detail.loadError')));
      },
    });
  }

  protected changeStatus(status: string): void {
    const invoice = this.invoice();
    if (!invoice || !status || status === invoice.status) {
      return;
    }
    this.updatingStatus.set(true);
    this.invoiceService.updateStatus(invoice.id, status as InvoiceStatus).subscribe({
      next: (res) => {
        this.updatingStatus.set(false);
        if (res.isSucceeded) {
          this.notifications.success(this.localization.translate('invoices.detail.statusUpdateSuccess'));
          this.load();
        } else {
          this.notifications.error(res.message || this.localization.translate('invoices.detail.statusUpdateError'));
        }
      },
      error: (err) => {
        this.updatingStatus.set(false);
        this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.detail.statusUpdateError')));
      },
    });
  }

  protected exportPdf(): void {
    const invoice = this.invoice();
    if (!invoice) {
      return;
    }
    this.exporting.set(true);
    this.invoiceService.exportToPdf(invoice.id).subscribe({
      next: (blob) => {
        this.exporting.set(false);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `invoice-${invoice.number}.pdf`;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.exporting.set(false);
        this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.detail.exportError')));
      },
    });
  }

  protected addPayment(): void {
    const invoice = this.invoice();
    if (!invoice || this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    const raw = this.paymentForm.getRawValue();
    this.savingPayment.set(true);
    this.invoiceService
      .addPayment(invoice.id, {
        amount: raw.amount,
        paymentDate: new Date(raw.paymentDate).toISOString(),
        note: raw.note || null,
      })
      .subscribe({
        next: (res) => {
          this.savingPayment.set(false);
          if (res.isSucceeded && res.data) {
            this.notifications.success(this.localization.translate('invoices.payments.addSuccess'));
            this.invoice.set(res.data);
            this.paymentForm.reset({
              amount: 0,
              paymentDate: new Date().toISOString().slice(0, 10),
              note: '',
            });
          } else {
            this.notifications.error(
              this.localization.translate(res.message) || this.localization.translate('invoices.payments.addError'),
            );
          }
        },
        error: (err) => {
          this.savingPayment.set(false);
          this.notifications.error(
            extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.payments.addError')),
          );
        },
      });
  }

  protected async deletePayment(payment: PaymentResponse): Promise<void> {
    const invoice = this.invoice();
    if (!invoice) {
      return;
    }

    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('invoices.payments.deleteConfirm.title'),
      message: this.localization.translate('invoices.payments.deleteConfirm.message'),
      confirmLabel: this.localization.translate('common.actions.delete'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }

    this.invoiceService.deletePayment(invoice.id, payment.id).subscribe({
      next: (res) => {
        if (res.isSucceeded && res.data) {
          this.notifications.success(this.localization.translate('invoices.payments.deleteSuccess'));
          this.invoice.set(res.data);
        }
      },
      error: (err) =>
        this.notifications.error(
          extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.payments.deleteError')),
        ),
    });
  }

  protected async archive(): Promise<void> {
    const invoice = this.invoice();
    if (!invoice) {
      return;
    }
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('invoices.archiveConfirm.title'),
      message: this.localization.translate('invoices.archiveConfirm.message'),
      confirmLabel: this.localization.translate('common.actions.archive'),
    });
    if (!confirmed) {
      return;
    }
    this.invoiceService.archive(invoice.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('invoices.archiveSuccess'));
        this.load();
      },
      error: (err) => this.notifications.error(extractApiError(err, (k) => this.localization.translate(k))),
    });
  }

  protected async remove(): Promise<void> {
    const invoice = this.invoice();
    if (!invoice) {
      return;
    }
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('invoices.deleteConfirm.title'),
      message: this.localization.translate('invoices.detail.deleteConfirm.message'),
      confirmLabel: this.localization.translate('common.actions.delete'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    this.invoiceService.delete(invoice.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('invoices.deleteSuccess'));
        this.router.navigateByUrl('/invoices');
      },
      error: (err) => this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.deleteError'))),
    });
  }
}
