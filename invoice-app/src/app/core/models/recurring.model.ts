import { DiscountType } from './invoice.model';

export enum RecurrenceFrequency {
  Weekly = 'Weekly',
  Monthly = 'Monthly',
  Quarterly = 'Quarterly',
  Yearly = 'Yearly',
}

export const RECURRENCE_FREQUENCIES: RecurrenceFrequency[] = [
  RecurrenceFrequency.Weekly,
  RecurrenceFrequency.Monthly,
  RecurrenceFrequency.Quarterly,
  RecurrenceFrequency.Yearly,
];

export interface RecurringInvoiceRowRequest {
  service: string;
  quantity: number;
  rate: number;
}

export interface RecurringInvoiceRowResponse {
  id: string;
  service: string;
  quantity: number;
  rate: number;
}

export interface CreateRecurringInvoiceRequest {
  customerId: string;
  frequency: RecurrenceFrequency;
  nextRunDate: string;
  endDate?: string | null;
  dueInDays: number;
  vatRate: number;
  discountType: DiscountType;
  discountValue: number;
  comment?: string | null;
  rows: RecurringInvoiceRowRequest[];
}

export type UpdateRecurringInvoiceRequest = CreateRecurringInvoiceRequest;

export interface RecurringInvoiceResponse {
  id: string;
  customerId: string;
  frequency: RecurrenceFrequency;
  nextRunDate: string;
  endDate?: string | null;
  isActive: boolean;
  dueInDays: number;
  vatRate: number;
  discountType: DiscountType;
  discountValue: number;
  comment?: string | null;
  rows: RecurringInvoiceRowResponse[];
  createdAt: string;
  updatedAt: string;
}

export interface RecurringInvoiceIdEvent {
  recurringInvoiceId: string;
}
