"use client";
import { useState, useEffect } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/store/auth";
import Checkbox from "@/components/tailadmin/form/input/Checkbox";
import Input from "@/components/tailadmin/form/input/InputField";
import Label from "@/components/tailadmin/form/Label";
import Button from "@/components/tailadmin/ui/button/Button";
import { EyeCloseIcon, EyeIcon } from "@/icons";
import Alert from "@/components/tailadmin/ui/alert/Alert";

export default function LoginPage() {
  const router = useRouter();
  const auth = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isChecked, setIsChecked] = useState(false);

  useEffect(() => {
    auth.hydrate();
  }, []);

  useEffect(() => {
    if (auth.accessToken) {
      router.replace("/dashboard");
    }
  }, [auth.accessToken, router]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-12 dark:bg-gray-900 sm:px-6 lg:px-8">
      <div className="w-full max-w-md">
        <div className="mb-5 sm:mb-8 text-center">
          <h1 className="mb-2 font-semibold text-gray-800 text-title-sm dark:text-white/90 sm:text-title-md">
            Sign In
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Enter your email and password to sign in!
          </p>
        </div>

        {auth.error && (
          <Alert className="mb-4" variant="error" title="Login Error" message={auth.error} />
        )}

        <form
          onSubmit={async (e) => {
            e.preventDefault();
            const ok = await auth.login(email, password);
            if (ok) router.replace("/dashboard");
          }}
        >
          <div className="space-y-6">
            <div>
              <Label>
                Email <span className="text-error-500">*</span>
              </Label>
              <Input
                type="email"
                placeholder="info@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div>
              <Label>
                Password <span className="text-error-500">*</span>
              </Label>
              <div className="relative">
                <Input
                  type={showPassword ? "text" : "password"}
                  placeholder="Enter your password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
                <span
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute z-30 -translate-y-1/2 cursor-pointer right-4 top-1/2"
                >
                  {showPassword ? (
                    <EyeIcon className="fill-gray-500 dark:fill-gray-400" />
                  ) : (
                    <EyeCloseIcon className="fill-gray-500 dark:fill-gray-400" />
                  )}
                </span>
              </div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Checkbox checked={isChecked} onChange={setIsChecked} />
                <span className="block font-normal text-gray-700 text-theme-sm dark:text-gray-400">
                  Keep me logged in
                </span>
              </div>
              <Link
                href="#"
                className="text-sm text-brand-500 hover:text-brand-600 dark:text-brand-400"
              >
                Forgot password?
              </Link>
            </div>
            <div>
              <Button className="w-full" size="sm" type="submit" disabled={auth.loggingIn}>
                {auth.loggingIn ? "Signing in..." : "Sign in"}
              </Button>
            </div>
          </div>
        </form>

        <div className="mt-6 text-center">
          <p className="text-sm font-normal text-gray-700 dark:text-gray-400">
            Don&apos;t have an account?{" "}
            <Link
              href="/register"
              className="text-brand-500 hover:text-brand-600 dark:text-brand-400 font-medium"
            >
              Sign Up
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
