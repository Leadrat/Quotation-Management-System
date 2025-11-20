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
      let errorMessage = "Login failed";
      if (e.status === 401) {
        errorMessage = "Invalid email or password. Please check your credentials and try again.";
      } else if (e.status === 403) {
        errorMessage = "Your account is not active. Please contact support.";
      } else if (e.message && e.message !== `HTTP ${e.status}`) {
        errorMessage = e.message;
      }
      set({ error: errorMessage, loggingIn: false });
      return false;
    }
  },
  logout: async () => {
    try { await AuthApi.logout(); } catch {}
    setAccessToken(null);
    set({ user: null, accessToken: null });
  }
}));
