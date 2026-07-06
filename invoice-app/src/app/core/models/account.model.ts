export interface RegisterRequest {
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  address?: string | null;
  phoneNumber?: string | null;
  password: string;
}

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpireTime: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ConfirmEmailCodeRequest {
  code: string;
}

export interface RequestConfirmationCodeRequest {
  email: string;
}

export interface ForgetPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  code: string;
  newPassword: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface UpdateProfileRequest {
  firstName?: string | null;
  lastName?: string | null;
  username?: string | null;
  email?: string | null;
  address?: string | null;
  phoneNumber?: string | null;
}

export interface UserResponse {
  id: string;
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  address?: string | null;
  phoneNumber?: string | null;
  createdAt: string;
  updatedAt: string;
}
