export const apiBase = import.meta.env.VITE_API_URL ?? "";

type ProblemPayload = { detail?: string; title?: string; message?: string; items?: unknown };

/** 调用新的模块化 API：直接返回 JSON，错误使用 ProblemDetails。 */
export async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem("starblog_token");
  const headers: Record<string, string> = {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(init.headers as Record<string, string> | undefined),
  };
  if (!(init.body instanceof FormData) && !headers["Content-Type"]) {
    headers["Content-Type"] = "application/json";
  }

  const response = await fetch(`${apiBase}${path}`, { ...init, headers });
  if (response.status === 204) return undefined as T;

  const payload = (await response.json().catch(() => ({}))) as T & ProblemPayload;
  if (!response.ok) {
    throw new Error(payload.detail || payload.title || payload.message || `请求失败 (${response.status})`);
  }
  return payload;
}

export const platformNames: Record<number, string> = { 1: "微信公众号", 2: "知乎", 3: "掘金", 99: "自定义" };
