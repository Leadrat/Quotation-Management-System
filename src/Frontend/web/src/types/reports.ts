export enum ExportFormat {
  Pdf = "pdf",
  Excel = "excel",
  Csv = "csv",
}

export enum RecurrencePattern {
  Daily = "daily",
  Weekly = "weekly",
  Monthly = "monthly",
}

export interface ReportGenerationRequest {
  reportType: string;
  filters?: Record<string, any>;
  groupBy?: string;
  sortBy?: string;
  limit?: number;
  format: string;
}

export interface KPIMetric {
  name: string;
  value: string;
  unit?: string;
  numericValue?: number;
  trend?: "up" | "down" | "stable";
  color?: "green" | "red" | "yellow";
}

export interface ChartSeries {
  name: string;
  data: number[];
}

export interface ChartData {
  chartType: string;
  title: string;
  series: ChartSeries[];
  categories: string[];
}

export interface ReportData {
  reportType: string;
  title: string;
  summary?: string;
  metrics: KPIMetric[];
  charts: ChartData[];
  details: Record<string, any>[];
}

export interface DashboardConfig {
  layout: "grid" | "flex";
  widgets: DashboardWidget[];
  filters?: Record<string, any>;
}

export interface DashboardWidget {
  id: string;
  type: string;
  metric: string;
  position: WidgetPosition;
}

export interface WidgetPosition {
  row: number;
  col: number;
  width: number;
  height: number;
}

export interface ScheduleReportRequest {
  reportName: string;
  reportType: string;
  reportConfig: Record<string, any>;
  recurrencePattern: RecurrencePattern;
  emailRecipients: string;
}

export interface ScheduledReport {
  reportId: string;
  reportName: string;
  reportType: string;
  reportConfig?: Record<string, any>;
  recurrencePattern: RecurrencePattern;
  emailRecipients: string;
  isActive: boolean;
  lastSentAt?: string;
  nextScheduledAt: string;
  createdAt: string;
}

export interface ExportReportRequest {
  reportId: string;
  format: ExportFormat;
}

export interface ExportedReport {
  exportId: string;
  reportType: string;
  exportFormat: ExportFormat;
  filePath: string;
  fileSize: number;
  createdAt: string;
  downloadUrl: string;
}

// Dashboard Metrics DTOs
export interface SalesDashboardMetrics {
  quotationsCreatedThisMonth: number;
  totalPipelineValue: number;
  conversionRate: number;
  pendingApprovals: number;
  quotationsSentThisMonth: number;
  quotationsAcceptedThisMonth: number;
  quotationTrend: QuotationTrendData[];
  statusBreakdown: StatusBreakdownData[];
  topClients: TopClientData[];
  recentQuotations: RecentQuotationData[];
}

export interface QuotationTrendData {
  date: string;
  created: number;
  sent: number;
}

export interface StatusBreakdownData {
  status: string;
  count: number;
  percentage: number;
}

export interface TopClientData {
  clientId: string;
  clientName: string;
  totalValue: number;
  quotationCount: number;
}

export interface RecentQuotationData {
  quotationId: string;
  quotationNumber: string;
  clientName: string;
  status: string;
  createdAt: string;
}

export interface TeamPerformance {
  userId: string;
  userName: string;
  quotationsCreated: number;
  quotationsSent: number;
  quotationsAccepted: number;
  conversionRate: number;
  pipelineValue: number;
  averageDiscount: number;
  pendingApprovals: number;
  rank: number;
  trend: PerformanceTrendData[];
}

export interface PerformanceTrendData {
  date: string;
  quotationsCreated: number;
  conversionRate: number;
}

export interface ApprovalMetrics {
  averageApprovalTAT: number;
  rejectionRate: number;
  escalationPercent: number;
  pendingApprovals: number;
  approvalTATByPeriod: ApprovalTATData[];
  approvalStatusBreakdown: ApprovalStatusData[];
  escalations: EscalationData[];
}

export interface ApprovalTATData {
  period: string;
  averageHours: number;
}

export interface ApprovalStatusData {
  status: string;
  count: number;
  percentage: number;
}

export interface EscalationData {
  approvalId: string;
  quotationId: string;
  quotationNumber: string;
  discountAmount: number;
  escalatedAt: string;
}

export interface DiscountAnalytics {
  averageDiscountPercent: number;
  approvalRate: number;
  marginImpact: number;
  discountByRep: DiscountByRepData[];
  approvalRates: ApprovalRateData[];
  marginImpactByPeriod: MarginImpactData[];
}

export interface DiscountByRepData {
  userId: string;
  userName: string;
  averageDiscount: number;
  requestCount: number;
}

export interface ApprovalRateData {
  status: string;
  count: number;
  percentage: number;
}

export interface MarginImpactData {
  period: string;
  totalDiscountAmount: number;
  marginImpact: number;
}

export interface PaymentAnalytics {
  collectionRate: number;
  failedPaymentsCount: number;
  totalRefunds: number;
  paymentMethodDistribution: PaymentMethodDistributionData[];
  paymentStatusBreakdown: PaymentStatusData[];
  refunds: RefundData[];
}

export interface PaymentMethodDistributionData {
  paymentMethod: string;
  count: number;
  amount: number;
  percentage: number;
}

