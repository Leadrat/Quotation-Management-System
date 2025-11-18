"use client";
import { create } from "zustand";
import { AuthApi } from "@/lib/api";
import { setAccessToken, getAccessToken } from "@/lib/session";

interface AuthState {
  user: any | null;
  accessToken: string | null;
  loggingIn: boolean;
  error: string | null;
  login: (email: string, password: string) => Promise<boolean>;
  logout: () => Promise<void>;
  hydrate: () => void;
}

export const useAuth = create<AuthState>((set, get) => ({
  user: null,
  accessToken: null,
  loggingIn: false,
  error: null,
  hydrate: () => {
    const token = getAccessToken();
    if (token) set({ accessToken: token });
  },
  login: async (email, password) => {
    set({ loggingIn: true, error: null });
    try {
      const res = await AuthApi.login(email, password);
      setAccessToken(res.accessToken);
      set({ accessToken: res.accessToken, user: res.user, loggingIn: false });
      return true;
    } catch (e: any) {
      set({ error: e.message || "Login failed", loggingIn: false });
      return false;
    }
  },
  logout: async () => {
    try { await AuthApi.logout(); } catch {}
    setAccessToken(null);
    set({ user: null, accessToken: null });
  }
}));
