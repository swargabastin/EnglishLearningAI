const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5288/api/v1";

export interface ApiError {
  status: number;
  title: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export class ApiException extends Error {
  public status: number;
  public details?: string;
  public fieldErrors?: Record<string, string[]>;

  constructor(status: number, message: string, details?: string, fieldErrors?: Record<string, string[]>) {
    super(message);
    this.name = "ApiException";
    this.status = status;
    this.details = details;
    this.fieldErrors = fieldErrors;
  }
}

export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const url = `${API_BASE_URL}${endpoint.startsWith("/") ? endpoint : `/${endpoint}`}`;

  const headers = new Headers(options.headers || {});
  if (!headers.has("Content-Type") && !(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }

  // Attach token if present in browser storage
  if (typeof window !== "undefined") {
    const token = localStorage.getItem("el_access_token");
    if (token && !headers.has("Authorization")) {
      headers.set("Authorization", `Bearer ${token}`);
    }
  }

  try {
    const response = await fetch(url, {
      ...options,
      headers,
    });

    if (!response.ok) {
      let errorMessage = `HTTP Error ${response.status}`;
      let errorDetail: string | undefined;
      let fieldErrors: Record<string, string[]> | undefined;

      try {
        const errorJson = await response.json();
        errorDetail = errorJson.detail;
        fieldErrors = errorJson.errors;
        if (errorJson.errors && typeof errorJson.errors === "object") {
          const messages = Object.values(errorJson.errors).flat().filter((m): m is string => typeof m === "string" && m.length > 0);
          if (messages.length > 0) {
            errorMessage = messages.join(". ");
          } else {
            errorMessage = errorJson.detail || errorJson.message || errorJson.title || errorMessage;
          }
        } else {
          errorMessage = errorJson.detail || errorJson.message || errorJson.title || errorMessage;
        }
      } catch {
        // Response wasn't JSON
      }

      if (response.status === 401 && typeof window !== "undefined") {
        // Clear invalid token
        localStorage.removeItem("el_access_token");
      }

      throw new ApiException(response.status, errorMessage, errorDetail, fieldErrors);
    }

    // Parse success payload
    if (response.status === 204) {
      return {} as T;
    }

    const data = await response.json();
    return data;
  } catch (error) {
    if (error instanceof ApiException) {
      throw error;
    }
    throw new ApiException(500, (error as Error).message || "Network connection failed");
  }
}
