# Spec-015: Reporting, Analytics & Business Intelligence

**Project**: CRM Quotation Management System  
**Spec Number**: Spec-015  
**Spec Name**: Reporting, Analytics & Business Intelligence  
**Group**: Analytics & Reporting (Group 6 of 11)  
**Priority**: HIGH (Phase 2, after Payment Processing)  
**Status**: Specification Complete

## Dependencies

- **Spec-009**: QuotationEntity (Quotation CRUD)
- **Spec-010**: QuotationManagement (Quotation lifecycle, responses)
- **Spec-012**: DiscountApprovalWorkflow (Approval metrics)
- **Spec-014**: PaymentProcessing (Payment analytics)

## Related Specs

- **Spec-013**: NotificationSystem (Report delivery)
- **Spec-016**: Refund & Adjustment Workflow (Future)
- **Spec-017**: Advanced Features (Future)

---

## Overview

This specification defines comprehensive reporting and analytics capabilities for all stakeholders (Sales Reps, Managers, Finance, Admin). The system provides insights into quotation pipeline, conversion rates, discount trends, approval metrics, revenue forecasts, payment status, team performance, and client activity.

**Key Features:**
- Real-time dashboards for sales, managers, finance, and admins
- Pre-built reports: Pipeline, Conversion, Revenue, Discounts, Approval Metrics, Payment Status
- Custom report builder (select metrics, filters, group by, sort, date range)
- Export reports (PDF, Excel, CSV) with formatting
- Scheduled reports (daily, weekly, monthly via email)
- Interactive charts and visualizations (line, bar, pie, funnel)
- Drill-down capability (click chart point to see details)
- Performance metrics by user, team, client, time period
- Forecasting & trend analysis (quotation success rate, revenue prediction)
- Approval workflow analytics (approval TAT, rejection rate, escalation %)
- Discount analytics (% by rep, manager approvals, profit impact)
- Client engagement metrics (quotations sent, viewed %, response time)
- Payment analytics (collection rate, failed payments, refunds)
- Audit trail exports (compliance reports)
- Advanced filtering and segmentation
- Benchmark comparisons (team vs company average)
- KPI cards with targets and variance indicators
- Mobile-responsive dashboards

---

## JTBD Alignment

### Persona: Sales Manager
**JTBD**: "I want to see team performance, quotation pipeline, and approval bottlenecks at a glance"  
**Success Metric**: "Identify top performers, spot delays, and forecast revenue in seconds"

### Persona: Finance/Admin
**JTBD**: "I need to track payments, discounts, and ensure compliance with audit reports"  
**Success Metric**: "All financial metrics accurate and exportable for audits/leadership"

### Persona: Sales Rep
**JTBD**: "I want to see my performance metrics and track my quotations' progress"  
**Success Metric**: "Quickly understand my conversion rate and pipeline value"

---

## Business Value

- **Data-driven decision making** for sales and finance teams
- **Identifies bottlenecks** in approval process
- **Tracks team performance** and individual productivity
- **Forecasts revenue** and predicts closure rates
- **Monitors discount compliance** and impact on margins
- **Enables compliance** with audit and financial regulations
- **Uncovers trends** and opportunities for process improvement
- **Improves accountability** through transparent metrics
- **Optimizes resource allocation** based on performance data

---

## Database Schema Summary

### AnalyticsMetricsSnapshot
Caches pre-calculated metrics for performance:
- `SnapshotId` (UUID, PK)
- `MetricType` (string: "DailySales", "TeamPerformance", "PaymentStatus", etc.)
- `UserId` (UUID, FK, nullable if global metric)
- `MetricData` (JSONB, stores aggregated values)
- `CalculatedAt` (TIMESTAMPTZ)
- `PeriodDate` (DATE, for time-series)
- `CreatedAt` (TIMESTAMPTZ)

### DashboardBookmarks
Saves user dashboard configurations:
- `BookmarkId` (UUID, PK)
- `UserId` (UUID, FK)
- `DashboardName` (string)
- `DashboardConfig` (JSONB, stores layout, selected metrics, filters)
- `IsDefault` (bool)
- `CreatedAt`, `UpdatedAt`

