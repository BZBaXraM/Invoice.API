export enum InvoiceStatus {
  Created = 'Created',
  Sent = 'Sent',
  Received = 'Received',
  Paid = 'Paid',
  Cancelled = 'Cancelled',
  Rejected = 'Rejected',
  PartiallyPaid = 'PartiallyPaid',
  Overdue = 'Overdue',
}

export const INVOICE_STATUSES: InvoiceStatus[] = [
  InvoiceStatus.Created,
  InvoiceStatus.Sent,
  InvoiceStatus.Received,
  InvoiceStatus.Paid,
  InvoiceStatus.PartiallyPaid,
  InvoiceStatus.Overdue,
  InvoiceStatus.Cancelled,
  InvoiceStatus.Rejected,
];

export enum DiscountType {
  None = 'None',
  Percent = 'Percent',
  Fixed = 'Fixed',
}

export const DISCOUNT_TYPES: DiscountType[] = [
  DiscountType.None,
  DiscountType.Percent,
  DiscountType.Fixed,
];

export interface InvoiceRowResponse {
  id: string;
  invoiceId: string;
  service: string;
  quantity: number;
  rate: number;
  sum: number;
}

export interface PaymentResponse {
  id: string;
  invoiceId: string;
  amount: number;
  paymentDate: string;
  note?: string | null;
  createdAt: string;
}

export interface CreatePaymentRequest {
  amount: number;
  paymentDate: string;
  note?: string | null;
}

export interface InvoiceResponse {
  id: string;
  customerId: string;
  invoiceNumber: number;
  number: string;
  startDate: string;
  endDate: string;
  dueDate?: string | null;
  rows: InvoiceRowResponse[];
  vatRate: number;
  discountType: DiscountType;
  discountValue: number;
  subtotal: number;
  discountAmount: number;
  vatAmount: number;
  totalSum: number;
  paidAmount: number;
  balanceDue: number;
  payments: PaymentResponse[];
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
  dueDate?: string | null;
  rows: CreateInvoiceRowRequest[];
  vatRate: number;
  discountType: DiscountType;
  discountValue: number;
  comment?: string | null;
}

export interface UpdateInvoiceRequest {
  customerId: string;
  startDate: string;
  endDate: string;
  dueDate?: string | null;
  rows: UpdateInvoiceRowRequest[];
  vatRate: number;
  discountType: DiscountType;
  discountValue: number;
  comment?: string | null;
}

export interface UpdateInvoiceStatusRequest {
  status: InvoiceStatus;
}

/**
 * Mirrors the backend InvoiceTotalsCalculator: round half away from zero to 2 decimals,
 * discount applied to the subtotal, VAT charged on the discounted base.
 */
export function round2(value: number): number {
  return Math.sign(value) * Math.round(Math.abs(value) * 100) / 100;
}

export interface InvoiceTotals {
  subtotal: number;
  discountAmount: number;
  vatAmount: number;
  total: number;
}

export function computeInvoiceTotals(
  rows: { quantity: number; rate: number }[],
  discountType: DiscountType,
  discountValue: number,
  vatRate: number,
): InvoiceTotals {
  const subtotal = rows.reduce((acc, row) => acc + round2((row.quantity || 0) * (row.rate || 0)), 0);

  let discountAmount = 0;
  if (discountType === DiscountType.Percent) {
    discountAmount = round2((subtotal * (discountValue || 0)) / 100);
  } else if (discountType === DiscountType.Fixed) {
    discountAmount = round2(Math.min(discountValue || 0, subtotal));
  }

  const vatAmount = round2(((subtotal - discountAmount) * (vatRate || 0)) / 100);
  const total = subtotal - discountAmount + vatAmount;

  return { subtotal, discountAmount, vatAmount, total };
}
