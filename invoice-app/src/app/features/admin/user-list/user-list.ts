import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin.service';
import { AuthService } from '../../../core/services/auth.service';
import { BackupService } from '../../../core/services/backup.service';
import { FormatService } from '../../../core/services/format.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AdminUser } from '../../../core/models/admin.model';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { extractApiError } from '../../../shared/utils/api-error';

@Component({
  selector: 'app-admin-user-list',
  imports: [FormsModule, PaginationComponent, TranslatePipe],
  templateUrl: './user-list.html',
})
export class AdminUserListComponent {
  private readonly adminService = inject(AdminService);
  private readonly backupService = inject(BackupService);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly auth = inject(AuthService);
  protected readonly localization = inject(LocalizationService);
  protected readonly format = inject(FormatService);
  private searchDebounce?: ReturnType<typeof setTimeout>;

  protected readonly loading = signal(true);
  protected readonly users = signal<AdminUser[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly searchFilter = signal('');
  protected readonly downloadingBackup = signal(false);

  constructor() {
    this.load();
  }

  protected downloadBackup(): void {
    this.downloadingBackup.set(true);
    this.backupService.downloadBackup().subscribe({
      next: (blob) => {
        this.downloadingBackup.set(false);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `backup-${new Date().toISOString().slice(0, 10)}.dump`;
        link.click();
        URL.revokeObjectURL(url);
        this.notifications.success(this.localization.translate('admin.backup.success'));
      },
      error: (err) => {
        this.downloadingBackup.set(false);
        this.notifications.error(
          extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('admin.backup.error')),
        );
      },
    });
  }

  protected onSearchChange(value: string): void {
    this.searchFilter.set(value);
    clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => {
      this.pageNumber.set(1);
      this.load();
    }, 300);
  }

  protected onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.adminService
      .getUsers({
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        searchFilter: this.searchFilter() || null,
      })
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.isSucceeded && res.data) {
            this.users.set(res.data.items);
            this.totalCount.set(res.data.totalCount);
          }
        },
        error: (err) => {
          this.loading.set(false);
          this.notifications.error(
            extractApiError(err, this.translateError, this.localization.translate('admin.users.loadError')),
          );
        },
      });
  }

  protected fullName(user: AdminUser): string {
    return `${user.firstName} ${user.lastName}`.trim();
  }

  protected canManage(user: AdminUser): boolean {
    return user.role !== 'Admin' && user.id !== this.auth.claims().userId;
  }

  protected async disable(user: AdminUser): Promise<void> {
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('admin.users.disableConfirm.title'),
      message: this.localization.translate('admin.users.disableConfirm.message', { name: this.fullName(user) }),
      confirmLabel: this.localization.translate('admin.users.actions.disable'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    this.adminService.disableUser(user.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('admin.users.disableSuccess'));
        this.load();
      },
      error: (err) =>
        this.notifications.error(
          extractApiError(err, this.translateError, this.localization.translate('admin.users.disableError')),
        ),
    });
  }

  protected enable(user: AdminUser): void {
    this.adminService.enableUser(user.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('admin.users.enableSuccess'));
        this.load();
      },
      error: (err) =>
        this.notifications.error(
          extractApiError(err, this.translateError, this.localization.translate('admin.users.enableError')),
        ),
    });
  }

  protected async remove(user: AdminUser): Promise<void> {
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('admin.users.deleteConfirm.title'),
      message: this.localization.translate('admin.users.deleteConfirm.message', { name: this.fullName(user) }),
      confirmLabel: this.localization.translate('admin.users.actions.delete'),
      danger: true,
    });
    if (!confirmed) {
      return;
    }
    this.adminService.deleteUser(user.id).subscribe({
      next: () => {
        this.notifications.success(this.localization.translate('admin.users.deleteSuccess'));
        this.load();
      },
      error: (err) =>
        this.notifications.error(
          extractApiError(err, this.translateError, this.localization.translate('admin.users.deleteError')),
        ),
    });
  }

  private readonly translateError = (key: string): string => this.localization.translate(key);
}
