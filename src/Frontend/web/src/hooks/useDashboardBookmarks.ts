import { useState, useEffect, useCallback } from "react";
import { DashboardBookmarksApi } from "@/lib/api";
import type { DashboardConfig } from "@/types/reports";

export function useDashboardBookmarks() {
  const [bookmarks, setBookmarks] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadBookmarks = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await DashboardBookmarksApi.list();
      if (response.success && response.data) {
        setBookmarks(response.data);
      }
    } catch (err: any) {
      console.error("Error loading bookmarks:", err);
      setError(err.message || "Failed to load bookmarks");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadBookmarks();
  }, [loadBookmarks]);

  const saveBookmark = useCallback(
    async (dashboardName: string, dashboardConfig: DashboardConfig, isDefault = false) => {
      try {
        setLoading(true);
        setError(null);
        const response = await DashboardBookmarksApi.create({
          dashboardName,
          dashboardConfig,
          isDefault,
        });
        if (response.success) {
          await loadBookmarks();
          return response.data;
        }
        return null;
      } catch (err: any) {
        console.error("Error saving bookmark:", err);
        setError(err.message || "Failed to save bookmark");
        return null;
      } finally {
        setLoading(false);
      }
    },
    [loadBookmarks]
  );

  const deleteBookmark = useCallback(
    async (bookmarkId: string) => {
      try {
        setLoading(true);
        setError(null);
        await DashboardBookmarksApi.delete(bookmarkId);
        await loadBookmarks();
      } catch (err: any) {
        console.error("Error deleting bookmark:", err);
        setError(err.message || "Failed to delete bookmark");
      } finally {
        setLoading(false);
      }
    },
    [loadBookmarks]
  );

  return {
    bookmarks,
    loading,
    error,
    saveBookmark,
    deleteBookmark,
    refetch: loadBookmarks,
  };
}

