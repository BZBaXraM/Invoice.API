import { isPlatformBrowser } from '@angular/common';
import { Component, OnDestroy, OnInit, PLATFORM_ID, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { LocalizationService } from '../../core/services/localization.service';
import { NotificationService } from '../../core/services/notification.service';
import { RealtimeService } from '../../core/services/realtime.service';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog.service';
import { LanguageSwitcherComponent } from '../../shared/components/language-switcher/language-switcher';
import { ThemeToggleComponent } from '../../shared/components/theme-toggle/theme-toggle';
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

const SIDEBAR_OPEN_KEY = 'invoice_sidebar_open';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LanguageSwitcherComponent, ThemeToggleComponent, TranslatePipe],
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
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  protected readonly navItems = NAV_ITEMS;
  protected readonly claims = this.auth.claims;
  protected readonly sidebarOpen = signal(this.resolveInitialSidebarOpen());

  ngOnInit(): void {
    this.realtime.connect();
  }

  ngOnDestroy(): void {
    this.realtime.disconnect();
  }

  protected toggleSidebar(): void {
    const next = !this.sidebarOpen();
    this.sidebarOpen.set(next);
    if (this.isBrowser) {
      localStorage.setItem(SIDEBAR_OPEN_KEY, String(next));
    }
  }

  private resolveInitialSidebarOpen(): boolean {
    if (!this.isBrowser) {
      return true;
    }
    const stored = localStorage.getItem(SIDEBAR_OPEN_KEY);
    if (stored !== null) {
      return stored === 'true';
    }
    // No saved preference yet (first visit on this device): default to closed
    // on narrow viewports so the drawer doesn't cover the whole screen.
    return window.innerWidth > 900;
  }

  protected get sidebarToggleLabel(): string {
    return this.sidebarOpen()
      ? this.localization.translate('shell.sidebar.hide')
      : this.localization.translate('shell.sidebar.show');
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
