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

  async uploadAvatar(file: File): Promise<string> {
    const formData = new FormData();
    formData.append("file", file);

    const token = typeof window !== "undefined" ? localStorage.getItem("el_access_token") : null;
    const apiBase = (process.env.NEXT_PUBLIC_API_URL || process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5288").replace(/\/+$/, "");
    const url = `${apiBase}/api/v1/auth/avatar`;

    const res = await fetch(url, {
      method: "POST",
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
      body: formData,
    });

    if (!res.ok) {
      const errJson = await res.json().catch(() => null);
      throw new Error(errJson?.detail || errJson?.message || "Failed to upload avatar photo.");
    }

    const json = await res.json();
    const avatarPath = json.data?.avatarUrl || `/api/v1/auth/avatar`;
    return avatarPath.startsWith("http") ? avatarPath : `${apiBase}${avatarPath}`;
  },
};
