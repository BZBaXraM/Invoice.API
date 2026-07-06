import { InvoiceStatus } from './invoice.model';

export interface CustomerStatResponse {
  customerId: string;
  customerName: string;
  invoiceCount: number;
  totalSum: number;
  currency: string;
}

export interface ServiceStatResponse {
  service: string;
  invoiceCount: number;
  totalSum: number;
  currency: string;
}

export interface InvoiceStatusStatResponse {
  status: InvoiceStatus;
  count: number;
}
