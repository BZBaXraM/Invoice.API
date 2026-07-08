export interface CustomerResponse {
  id: string;
  firstName: string;
  lastName: string;
  companyName?: string | null;
  address?: string | null;
  email: string;
  phoneNumber?: string | null;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string | null;
}

export interface CreateCustomerRequest {
  firstName: string;
  lastName: string;
  companyName?: string | null;
  address?: string | null;
  email: string;
  phoneNumber?: string | null;
}

export interface UpdateCustomerRequest {
  firstName: string;
  lastName: string;
  companyName?: string | null;
  address?: string | null;
  email: string;
  phoneNumber?: string | null;
}

export function customerFullName(customer: Pick<CustomerResponse, 'firstName' | 'lastName'>): string {
  return `${customer.firstName} ${customer.lastName}`;
}
