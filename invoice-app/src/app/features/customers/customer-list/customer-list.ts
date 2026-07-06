import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { merge } from 'rxjs';
import { CustomerService } from '../../../core/services/customer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { RealtimeService } from '../../../core/services/realtime.service';
import { CustomerResponse } from '../../../core/models/customer.model';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

type SortField = 'name' | 'email' | 'createdAt';

@Component({
  selector: 'app-customer-list',
  imports: [FormsModule, RouterLink, PaginationComponent, TranslatePipe],
  templateUrl: './customer-list.html',
})
export class CustomerListComponent {
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly realtime = inject(RealtimeService);
  private readonly router = inject(Router);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);
  private searchDebounce?: ReturnType<typeof setTimeout>;

  protected readonly loading = signal(true);
  protected readonly customers = signal<CustomerResponse[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly nameFilter = signal('');
  protected readonly sortBy = signal<SortField>('name');
  protected readonly sortDescending = signal(false);
  protected readonly showArchived = signal(false);

  constructor() {
    this.load();

    merge(
      this.realtime.customerCreated$,
      this.realtime.customerUpdated$,
      this.realtime.customerArchived$,
      this.realtime.customerUnarchived$,
      this.realtime.customerDeleted$,
    )
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.load());
  }

  protected onSearchChange(value: string): void {
    this.nameFilter.set(value);
    clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => {
      this.pageNumber.set(1);
      this.load();
    }, 300);
  }

  protected onShowArchivedChange(value: boolean): void {
    this.showArchived.set(value);
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
    this.customerService
      .getList({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        nameFilter: this.nameFilter() || null,
        sortBy: this.sortBy(),
        sortDescending: this.sortDescending(),
        includeArchived: this.showArchived(),
      })
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.isSucceeded && res.data) {
            this.customers.set(res.data.items);
            this.totalCount.set(res.data.totalCount);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.notifications.error(extractApiError(err, this.localization.translate('customers.list.loadError')));
        },
      });
  }

  protected view(id: string): void {
    this.router.navigate(['/customers', id]);
  }

  protected async archive(customer: CustomerResponse, event: Event): Promise<void> {
    event.stopPropagation();
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('customers.archiveConfirm.title'),
      message: this.localization.translate('customers.list.archiveConfirm.message', { name: customer.name }),
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
      error: (err) => this.notifications.error(extractApiError(err)),
    });
  }

  protected unarchive(customer: CustomerResponse, event: Event): void {
    event.stopPropagation();
    this.customerService.unarchive(customer.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('customers.unarchiveSuccess'));
        this.load();
      },
      error: (err) => this.notifications.error(extractApiError(err, this.localization.translate('customers.unarchiveError'))),
    });
  }

  protected async remove(customer: CustomerResponse, event: Event): Promise<void> {
    event.stopPropagation();
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('customers.deleteConfirm.title'),
      message: this.localization.translate('customers.list.deleteConfirm.message', { name: customer.name }),
      confirmLabel: this.localization.translate('common.actions.delete'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    this.customerService.delete(customer.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('customers.deleteSuccess'));
        this.load();
      },
      error: (err) => {
        const message = err instanceof HttpErrorResponse && err.status === 409
          ? this.localization.translate('customers.deleteError.hasInvoices')
          : extractApiError(err, this.localization.translate('customers.deleteError'));
        this.notifications.error(message);
      },
    });
  }
}
