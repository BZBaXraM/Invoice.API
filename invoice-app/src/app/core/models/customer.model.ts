export interface CustomerResponse {
  id: string;
  name: string;
  address?: string | null;
  email: string;
  phoneNumber?: string | null;
  createdAt: string;
  updatedAt: string;
  deletedAt?: string | null;
}

export interface CreateCustomerRequest {
  name: string;
  address?: string | null;
  email: string;
  phoneNumber?: string | null;
}

export interface UpdateCustomerRequest {
  name: string;
  address?: string | null;
  email: string;
  phoneNumber?: string | null;
}
