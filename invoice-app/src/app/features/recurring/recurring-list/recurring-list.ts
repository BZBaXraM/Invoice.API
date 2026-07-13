import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { merge } from 'rxjs';
import { customerFullName } from '../../../core/models/customer.model';
import { RecurringInvoiceResponse } from '../../../core/models/recurring.model';
import { CustomerService } from '../../../core/services/customer.service';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RealtimeService } from '../../../core/services/realtime.service';
import { RecurringService } from '../../../core/services/recurring.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-recurring-list',
  imports: [RouterLink, PaginationComponent, TranslatePipe],
  templateUrl: './recurring-list.html',
})
export class RecurringListComponent {
  private readonly recurringService = inject(RecurringService);
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly realtime = inject(RealtimeService);
  private readonly router = inject(Router);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);

  protected readonly loading = signal(true);
  protected readonly templates = signal<RecurringInvoiceResponse[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly customerNames = signal<Map<string, string>>(new Map());

  constructor() {
    this.loadCustomerNames();
    this.load();

    merge(
      this.realtime.recurringInvoiceCreated$,
      this.realtime.recurringInvoiceUpdated$,
      this.realtime.recurringInvoiceDeleted$,
    )
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.load());
  }

  protected customerName(id: string): string {
    return this.customerNames().get(id) ?? id.slice(0, 8);
  }

  protected onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.recurringService.getList(this.pageNumber(), this.pageSize()).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSucceeded && res.data) {
          this.templates.set(res.data.items);
          this.totalCount.set(res.data.totalCount);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.notifications.error(
          extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('recurring.list.loadError')),
        );
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

  protected edit(id: string): void {
    this.router.navigate(['/recurring', id, 'edit']);
  }

  protected toggle(template: RecurringInvoiceResponse, event: Event): void {
    event.stopPropagation();
    this.recurringService.toggle(template.id).subscribe({
      next: (res) => {
        if (res.isSucceeded) {
          this.notifications.success(
            this.localization.translate(
              res.data?.isActive ? 'recurring.activatedNotice' : 'recurring.pausedNotice',
            ),
          );
          this.load();
        }
      },
      error: (err) =>
        this.notifications.error(extractApiError(err, (k) => this.localization.translate(k))),
    });
  }

  protected async remove(template: RecurringInvoiceResponse, event: Event): Promise<void> {
    event.stopPropagation();
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('recurring.deleteConfirm.title'),
      message: this.localization.translate('recurring.deleteConfirm.message'),
      confirmLabel: this.localization.translate('common.actions.delete'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    this.recurringService.delete(template.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('recurring.deleteSuccess'));
        this.load();
      },
      error: (err) =>
        this.notifications.error(
          extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('recurring.deleteError')),
        ),
    });
  }
}