### ScheduledReports
Manages scheduled report delivery:
- `ReportId` (UUID, PK)
- `CreatedByUserId` (UUID, FK)
- `ReportName` (string)
- `ReportType` (string)
- `ReportConfig` (JSONB)
- `RecurrencePattern` (string: "daily", "weekly", "monthly")
- `EmailRecipients` (VARCHAR)
- `IsActive` (bool)
- `LastSentAt` (TIMESTAMPTZ, nullable)
- `NextScheduledAt` (TIMESTAMPTZ)
- `CreatedAt`, `UpdatedAt`

### ExportedReports
Tracks exported report files:
- `ExportId` (UUID, PK)
- `CreatedByUserId` (UUID, FK)
- `ReportType` (string)
- `ExportFormat` (string: "pdf", "excel", "csv")
- `FilePath` (string)
- `FileSize` (int)
- `CreatedAt` (TIMESTAMPTZ)

---

## Backend Services & Commands

### Queries

1. **GetSalesDashboardMetricsQuery**
   - Returns: Pipeline value, conversion rate, quotations created, pending approvals
   - Filters: UserId, DateRange

2. **GetTeamPerformanceMetricsQuery**
   - Returns: Per-user metrics (quotations, conversion, pipeline value)
   - Filters: TeamId, DateRange, UserId

3. **GetApprovalWorkflowMetricsQuery**
   - Returns: Approval TAT, rejection rate, escalation %, pending count
   - Filters: ManagerId, DateRange

4. **GetDiscountAnalyticsQuery**
   - Returns: Discount % by rep, approval rates, margin impact
   - Filters: DateRange, UserId, TeamId

5. **GetPaymentAnalyticsQuery**
   - Returns: Collection %, failed payments, refunds, payment methods
   - Filters: DateRange, Gateway, Status

6. **GetClientEngagementMetricsQuery**
   - Returns: Views, response %, conversion %, response time
   - Filters: ClientId, DateRange

7. **GenerateCustomReportQuery**
   - Returns: Flexible report with custom metrics, filters, grouping
   - Input: ReportType, Filters (JSONB), GroupBy, SortBy, Limit

8. **GetForecastingDataQuery**
   - Returns: Revenue forecast, trend prediction, success rate projection
   - Input: Days, ConfidenceLevel

9. **GetAuditComplianceReportQuery**
   - Returns: All changes, approvals, payments for compliance
   - Filters: DateRange, EntityType, UserId

### Commands

1. **GenerateReportCommand**
   - Creates report, handles caching/processing
   - Input: ReportGenerationRequest

2. **ExportReportCommand**
   - Exports report to PDF/Excel/CSV
   - Input: ReportId, Format

3. **ScheduleReportCommand**
   - Creates/updates scheduled report
   - Input: ScheduleReportRequest

4. **CancelScheduledReportCommand**
   - Deactivates scheduled report
   - Input: ReportId

5. **SaveDashboardBookmarkCommand**
   - Saves custom dashboard configuration
   - Input: DashboardConfig

6. **DeleteDashboardBookmarkCommand**
   - Deletes saved dashboard
   - Input: BookmarkId

### Background Jobs

1. **DailyMetricsCalculationJob**
   - Recalculates snapshots for caching
   - Runs daily at 2 AM

2. **ScheduledReportExecutionJob**
   - Triggers scheduled report emails
   - Runs every hour, checks NextScheduledAt

3. **ReportCleanupJob**
   - Archives old exports (>90 days)
   - Runs weekly

---

## API Endpoints

### Dashboard Endpoints

1. **GET** `/api/v1/reports/dashboard/sales`
   - Sales rep dashboard metrics
   - Auth: SalesRep, Manager, Admin

2. **GET** `/api/v1/reports/dashboard/manager`
   - Manager dashboard metrics
   - Auth: Manager, Admin

3. **GET** `/api/v1/reports/dashboard/finance`
   - Finance/payment dashboard
   - Auth: Finance, Admin

4. **GET** `/api/v1/reports/dashboard/admin`
   - Admin overview dashboard
   - Auth: Admin only

### Report Generation

5. **GET** `/api/v1/reports/custom`
   - Generate custom report with filters
   - Query params: ReportType, Filters (JSON), GroupBy, SortBy, Format

