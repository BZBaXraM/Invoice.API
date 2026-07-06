import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { debounceTime, merge } from 'rxjs';
import { ReportService } from '../../core/services/report.service';
import { FormatService } from '../../core/services/format.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NotificationService } from '../../core/services/notification.service';
import { RealtimeService } from '../../core/services/realtime.service';
import { CustomerStatResponse, InvoiceStatusStatResponse, ServiceStatResponse } from '../../core/models/report.model';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { extractApiError } from '../../shared/utils/api-error';

function toInputDate(date: Date): string {
  return date.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-dashboard',
  imports: [FormsModule, TranslatePipe],
  templateUrl: './dashboard.html',
})
export class DashboardComponent {
  private readonly reportService = inject(ReportService);
  private readonly notifications = inject(NotificationService);
  private readonly realtime = inject(RealtimeService);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);

  // Reports match invoices whose *entire* period (start and end) falls
  // inside [from, to], and invoice periods can extend past today — so the
  // default window is widened well beyond "the last 30 days" to actually
  // catch them (90 days back, 90 days forward).
  private readonly today = new Date();
  private readonly rangeStart = new Date(this.today.getTime() - 90 * 24 * 60 * 60 * 1000);
  private readonly rangeEnd = new Date(this.today.getTime() + 90 * 24 * 60 * 60 * 1000);

  protected readonly from = signal(toInputDate(this.rangeStart));
  protected readonly to = signal(toInputDate(this.rangeEnd));
  protected readonly loading = signal(true);

  protected readonly customerStats = signal<CustomerStatResponse[]>([]);
  protected readonly serviceStats = signal<ServiceStatResponse[]>([]);
  protected readonly statusStats = signal<InvoiceStatusStatResponse[]>([]);

  protected readonly totalRevenue = computed(() =>
    this.customerStats().reduce((sum, s) => sum + s.totalSum, 0),
  );
  protected readonly totalInvoices = computed(() =>
    this.statusStats().reduce((sum, s) => sum + s.count, 0),
  );
  protected readonly maxCustomerSum = computed(() =>
    Math.max(1, ...this.customerStats().map((s) => s.totalSum)),
  );
  protected readonly maxServiceSum = computed(() => Math.max(1, ...this.serviceStats().map((s) => s.totalSum)));
  protected readonly maxStatusCount = computed(() => Math.max(1, ...this.statusStats().map((s) => s.count)));

  constructor() {
    this.load();

    merge(
      this.realtime.customerCreated$,
      this.realtime.customerUpdated$,
      this.realtime.customerArchived$,
      this.realtime.customerDeleted$,
      this.realtime.invoiceCreated$,
      this.realtime.invoiceUpdated$,
      this.realtime.invoiceStatusChanged$,
      this.realtime.invoiceArchived$,
      this.realtime.invoiceDeleted$,
    )
      .pipe(debounceTime(300), takeUntilDestroyed())
      .subscribe(() => this.load());
  }

  protected onRangeChange(): void {
    this.load();
  }

  protected load(): void {
    // Date-only strings parse as UTC midnight, so "to" must be pushed to the
    // end of that calendar day — otherwise invoices ending later that day
    // (or reported in a timezone ahead of UTC) fall outside the range.
    const from = new Date(`${this.from()}T00:00:00.000Z`);
    const to = new Date(`${this.to()}T23:59:59.999Z`);
    if (Number.isNaN(from.getTime()) || Number.isNaN(to.getTime())) {
      return;
    }

    this.loading.set(true);

    this.reportService.getCustomerStats(from, to).subscribe({
      next: (res) => {
        if (res.isSucceeded && res.data) {
          this.customerStats.set([...res.data].sort((a, b) => b.totalSum - a.totalSum));
        }
      },
      error: (err) => this.notifications.error(extractApiError(err, this.localization.translate('dashboard.customerStatsError'))),
    });

    this.reportService.getServiceStats(from, to).subscribe({
      next: (res) => {
        if (res.isSucceeded && res.data) {
          this.serviceStats.set([...res.data].sort((a, b) => b.totalSum - a.totalSum));
        }
      },
      error: (err) => this.notifications.error(extractApiError(err, this.localization.translate('dashboard.serviceStatsError'))),
    });

    this.reportService.getInvoiceStatusStats(from, to).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.isSucceeded && res.data) {
          this.statusStats.set(res.data);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.notifications.error(extractApiError(err, this.localization.translate('dashboard.statusStatsError')));
      },
    });
  }
}