export interface PaymentStatusData {
  status: string;
  count: number;
  amount: number;
  percentage: number;
}

export interface RefundData {
  paymentId: string;
  quotationId: string;
  quotationNumber: string;
  refundAmount: number;
  reason: string;
  refundDate: string;
}

export interface ClientEngagement {
  viewRate: number;
  responseRate: number;
  conversionRate: number;
  averageResponseTimeHours: number;
  clientEngagement: ClientEngagementData[];
  responseTimeByPeriod: ResponseTimeData[];
}

export interface ClientEngagementData {
  clientId: string;
  clientName: string;
  quotationsSent: number;
  quotationsViewed: number;
  quotationsResponded: number;
  quotationsAccepted: number;
  viewRate: number;
  responseRate: number;
  conversionRate: number;
}

export interface ResponseTimeData {
  period: string;
  averageHours: number;
}

export interface ForecastingData {
  predictedRevenue: number;
  confidenceLevel: number;
  predictedSuccessRate: number;
  revenueForecast: RevenueForecastData[];
  trend: TrendData[];
}

export interface RevenueForecastData {
  date: string;
  predictedRevenue: number;
  lowerBound?: number;
  upperBound?: number;
}

export interface TrendData {
  date: string;
  actualValue: number;
  predictedValue?: number;
}

export interface AdminDashboardMetrics {
  activeUsers: number;
  activeSalesReps: number;
  activeManagers: number;
  totalQuotationsLifetime: number;
  totalRevenue: number;
  systemHealth: SystemHealthData;
  growthChart: GrowthData[];
  usageChart: UsageData[];
}

export interface SystemHealthData {
  errorCount: number;
  apiUptime: number;
  databaseSizeMB: number;
  averageResponseTimeMs: number;
}

export interface GrowthData {
  period: string;
  quotations: number;
  revenue: number;
}

export interface UsageData {
  date: string;
  dailyActiveUsers: number;
}

export interface ManagerDashboardMetrics {
  teamQuotationsThisMonth: number;
  teamConversionRate: number;
  averageDiscountPercent: number;
  pendingApprovals: number;
  totalValueAtRisk: number;
  teamQuotaVsActual: TeamQuotaData[];
  repPerformance: RepPerformanceData[];
  pipelineStages: PipelineStageData[];
  discountCompliance: DiscountComplianceData[];
  teamMembers: TeamMemberData[];
  pendingApprovalsList: PendingApprovalData[];
}

export interface TeamQuotaData {
  period: string;
  quota: number;
  actual: number;
}

export interface RepPerformanceData {
  userId: string;
  userName: string;
  quotationsCreated: number;
  conversionRate: number;
  averageDiscount: number;
}

export interface PipelineStageData {
  stage: string;
  count: number;
  value: number;
}

export interface DiscountComplianceData {
  userId: string;
  userName: string;
  averageDiscount: number;
  status: "green" | "yellow" | "red";
}

export interface TeamMemberData {
  userId: string;
  userName: string;
  quotationsCreated: number;
  pipelineValue: number;
  conversionRate: number;
  pendingApprovals: number;
  status: "green" | "yellow" | "red";
}

export interface PendingApprovalData {
  approvalId: string;
  quotationId: string;
  quotationNumber: string;
  clientName: string;
  discountAmount: number;
  discountPercent: number;
  requestedAt: string;
}

export interface FinanceDashboardMetrics {
  totalPaymentsReceivedThisMonth: number;
  paymentSuccessRate: number;
  failedPaymentsCount: number;
  totalRefunds: number;
  collectionPercent: number;
  paymentTrend: PaymentTrendData[];
  paymentMethodDistribution: PaymentMethodDistributionData[];
  paymentFunnel: PaymentFunnelData[];
  payments: PaymentListData[];
}

export interface PaymentTrendData {
  date: string;
  amount: number;
  count: number;
}

export interface PaymentFunnelData {
  stage: string;
  count: number;
  value: number;
}

export interface PaymentListData {
  paymentId: string;
  quotationId: string;
  quotationNumber: string;
  clientName: string;
  paymentGateway: string;
  amount: number;
  currency: string;
  status: string;
  paymentDate?: string;
  createdAt: string;
}

export interface AuditReport {
  fromDate: string;
  toDate: string;
  changes: AuditEntryData[];
  approvals: ApprovalHistoryData[];
  payments: PaymentHistoryData[];
  userActivity: UserActivityData[];
}

export interface AuditEntryData {
  entryId: string;
  entityType: string;
  entityId: string;
  action: string;
  userId: string;
  userName: string;
  timestamp: string;
  changes?: Record<string, any>;
}

export interface ApprovalHistoryData {
  approvalId: string;
  quotationId: string;
  quotationNumber: string;
  status: string;
  requestedByUserId: string;
  requestedByUserName: string;
  approvedByUserId?: string;
  approvedByUserName?: string;
  requestedAt: string;
  approvedAt?: string;
}

export interface PaymentHistoryData {
  paymentId: string;
  quotationId: string;
  quotationNumber: string;
  paymentGateway: string;
  amount: number;
  status: string;
  createdAt: string;
  paymentDate?: string;
}

export interface UserActivityData {
  userId: string;
  userName: string;
  activityType: string;
  timestamp: string;
  details?: string;
}

