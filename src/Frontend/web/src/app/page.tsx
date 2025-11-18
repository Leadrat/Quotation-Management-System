"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { getAccessToken } from "@/lib/session";

export default function Home() {
  const router = useRouter();

  useEffect(() => {
    const token = getAccessToken();
    if (token) router.replace("/dashboard");
    else router.replace("/login");
  }, [router]);

  return null;
}
