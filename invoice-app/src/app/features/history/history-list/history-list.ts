import { Component, inject, signal } from '@angular/core';
import { AuditChangeEntry, AuditLogResponse, parseAuditChanges } from '../../../core/models/audit.model';
import { AuditService } from '../../../core/services/audit.service';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

const ENTITY_TYPES = [
  'Invoice',
  'InvoiceRow',
  'Customer',
  'Payment',
  'RecurringInvoice',
  'RecurringInvoiceRow',
  'CompanyProfile',
];

@Component({
  selector: 'app-history-list',
  imports: [PaginationComponent, TranslatePipe],
  templateUrl: './history-list.html',
})
export class HistoryListComponent {
  private readonly auditService = inject(AuditService);
  private readonly notifications = inject(NotificationService);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);

  protected readonly entityTypes = ENTITY_TYPES;
  protected readonly loading = signal(true);
  protected readonly entries = signal<AuditLogResponse[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = signal(20);
  protected readonly entityTypeFilter = signal('');
  protected readonly expandedId = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected onEntityTypeChange(entityType: string): void {
    this.entityTypeFilter.set(entityType);
    this.pageNumber.set(1);
    this.load();
  }

  protected onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  protected toggleExpanded(id: string): void {
    this.expandedId.set(this.expandedId() === id ? null : id);
  }

  protected changes(entry: AuditLogResponse): AuditChangeEntry[] {
    return parseAuditChanges(entry.changesJson);
  }

  protected renderValue(value: unknown): string {
    if (value === null || value === undefined || value === '') {
      return '—';
    }
    if (typeof value === 'object') {
      return JSON.stringify(value);
    }
    return String(value);
  }

  protected entityLabel(entityType: string): string {
    const key = `history.entity.${entityType.charAt(0).toLowerCase()}${entityType.slice(1)}`;
    const translated = this.localization.translate(key);
    return translated === key ? entityType : translated;
  }

  protected load(): void {
    this.loading.set(true);
    this.auditService
      .getList(this.pageNumber(), this.pageSize(), this.entityTypeFilter() || null)
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.isSucceeded && res.data) {
            this.entries.set(res.data.items);
            this.totalCount.set(res.data.totalCount);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.notifications.error(
            extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('history.loadError')),
          );
        },
      });
  }
}
