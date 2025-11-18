let accessToken: string | null = null;

export function setAccessToken(token: string | null) {
  accessToken = token;
  if (typeof window !== "undefined") {
    if (token) localStorage.setItem("accessToken", token);
    else localStorage.removeItem("accessToken");
  }
}

export function getAccessToken(): string | null {
  if (accessToken) return accessToken;
  if (typeof window !== "undefined") {
    const t = localStorage.getItem("accessToken");
    accessToken = t;
    return t;
  }
  return null;
}

export function parseJwt(token: string | null): any | null {
  if (!token) return null;
  try {
    const payload = token.split(".")[1];
    const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(json);
  } catch {
    return null;
  }
}

export function getRoleFromToken(token: string | null): string | null {
  const payload = parseJwt(token);
  return payload?.role || payload?.["role"] || null;
}