6. **POST** `/api/v1/reports/export`
   - Export report to PDF/Excel/CSV
   - Body: ReportId, Format

7. **GET** `/api/v1/reports/export-history`
   - List of exported reports
   - Auth: User sees own exports, Admin sees all

### Scheduled Reports

8. **POST** `/api/v1/reports/scheduled`
   - Create/update scheduled report
   - Body: ScheduleReportRequest

9. **GET** `/api/v1/reports/scheduled`
   - List scheduled reports
   - Auth: User sees own, Admin sees all

10. **DELETE** `/api/v1/reports/scheduled/{reportId}`
    - Delete scheduled report
    - Auth: Owner or Admin

### Advanced Reports

11. **GET** `/api/v1/reports/forecasting`
    - Revenue forecast, trend data
    - Query params: Days, ConfidenceLevel

12. **GET** `/api/v1/reports/audit`
    - Compliance audit report
    - Auth: Admin only
    - Query params: DateRange, EntityType

### Dashboard Bookmarks

13. **POST** `/api/v1/dashboard/bookmarks`
    - Save custom dashboard layout
    - Body: DashboardConfig

14. **GET** `/api/v1/dashboard/bookmarks`
    - List saved dashboards
    - Auth: User sees own bookmarks

15. **POST** `/api/v1/reports/send-test`
    - Admin send test email
    - Auth: Admin only
    - Body: ReportId, EmailRecipients

---

## Frontend UI Components

### Critical: Frontend UI is NOT OPTIONAL

All dashboards and reports must be implemented in the frontend using TailAdmin Next.js theme.

### Dashboard Pages

#### REP-D01: Sales Rep Dashboard (`/dashboard`)
- **KPI Cards** (top row):
  - Quotations Created (this month)
  - Total Value (open pipeline)
  - Conversion Rate (%)
  - Pending Approvals (count)

- **Charts**:
  - Line chart: Quotations created/sent trend (30 days)
  - Pie chart: Status breakdown (DRAFT, SENT, VIEWED, ACCEPTED, REJECTED)
  - Bar chart: Top clients by quotation value

- **Recent Activity**:
  - List of last 10 quotations with status and date
  - Click to view detail

- **Action buttons**: "Create Quotation", "View Pending Approvals"

