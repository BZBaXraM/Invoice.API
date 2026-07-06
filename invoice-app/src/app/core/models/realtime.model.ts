import { InvoiceStatus } from './invoice.model';

export interface CustomerIdEvent {
  customerId: string;
}

export interface InvoiceIdEvent {
  invoiceId: string;
}

export interface InvoiceStatusChangedEvent {
  invoiceId: string;
  status: InvoiceStatus;
}
