export enum InvoiceStatus {
  Created = 'Created',
  Sent = 'Sent',
  Received = 'Received',
  Paid = 'Paid',
  Cancelled = 'Cancelled',
  Rejected = 'Rejected',
}

export const INVOICE_STATUSES: InvoiceStatus[] = [
  InvoiceStatus.Created,
  InvoiceStatus.Sent,
  InvoiceStatus.Received,
  InvoiceStatus.Paid,
  InvoiceStatus.Cancelled,
  InvoiceStatus.Rejected,
];

export interface InvoiceRowResponse {
  id: string;
  invoiceId: string;
  service: string;
  quantity: number;
  rate: number;
  sum: number;
}

export interface InvoiceResponse {
  id: string;
  customerId: string;
  startDate: string;
  endDate: string;
  rows: InvoiceRowResponse[];
  totalSum: number;
  currency: string;
  comment?: string | null;
  status: InvoiceStatus;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string | null;
}

export interface CreateInvoiceRowRequest {
  service: string;
  quantity: number;
  rate: number;
}

export interface UpdateInvoiceRowRequest {
  service: string;
  quantity: number;
  rate: number;
}

export interface CreateInvoiceRequest {
  customerId: string;
  startDate: string;
  endDate: string;
  rows: CreateInvoiceRowRequest[];
  comment?: string | null;
}

export interface UpdateInvoiceRequest {
  customerId: string;
  startDate: string;
  endDate: string;
  rows: UpdateInvoiceRowRequest[];
  comment?: string | null;
}

export interface UpdateInvoiceStatusRequest {
  status: InvoiceStatus;
}
