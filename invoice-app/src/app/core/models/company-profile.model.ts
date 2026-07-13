export interface CompanyProfileResponse {
  id: string;
  companyName: string;
  voen: string;
  bankName?: string | null;
  bankVoen?: string | null;
  iban?: string | null;
  bankAccount?: string | null;
  swiftCode?: string | null;
  hasLogo: boolean;
  hasSignature: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface UpsertCompanyProfileRequest {
  companyName: string;
  voen: string;
  bankName?: string | null;
  bankVoen?: string | null;
  iban?: string | null;
  bankAccount?: string | null;
  swiftCode?: string | null;
}

export type CompanyImageKind = 'logo' | 'signature';
