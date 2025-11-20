export const API_BASE = process.env.NEXT_PUBLIC_API_BASE_URL || "";

export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE" | "PATCH";

import { getAccessToken, setAccessToken } from "./session";
import type { Currency } from "../types/localization";

// Track if we're currently refreshing to avoid multiple simultaneous refresh attempts
let isRefreshing = false;
let refreshPromise: Promise<string | null> | null = null;

async function refreshAccessToken(): Promise<string | null> {
  // If already refreshing, return the existing promise
  if (isRefreshing && refreshPromise) {
    return refreshPromise;
  }

  isRefreshing = true;
  refreshPromise = (async () => {
    try {
      // Call refresh endpoint directly to avoid circular dependency
      const url = `${API_BASE}/api/v1/auth/refresh-token`;
      const res = await fetch(url, {
        method: "POST",
        credentials: "include", // Include HttpOnly cookie with refresh token
      });
      
      if (!res.ok) {
        throw new Error("Token refresh failed");
      }
      
      const result = await res.json();
      if (result.success && result.accessToken) {
        setAccessToken(result.accessToken);
        return result.accessToken;
      }
      return null;
    } catch (err) {
      // Refresh failed - clear token and redirect to login
      setAccessToken(null);
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      return null;
    } finally {
      isRefreshing = false;
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

export async function apiFetch<T>(path: string, options: RequestInit & { auth?: boolean } = {}): Promise<T> {
  if (!path || path.trim() === "") {
    throw new Error("API path cannot be empty");
  }
  const url = `${API_BASE}${path}`;
  if (!url || url.trim() === "") {
    throw new Error("API URL cannot be empty. Check NEXT_PUBLIC_API_BASE_URL environment variable.");
  }
  const headers = new Headers(options.headers || {});
  // Only set Content-Type for JSON, not for FormData
  if (!(options.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }
  const token = getAccessToken();
  if (token) headers.set("Authorization", `Bearer ${token}`);
  
  let res = await fetch(url, {
    ...options,
    headers,
    credentials: "include",
  });
  
  // If we get a 401 and this is not an auth endpoint, try to refresh the token
  if (res.status === 401 && !path.includes("/auth/") && token) {
    const newToken = await refreshAccessToken();
    if (newToken) {
      // Retry the original request with the new token
      headers.set("Authorization", `Bearer ${newToken}`);
      res = await fetch(url, {
        ...options,
        headers,
        credentials: "include",
      });
    }
  }
  
  if (!res.ok) {
    let message = `HTTP ${res.status}`;
    let errors: any = null;
    let details: any = null;
    try {
      const body = await res.json();
      message = body?.error || body?.message || message;
      errors = body?.errors || null;
      details = body?.details || null;
    } catch {}
    const error = new Error(message);
    // Add status code, errors, and details to error for easier handling
    (error as any).status = res.status;
    (error as any).errors = errors;
    (error as any).details = details;
    throw error;
  }
  // Handle blob responses (for file downloads)
  const contentType = res.headers.get("content-type");
  if (contentType && (contentType.includes("application/octet-stream") || contentType.includes("text/csv"))) {
    return res.blob() as unknown as T;
  }
  
  // try parse json
  const text = await res.text();
  if (!text) return undefined as T;
  try {
    return JSON.parse(text) as T;
  } catch {
    return text as unknown as T;
  }
}

export const AuthApi = {
  login: (email: string, password: string) =>
    apiFetch<{ success: boolean; accessToken: string; refreshToken?: string; expiresIn: number; user: any }>(
      "/api/v1/auth/login",
      { method: "POST", body: JSON.stringify({ email, password }) }
    ),
  refresh: () => apiFetch<{ success: boolean; accessToken: string; refreshToken?: string; expiresIn: number }>(
    "/api/v1/auth/refresh-token",
    { method: "POST" }
  ),
  logout: () => apiFetch<{ success: boolean; message: string }>(
    "/api/v1/auth/logout",
    { method: "POST" }
  ),
};

export const UsersApi = {
  list: (params?: { pageNumber?: number; pageSize?: number; searchTerm?: string }) => {
    const q = new URLSearchParams();
    if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
    if (params?.searchTerm) q.append("searchTerm", params.searchTerm);
    return apiFetch<{ success: boolean; data: any[]; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/users${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  getById: (userId: string) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/users/${userId}`
    ),
  create: (payload: any) =>
    apiFetch<{ success: boolean; userId: string }>(
      "/api/v1/users",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  updateProfile: (userId: string, payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/users/${userId}/profile`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
};

export const PasswordApi = {
  change: (payload: { currentPassword: string; newPassword: string; confirmPassword: string }) =>
    apiFetch<{ success: boolean; message?: string }>(
      "/api/v1/auth/change-password",
      { method: "POST", body: JSON.stringify(payload) }
    ),
};

export const RolesApi = {
  list: (params?: { isActive?: boolean; pageNumber?: number; pageSize?: number }) => {
    const q = new URLSearchParams();
    if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
    if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
    return apiFetch<{ success: boolean; data: Array<{ roleId: string; roleName: string; description?: string; isActive: boolean }>; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/roles${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  getById: (roleId: string) =>
    apiFetch<{ success: boolean; data: { roleId: string; roleName: string; description?: string; isActive: boolean } }>(
      `/api/v1/roles/${roleId}`
    ),
};

export const LocalizationApi = {
  getSupportedCurrencies: () =>
    apiFetch<Currency[]>(
      "/api/Currencies",
      { method: "GET" }
    ),
  createCurrency: (payload: import("../types/localization").CreateCurrencyRequest) =>
    apiFetch<Currency>(
      "/api/Currencies",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  getSupportedLanguages: () =>
    apiFetch<import("../types/localization").SupportedLanguage[]>(
      "/api/Languages",
      { method: "GET" }
    ),
  getUserPreferences: (includeEffective?: boolean) => {
    const params = new URLSearchParams();
    if (includeEffective) params.append("includeEffective", "true");
    return apiFetch<import("../types/localization").UserPreferences>(
      `/api/UserPreferences/me?${params.toString()}`,
      { method: "GET" }
    );
  },
  updateUserPreferences: (payload: import("../types/localization").UpdateUserPreferencesRequest) =>
    apiFetch<import("../types/localization").UserPreferences>(
      "/api/UserPreferences/me",
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  getCompanyPreferences: (companyId: string) =>
    apiFetch<import("../types/localization").CompanyPreferences>(
      `/api/CompanyPreferences/${companyId}`,
      { method: "GET" }
    ),
  updateCompanyPreferences: (companyId: string, payload: import("../types/localization").UpdateCompanyPreferencesRequest) =>
    apiFetch<import("../types/localization").CompanyPreferences>(
      `/api/CompanyPreferences/${companyId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  getLocalizationResources: (languageCode: string, category?: string) => {
    const params = new URLSearchParams();
    if (category) params.append("category", category);
    return apiFetch<Record<string, string>>(
      `/api/LocalizationResources/${languageCode}?${params.toString()}`,
      { method: "GET" }
    );
  },
  createLocalizationResource: (payload: import("../types/localization").CreateLocalizationResourceRequest) =>
    apiFetch<import("../types/localization").LocalizationResource>(
      "/api/LocalizationResources",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  updateLocalizationResource: (resourceId: string, payload: import("../types/localization").UpdateLocalizationResourceRequest) =>
    apiFetch<import("../types/localization").LocalizationResource>(
      `/api/LocalizationResources/${resourceId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  deleteLocalizationResource: (resourceId: string) =>
    apiFetch<void>(
      `/api/LocalizationResources/${resourceId}`,
      { method: "DELETE" }
    ),
  getExchangeRates: (fromCurrencyCode?: string, toCurrencyCode?: string, asOfDate?: string) => {
    const params = new URLSearchParams();
    if (fromCurrencyCode) params.append("fromCurrencyCode", fromCurrencyCode);
    if (toCurrencyCode) params.append("toCurrencyCode", toCurrencyCode);
    if (asOfDate) params.append("asOfDate", asOfDate);
    return apiFetch<import("../types/localization").ExchangeRate[]>(
      `/api/ExchangeRates?${params.toString()}`,
      { method: "GET" }
    );
  },
  convertCurrency: (payload: import("../types/localization").CurrencyConversionRequest) =>
    apiFetch<import("../types/localization").CurrencyConversionResponse>(
      "/api/ExchangeRates/convert",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  updateExchangeRate: (payload: import("../types/localization").UpdateExchangeRateRequest) =>
    apiFetch<import("../types/localization").ExchangeRate>(
      "/api/ExchangeRates",
      { method: "POST", body: JSON.stringify(payload) }
    ),
};

export const ReportsApi = {
  getDashboardStats: () =>
    apiFetch<{ success: boolean; data: { totalClients: number; totalQuotations: number; totalPayments: number; pendingApprovals: number } }>(
      "/api/v1/reports/dashboard/stats"
    ),
  getSalesDashboard: (fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append("fromDate", fromDate);
    if (toDate) params.append("toDate", toDate);
    const queryString = params.toString();
    return apiFetch<{ success: boolean; data: import("../types/reports").SalesDashboardMetrics }>(
      `/api/v1/reports/dashboard/sales${queryString ? `?${queryString}` : ""}`
    );
  },
  getManagerDashboard: (teamId?: string, fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (teamId) params.append("teamId", teamId);
    if (fromDate) params.append("fromDate", fromDate);
    if (toDate) params.append("toDate", toDate);
    return apiFetch<{ success: boolean; data: import("../types/reports").ManagerDashboardMetrics }>(
      `/api/v1/reports/dashboard/manager?${params.toString()}`
    );
  },
  getFinanceDashboard: (fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams();
    if (fromDate) params.append("fromDate", fromDate);
    if (toDate) params.append("toDate", toDate);
    return apiFetch<{ success: boolean; data: import("../types/reports").FinanceDashboardMetrics }>(
      `/api/v1/reports/dashboard/finance?${params.toString()}`
    );
  },
  getAdminDashboard: () =>
    apiFetch<{ success: boolean; data: import("../types/reports").AdminDashboardMetrics }>(
      "/api/v1/reports/dashboard/admin"
    ),
  getCustomReport: (params: {
    reportType: string;
    filters?: string;
    groupBy?: string;
    sortBy?: string;
    limit?: number;
  }) => {
    const query = new URLSearchParams();
    query.append("reportType", params.reportType);
    if (params.filters) query.append("filters", params.filters);
    if (params.groupBy) query.append("groupBy", params.groupBy);
    if (params.sortBy) query.append("sortBy", params.sortBy);
    if (params.limit) query.append("limit", params.limit.toString());
    return apiFetch<{ success: boolean; data: import("../types/reports").ReportData }>(
      `/api/v1/reports/custom?${query.toString()}`
    );
  },
  exportReport: (payload: import("../types/reports").ExportReportRequest) =>
    apiFetch<{ success: boolean; data: import("../types/reports").ExportedReport }>(
      "/api/v1/reports/export",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  getExportHistory: (pageNumber = 1, pageSize = 20) =>
    apiFetch<{
      success: boolean;
      data: import("../types/reports").ExportedReport[];
      pageNumber: number;
      pageSize: number;
      totalCount: number;
    }>(`/api/v1/reports/export-history?pageNumber=${pageNumber}&pageSize=${pageSize}`),
  getForecasting: (days = 30, confidenceLevel = 0.95) =>
    apiFetch<{ success: boolean; data: import("../types/reports").ForecastingData }>(
      `/api/v1/reports/forecasting?days=${days}&confidenceLevel=${confidenceLevel}`
    ),
  getAuditReport: (params: {
    fromDate: string;
    toDate: string;
    entityType?: string;
    userId?: string;
  }) => {
    const query = new URLSearchParams();
    query.append("fromDate", params.fromDate);
    query.append("toDate", params.toDate);
    if (params.entityType) query.append("entityType", params.entityType);
    if (params.userId) query.append("userId", params.userId);
    return apiFetch<{ success: boolean; data: import("../types/reports").AuditReport }>(
      `/api/v1/reports/audit?${query.toString()}`
    );
  },
  getTeamPerformance: (params?: {
    teamId?: string;
    userId?: string;
    fromDate?: string;
    toDate?: string;
  }) => {
    const query = new URLSearchParams();
    if (params?.teamId) query.append("teamId", params.teamId);
    if (params?.userId) query.append("userId", params.userId);
    if (params?.fromDate) query.append("fromDate", params.fromDate);
    if (params?.toDate) query.append("toDate", params.toDate);
    return apiFetch<{ success: boolean; data: import("../types/reports").TeamPerformance[] }>(
      `/api/v1/reports/team-performance?${query.toString()}`
    );
  },
  getApprovalMetrics: (params?: {
    managerId?: string;
    fromDate?: string;
    toDate?: string;
  }) => {
    const query = new URLSearchParams();
    if (params?.managerId) query.append("managerId", params.managerId);
    if (params?.fromDate) query.append("fromDate", params.fromDate);
    if (params?.toDate) query.append("toDate", params.toDate);
    return apiFetch<{ success: boolean; data: import("../types/reports").ApprovalMetrics }>(
      `/api/v1/reports/approval-metrics?${query.toString()}`
    );
  },
  getDiscountAnalytics: (params?: {
    fromDate?: string;
    toDate?: string;
    userId?: string;
    teamId?: string;
  }) => {
    const query = new URLSearchParams();
    if (params?.fromDate) query.append("fromDate", params.fromDate);
    if (params?.toDate) query.append("toDate", params.toDate);
    if (params?.userId) query.append("userId", params.userId);
    if (params?.teamId) query.append("teamId", params.teamId);
    return apiFetch<{ success: boolean; data: import("../types/reports").DiscountAnalytics }>(
      `/api/v1/reports/discount-analytics?${query.toString()}`
    );
  },
  getPaymentAnalytics: (params?: {
    fromDate?: string;
    toDate?: string;
    gateway?: string;
    status?: string;
  }) => {
    const query = new URLSearchParams();
    if (params?.fromDate) query.append("fromDate", params.fromDate);
    if (params?.toDate) query.append("toDate", params.toDate);
    if (params?.gateway) query.append("gateway", params.gateway);
    if (params?.status) query.append("status", params.status);
    return apiFetch<{ success: boolean; data: import("../types/reports").PaymentAnalytics }>(
      `/api/v1/reports/payment-analytics?${query.toString()}`
    );
  },
  getClientEngagement: (params?: {
    clientId?: string;
    fromDate?: string;
    toDate?: string;
  }) => {
    const query = new URLSearchParams();
    if (params?.clientId) query.append("clientId", params.clientId);
    if (params?.fromDate) query.append("fromDate", params.fromDate);
    if (params?.toDate) query.append("toDate", params.toDate);
    return apiFetch<{ success: boolean; data: import("../types/reports").ClientEngagement }>(
      `/api/v1/reports/client-engagement?${query.toString()}`
    );
  },
};

export const ScheduledReportsApi = {
  create: (payload: import("../types/reports").ScheduleReportRequest) =>
    apiFetch<{ success: boolean; data: import("../types/reports").ScheduledReport }>(
      "/api/v1/reports/scheduled",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  list: () =>
    apiFetch<{ success: boolean; data: import("../types/reports").ScheduledReport[] }>(
      "/api/v1/reports/scheduled"
    ),
  delete: (reportId: string) =>
    apiFetch<{ success: boolean; message: string }>(
      `/api/v1/reports/scheduled/${reportId}`,
      { method: "DELETE" }
    ),
  sendTest: (payload: Record<string, any>) =>
    apiFetch<{ success: boolean; message: string }>(
      "/api/v1/reports/scheduled/send-test",
      { method: "POST", body: JSON.stringify(payload) }
    ),
};

export const RefundsApi = {
  create: (request: import("../types/refunds").CreateRefundRequest) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto }>(
      "/api/v1/refunds",
      { method: "POST", body: JSON.stringify(request) }
    ),
  getById: (refundId: string) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto }>(
      `/api/v1/refunds/${refundId}`
    ),
  getByPayment: (paymentId: string) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto[] }>(
      `/api/v1/refunds/payment/${paymentId}`
    ),
  getByQuotation: (quotationId: string) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto[] }>(
      `/api/v1/refunds/quotation/${quotationId}`
    ),
  approve: (refundId: string, request: import("../types/refunds").ApproveRefundRequest) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto }>(
      `/api/v1/refunds/${refundId}/approve`,
      { method: "POST", body: JSON.stringify(request) }
    ),
  reject: (refundId: string, request: import("../types/refunds").RejectRefundRequest) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto }>(
      `/api/v1/refunds/${refundId}/reject`,
      { method: "POST", body: JSON.stringify(request) }
    ),
  process: (refundId: string) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto }>(
      `/api/v1/refunds/${refundId}/process`,
      { method: "POST" }
    ),
  reverse: (refundId: string, request: import("../types/refunds").ReverseRefundRequest) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto }>(
      `/api/v1/refunds/${refundId}/reverse`,
      { method: "POST", body: JSON.stringify(request) }
    ),
  getTimeline: (refundId: string) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").RefundTimelineDto[] }>(
      `/api/v1/refunds/${refundId}/timeline`
    ),
  getPending: (approvalLevel?: string) => {
    const params = new URLSearchParams();
    if (approvalLevel) params.append("approvalLevel", approvalLevel);
    return apiFetch<{ success: boolean; data: import("../types/refunds").RefundDto[] }>(
      `/api/v1/refunds/pending?${params.toString()}`
    );
  },
  getMetrics: (startDate?: string, endDate?: string) => {
    const params = new URLSearchParams();
    if (startDate) params.append("startDate", startDate);
    if (endDate) params.append("endDate", endDate);
    return apiFetch<{ success: boolean; data: import("../types/refunds").RefundMetricsDto }>(
      `/api/v1/refunds/metrics?${params.toString()}`
    );
  },
  bulkProcess: (request: import("../types/refunds").BulkProcessRefundsRequest) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").BulkProcessRefundsResult }>(
      "/api/v1/refunds/bulk-process",
      { method: "POST", body: JSON.stringify(request) }
    ),
};

export const AdjustmentsApi = {
  create: (request: import("../types/refunds").CreateAdjustmentRequest) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").AdjustmentDto }>(
      "/api/v1/adjustments",
      { method: "POST", body: JSON.stringify(request) }
    ),
  getById: (adjustmentId: string) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").AdjustmentDto }>(
      `/api/v1/adjustments/${adjustmentId}`
    ),
  approve: (adjustmentId: string, request: import("../types/refunds").ApproveAdjustmentRequest) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").AdjustmentDto }>(
      `/api/v1/adjustments/${adjustmentId}/approve`,
      { method: "POST", body: JSON.stringify(request) }
    ),
  apply: (adjustmentId: string) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").AdjustmentDto }>(
      `/api/v1/adjustments/${adjustmentId}/apply`,
      { method: "POST" }
    ),
  getByQuotation: (quotationId: string) =>
    apiFetch<{ success: boolean; data: import("../types/refunds").AdjustmentDto[] }>(
      `/api/v1/adjustments/quotation/${quotationId}`
    ),
  getPending: (approvalLevel?: string) => {
    const params = new URLSearchParams();
    if (approvalLevel) params.append("approvalLevel", approvalLevel);
    return apiFetch<{ success: boolean; data: import("../types/refunds").AdjustmentDto[] }>(
      `/api/v1/adjustments/pending?${params.toString()}`
    );
  },
};

export const DashboardBookmarksApi = {
  save: (payload: {
    dashboardConfig: import("../types/reports").DashboardConfig;
    dashboardName: string;
    isDefault: boolean;
    bookmarkId?: string;
  }) =>
    apiFetch<{ success: boolean; data: any }>(
      "/api/v1/dashboard/bookmarks",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  list: () =>
    apiFetch<{ success: boolean; data: any[] }>(
      "/api/v1/dashboard/bookmarks"
    ),
  delete: (bookmarkId: string) =>
    apiFetch<{ success: boolean; message: string }>(
      `/api/v1/dashboard/bookmarks/${bookmarkId}`,
      { method: "DELETE" }
    ),
};

export const RegistrationApi = {
  registerClient: (payload: any) =>
    apiFetch<{ success: boolean; userId: string; email: string }>(
      "/api/v1/auth/register",
      { method: "POST", body: JSON.stringify(payload) }
    ),
};

export const ClientsApi = {
  list: (pageNumber = 1, pageSize = 10) =>
    apiFetch<{ success: boolean; data: any[]; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/clients?pageNumber=${pageNumber}&pageSize=${pageSize}`
    ),
  get: (id: string) => apiFetch<{ success: boolean; data: any }>(`/api/v1/clients/${id}`),
  create: (payload: any) => apiFetch<{ success: boolean; data: any }>(
    "/api/v1/clients",
    { method: "POST", body: JSON.stringify(payload) }
  ),
  update: (id: string, payload: any) => apiFetch<{ success: boolean; data: any }>(
    `/api/v1/clients/${id}`,
    { method: "PUT", body: JSON.stringify(payload) }
  ),
  remove: (id: string) => apiFetch<{ success: boolean; message?: string }>(
    `/api/v1/clients/${id}`,
    { method: "DELETE" }
  ),
  search: (params: Record<string, any>) => {
    const q = new URLSearchParams();
    Object.entries(params || {}).forEach(([k, v]) => {
      if (v === undefined || v === null || v === "") return;
      q.set(k, String(v));
    });
    return apiFetch<{ success: boolean; data: any[]; pageNumber: number; pageSize: number; totalCount: number; hasMore: boolean; searchExecutedIn: string }>(
      `/api/v1/clients/search?${q.toString()}`
    );
  },
  suggestions: (term: string, type: "CompanyName"|"Email"|"City"|"ContactName" = "CompanyName", maxSuggestions = 10) =>
    apiFetch<{ success: boolean; data: string[] }>(
      `/api/v1/clients/search/suggestions?term=${encodeURIComponent(term)}&type=${type}&maxSuggestions=${maxSuggestions}`
    ),
  filterOptions: () => apiFetch<{ success: boolean; data: { states: { state: string; count: number }[]; cities: { city: string; count: number }[]; createdDateRanges: { label: string; from: string; to: string }[]; stateCodes: { code: string; name: string }[] } }>(
    "/api/v1/clients/search/filter-options"
  ),
  saveSearch: (body: { searchName: string; filterCriteria: Record<string, any>; sortBy?: string }) =>
    apiFetch<{ success: boolean; message: string; data: { savedSearchId: string } }>(
      "/api/v1/clients/search/save",
      { method: "POST", body: JSON.stringify(body) }
    ),
  getSaved: (userId?: string) => apiFetch<{ success: boolean; data: Array<{ savedSearchId: string; searchName: string; filterCriteria: Record<string, any>; sortBy?: string; createdAt: string }> }>(
    `/api/v1/clients/search/saved${userId ? `?userId=${userId}` : ""}`
  ),
  deleteSaved: (savedSearchId: string) => apiFetch<{ success: boolean; message: string }>(
    `/api/v1/clients/search/saved/${savedSearchId}`,
    { method: "DELETE" }
  ),
  exportCsv: (params: Record<string, any>) => {
    const q = new URLSearchParams();
    Object.entries(params || {}).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== "") q.set(k, String(v)); });
    const url = `${API_BASE}/api/v1/clients/export?${q.toString()}&format=csv`;
    window.open(url, "_blank");
  }
};

export const ClientHistoryApi = {
  getHistory: (clientId: string, pageNumber = 1, pageSize = 20, includeAccessLogs = false) => {
    const q = new URLSearchParams();
    q.set("pageNumber", String(pageNumber));
    q.set("pageSize", String(pageSize));
    if (includeAccessLogs) q.set("includeAccessLogs", "true");
    return apiFetch<{ success: boolean; data: any[]; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/clients/${clientId}/history?${q.toString()}`
    );
  },
  getTimeline: (clientId: string) =>
    apiFetch<{ success: boolean; data: any }>(`/api/v1/clients/${clientId}/timeline`),
  restore: (clientId: string, reason: string) =>
    apiFetch<{ success: boolean; data: any; message?: string }>(
      `/api/v1/clients/${clientId}/restore`,
      { method: "POST", body: JSON.stringify({ reason }) }
    ),
  getUserActivity: (userId: string, params: { pageNumber?: number; pageSize?: number; actionType?: string; dateFrom?: string; dateTo?: string } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<{ success: boolean; data: any[]; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/users/${userId}/activity?${q.toString()}`
    );
  },
  getSuspiciousActivity: (params: { pageNumber?: number; pageSize?: number; minScore?: number; status?: string; dateFrom?: string; dateTo?: string } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<{ success: boolean; data: any[]; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/admin/suspicious-activity?${q.toString()}`
    );
  },
  exportHistory: (params: { clientIds?: string; actionType?: string; dateFrom?: string; dateTo?: string; format?: string } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    const url = `${API_BASE}/api/v1/clients/history/export?${q.toString()}&format=${params.format || "csv"}`;
    window.open(url, "_blank");
  }
};

export const QuotationsApi = {
  list: (params: { pageNumber?: number; pageSize?: number; clientId?: string; userId?: string; status?: string; dateFrom?: string; dateTo?: string } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<{ success: boolean; data: any[]; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/quotations?${q.toString()}`
    );
  },
  get: (quotationId: string) =>
    apiFetch<{ success: boolean; data: any }>(`/api/v1/quotations/${quotationId}`),
  getByClient: (clientId: string) =>
    apiFetch<{ success: boolean; data: any[] }>(`/api/v1/quotations/client/${clientId}`),
  create: (payload: {
    clientId: string;
    quotationDate?: string;
    validUntil?: string;
    discountPercentage?: number;
    notes?: string;
    lineItems: Array<{
      itemName: string;
      description?: string;
      quantity: number;
      unitRate: number;
    }>;
  }) =>
    apiFetch<{ success: boolean; data: any; message?: string }>(
      "/api/v1/quotations",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (quotationId: string, payload: {
    quotationDate?: string;
    validUntil?: string;
    discountPercentage?: number;
    notes?: string;
    lineItems?: Array<{
      lineItemId?: string;
      itemName: string;
      description?: string;
      quantity: number;
      unitRate: number;
    }>;
  }) =>
    apiFetch<{ success: boolean; data: any; message?: string }>(
      `/api/v1/quotations/${quotationId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  delete: (quotationId: string) =>
    apiFetch<{ success: boolean; message?: string }>(
      `/api/v1/quotations/${quotationId}`,
      { method: "DELETE" }
    ),
  send: (quotationId: string, payload: { recipientEmail: string; ccEmails?: string[]; bccEmails?: string[]; customMessage?: string }) =>
    apiFetch<{ success: boolean; data: any; message?: string }>(
      `/api/v1/quotations/${quotationId}/send`,
      { method: "POST", body: JSON.stringify(payload) }
    ),
  resend: (quotationId: string, payload: { recipientEmail: string; ccEmails?: string[]; bccEmails?: string[]; customMessage?: string }) =>
    apiFetch<{ success: boolean; data: any; message?: string }>(
      `/api/v1/quotations/${quotationId}/resend`,
      { method: "POST", body: JSON.stringify(payload) }
    ),
  statusHistory: (quotationId: string) =>
    apiFetch<{ success: boolean; data: Array<{ historyId: string; previousStatus?: string; newStatus: string; changedAt: string; changedByUserName: string; reason?: string }> }>(
      `/api/v1/quotations/${quotationId}/status-history`
    ),
  response: (quotationId: string) =>
    apiFetch<{ success: boolean; data: any } | undefined>(`/api/v1/quotations/${quotationId}/response`).catch((err) => {
      if (err.message?.includes("204")) return undefined;
      throw err;
    }),
  accessLink: (quotationId: string) =>
    apiFetch<{ success: boolean; data: any } | undefined>(`/api/v1/quotations/${quotationId}/access-link`).catch((err) => {
      if (err.message?.includes("204")) return undefined;
      throw err;
    }),
  downloadPdf: (quotationId: string) => {
    const url = `${API_BASE}/api/v1/quotations/${quotationId}/download-pdf`;
    window.open(url, "_blank");
  }
};

export const ClientPortalApi = {
  validateLink: (quotationId: string, accessToken: string) =>
    apiFetch<{ success: boolean; clientEmail?: string; error?: string }>(
      `/api/v1/client-portal/quotations/${quotationId}/${accessToken}/validate`
    ),
  requestOtp: (quotationId: string, accessToken: string, email: string) =>
    apiFetch<{ success: boolean; message: string }>(
      `/api/v1/client-portal/quotations/${quotationId}/${accessToken}/request-otp`,
      { method: "POST", body: JSON.stringify({ email }) }
    ),
  verifyOtp: (quotationId: string, accessToken: string, email: string, otpCode: string) =>
    apiFetch<{ success: boolean; message: string }>(
      `/api/v1/client-portal/quotations/${quotationId}/${accessToken}/verify-otp`,
      { method: "POST", body: JSON.stringify({ email, otpCode }) }
    ),
  startPageView: (quotationId: string, accessToken: string, email?: string) =>
    apiFetch<{ success: boolean; viewId: string }>(
      `/api/v1/client-portal/quotations/${quotationId}/${accessToken}/start-view`,
      { method: "POST", body: JSON.stringify({ email }) }
    ),
  endPageView: (quotationId: string, accessToken: string, viewId: string) =>
    apiFetch<{ success: boolean; durationSeconds?: number }>(
      `/api/v1/client-portal/quotations/${quotationId}/${accessToken}/end-view`,
      { method: "POST", body: JSON.stringify({ viewId }) }
    ),
  getQuotation: (quotationId: string, accessToken: string) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/client-portal/quotations/${quotationId}/${accessToken}`
    ),
  submitResponse: (
    quotationId: string,
    accessToken: string,
    payload: { responseType: string; clientName?: string; clientEmail?: string; responseMessage?: string }
  ) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/client-portal/quotations/${quotationId}/${accessToken}/respond`,
      { method: "POST", body: JSON.stringify(payload) }
    ),
  downloadPdf: (quotationId: string, accessToken: string) => {
    const url = `${API_BASE}/api/v1/client-portal/quotations/${quotationId}/${accessToken}/download`;
    window.open(url, "_blank");
  },
};

export const TemplatesApi = {
  list: (params: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    visibility?: string;
    isApproved?: boolean;
    ownerUserId?: string;
  } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<{
      success: boolean;
      data: {
        data: import("../types/templates").QuotationTemplate[];
        pageNumber: number;
        pageSize: number;
        totalCount: number;
      };
    }>(`/api/v1/quotation-templates?${q.toString()}`);
  },
  get: (templateId: string) =>
    apiFetch<{ success: boolean; data: import("../types/templates").QuotationTemplate }>(
      `/api/v1/quotation-templates/${templateId}`
    ),
  create: (payload: import("../types/templates").CreateQuotationTemplateRequest) =>
    apiFetch<{ success: boolean; data: import("../types/templates").QuotationTemplate }>(
      "/api/v1/quotation-templates",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (templateId: string, payload: import("../types/templates").UpdateQuotationTemplateRequest) =>
    apiFetch<{ success: boolean; data: import("../types/templates").QuotationTemplate }>(
      `/api/v1/quotation-templates/${templateId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  delete: (templateId: string) =>
    apiFetch<{ success: boolean; message?: string }>(
      `/api/v1/quotation-templates/${templateId}`,
      { method: "DELETE" }
    ),
  restore: (templateId: string) =>
    apiFetch<{ success: boolean; data: import("../types/templates").QuotationTemplate }>(
      `/api/v1/quotation-templates/${templateId}/restore`,
      { method: "POST" }
    ),
  apply: (templateId: string, clientId: string) =>
    apiFetch<{
      success: boolean;
      data: {
        clientId: string;
        quotationDate?: string;
        validUntil?: string;
        discountPercentage: number;
        notes?: string;
        lineItems: Array<{
          itemName: string;
          description?: string;
          quantity: number;
          unitRate: number;
        }>;
      };
    }>(`/api/v1/quotation-templates/${templateId}/apply?clientId=${clientId}`, {
      method: "POST",
    }),
  getVersions: (templateId: string) =>
    apiFetch<{
      success: boolean;
      data: import("../types/templates").QuotationTemplateVersion[];
    }>(`/api/v1/quotation-templates/${templateId}/versions`),
  approve: (templateId: string) =>
    apiFetch<{ success: boolean; data: import("../types/templates").QuotationTemplate }>(
      `/api/v1/quotation-templates/${templateId}/approve`,
      { method: "POST" }
    ),
  getUsageStats: (params: { startDate?: string; endDate?: string } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<{
      success: boolean;
      data: import("../types/templates").TemplateUsageStats;
    }>(`/api/v1/quotation-templates/usage-stats?${q.toString()}`);
  },
  getPublic: () =>
    apiFetch<{
      success: boolean;
      data: import("../types/templates").QuotationTemplate[];
    }>("/api/v1/quotation-templates/public"),
  upload: (formData: FormData) =>
    apiFetch<{ success: boolean; data: import("../types/templates").QuotationTemplate }>(
      "/api/v1/quotation-templates/upload",
      { method: "POST", body: formData, auth: true }
    ),
};

export const DiscountApprovalsApi = {
  request: (request: import("../types/discount-approvals").CreateDiscountApprovalRequest) =>
    apiFetch<{ success: boolean; data: import("../types/discount-approvals").DiscountApproval }>(
      "/api/v1/discount-approvals/request",
      { method: "POST", body: JSON.stringify(request) }
    ),
  getPending: (params: {
    approverUserId?: string;
    status?: string;
    discountPercentageMin?: number;
    discountPercentageMax?: number;
    dateFrom?: string;
    dateTo?: string;
    requestedByUserId?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<import("../types/discount-approvals").PagedApprovalsResult>(
      `/api/v1/discount-approvals/pending?${q.toString()}`
    );
  },
  approve: (approvalId: string, request: import("../types/discount-approvals").ApproveDiscountApprovalRequest) =>
    apiFetch<{ success: boolean; data: import("../types/discount-approvals").DiscountApproval }>(
      `/api/v1/discount-approvals/${approvalId}/approve`,
      { method: "POST", body: JSON.stringify(request) }
    ),
  reject: (approvalId: string, request: import("../types/discount-approvals").RejectDiscountApprovalRequest) =>
    apiFetch<{ success: boolean; data: import("../types/discount-approvals").DiscountApproval }>(
      `/api/v1/discount-approvals/${approvalId}/reject`,
      { method: "POST", body: JSON.stringify(request) }
    ),
  getById: (approvalId: string) =>
    apiFetch<{ success: boolean; data: import("../types/discount-approvals").DiscountApproval }>(
      `/api/v1/discount-approvals/${approvalId}`
    ),
  escalate: (approvalId: string, reason?: string) =>
    apiFetch<{ success: boolean; data: import("../types/discount-approvals").DiscountApproval }>(
      `/api/v1/discount-approvals/${approvalId}/escalate`,
      { method: "POST", body: JSON.stringify({ reason }) }
    ),
  getQuotationApprovals: (quotationId: string) =>
    apiFetch<{ success: boolean; data: import("../types/discount-approvals").DiscountApproval[] }>(
      `/api/v1/discount-approvals/by-quotation/${quotationId}`
    ),
  getReports: (params: { dateFrom?: string; dateTo?: string; approverUserId?: string } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<{ success: boolean; data: import("../types/discount-approvals").ApprovalMetrics }>(
      `/api/v1/discount-approvals/reports?${q.toString()}`
    );
  },
  getTimeline: (params: { approvalId?: string; quotationId?: string }) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<{ success: boolean; data: import("../types/discount-approvals").ApprovalTimeline[] }>(
      `/api/v1/discount-approvals/timeline?${q.toString()}`
    );
  },
  resubmit: (approvalId: string, request: import("../types/discount-approvals").ResubmitDiscountApprovalRequest) =>
    apiFetch<{ success: boolean; data: import("../types/discount-approvals").DiscountApproval }>(
      `/api/v1/discount-approvals/${approvalId}/resubmit`,
      { method: "POST", body: JSON.stringify(request) }
    ),
  bulkApprove: (request: import("../types/discount-approvals").BulkApproveRequest) =>
    apiFetch<{ success: boolean; data: import("../types/discount-approvals").DiscountApproval[] }>(
      "/api/v1/discount-approvals/bulk-approve",
      { method: "POST", body: JSON.stringify(request) }
    ),
};

export const NotificationsApi = {
  get: (params: {
    unread?: boolean;
    archived?: boolean;
    eventType?: string;
    entityType?: string;
    entityId?: string;
    dateFrom?: string;
    dateTo?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<import("../types/notifications").PagedNotificationsResult>(
      `/api/v1/notifications?${q.toString()}`
    );
  },
  getUnreadCount: () =>
    apiFetch<{ success: boolean; data: import("../types/notifications").UnreadCount }>(
      "/api/v1/notifications/unread-count"
    ),
  getPreferences: () =>
    apiFetch<{ success: boolean; data: import("../types/notifications").NotificationPreferences }>(
      "/api/v1/notifications/preferences"
    ),
  getEntityNotifications: (entityType: string, entityId: string) =>
    apiFetch<{ success: boolean; data: import("../types/notifications").Notification[] }>(
      `/api/v1/notifications/entity/${entityType}/${entityId}`
    ),
  markRead: (request: import("../types/notifications").MarkNotificationsReadRequest) =>
    apiFetch<{ success: boolean }>(
      "/api/v1/notifications/mark-read",
      { method: "POST", body: JSON.stringify(request) }
    ),
  archive: (request: import("../types/notifications").ArchiveNotificationsRequest) =>
    apiFetch<{ success: boolean }>(
      "/api/v1/notifications/archive",
      { method: "POST", body: JSON.stringify(request) }
    ),
  unarchive: (request: import("../types/notifications").UnarchiveNotificationsRequest) =>
    apiFetch<{ success: boolean }>(
      "/api/v1/notifications/unarchive",
      { method: "POST", body: JSON.stringify(request) }
    ),
  updatePreferences: (request: import("../types/notifications").UpdateNotificationPreferencesRequest) =>
    apiFetch<{ success: boolean; data: import("../types/notifications").NotificationPreferences }>(
      "/api/v1/notifications/preferences",
      { method: "PUT", body: JSON.stringify(request) }
    ),
  getEmailLogs: (params: {
    userId?: string;
    recipientEmail?: string;
    eventType?: string;
    status?: string;
    dateFrom?: string;
    dateTo?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<import("../types/notifications").PagedEmailLogsResult>(
      `/api/v1/notifications/email-logs?${q.toString()}`
    );
  },
};

export const PaymentsApi = {
  initiate: (request: import("../types/payments").InitiatePaymentRequest) =>
    apiFetch<import("../types/payments").PaymentDto>(
      "/api/v1/payments/initiate",
      { method: "POST", body: JSON.stringify(request) }
    ),
  getById: (paymentId: string) =>
    apiFetch<import("../types/payments").PaymentDto>(
      `/api/v1/payments/${paymentId}`
    ),
  getByQuotation: (quotationId: string) =>
    apiFetch<import("../types/payments").PaymentDto[]>(
      `/api/v1/payments/quotations/${quotationId}`
    ),
  refund: (paymentId: string, request: import("../types/payments").RefundPaymentRequest) =>
    apiFetch<import("../types/payments").PaymentDto>(
      `/api/v1/payments/${paymentId}/refund`,
      { method: "POST", body: JSON.stringify(request) }
    ),
  cancel: (paymentId: string) =>
    apiFetch<import("../types/payments").PaymentDto>(
      `/api/v1/payments/${paymentId}/cancel`,
      { method: "POST" }
    ),
  getDashboard: (params: {
    userId?: string;
    startDate?: string;
    endDate?: string;
  } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<import("../types/payments").PaymentDashboardDto>(
      `/api/v1/payments/dashboard?${q.toString()}`
    );
  },
  getByUser: (userId: string, params: {
    status?: string;
    startDate?: string;
    endDate?: string;
    quotationId?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}) => {
    const q = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
    });
    return apiFetch<{ success: boolean; data: import("../types/payments").PaymentDto[]; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/payments/user/${userId}?${q.toString()}`
    );
  },
};

export const PaymentGatewaysApi = {
  createConfig: (request: import("../types/payments").CreatePaymentGatewayConfigRequest) =>
    apiFetch<import("../types/payments").PaymentGatewayConfigDto>(
      "/api/v1/payment-gateways/config",
      { method: "POST", body: JSON.stringify(request) }
    ),
  updateConfig: (configId: string, request: import("../types/payments").UpdatePaymentGatewayConfigRequest) =>
    apiFetch<import("../types/payments").PaymentGatewayConfigDto>(
      `/api/v1/payment-gateways/config/${configId}`,
      { method: "PUT", body: JSON.stringify(request) }
    ),
  deleteConfig: (configId: string) =>
    apiFetch<void>(
      `/api/v1/payment-gateways/config/${configId}`,
      { method: "DELETE" }
    ),
  getConfigs: (companyId?: string) => {
    const q = companyId ? `?companyId=${companyId}` : "";
    return apiFetch<import("../types/payments").PaymentGatewayConfigDto[]>(
      `/api/v1/payment-gateways/config${q}`
    );
  },
};

// Admin API (Spec-018)
export const AdminApi = {
  // System Settings
  getSystemSettings: () =>
    apiFetch<{ success: boolean; data: import("../types/admin").SystemSettingsDto }>(
      "/api/v1/admin/settings"
    ),
  updateSystemSettings: (payload: import("../types/admin").UpdateSystemSettingsRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/admin").SystemSettingsDto }>(
      "/api/v1/admin/settings",
      { method: "POST", body: JSON.stringify(payload) }
    ),

  // Integration Keys
  getIntegrationKeys: () =>
    apiFetch<{ success: boolean; data: import("../types/admin").IntegrationKeyDto[] }>(
      "/api/v1/admin/integrations"
    ),
  getIntegrationKeyById: (id: string) =>
    apiFetch<{ success: boolean; data: import("../types/admin").IntegrationKeyDto }>(
      `/api/v1/admin/integrations/${id}`
    ),
  getIntegrationKeyWithValue: (id: string) =>
    apiFetch<{ success: boolean; data: import("../types/admin").IntegrationKeyDto }>(
      `/api/v1/admin/integrations/${id}/show`
    ),
  createIntegrationKey: (payload: import("../types/admin").CreateIntegrationKeyRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/admin").IntegrationKeyDto }>(
      "/api/v1/admin/integrations",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  updateIntegrationKey: (id: string, payload: import("../types/admin").UpdateIntegrationKeyRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/admin").IntegrationKeyDto }>(
      `/api/v1/admin/integrations/${id}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  deleteIntegrationKey: (id: string) =>
    apiFetch<{ success: boolean; message: string }>(
      `/api/v1/admin/integrations/${id}`,
      { method: "DELETE" }
    ),

  // Audit Logs
  getAuditLogs: (params?: {
    actionType?: string;
    entity?: string;
    performedBy?: string;
    startDate?: string;
    endDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }) => {
    const query = new URLSearchParams();
    if (params?.actionType) query.append("actionType", params.actionType);
    if (params?.entity) query.append("entity", params.entity);
    if (params?.performedBy) query.append("performedBy", params.performedBy);
    if (params?.startDate) query.append("startDate", params.startDate);
    if (params?.endDate) query.append("endDate", params.endDate);
    if (params?.pageNumber) query.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize) query.append("pageSize", params.pageSize.toString());
    return apiFetch<{
      success: boolean;
      data: import("../types/admin").AuditLogDto[];
      pageNumber: number;
      pageSize: number;
      totalCount: number;
    }>(`/api/v1/admin/audit-logs${query.toString() ? `?${query.toString()}` : ""}`);
  },
  getAuditLogById: (id: string) =>
    apiFetch<{ success: boolean; data: import("../types/admin").AuditLogDto }>(
      `/api/v1/admin/audit-logs/${id}`
    ),
  exportAuditLogs: async (params?: {
    actionType?: string;
    entity?: string;
    performedBy?: string;
    startDate?: string;
    endDate?: string;
  }) => {
    const query = new URLSearchParams();
    if (params?.actionType) query.append("actionType", params.actionType);
    if (params?.entity) query.append("entity", params.entity);
    if (params?.performedBy) query.append("performedBy", params.performedBy);
    if (params?.startDate) query.append("startDate", params.startDate);
    if (params?.endDate) query.append("endDate", params.endDate);
    
    const url = `${API_BASE}/api/v1/admin/audit-logs/export${query.toString() ? `?${query.toString()}` : ""}`;
    const token = getAccessToken();
    const headers = new Headers();
    if (token) headers.set("Authorization", `Bearer ${token}`);
    
    const res = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
    });
    
    if (!res.ok) {
      throw new Error(`Failed to export audit logs: ${res.statusText}`);
    }
    
    const blob = await res.blob();
    const downloadUrl = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = downloadUrl;
    a.download = `audit-logs-${new Date().toISOString().split("T")[0]}.csv`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(downloadUrl);
    document.body.removeChild(a);
  },

  // Branding
  getBranding: () =>
    apiFetch<{ success: boolean; data: import("../types/admin").CustomBrandingDto | null }>(
      "/api/v1/admin/branding"
    ),
  updateBranding: (payload: import("../types/admin").UpdateBrandingRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/admin").CustomBrandingDto }>(
      "/api/v1/admin/branding",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  uploadLogo: (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return apiFetch<{ success: boolean; message: string; data: import("../types/admin").CustomBrandingDto }>(
      "/api/v1/admin/branding/logo",
      {
        method: "POST",
        body: formData,
        headers: {}, // Let browser set Content-Type with boundary
      }
    );
  },

  // Data Retention
  getDataRetentionPolicies: () =>
    apiFetch<{ success: boolean; data: import("../types/admin").DataRetentionPolicyDto[] }>(
      "/api/v1/admin/data-retention"
    ),
  updateDataRetentionPolicy: (payload: import("../types/admin").UpdateDataRetentionPolicyRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/admin").DataRetentionPolicyDto }>(
      "/api/v1/admin/data-retention",
      { method: "POST", body: JSON.stringify(payload) }
    ),

  // Notification Settings
  getNotificationSettings: () =>
    apiFetch<{ success: boolean; data: import("../types/admin").NotificationSettingsDto | null }>(
      "/api/v1/admin/notification-settings"
    ),
  updateNotificationSettings: (payload: import("../types/admin").UpdateNotificationSettingsRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/admin").NotificationSettingsDto }>(
      "/api/v1/admin/notification-settings",
      { method: "POST", body: JSON.stringify(payload) }
    ),
};

// User Management API (Spec-019)
export const CountriesApi = {
  list: (params?: { isActive?: boolean; pageNumber?: number; pageSize?: number }) => {
    const q = new URLSearchParams();
    if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
    if (params?.pageNumber !== undefined) q.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize !== undefined) q.append("pageSize", params.pageSize.toString());
    return apiFetch<{ success: boolean; data: any[]; pageNumber?: number; pageSize?: number; totalCount?: number }>(
      `/api/v1/admin/tax/countries${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  getById: (countryId: string) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/countries/${countryId}`
    ),
  create: (payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      "/api/v1/admin/tax/countries",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (countryId: string, payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/countries/${countryId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
};

export const TaxFrameworksApi = {
  list: (params?: { countryId?: string; isActive?: boolean }) => {
    const q = new URLSearchParams();
    if (params?.countryId) q.append("countryId", params.countryId);
    if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
    return apiFetch<{ success: boolean; data: any[] }>(
      `/api/v1/admin/tax/frameworks${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  getById: (frameworkId: string) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/frameworks/${frameworkId}`
    ),
  create: (payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      "/api/v1/admin/tax/frameworks",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (frameworkId: string, payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/frameworks/${frameworkId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
};

export const JurisdictionsApi = {
  listByCountry: (countryId: string) =>
    apiFetch<{ success: boolean; data: any[] }>(
      `/api/v1/admin/tax/countries/${countryId}/jurisdictions`
    ),
  getById: (jurisdictionId: string) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/jurisdictions/${jurisdictionId}`
    ),
  create: (payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      "/api/v1/admin/tax/jurisdictions",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (jurisdictionId: string, payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/jurisdictions/${jurisdictionId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  delete: (jurisdictionId: string) =>
    apiFetch<{ success: boolean }>(
      `/api/v1/admin/tax/jurisdictions/${jurisdictionId}`,
      { method: "DELETE" }
    ),
};

export const ProductServiceCategoriesApi = {
  list: (params?: { isActive?: boolean }) => {
    const q = new URLSearchParams();
    if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
    return apiFetch<{ success: boolean; data: any[] }>(
      `/api/v1/admin/tax/categories${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  getById: (categoryId: string) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/categories/${categoryId}`
    ),
  create: (payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      "/api/v1/admin/tax/categories",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (categoryId: string, payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/categories/${categoryId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
};

export const TaxRatesApi = {
  list: (params?: { jurisdictionId?: string; taxFrameworkId?: string; productServiceCategoryId?: string; asOfDate?: string }) => {
    const q = new URLSearchParams();
    if (params?.jurisdictionId) q.append("jurisdictionId", params.jurisdictionId);
    if (params?.taxFrameworkId) q.append("taxFrameworkId", params.taxFrameworkId);
    if (params?.productServiceCategoryId) q.append("productServiceCategoryId", params.productServiceCategoryId);
    if (params?.asOfDate) q.append("asOfDate", params.asOfDate);
    return apiFetch<{ success: boolean; data: any[] }>(
      `/api/v1/admin/tax/rates${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  getByJurisdiction: (jurisdictionId: string, params?: { productServiceCategoryId?: string; asOfDate?: string }) => {
    const q = new URLSearchParams();
    if (params?.productServiceCategoryId) q.append("productServiceCategoryId", params.productServiceCategoryId);
    if (params?.asOfDate) q.append("asOfDate", params.asOfDate);
    return apiFetch<{ success: boolean; data: any[] }>(
      `/api/v1/admin/tax/rates/jurisdiction/${jurisdictionId}${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  create: (payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      "/api/v1/admin/tax/rates",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (taxRateId: string, payload: any) =>
    apiFetch<{ success: boolean; data: any }>(
      `/api/v1/admin/tax/rates/${taxRateId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  delete: (taxRateId: string) =>
    apiFetch<{ success: boolean }>(
      `/api/v1/admin/tax/rates/${taxRateId}`,
      { method: "DELETE" }
    ),
};

export const TaxCalculationApi = {
  getCountries: () =>
    apiFetch<{ success: boolean; data: Array<{ countryId: string; countryName: string; countryCode: string; isDefault: boolean }> }>(
      "/api/v1/tax/calculation/countries"
    ),
  preview: (payload: {
    clientId: string;
    lineItems: Array<{ lineItemId: string; productServiceCategoryId?: string; amount: number }>;
    subtotal: number;
    discountAmount: number;
    calculationDate?: string;
    countryId?: string;
  }) =>
    apiFetch<{ success: boolean; data: any }>(
      "/api/v1/tax/calculation/preview",
      { method: "POST", body: JSON.stringify(payload) }
    ),
};

export const TaxAuditLogApi = {
  list: (params?: {
    quotationId?: string;
    countryId?: string;
    jurisdictionId?: string;
    fromDate?: string;
    toDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }) => {
    const q = new URLSearchParams();
    if (params?.quotationId) q.append("quotationId", params.quotationId);
    if (params?.countryId) q.append("countryId", params.countryId);
    if (params?.jurisdictionId) q.append("jurisdictionId", params.jurisdictionId);
    if (params?.fromDate) q.append("fromDate", params.fromDate);
    if (params?.toDate) q.append("toDate", params.toDate);
    if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
    return apiFetch<{ success: boolean; data: any[]; pageNumber: number; pageSize: number; totalCount: number }>(
      `/api/v1/admin/tax/audit-log${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
};

export const UserManagementApi = {
  // Teams
  teams: {
    list: (params?: { pageNumber?: number; pageSize?: number; teamLeadUserId?: string; companyId?: string; isActive?: boolean }) => {
      const q = new URLSearchParams();
      if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
      if (params?.teamLeadUserId) q.append("teamLeadUserId", params.teamLeadUserId);
      if (params?.companyId) q.append("companyId", params.companyId);
      if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
      return apiFetch<import("../types/userManagement").PagedResult<import("../types/userManagement").Team>>(
        `/api/v1/teams${q.toString() ? `?${q.toString()}` : ""}`
      );
    },
    getById: (teamId: string) =>
      apiFetch<{ success: boolean; data: import("../types/userManagement").Team }>(
        `/api/v1/teams/${teamId}`
      ),
    create: (payload: import("../types/userManagement").CreateTeamRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").Team }>(
        "/api/v1/teams",
        { method: "POST", body: JSON.stringify(payload) }
      ),
    update: (teamId: string, payload: import("../types/userManagement").UpdateTeamRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").Team }>(
        `/api/v1/teams/${teamId}`,
        { method: "PUT", body: JSON.stringify(payload) }
      ),
    delete: (teamId: string) =>
      apiFetch<{ success: boolean; message: string }>(
        `/api/v1/teams/${teamId}`,
        { method: "DELETE" }
      ),
    addMember: (teamId: string, payload: import("../types/userManagement").AddTeamMemberRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").TeamMember }>(
        `/api/v1/teams/${teamId}/members`,
        { method: "POST", body: JSON.stringify(payload) }
      ),
    removeMember: (teamId: string, userId: string) =>
      apiFetch<{ success: boolean; message: string }>(
        `/api/v1/teams/${teamId}/members/${userId}`,
        { method: "DELETE" }
      ),
    getMembers: (teamId: string, params?: { pageNumber?: number; pageSize?: number }) => {
      const q = new URLSearchParams();
      if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
      return apiFetch<import("../types/userManagement").PagedResult<import("../types/userManagement").TeamMember>>(
        `/api/v1/teams/${teamId}/members${q.toString() ? `?${q.toString()}` : ""}`
      );
    },
  },

  // User Groups
  userGroups: {
    list: (params?: { pageNumber?: number; pageSize?: number; createdByUserId?: string }) => {
      const q = new URLSearchParams();
      if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
      if (params?.createdByUserId) q.append("createdByUserId", params.createdByUserId);
      return apiFetch<import("../types/userManagement").PagedResult<import("../types/userManagement").UserGroup>>(
        `/api/v1/user-groups${q.toString() ? `?${q.toString()}` : ""}`
      );
    },
    getById: (groupId: string) =>
      apiFetch<{ success: boolean; data: import("../types/userManagement").UserGroup }>(
        `/api/v1/user-groups/${groupId}`
      ),
    create: (payload: import("../types/userManagement").CreateUserGroupRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").UserGroup }>(
        "/api/v1/user-groups",
        { method: "POST", body: JSON.stringify(payload) }
      ),
    update: (groupId: string, payload: import("../types/userManagement").UpdateUserGroupRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").UserGroup }>(
        `/api/v1/user-groups/${groupId}`,
        { method: "PUT", body: JSON.stringify(payload) }
      ),
    addMember: (groupId: string, userId: string) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").UserGroupMember }>(
        `/api/v1/user-groups/${groupId}/members`,
        { method: "POST", body: JSON.stringify({ userId }) }
      ),
    removeMember: (groupId: string, userId: string) =>
      apiFetch<{ success: boolean; message: string }>(
        `/api/v1/user-groups/${groupId}/members/${userId}`,
        { method: "DELETE" }
      ),
  },

  // Task Assignments
  tasks: {
    assign: (payload: import("../types/userManagement").AssignTaskRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").TaskAssignment }>(
        "/api/v1/task-assignments",
        { method: "POST", body: JSON.stringify(payload) }
      ),
    getUserTasks: (userId: string, params?: { pageNumber?: number; pageSize?: number; status?: string; entityType?: string; dueDateFrom?: string; dueDateTo?: string }) => {
      const q = new URLSearchParams();
      if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
      if (params?.status) q.append("status", params.status);
      if (params?.entityType) q.append("entityType", params.entityType);
      if (params?.dueDateFrom) q.append("dueDateFrom", params.dueDateFrom);
      if (params?.dueDateTo) q.append("dueDateTo", params.dueDateTo);
      return apiFetch<import("../types/userManagement").PagedResult<import("../types/userManagement").TaskAssignment>>(
        `/api/v1/task-assignments/user/${userId}${q.toString() ? `?${q.toString()}` : ""}`
      );
    },
    updateStatus: (assignmentId: string, payload: import("../types/userManagement").UpdateTaskStatusRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").TaskAssignment }>(
        `/api/v1/task-assignments/${assignmentId}/status`,
        { method: "PUT", body: JSON.stringify(payload) }
      ),
    delete: (assignmentId: string) =>
      apiFetch<{ success: boolean; message: string }>(
        `/api/v1/task-assignments/${assignmentId}`,
        { method: "DELETE" }
      ),
  },

  // Activity Feed
  activity: {
    getFeed: (params?: { pageNumber?: number; pageSize?: number; userId?: string; actionType?: string; entityType?: string; fromDate?: string; toDate?: string }) => {
      const q = new URLSearchParams();
      if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
      if (params?.userId) q.append("userId", params.userId);
      if (params?.actionType) q.append("actionType", params.actionType);
      if (params?.entityType) q.append("entityType", params.entityType);
      if (params?.fromDate) q.append("fromDate", params.fromDate);
      if (params?.toDate) q.append("toDate", params.toDate);
      return apiFetch<import("../types/userManagement").PagedActivityFeedResult>(
        `/api/v1/activity-feed${q.toString() ? `?${q.toString()}` : ""}`
      );
    },
    getUserActivity: (userId: string, params?: { pageNumber?: number; pageSize?: number; actionType?: string; fromDate?: string; toDate?: string }) => {
      const q = new URLSearchParams();
      if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
      if (params?.actionType) q.append("actionType", params.actionType);
      if (params?.fromDate) q.append("fromDate", params.fromDate);
      if (params?.toDate) q.append("toDate", params.toDate);
      return apiFetch<import("../types/userManagement").PagedActivityFeedResult>(
        `/api/v1/activity-feed/users/${userId}/activity${q.toString() ? `?${q.toString()}` : ""}`
      );
    },
  },

  // Mentions
  mentions: {
    create: (payload: import("../types/userManagement").CreateMentionRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").Mention }>(
        "/api/v1/mentions",
        { method: "POST", body: JSON.stringify(payload) }
      ),
    getUserMentions: (userId: string, params?: { pageNumber?: number; pageSize?: number; isRead?: boolean }) => {
      const q = new URLSearchParams();
      if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
      if (params?.isRead !== undefined) q.append("isRead", params.isRead.toString());
      return apiFetch<import("../types/userManagement").PagedMentionsResult>(
        `/api/v1/mentions/user/${userId}${q.toString() ? `?${q.toString()}` : ""}`
      );
    },
    getUnreadCount: (userId: string) =>
      apiFetch<{ success: boolean; count: number }>(
        `/api/v1/mentions/user/${userId}/unread-count`
      ),
    markAsRead: (mentionId: string) =>
      apiFetch<{ success: boolean; message: string }>(
        `/api/v1/mentions/${mentionId}/mark-read`,
        { method: "PUT" }
      ),
  },

  // User Profiles
  profiles: {
    getProfile: (userId: string) =>
      apiFetch<{ success: boolean; data: import("../types/userManagement").EnhancedUserProfile }>(
        `/api/v1/user-profiles/${userId}`
      ),
    updateProfile: (userId: string, payload: import("../types/userManagement").UpdateUserProfileRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").EnhancedUserProfile }>(
        `/api/v1/user-profiles/${userId}`,
        { method: "PUT", body: JSON.stringify(payload) }
      ),
    setOutOfOffice: (userId: string, payload: import("../types/userManagement").SetOutOfOfficeRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").EnhancedUserProfile }>(
        `/api/v1/user-profiles/${userId}/out-of-office`,
        { method: "PUT", body: JSON.stringify(payload) }
      ),
    updatePresence: (userId: string, status: string) =>
      apiFetch<{ success: boolean; message: string }>(
        `/api/v1/user-profiles/${userId}/presence`,
        { method: "PUT", body: JSON.stringify(status) }
      ),
  },

  // Bulk Operations
  bulk: {
    inviteUsers: (payload: import("../types/userManagement").BulkInviteUsersRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").BulkOperationResult }>(
        "/api/v1/bulk-user-operations/invite",
        { method: "POST", body: JSON.stringify(payload) }
      ),
    updateUsers: (payload: import("../types/userManagement").BulkUpdateUsersRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").BulkOperationResult }>(
        "/api/v1/bulk-user-operations/update",
        { method: "PUT", body: JSON.stringify(payload) }
      ),
    deactivateUsers: (userIds: string[]) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").BulkOperationResult }>(
        "/api/v1/bulk-user-operations/deactivate",
        { method: "POST", body: JSON.stringify(userIds) }
      ),
    exportUsers: (params?: { format?: string; roleId?: string; teamId?: string; isActive?: boolean; createdFrom?: string; createdTo?: string }) => {
      const q = new URLSearchParams();
      if (params?.format) q.append("format", params.format);
      if (params?.roleId) q.append("roleId", params.roleId);
      if (params?.teamId) q.append("teamId", params.teamId);
      if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
      if (params?.createdFrom) q.append("createdFrom", params.createdFrom);
      if (params?.createdTo) q.append("createdTo", params.createdTo);
      const url = `${API_BASE}/api/v1/bulk-user-operations/export${q.toString() ? `?${q.toString()}` : ""}`;
      const token = getAccessToken();
      const headers = new Headers();
      if (token) headers.set("Authorization", `Bearer ${token}`);
      return fetch(url, { headers, credentials: "include" })
        .then(res => res.blob())
        .then(blob => {
          const downloadUrl = window.URL.createObjectURL(blob);
          const a = document.createElement("a");
          a.href = downloadUrl;
          a.download = `users_export_${new Date().toISOString().split("T")[0]}.${params?.format?.toLowerCase() || "csv"}`;
          document.body.appendChild(a);
          a.click();
          window.URL.revokeObjectURL(downloadUrl);
          document.body.removeChild(a);
        });
    },
  },

  // Custom Roles
  customRoles: {
    list: (params?: { pageNumber?: number; pageSize?: number; isActive?: boolean; includeBuiltIn?: boolean }) => {
      const q = new URLSearchParams();
      if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
      if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
      if (params?.includeBuiltIn !== undefined) q.append("includeBuiltIn", params.includeBuiltIn.toString());
      return apiFetch<import("../types/userManagement").PagedResult<import("../types/userManagement").CustomRole>>(
        `/api/v1/custom-roles${q.toString() ? `?${q.toString()}` : ""}`
      );
    },
    getAvailablePermissions: () =>
      apiFetch<{ success: boolean; data: import("../types/userManagement").Permission[] }>(
        "/api/v1/custom-roles/permissions"
      ),
    create: (payload: import("../types/userManagement").CreateCustomRoleRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").CustomRole }>(
        "/api/v1/custom-roles",
        { method: "POST", body: JSON.stringify(payload) }
      ),
    updatePermissions: (roleId: string, payload: import("../types/userManagement").UpdateRolePermissionsRequest) =>
      apiFetch<{ success: boolean; message: string; data: import("../types/userManagement").CustomRole }>(
        `/api/v1/custom-roles/${roleId}/permissions`,
        { method: "PUT", body: JSON.stringify(payload) }
      ),
  },
};

export const ProductsApi = {
  list: (params?: {
    pageNumber?: number;
    pageSize?: number;
    productType?: import("../types/products").ProductType;
    categoryId?: string;
    isActive?: boolean;
    search?: string;
    currency?: string;
  }) => {
    const q = new URLSearchParams();
    if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
    if (params?.productType) q.append("productType", params.productType);
    if (params?.categoryId) q.append("categoryId", params.categoryId);
    if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
    if (params?.search) q.append("search", params.search);
    if (params?.currency) q.append("currency", params.currency);
    return apiFetch<import("../types/products").PagedProductResult>(
      `/api/v1/products${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  getById: (productId: string) =>
    apiFetch<{ success: boolean; data: import("../types/products").Product }>(
      `/api/v1/products/${productId}`
    ),
  create: (payload: import("../types/products").CreateProductRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/products").Product }>(
      "/api/v1/products",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (productId: string, payload: Partial<import("../types/products").CreateProductRequest>) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/products").Product }>(
      `/api/v1/products/${productId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  delete: (productId: string) =>
    apiFetch<{ success: boolean; message?: string }>(
      `/api/v1/products/${productId}`,
      { method: "DELETE" }
    ),
  getCatalog: (params?: {
    pageNumber?: number;
    pageSize?: number;
    productType?: import("../types/products").ProductType;
    categoryId?: string;
    search?: string;
    currency?: string;
  }) => {
    const q = new URLSearchParams();
    if (params?.pageNumber) q.append("pageNumber", params.pageNumber.toString());
    if (params?.pageSize) q.append("pageSize", params.pageSize.toString());
    if (params?.productType) {
      // Convert ProductType string to enum integer value (backend enum starts at 1)
      const productTypeMap: Record<import("../types/products").ProductType, number> = {
        "Subscription": 1,
        "AddOnSubscription": 2,
        "AddOnOneTime": 3,
        "CustomDevelopment": 4,
      };
      const enumValue = productTypeMap[params.productType];
      if (enumValue !== undefined) {
        q.append("productType", enumValue.toString());
      }
    }
    if (params?.categoryId) q.append("categoryId", params.categoryId);
    if (params?.search) q.append("search", params.search);
    if (params?.currency) q.append("currency", params.currency);
    return apiFetch<import("../types/products").PagedProductCatalogResult>(
      `/api/v1/products/catalog${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  calculatePrice: (payload: import("../types/products").ProductPriceCalculationRequest) =>
    apiFetch<{ success: boolean; data: import("../types/products").ProductPriceCalculationResponse }>(
      "/api/v1/products/calculate-price",
      { method: "POST", body: JSON.stringify(payload) }
    ),
    getUsageStats: (productId: string) =>
      apiFetch<{ success: boolean; data: import("../types/products").ProductUsageStats }>(
        `/api/v1/products/${productId}/usage`
      ),
    addProductToQuotation: (quotationId: string, payload: {
      productId: string;
      quantity: number;
      billingCycle?: number;
      hours?: number;
      taxCategoryId?: string;
    }) =>
      apiFetch<{ success: boolean; data: import("../types/quotations").LineItemDto; message?: string }>(
        `/api/v1/products/quotations/${quotationId}/add-product`,
        { method: "POST", body: JSON.stringify(payload) }
      ),
  };

export const ProductCategoriesApi = {
  list: (params?: { parentCategoryId?: string; isActive?: boolean }) => {
    const q = new URLSearchParams();
    if (params?.parentCategoryId) q.append("parentCategoryId", params.parentCategoryId);
    if (params?.isActive !== undefined) q.append("isActive", params.isActive.toString());
    return apiFetch<{ success: boolean; data: import("../types/products").ProductCategory[] }>(
      `/api/v1/product-categories${q.toString() ? `?${q.toString()}` : ""}`
    );
  },
  getById: (categoryId: string) =>
    apiFetch<{ success: boolean; data: import("../types/products").ProductCategory }>(
      `/api/v1/product-categories/${categoryId}`
    ),
  create: (payload: import("../types/products").CreateProductCategoryRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/products").ProductCategory }>(
      "/api/v1/product-categories",
      { method: "POST", body: JSON.stringify(payload) }
    ),
  update: (categoryId: string, payload: import("../types/products").UpdateProductCategoryRequest) =>
    apiFetch<{ success: boolean; message: string; data: import("../types/products").ProductCategory }>(
      `/api/v1/product-categories/${categoryId}`,
      { method: "PUT", body: JSON.stringify(payload) }
    ),
};

export const CompanyDetailsApi = {
  get: () =>
    apiFetch<{ success: boolean; data: {
      companyDetailsId: string;
      panNumber?: string;
      tanNumber?: string;
      gstNumber?: string;
      companyName?: string;
      companyAddress?: string;
      city?: string;
      state?: string;
      postalCode?: string;
      country?: string;
      countryId?: string;
      contactEmail?: string;
      contactPhone?: string;
      website?: string;
      legalDisclaimer?: string;
      logoUrl?: string;
      updatedAt: string;
      bankDetails: Array<{
        bankDetailsId: string;
        country: string;
        accountNumber: string;
        ifscCode?: string;
        iban?: string;
        swiftCode?: string;
        bankName: string;
        branchName?: string;
      }>;
    } }>(
      "/api/v1/company-details"
    ),
  update: (payload: {
    panNumber?: string;
    tanNumber?: string;
    gstNumber?: string;
    companyName?: string;
    companyAddress?: string;
    city?: string;
    state?: string;
    postalCode?: string;
    country?: string;
    countryId?: string; // Required for country-specific storage
    contactEmail?: string;
    contactPhone?: string;
    website?: string;
    legalDisclaimer?: string;
    logoUrl?: string;
    bankDetails: Array<{
      bankDetailsId?: string;
      country: string;
      accountNumber: string;
      ifscCode?: string;
      iban?: string;
      swiftCode?: string;
      bankName: string;
      branchName?: string;
    }>;
  }) =>
    apiFetch<{ success: boolean; message: string; data: any }>(
      "/api/v1/company-details",
      { method: "PUT", body: JSON.stringify(payload) }
    ),
  uploadLogo: (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    return apiFetch<{ success: boolean; message: string; data: any }>(
      "/api/v1/company-details/logo",
      { method: "POST", body: formData }
    );
  },
};

// Export new API clients
export { IdentifierTypesApi } from "./api/identifierTypes";
export { CountryIdentifierConfigurationsApi } from "./api/countryIdentifierConfigurations";
export { BankFieldTypesApi } from "./api/bankFieldTypes";
export { CountryBankFieldConfigurationsApi } from "./api/countryBankFieldConfigurations";
export { CompanyIdentifiersApi } from "./api/companyIdentifiers";
export { CompanyBankDetailsApi } from "./api/companyBankDetails";
export type { IdentifierType, CreateIdentifierTypeRequest, UpdateIdentifierTypeRequest } from "./api/identifierTypes";
export type { CountryIdentifierConfiguration, ConfigureCountryIdentifierRequest, UpdateCountryIdentifierConfigurationRequest } from "./api/countryIdentifierConfigurations";
export type { BankFieldType, CreateBankFieldTypeRequest, UpdateBankFieldTypeRequest } from "./api/bankFieldTypes";
export type { CountryBankFieldConfiguration, ConfigureCountryBankFieldRequest, UpdateCountryBankFieldConfigurationRequest } from "./api/countryBankFieldConfigurations";
export type { CompanyIdentifierField, CompanyIdentifierValues, SaveCompanyIdentifierValuesRequest } from "./api/companyIdentifiers";
export type { CompanyBankField, CompanyBankDetails, SaveCompanyBankDetailsRequest } from "./api/companyBankDetails";