"use client";

import React, { useState } from "react";
import { postChat } from "@/lib/api/imports";

type Message = { role: "user" | "assistant"; content: string };

type Props = { sessionId: string };

export default function ChatPanel({ sessionId }: Props) {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);

  const send = async () => {
    if (!input.trim()) return;
    const next = [...messages, { role: "user", content: input } as Message];
    setMessages(next);
    setInput("");
    setLoading(true);
    try {
      const res = await postChat(sessionId, input);
      const reply = (res?.reply as string) ?? "";
      setMessages([...next, { role: "assistant", content: reply }]);
    } catch (e: any) {
      setMessages([...next, { role: "assistant", content: e?.message ?? "Error" }]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-800 p-4 bg-white dark:bg-gray-900">
      <h2 className="font-semibold mb-2 text-gray-900 dark:text-white">Mapping Assistant</h2>
      <div className="h-48 overflow-auto space-y-2 bg-gray-50 dark:bg-gray-800/60 p-2 rounded" aria-live="polite">
        {messages.map((m, i) => (
          <div key={i} className={m.role === "user" ? "text-right" : "text-left"}>
            <span className={
              "inline-block px-3 py-2 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-primary/60 " +
              (m.role === "user"
                ? "bg-primary text-white"
                : "bg-gray-200 dark:bg-gray-700 text-gray-900 dark:text-gray-100")
            }>
              {m.content}
            </span>
          </div>
        ))}
      </div>
      <div className="mt-3 flex gap-2">
        <input
          aria-label="Chat input to map fields"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && send()}
          placeholder="Ask to map fields (e.g., Map GSTIN to identifiers.gstin)"
          className="flex-1 rounded border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/60"
        />
        <button
          aria-label="Send chat message"
          onClick={send}
          disabled={loading}
          className="rounded bg-primary text-white px-4 py-2 disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-primary/60"
        >
          {loading ? "Sending..." : "Send"}
        </button>
      </div>
    </div>
  );
}
