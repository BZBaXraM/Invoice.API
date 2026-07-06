import { Component, inject } from '@angular/core';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { ConfirmDialogService } from './confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  imports: [TranslatePipe],
  template: `
    @if (dialog.state(); as state) {
      <div class="modal-backdrop" (click)="dialog.resolve(false)">
        <div class="modal-panel" (click)="$event.stopPropagation()">
          <h3>{{ state.title }}</h3>
          <p>{{ state.message }}</p>
          <div class="form-actions">
            <button type="button" class="btn btn--secondary" (click)="dialog.resolve(false)">
              {{ state.cancelLabel ?? ('confirmDialog.defaultCancel' | translate) }}
            </button>
            <button
              type="button"
              class="btn"
              [class.btn--danger]="state.danger"
              [class.btn--accent]="!state.danger"
              (click)="dialog.resolve(true)"
            >
              {{ state.confirmLabel ?? ('confirmDialog.defaultConfirm' | translate) }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: `
    .modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgba(16, 24, 40, 0.45);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1100;
      padding: var(--space-4);
    }

    .modal-panel {
      background: var(--surface-card);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
      padding: var(--space-6);
      width: 100%;
      max-width: 26rem;
    }
  `,
})
export class ConfirmDialogComponent {
  protected readonly dialog = inject(ConfirmDialogService);
}
