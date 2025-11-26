"use client";
import React, { useEffect, useState } from "react";
import { UserManagementApi } from "@/lib/api";
import { getAccessToken, parseJwt } from "@/lib/session";
import PageBreadcrumb from "@/components/tailadmin/common/PageBreadCrumb";
import ComponentCard from "@/components/tailadmin/common/ComponentCard";
import MentionBadge from "@/components/user-management/MentionBadge";
import { Mention } from "@/types/userManagement";
import Alert from "@/components/tailadmin/ui/alert/Alert";
import Badge from "@/components/tailadmin/ui/badge/Badge";

export default function MentionsPage() {
  const [userId, setUserId] = useState<string>("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [mentions, setMentions] = useState<Mention[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [filter, setFilter] = useState<"all" | "unread" | "read">("all");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    const token = getAccessToken();
    const jwt = parseJwt(token);
    setUserId(jwt?.sub || jwt?.userId || "");
  }, []);

  const loadMentions = async () => {
    if (!getAccessToken() || !userId) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await UserManagementApi.mentions.getUserMentions(userId, {
        pageNumber,
        pageSize,
        isRead: filter === "all" ? undefined : filter === "read",
      });
      setMentions(result.data || []);
      setTotalCount(result.totalCount || 0);
    } catch (e: any) {
      if (e?.message?.includes("401")) {
        setMentions([]);
        setTotalCount(0);
        return;
      }
      setError(e.message || "Failed to load mentions");
    } finally {
      setLoading(false);
    }
  };

  const loadUnreadCount = async () => {
    if (!userId) return;
    try {
      const result = await UserManagementApi.mentions.getUnreadCount(userId);
      setUnreadCount(result.count || 0);
    } catch (e: any) {
      console.error("Failed to load unread count", e);
    }
  };

  useEffect(() => {
    if (userId) {
      loadMentions();
      loadUnreadCount();
    }
  }, [userId, pageNumber, filter]);

  const handleMarkAsRead = async (mentionId: string) => {
    try {
      await UserManagementApi.mentions.markAsRead(mentionId);
      await loadMentions();
      await loadUnreadCount();
    } catch (e: any) {
      alert(e.message || "Failed to mark mention as read");
    }
  };

  if (loading) {
    return (
      <div className="p-6">
        <PageBreadcrumb pageTitle="Mentions" />
        <ComponentCard title="Mentions">
          <div className="text-center py-8">Loading mentions...</div>
        </ComponentCard>
      </div>
    );
  }

  return (
    <div className="p-6">
      <PageBreadcrumb pageTitle="Mentions" />

      <ComponentCard title="Mentions">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-4">
            <h2 className="text-2xl font-bold text-black dark:text-white">Mentions</h2>
            {unreadCount > 0 && (
              <Badge color="danger" className="text-sm">
                {unreadCount} unread
              </Badge>
            )}
          </div>
          <div className="flex gap-2 border border-stroke rounded-lg p-1 dark:border-strokedark">
            <button
              onClick={() => setFilter("all")}
              className={`px-3 py-1 rounded text-sm ${filter === "all"
                  ? "bg-primary text-white"
                  : "text-body-color dark:text-body-color-dark"
                }`}
            >
              All
            </button>
            <button
              onClick={() => setFilter("unread")}
              className={`px-3 py-1 rounded text-sm ${filter === "unread"
                  ? "bg-primary text-white"
                  : "text-body-color dark:text-body-color-dark"
                }`}
            >
              Unread
            </button>
            <button
              onClick={() => setFilter("read")}
              className={`px-3 py-1 rounded text-sm ${filter === "read"
                  ? "bg-primary text-white"
                  : "text-body-color dark:text-body-color-dark"
                }`}
            >
              Read
            </button>
          </div>
        </div>

        {error && (
          <Alert color="danger" className="mb-4">
            {error}
          </Alert>
        )}

        <div className="space-y-3">
          {mentions.map((mention) => (
            <MentionBadge
              key={mention.mentionId}
              mention={mention}
              onMarkRead={() => handleMarkAsRead(mention.mentionId)}
            />
          ))}
        </div>

        {mentions.length === 0 && !loading && (
          <div className="text-center py-8 text-body-color dark:text-body-color-dark">
            No mentions found.
          </div>
        )}

        {totalCount > pageSize && (
          <div className="flex items-center justify-between mt-6">
            <button
              onClick={() => setPageNumber(p => Math.max(1, p - 1))}
              disabled={pageNumber === 1}
              className="px-4 py-2 border border-stroke rounded hover:bg-gray-50 dark:hover:bg-boxdark-2 disabled:opacity-50"
            >
              Previous
            </button>
            <span className="text-sm text-body-color dark:text-body-color-dark">
              Page {pageNumber} of {Math.ceil(totalCount / pageSize)}
            </span>
            <button
              onClick={() => setPageNumber(p => p + 1)}
              disabled={pageNumber >= Math.ceil(totalCount / pageSize)}
              className="px-4 py-2 border border-stroke rounded hover:bg-gray-50 dark:hover:bg-boxdark-2 disabled:opacity-50"
            >
              Next
            </button>
          </div>
        )}
      </ComponentCard>
    </div>
  );
}
