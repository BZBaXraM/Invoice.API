import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { merge } from 'rxjs';
import { CustomerService } from '../../../core/services/customer.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RealtimeService } from '../../../core/services/realtime.service';
import { customerFullName } from '../../../core/models/customer.model';
import { INVOICE_STATUSES, InvoiceResponse, InvoiceStatus } from '../../../core/models/invoice.model';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

type SortField = 'startDate' | 'endDate' | 'totalSum' | 'createdAt';

@Component({
  selector: 'app-invoice-list',
  imports: [FormsModule, RouterLink, PaginationComponent, TranslatePipe],
  templateUrl: './invoice-list.html',
})
export class InvoiceListComponent {
  private readonly invoiceService = inject(InvoiceService);
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly realtime = inject(RealtimeService);
  private readonly router = inject(Router);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);

  protected readonly statuses = INVOICE_STATUSES;
  protected readonly loading = signal(true);
  protected readonly invoices = signal<InvoiceResponse[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly statusFilter = signal<InvoiceStatus | ''>('');
  protected readonly sortBy = signal<SortField>('createdAt');
  protected readonly sortDescending = signal(true);
  protected readonly customerNames = signal<Map<string, string>>(new Map());

  constructor() {
    this.loadCustomerNames();
    this.load();

    merge(
      this.realtime.invoiceCreated$,
      this.realtime.invoiceUpdated$,
      this.realtime.invoiceStatusChanged$,
      this.realtime.invoiceArchived$,
      this.realtime.invoiceDeleted$,
    )
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.load());
  }

  protected customerName(id: string): string {
    return this.customerNames().get(id) ?? id.slice(0, 8);
  }

  protected onStatusChange(status: string): void {
    this.statusFilter.set(status as InvoiceStatus | '');
    this.pageNumber.set(1);
    this.load();
  }

  protected sort(field: SortField): void {
    if (this.sortBy() === field) {
      this.sortDescending.set(!this.sortDescending());
    } else {
      this.sortBy.set(field);
      this.sortDescending.set(false);
    }
    this.load();
  }

  protected onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.invoiceService
      .getList({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        status: this.statusFilter() || null,
        sortBy: this.sortBy(),
        sortDescending: this.sortDescending(),
      })
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.isSucceeded && res.data) {
            this.invoices.set(res.data.items);
            this.totalCount.set(res.data.totalCount);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.list.loadError')));
        },
      });
  }

  private loadCustomerNames(): void {
    this.customerService.getList({ pageNumber: 1, pageSize: 200 }).subscribe({
      next: (res) => {
        if (res.isSucceeded && res.data) {
          this.customerNames.set(new Map(res.data.items.map((c) => [c.id, customerFullName(c)])));
        }
      },
    });
  }

  protected view(id: string): void {
    this.router.navigate(['/invoices', id]);
  }

  protected async remove(invoice: InvoiceResponse, event: Event): Promise<void> {
    event.stopPropagation();
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('invoices.deleteConfirm.title'),
      message: this.localization.translate('invoices.list.deleteConfirm.message'),
      confirmLabel: this.localization.translate('common.actions.delete'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    this.invoiceService.delete(invoice.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('invoices.deleteSuccess'));
        this.load();
      },
      error: (err) => this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('invoices.deleteError'))),
    });
  }
}
