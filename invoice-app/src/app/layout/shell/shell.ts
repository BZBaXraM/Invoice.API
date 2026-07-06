import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NotificationService } from '../../core/services/notification.service';
import { RealtimeService } from '../../core/services/realtime.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { LanguageSwitcherComponent } from '../../shared/components/language-switcher/language-switcher';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

interface NavItem {
  labelKey: string;
  path: string;
  icon: string;
}

const NAV_ITEMS: NavItem[] = [
  { labelKey: 'nav.dashboard', path: '/dashboard', icon: 'grid' },
  { labelKey: 'nav.customers', path: '/customers', icon: 'users' },
  { labelKey: 'nav.invoices', path: '/invoices', icon: 'file' },
  { labelKey: 'nav.profile', path: '/profile', icon: 'user' },
];

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LanguageSwitcherComponent, TranslatePipe],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class ShellComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly realtime = inject(RealtimeService);
  private readonly localization = inject(LocalizationService);

  protected readonly navItems = NAV_ITEMS;
  protected readonly claims = this.auth.claims;
  protected readonly realtimeConnected = this.realtime.connected;

  ngOnInit(): void {
    this.realtime.connect();
  }

  ngOnDestroy(): void {
    this.realtime.disconnect();
  }

  protected async logout(): Promise<void> {
    const confirmed = await this.confirmDialog.ask({
      title: this.localization.translate('shell.logout.title'),
      message: this.localization.translate('shell.logout.message'),
      confirmLabel: this.localization.translate('shell.logout.confirmLabel'),
    });

    if (!confirmed) {
      return;
    }

    this.realtime.disconnect();
    this.auth.logout().subscribe({
      next: () => {
        this.notifications.info(this.localization.translate('shell.logout.successMessage'));
        this.router.navigateByUrl('/login');
      },
      error: () => {
        this.auth.clearSession();
        this.router.navigateByUrl('/login');
      },
    });
  }
}
