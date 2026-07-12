export interface AdminUser {
  id: string;
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  phoneNumber?: string | null;
  isEmailConfirmed: boolean;
  isActive: boolean;
  role: string;
  createdAt: string;
}
