import { apiRequest } from "./apiClient";
import {
  ApiResult,
  AuthResponseData,
  AuthUserDto,
  LoginPayload,
  RegisterPayload,
} from "@/types";

export const authService = {
  async register(payload: RegisterPayload): Promise<AuthResponseData> {
    const response = await apiRequest<ApiResult<AuthResponseData>>("/auth/register", {
      method: "POST",
      body: JSON.stringify(payload),
    });

    if (!response.data) {
      throw new Error(response.message || "Registration failed.");
    }

    return response.data;
  },

  async login(payload: LoginPayload): Promise<AuthResponseData> {
    const response = await apiRequest<ApiResult<AuthResponseData>>("/auth/login", {
      method: "POST",
      body: JSON.stringify(payload),
    });

    if (!response.data) {
      throw new Error(response.message || "Login failed.");
    }

    return response.data;
  },

  async getMe(): Promise<AuthUserDto> {
    const response = await apiRequest<ApiResult<AuthUserDto>>("/auth/me", {
      method: "GET",
    });

    if (!response.data) {
      throw new Error("Unable to fetch user profile.");
    }

    return response.data;
  },
};
