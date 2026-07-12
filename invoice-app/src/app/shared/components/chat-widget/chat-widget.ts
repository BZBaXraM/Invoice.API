import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../core/services/chat.service';
import { InvoiceService } from '../../../core/services/invoice.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LocalizationService } from '../../../core/services/localization.service';
import { ChatDisplayMessage, ChatMessageDto } from '../../../core/models/chat.model';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { extractApiError } from '../../utils/api-error';

const STARTER_SUGGESTION_KEYS = [
  'chat.starters.unpaidInvoices',
  'chat.starters.recentInvoices',
  'chat.starters.draftInvoices',
];

@Component({
  selector: 'app-chat-widget',
  imports: [FormsModule, TranslatePipe],
  templateUrl: './chat-widget.html',
})
export class ChatWidgetComponent {
  private readonly chatService = inject(ChatService);
  private readonly invoiceService = inject(InvoiceService);
  private readonly notifications = inject(NotificationService);
  private readonly localization = inject(LocalizationService);

  private readonly scrollAnchor = viewChild<ElementRef<HTMLElement>>('scrollAnchor');

  protected readonly open = signal(false);
  protected readonly messages = signal<ChatDisplayMessage[]>([]);
  protected readonly draft = signal('');
  protected readonly sending = signal(false);
  protected readonly downloadingId = signal<string | null>(null);

  protected readonly starterSuggestions = computed(() =>
    STARTER_SUGGESTION_KEYS.map((key) => this.localization.translate(key)),
  );

  protected toggle(): void {
    this.open.update((v) => !v);
  }

  protected sendDraft(): void {
    this.send(this.draft());
  }

  protected send(text: string): void {
    const message = text.trim();
    if (!message || this.sending()) {
      return;
    }

    const history: ChatMessageDto[] = this.messages().map((m) => ({ role: m.role, content: m.content }));
    this.messages.update((list) => [...list, { role: 'user', content: message }]);
    this.draft.set('');
    this.sending.set(true);
    this.scrollToBottom();

    this.chatService.sendMessage(message, history).subscribe({
      next: (res) => {
        this.sending.set(false);
        if (res.isSucceeded && res.data) {
          this.messages.update((list) => [
            ...list,
            {
              role: 'assistant',
              content: res.data!.reply,
              suggestions: res.data!.suggestions,
              invoiceIds: res.data!.invoiceIds,
            },
          ]);
        } else {
          this.notifications.error(res.message || this.localization.translate('chat.errorGeneric'));
        }
        this.scrollToBottom();
      },
      error: (err) => {
        this.sending.set(false);
        this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('chat.errorGeneric')));
      },
    });
  }

  protected downloadInvoice(id: string): void {
    this.downloadingId.set(id);
    this.invoiceService.exportToPdf(id).subscribe({
      next: (blob) => {
        this.downloadingId.set(null);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `invoice-${id}.pdf`;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.downloadingId.set(null);
        this.notifications.error(extractApiError(err, (k) => this.localization.translate(k), this.localization.translate('chat.errorDownload')));
      },
    });
  }

  private scrollToBottom(): void {
    setTimeout(() => this.scrollAnchor()?.nativeElement.scrollIntoView({ block: 'end' }), 0);
  }
}
