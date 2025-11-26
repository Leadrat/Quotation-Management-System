import { useState, useEffect, useCallback } from 'react';
import { dispatchApi } from '@/lib/api/dispatch';
import { 
  DispatchHistoryItem, 
  DispatchStatistics, 
  GetDispatchHistoryParams,
  DispatchHistoryResponse 
} from '@/lib/types/dispatch.types';

export interface UseDispatchHistoryResult {
  items: DispatchHistoryItem[];
  statistics: DispatchStatistics | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
  refresh: () => Promise<void>;
  loadMore: () => Promise<void>;
  retryDispatch: (dispatchAttemptId: number) => Promise<void>;
}

export function useDispatchHistory(
  initialParams: GetDispatchHistoryParams = {}
): UseDispatchHistoryResult {
  const [items, setItems] = useState<DispatchHistoryItem[]>([]);
  const [statistics, setStatistics] = useState<DispatchStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(initialParams.page || 1);
  const [pageSize] = useState(initialParams.pageSize || 20);
  const [params, setParams] = useState<GetDispatchHistoryParams>(initialParams);

  const loadDispatchHistory = useCallback(async (
    loadParams: GetDispatchHistoryParams,
    append: boolean = false
  ) => {
    try {
      setLoading(true);
      setError(null);

      const response: DispatchHistoryResponse = await dispatchApi.getDispatchHistory({
        ...loadParams,
        page: append ? loadParams.page : 1,
        pageSize
      });

      if (append) {
        setItems(prev => [...prev, ...response.items]);
      } else {
        setItems(response.items);
      }

      setStatistics(response.statistics);
      setTotalCount(response.totalCount);
      setPage(response.page);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load dispatch history');
      console.error('Error loading dispatch history:', err);
    } finally {
      setLoading(false);
    }
  }, [pageSize]);

  const refresh = useCallback(async () => {
    await loadDispatchHistory(params, false);
  }, [loadDispatchHistory, params]);

  const loadMore = useCallback(async () => {
    if (items.length >= totalCount) return;
    
    const nextPage = page + 1;
    await loadDispatchHistory({ ...params, page: nextPage }, true);
  }, [loadDispatchHistory, params, page, items.length, totalCount]);

  const retryDispatch = useCallback(async (dispatchAttemptId: number) => {
    try {
      await dispatchApi.retryFailedDispatch(dispatchAttemptId);
      // Refresh the list to show updated status
      await refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to retry dispatch');
      console.error('Error retrying dispatch:', err);
    }
  }, [refresh]);

  useEffect(() => {
    loadDispatchHistory(params);
  }, [loadDispatchHistory, params]);

  // Update params when they change
  const updateParams = useCallback((newParams: Partial<GetDispatchHistoryParams>) => {
    setParams(prev => ({ ...prev, ...newParams }));
    setPage(1);
  }, []);

  const hasMore = items.length < totalCount;

  return {
    items,
    statistics,
    loading,
    error,
    totalCount,
    page,
    pageSize,
    hasMore,
    refresh,
    loadMore,
    retryDispatch
  };
}

export function useFailedDispatches() {
  const [items, setItems] = useState<DispatchHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadFailedDispatches = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await dispatchApi.getFailedDispatches();
      setItems(response.items);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load failed dispatches');
      console.error('Error loading failed dispatches:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadFailedDispatches();
  }, [loadFailedDispatches]);

  return {
    items,
    loading,
    error,
    refresh: loadFailedDispatches
  };
}