"use client";
import { useState, useEffect } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/store/auth";

export default function LoginPage() {
  const router = useRouter();
  const auth = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  useEffect(() => {
    auth.hydrate();
  }, []);

  useEffect(() => {
    if (auth.accessToken) {
      router.replace("/dashboard");
    }
  }, [auth.accessToken, router]);

  return (
    <main className="min-h-screen bg-white">
      <div className="mx-auto flex min-h-screen max-w-7xl items-center justify-center px-4">
        <div className="w-full max-w-md">
          <h1 className="mb-1 text-2xl font-semibold text-black">Sign in</h1>
          <p className="mb-6 text-sm text-black">Enter your credentials to continue.</p>

          {auth.error && (
            <div className="mb-4 rounded-md border border-black/20 bg-white px-3 py-2 text-sm text-red-600">
              {auth.error}
            </div>
          )}

          <form
            onSubmit={async (e) => {
              e.preventDefault();
              const ok = await auth.login(email, password);
              if (ok) router.replace("/dashboard");
            }}
            className="space-y-4"
          >
            <div>
              <label className="mb-1 block text-sm font-medium text-black">Email</label>
              <input
                type="email"
                className="h-11 w-full rounded-md border border-black bg-white px-3 text-sm text-black placeholder:text-black/50 focus:outline-0"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div>
              <label className="mb-1 block text-sm font-medium text-black">Password</label>
              <input
                type="password"
                className="h-11 w-full rounded-md border border-black bg-white px-3 text-sm text-black placeholder:text-black/50 focus:outline-0"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
              <div className="mt-2 text-right">
                <Link href="#" className="text-sm text-black underline">Forgot password?</Link>
              </div>
            </div>

            <button
              type="submit"
              disabled={auth.loggingIn}
              className="inline-flex h-11 w-full items-center justify-center rounded-md bg-black px-4 text-sm font-medium text-white disabled:opacity-60"
            >
              {auth.loggingIn ? "Signing in..." : "Sign In"}
            </button>

            <p className="text-center text-sm text-black">
              Don’t have an account? {" "}
              <Link href="/register" className="underline">Register</Link>
            </p>
          </form>
        </div>
      </div>
    </main>
  );
}
