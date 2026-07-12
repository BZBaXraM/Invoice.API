import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { merge } from 'rxjs';
import { CustomerService } from '../../../core/services/customer.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RealtimeService } from '../../../core/services/realtime.service';
import { CustomerResponse, customerFullName } from '../../../core/models/customer.model';
import { InvoiceResponse } from '../../../core/models/invoice.model';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-customer-detail',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './customer-detail.html',
})
export class CustomerDetailComponent {
  private readonly customerService = inject(CustomerService);
  private readonly invoiceService = inject(InvoiceService);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly realtime = inject(RealtimeService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);

  private readonly customerId = this.route.snapshot.paramMap.get('id')!;

  protected readonly loading = signal(true);
  protected readonly customer = signal<CustomerResponse | null>(null);
  protected readonly invoices = signal<InvoiceResponse[]>([]);
  protected readonly invoicesLoading = signal(true);

  constructor() {
    this.load();
    this.loadInvoices();

    this.realtime.customerUpdated$
      .pipe(takeUntilDestroyed())
      .subscribe((customer) => customer.id === this.customerId && this.load());

    merge(this.realtime.customerArchived$, this.realtime.customerUnarchived$)
      .pipe(takeUntilDestroyed())
      .subscribe((event) => event.customerId === this.customerId && this.load());

    this.realtime.customerDeleted$.pipe(takeUntilDestroyed()).subscribe((event) => {
      if (event.customerId === this.customerId) {
        this.notifications.info(this.localization.translate('customers.detail.deletedNotice'));
        this.router.navigateByUrl('/customers');
      }
    });

    // Cheap to just re-fetch this customer's (small) invoice page on any invoice
    // change, rather than trying to filter every event shape by customer id.
    merge(
      this.realtime.invoiceCreated$,
      this.realtime.invoiceUpdated$,
      this.realtime.invoiceStatusChanged$,
      this.realtime.invoiceArchived$,
      this.realtime.invoiceDeleted$,
    )
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.loadInvoices());
  }

  private load(): void {
    this.customerService.getById(this.customerId).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSucceeded && res.data) {
          this.customer.set(res.data);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('customers.detail.loadError')));
      },
    });
  }

  private loadInvoices(): void {
    this.invoiceService
      .getList({ pageNumber: 1, pageSize: 10, customerId: this.customerId, sortBy: 'createdAt', sortDescending: true })
      .subscribe({
        next: (res) => {
          this.invoicesLoading.set(false);
          if (res.isSucceeded && res.data) {
            this.invoices.set(res.data.items);
          }
        },
        error: () => this.invoicesLoading.set(false),
      });
  }

  protected async archive(): Promise<void> {
    const customer = this.customer();
    if (!customer) {
      return;
    }
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('customers.archiveConfirm.title'),
      message: this.localization.translate('customers.detail.archiveConfirm.message', { name: customerFullName(customer) }),
      confirmLabel: this.localization.translate('common.actions.archive'),
    });
    if (!confirmed) {
      return;
    }
    this.customerService.archive(customer.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('customers.archiveSuccess'));
        this.load();
      },
      error: (err) => this.notifications.error(extractApiError(err, (k) => this.localization.translate(k))),
    });
  }

  protected unarchive(): void {
    const customer = this.customer();
    if (!customer) {
      return;
    }
    this.customerService.unarchive(customer.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('customers.unarchiveSuccess'));
        this.load();
      },
      error: (err) => this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('customers.unarchiveError'))),
    });
  }

  protected async remove(): Promise<void> {
    const customer = this.customer();
    if (!customer) {
      return;
    }
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('customers.deleteConfirm.title'),
      message: this.localization.translate('customers.detail.deleteConfirm.message', { name: customerFullName(customer) }),
      confirmLabel: this.localization.translate('common.actions.delete'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    this.customerService.delete(customer.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('customers.deleteSuccess'));
        this.router.navigateByUrl('/customers');
      },
      error: (err) => {
        const message = err instanceof HttpErrorResponse && err.status === 409
          ? this.localization.translate('customers.deleteError.hasInvoices')
          : extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('customers.deleteError'));
        this.notifications.error(message);
      },
    });
  }
}
