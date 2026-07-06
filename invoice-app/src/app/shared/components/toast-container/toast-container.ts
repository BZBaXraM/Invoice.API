import { Component, inject } from '@angular/core';
import { NotificationService } from '../../../core/services/notification.service';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-toast-container',
  imports: [TranslatePipe],
  template: `
    <div class="toast-stack">
      @for (toast of notifications.toasts(); track toast.id) {
        <div class="toast toast--{{ toast.kind }}" role="status">
          <span class="toast__message">{{ toast.message }}</span>
          <button
            type="button"
            class="toast__close"
            (click)="notifications.dismiss(toast.id)"
            [attr.aria-label]="'common.close' | translate"
          >
            &times;
          </button>
        </div>
      }
    </div>
  `,
})
export class ToastContainerComponent {
  protected readonly notifications = inject(NotificationService);
}