#### MGR-D01: Manager Dashboard (`/dashboard/manager`)
- **KPI Cards**:
  - Team Quotations (this month)
  - Team Conversion Rate
  - Average Discount %
  - Pending Approvals (this manager's queue)
  - Total Value at Risk

- **Charts**:
  - Line chart: Team quota vs actual (by week/month)
  - Bar chart: Per-rep performance (quotations, conversion, avg discount)
  - Funnel chart: Pipeline stages (draft → sent → viewed → accepted)
  - Heatmap: Discount % by rep (showing compliance)

- **Team Member Cards**:
  - Individual rep stats: quotations, pipeline value, conversion %, pending approvals
  - Traffic light: Green (on target), Yellow (watch), Red (action needed)

- **Approval Queue**:
  - Pending discount approvals for manager's team
  - Quick-approve, bulk-approve options

#### FIN-D01: Finance/Payment Dashboard (`/dashboard/finance`)
- **KPI Cards**:
  - Total Payments Received (this month)
  - Payment Success Rate (%)
  - Failed Payments (count, alert if >5%)
  - Total Refunds
  - Collection %

- **Charts**:
  - Line chart: Payment collection trend
  - Bar chart: Payment methods (Stripe, Razorpay, etc.) distribution
  - Funnel: Quotations → Accepted → Paid

- **Payment List**:
  - All payments with status (pending, success, failed, refunded)
  - Filter, search, sort by date, status, amount
  - Retry button for failed payments
  - Refund button for successful payments

#### ADM-D01: Admin Overview Dashboard (`/admin/dashboard`)
- **KPI Cards**:
  - Active Users (sales reps, managers)
  - Total Quotations (lifetime)
  - Total Revenue (all accepted & paid)
  - System Health (errors, API uptime)

- **System Metrics**:
  - Database size, API response time
  - Error rate, user activity

- **Charts**:
  - Growth chart: Quotations/revenue over time
  - Usage chart: Daily active users

### Reports & Export Pages

#### REP-R01: Custom Report Builder (`/reports/custom`)
- **Multi-select interface**:
  - Metric selection (checkboxes: quotations created, sent, viewed, accepted, etc.)
  - Date range picker (from/to dates, or presets: today, this week, this month, this year)
  - Filters: User, Team, Client, Status, Discount % range
  - Group By: Date (daily/weekly/monthly), User, Team, Client
  - Sort By: Date, metric name, ascending/descending

- **Preview**:
  - Table with selected data
  - Charts (auto-select appropriate chart type based on data)

- **Export**:
  - PDF (formatted, with charts)
  - Excel (with multiple sheets)
  - CSV (for data import)

- **"Run Report" button** (execute, display, or export)

#### REP-R02: Pre-built Reports (dropdown/menu)
- Sales Pipeline Report
- Team Performance Report
- Discount Analysis Report
- Approval Metrics Report
- Payment Status Report
- Client Engagement Report
- Forecasting Report
- Audit Trail Report

Each opens full page with metrics, filters, export, drill-down.

#### REP-R03: Scheduled Reports (`/reports/scheduled`)
- List of user's scheduled reports (name, type, recurrence, last sent, next scheduled)
- Create new: Modal to select report type, set frequency, add recipients, save
- Actions: Edit, Pause, Delete, Test (send immediately)
- View delivery history

#### ADM-R01: Scheduled Reports Admin (`/admin/reports/scheduled`)
- View/manage all users' scheduled reports
- Monitor delivery status and errors
- Disable problematic reports

#### REP-R04: Export History (`/reports/exports`)
- Table: Report name, format, size, created date, download link, delete button
- Filter by date, format, report type
- Clean up old exports

### Visualization Components

- **KPICard**: Large number + trend arrow + color
- **LineChart, BarChart, PieChart, FunnelChart, HeatmapChart**: Via ApexCharts
- **DataTable**: Sortable, filterable, paginated
- **MetricSelector**: Multi-checkbox with categories
- **DateRangePicker**: From/to, presets
- **FilterPanel**: Dropdown/select for user, team, client, status, discount %
- **ReportPreview**: Table + charts combined
- **ExportButton**: PDF, Excel, CSV options
- **ChartTooltip**: On hover, show exact values
- **DrillDown**: Click chart point, show detailed breakdown
- **TrafficLightIndicator**: Green/yellow/red based on target

### Custom Hooks

- `useReport(reportType, filters)` - Fetch report data, caching
- `useCharts(data)` - Transform data for chart components
- `useDashboardBookmarks()` - Save/load dashboard configs
- `useReportExport(format)` - Trigger export, handle download
- `useScheduledReports()` - CRUD scheduled reports
- `useForecast(days)` - Get forecast data with trend line

### API Service Layer

- `reportService.ts` - GET reports, POST exports, manage schedules
- `chartsService.ts` - Transform data for visualization
- `exportService.ts` - Handle PDF/Excel/CSV generation
- `forecastingService.ts` - Call forecasting API, return prediction data

### TypeScript Types

- `ReportType`, `ReportData`, `DashboardConfig`, `ScheduledReportConfig`
- `MetricFilter`, `ChartData`, `ForecastingData`

---

## UX/Design Considerations

- Dashboard widgets can be resized/rearranged (drag-drop or settings)
- Charts interactive (hover tooltips, click drill-down)
- Color-coding: Green for positive trends, red for risks, yellow for warnings
- Mobile-responsive: Cards stack, charts responsive, tables horizontal scroll
- Dark mode support (if company requests)
- Accessibility: All charts have data tables underneath, keyboard navigation
- Load time <2 seconds for dashboard (cache metrics snapshots)
- Real-time updates for critical KPIs (via WebSocket or polling)
- Export dialog shows progress bar and download link
- Scheduled report confirmation modal with recipient list

---

## Test Cases

### Backend

1. Dashboard metrics calculated correctly (queries return accurate aggregates)
2. Custom report builder generates report with correct filters/grouping
3. Export generates valid PDF/Excel/CSV files
4. Scheduled reports execute on schedule, email sent successfully
5. Forecasting model returns reasonable predictions
6. Audit report includes all compliance data
7. Performance: Dashboard loads <2 seconds, cached metrics used
8. Metrics snapshots updated daily without errors

### Frontend

1. Dashboard loads with all KPI cards and charts
2. Custom report builder filters apply correctly
3. Charts render with correct data and interactivity
4. Export button generates downloadable file in correct format
5. Scheduled report list shows and can be edited/deleted
6. Drill-down opens detailed view
7. Mobile responsive (dashboard cards, charts, tables)
8. Real-time metrics update (if WebSocket implemented)
9. Accessibility: Charts have data tables, keyboard nav works
10. E2E: Create custom report → export → verify file content
11. Performance: No lag on filter changes, export <5 seconds
12. Error handling: Show graceful messages on API failures

---

## Deliverables

### Backend (40+ files)
- All DTO, entity, query, and command models
- Report generation service (aggregation logic)
- Export service (PDF/Excel/CSV generation via libraries)
- Forecasting service (trend analysis, prediction)
- Scheduled report job and email trigger
- Dashboard metrics calculation and caching
- Controllers for all API endpoints
- Validators for report configs
- Migration scripts

### Backend Tests (20+)
- Query accuracy tests
- Export format validation
- Scheduled job execution
- Performance/load tests
- Forecasting model validation

### Frontend (50+ files)
- All dashboard pages (sales, manager, finance, admin)
- Custom report builder page
- Pre-built report templates
- Export history page
- Scheduled reports management
- All chart and visualization components
- Hooks, API services, TypeScript types
- Responsive/mobile styles
- Accessibility configurations

### Frontend Tests (25+)
- Component rendering tests
- Filter/sort logic tests
- Export flow tests
- Integration tests (API + UI)
- E2E tests for major report flows
- Mobile responsiveness tests
- Accessibility tests

---

## Acceptance Criteria

### Backend
✅ All report metrics calculated accurately from quotation/payment/approval data  
✅ Custom report builder generates correct reports with filters/grouping  
✅ Exports valid, correctly formatted (PDF/Excel/CSV)  
✅ Scheduled reports execute on schedule, emails delivered  
✅ Forecasting provides reasonable trend predictions  
✅ Dashboard metrics cached and updated daily  
✅ Performance: Reports generated <5 seconds

### Frontend
✅ All dashboards load with accurate KPI cards and charts  
✅ All charts interactive and responsive  
✅ Custom report builder intuitive and functional  
✅ Export button works, downloads valid files  
✅ Scheduled reports manageable from UI  
✅ Mobile responsive across all pages  
✅ Error messages clear and helpful  
✅ Accessibility standards met (WCAG 2.1 AA)

### Integration
✅ Backend and frontend built in parallel (not sequential)  
✅ API and UI seamlessly integrated  
✅ Data flows correctly from database → API → UI  
✅ No backend-only features (all exposed in UI)  
✅ All test cases pass (unit, integration, E2E)  
✅ Backend coverage ≥85%, Frontend coverage ≥80%

---

## Implementation Notes

### Performance
- Cache dashboard metrics daily (snapshot table)
- Lazy-load charts (render on tab click)
- Paginate report tables (50 rows per page)
- Use database aggregation (GROUP BY, SUM) not app-level
- Compress PDF exports (images optimized)

### Scalability
- Metrics calculation runs in background job (not on-demand)
- Reports queued (async processing for large datasets)
- WebSocket optional (polling fallback for metrics updates)
- Database indexes on frequently queried columns

### Frontend Best Practices
- Use React Query for caching report data
- Memoize expensive chart components
- Debounce filter changes (300ms)
- Lazy-load heavy chart libraries (ApexCharts)
- Server-side sorting for large datasets

### Compliance & Security
- Only authenticated users see reports
- Managers see only their team's data
- Finance sees payment data only if authorized
- Audit reports logged for compliance
- Export files encrypted (optional)
- IP whitelisting for scheduled report emails (optional)

---

## Next Specs (Sequence)

After Spec 15 completes:
→ **Spec 16**: Refund & Adjustment Workflow  
→ **Spec 17**: Advanced Features (multi-currency, multi-language, etc.)  
→ **Spec 18**: System Administration & Configuration

---

**END OF SPEC 15: Reporting, Analytics & Business Intelligence**

