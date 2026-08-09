export const apiBase = import.meta.env.VITE_API_URL ?? "";

export type ApiEnvelope<T> = { successful?: boolean; message?: string; data?: T; pagination?: { totalItemCount?: number } };

/** 统一处理 StarBlog 的 ApiResponse 包装、JWT 与错误消息。 */
export async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem("starblog_token");
  const response = await fetch(`${apiBase}${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...(token ? { Authorization: `Bearer ${token}` } : {}), ...init.headers },
  });
  const payload = (await response.json().catch(() => ({}))) as ApiEnvelope<T>;
  if (!response.ok || payload.successful === false) throw new Error(payload.message || `请求失败 (${response.status})`);
  return (payload.data ?? payload) as T;
}

export const platformNames: Record<number, string> = { 1: "微信公众号", 2: "知乎", 3: "掘金", 99: "自定义" };
