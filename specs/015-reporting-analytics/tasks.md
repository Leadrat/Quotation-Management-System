# Spec-015: Detailed Tasks - Reporting, Analytics & Business Intelligence

## Phase 1: Database & Entities

### Task 1.1: Create AnalyticsMetricsSnapshot Migration
- [ ] Create migration file: `YYYYMMDDHHMMSS_CreateAnalyticsMetricsSnapshotTable.cs`
- [ ] Define table schema with all columns
- [ ] Add foreign key to Users table
- [ ] Create indexes: MetricType+PeriodDate, UserId, CalculatedAt
- [ ] Test migration up/down

**File**: `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateAnalyticsMetricsSnapshotTable.cs`

### Task 1.2: Create DashboardBookmarks Migration
- [ ] Create migration file: `YYYYMMDDHHMMSS_CreateDashboardBookmarksTable.cs`
- [ ] Define table schema with all columns
- [ ] Add foreign key to Users table
- [ ] Create indexes: UserId, unique UserId+IsDefault
- [ ] Test migration up/down

**File**: `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateDashboardBookmarksTable.cs`

### Task 1.3: Create ScheduledReports Migration
- [ ] Create migration file: `YYYYMMDDHHMMSS_CreateScheduledReportsTable.cs`
- [ ] Define table schema with all columns
- [ ] Add foreign key to Users table
- [ ] Add check constraint for RecurrencePattern
- [ ] Create indexes: CreatedByUserId, IsActive+NextScheduledAt
- [ ] Test migration up/down

**File**: `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateScheduledReportsTable.cs`

### Task 1.4: Create ExportedReports Migration
- [ ] Create migration file: `YYYYMMDDHHMMSS_CreateExportedReportsTable.cs`
- [ ] Define table schema with all columns
- [ ] Add foreign key to Users table
- [ ] Add check constraint for ExportFormat
- [ ] Create indexes: CreatedByUserId, CreatedAt
- [ ] Test migration up/down

**File**: `src/Backend/CRM.Infrastructure/Migrations/YYYYMMDDHHMMSS_CreateExportedReportsTable.cs`

### Task 1.5: Create AnalyticsMetricsSnapshot Entity
- [ ] Create `AnalyticsMetricsSnapshot.cs` in Domain/Entities
- [ ] Add all properties with correct types
- [ ] Add navigation property to User
- [ ] Add XML documentation

**File**: `src/Backend/CRM.Domain/Entities/AnalyticsMetricsSnapshot.cs`

### Task 1.6: Create DashboardBookmark Entity
- [ ] Create `DashboardBookmark.cs` in Domain/Entities
- [ ] Add all properties with correct types
- [ ] Add navigation property to User
- [ ] Add XML documentation

**File**: `src/Backend/CRM.Domain/Entities/DashboardBookmark.cs`

### Task 1.7: Create ScheduledReport Entity
- [ ] Create `ScheduledReport.cs` in Domain/Entities
- [ ] Add all properties with correct types
- [ ] Add navigation property to User
- [ ] Add XML documentation

**File**: `src/Backend/CRM.Domain/Entities/ScheduledReport.cs`

### Task 1.8: Create ExportedReport Entity
- [ ] Create `ExportedReport.cs` in Domain/Entities
- [ ] Add all properties with correct types
- [ ] Add navigation property to User
- [ ] Add XML documentation

**File**: `src/Backend/CRM.Domain/Entities/ExportedReport.cs`

### Task 1.9: Create MetricType Constants
- [ ] Create `MetricType.cs` in Domain/Enums
- [ ] Add all constant values
- [ ] Add XML documentation

**File**: `src/Backend/CRM.Domain/Enums/MetricType.cs`

### Task 1.10: Create RecurrencePattern Enum
- [ ] Create `RecurrencePattern.cs` in Domain/Enums
- [ ] Add Daily, Weekly, Monthly values
- [ ] Add XML documentation

**File**: `src/Backend/CRM.Domain/Enums/RecurrencePattern.cs`

### Task 1.11: Create ExportFormat Enum
- [ ] Create `ExportFormat.cs` in Domain/Enums
- [ ] Add Pdf, Excel, Csv values
- [ ] Add XML documentation

**File**: `src/Backend/CRM.Domain/Enums/ExportFormat.cs`

### Task 1.12: Create Entity Configurations
- [ ] Create `AnalyticsMetricsSnapshotEntityConfiguration.cs`
- [ ] Create `DashboardBookmarkEntityConfiguration.cs`
- [ ] Create `ScheduledReportEntityConfiguration.cs`
- [ ] Create `ExportedReportEntityConfiguration.cs`
- [ ] Configure JSONB columns
- [ ] Configure relationships
- [ ] Configure indexes

**Files**: 
- `src/Backend/CRM.Infrastructure/EntityConfigurations/AnalyticsMetricsSnapshotEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/DashboardBookmarkEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/ScheduledReportEntityConfiguration.cs`
- `src/Backend/CRM.Infrastructure/EntityConfigurations/ExportedReportEntityConfiguration.cs`

### Task 1.13: Update DbContext
- [ ] Add DbSet<AnalyticsMetricsSnapshot> to AppDbContext
- [ ] Add DbSet<DashboardBookmark> to AppDbContext
- [ ] Add DbSet<ScheduledReport> to AppDbContext
- [ ] Add DbSet<ExportedReport> to AppDbContext
- [ ] Update IAppDbContext interface
- [ ] Apply entity configurations

**Files**: 
- `src/Backend/CRM.Infrastructure/Persistence/AppDbContext.cs`
- `src/Backend/CRM.Application/Common/Persistence/IAppDbContext.cs`

---

## Phase 2: DTOs & Request Models

### Task 2.1: Create ReportGenerationRequest DTO
- [ ] Create `ReportGenerationRequest.cs`
- [ ] Add ReportType, Filters (JSONB), GroupBy, SortBy, Limit, Format properties
- [ ] Add data annotations

**File**: `src/Backend/CRM.Application/Reports/Dtos/ReportGenerationRequest.cs`

### Task 2.2: Create ReportData DTO
- [ ] Create `ReportData.cs`
- [ ] Add ReportType, Title, Summary, Metrics, Charts, Details properties
- [ ] Add nested DTOs for metrics and charts

**File**: `src/Backend/CRM.Application/Reports/Dtos/ReportData.cs`

### Task 2.3: Create DashboardConfig DTO
- [ ] Create `DashboardConfig.cs`
- [ ] Add Layout, Widgets, Filters properties
- [ ] Add nested DTOs for widgets and filters

**File**: `src/Backend/CRM.Application/Reports/Dtos/DashboardConfig.cs`

### Task 2.4: Create ScheduleReportRequest DTO
- [ ] Create `ScheduleReportRequest.cs`
- [ ] Add ReportName, ReportType, ReportConfig, RecurrencePattern, EmailRecipients properties
- [ ] Add data annotations

**File**: `src/Backend/CRM.Application/Reports/Dtos/ScheduleReportRequest.cs`

### Task 2.5: Create Dashboard Metrics DTOs
- [ ] Create `SalesDashboardMetricsDto.cs`
- [ ] Create `ManagerDashboardMetricsDto.cs`
- [ ] Create `FinanceDashboardMetricsDto.cs`
- [ ] Create `AdminDashboardMetricsDto.cs`
- [ ] Add KPI properties for each

**Files**: 
- `src/Backend/CRM.Application/Reports/Dtos/SalesDashboardMetricsDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/ManagerDashboardMetricsDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/FinanceDashboardMetricsDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/AdminDashboardMetricsDto.cs`

### Task 2.6: Create Analytics DTOs
- [ ] Create `TeamPerformanceDto.cs`
- [ ] Create `ApprovalMetricsDto.cs`
- [ ] Create `DiscountAnalyticsDto.cs`
- [ ] Create `PaymentAnalyticsDto.cs`
- [ ] Create `ClientEngagementDto.cs`
- [ ] Create `ForecastingDataDto.cs`
- [ ] Create `AuditReportDto.cs`

**Files**: 
- `src/Backend/CRM.Application/Reports/Dtos/TeamPerformanceDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/ApprovalMetricsDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/DiscountAnalyticsDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/PaymentAnalyticsDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/ClientEngagementDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/ForecastingDataDto.cs`
- `src/Backend/CRM.Application/Reports/Dtos/AuditReportDto.cs`

### Task 2.7: Create Export Request DTOs
- [ ] Create `ExportReportRequest.cs`
- [ ] Create `ExportedReportDto.cs`
- [ ] Add properties for export configuration

**Files**: 
- `src/Backend/CRM.Application/Reports/Dtos/ExportReportRequest.cs`
- `src/Backend/CRM.Application/Reports/Dtos/ExportedReportDto.cs`

---

## Phase 3: Query Handlers - Dashboard Metrics

### Task 3.1: Create GetSalesDashboardMetricsQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Calculate pipeline value (sum of open quotations)
- [ ] Calculate conversion rate (accepted / sent)
- [ ] Count quotations created/sent this month
- [ ] Count pending approvals
- [ ] Return SalesDashboardMetricsDto

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GetSalesDashboardMetricsQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GetSalesDashboardMetricsQueryHandler.cs`

### Task 3.2: Create GetTeamPerformanceMetricsQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Aggregate per-user metrics
- [ ] Calculate team averages
- [ ] Generate performance rankings
- [ ] Return list of TeamPerformanceDto

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GetTeamPerformanceMetricsQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GetTeamPerformanceMetricsQueryHandler.cs`

### Task 3.3: Create GetApprovalWorkflowMetricsQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Calculate approval TAT (time from request to approval)
- [ ] Calculate rejection rate
- [ ] Calculate escalation percentage
- [ ] Count pending approvals
- [ ] Return ApprovalMetricsDto

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GetApprovalWorkflowMetricsQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GetApprovalWorkflowMetricsQueryHandler.cs`

### Task 3.4: Create GetDiscountAnalyticsQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Calculate discount % by rep
- [ ] Calculate approval rates
- [ ] Calculate margin impact
- [ ] Return DiscountAnalyticsDto

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GetDiscountAnalyticsQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GetDiscountAnalyticsQueryHandler.cs`

### Task 3.5: Create GetPaymentAnalyticsQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Calculate collection rate
- [ ] Count failed payments
- [ ] Calculate refund statistics
- [ ] Group by payment method
- [ ] Return PaymentAnalyticsDto

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GetPaymentAnalyticsQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GetPaymentAnalyticsQueryHandler.cs`

### Task 3.6: Create GetClientEngagementMetricsQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Calculate view statistics
- [ ] Calculate response rate
- [ ] Calculate conversion rate
- [ ] Calculate average response time
- [ ] Return ClientEngagementDto

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GetClientEngagementMetricsQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GetClientEngagementMetricsQueryHandler.cs`

### Task 3.7: Create Validators for Dashboard Queries
- [ ] Create validators for all 6 dashboard queries
- [ ] Add validation rules
- [ ] Register in DI

**Files**: 
- `src/Backend/CRM.Application/Reports/Validators/GetSalesDashboardMetricsQueryValidator.cs`
- `src/Backend/CRM.Application/Reports/Validators/GetTeamPerformanceMetricsQueryValidator.cs`
- `src/Backend/CRM.Application/Reports/Validators/GetApprovalWorkflowMetricsQueryValidator.cs`
- `src/Backend/CRM.Application/Reports/Validators/GetDiscountAnalyticsQueryValidator.cs`
- `src/Backend/CRM.Application/Reports/Validators/GetPaymentAnalyticsQueryValidator.cs`
- `src/Backend/CRM.Application/Reports/Validators/GetClientEngagementMetricsQueryValidator.cs`

---

## Phase 4: Query Handlers - Advanced Reports

### Task 4.1: Create GenerateCustomReportQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Implement flexible filtering
- [ ] Implement dynamic grouping
- [ ] Implement sorting
- [ ] Implement pagination
- [ ] Return ReportData

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GenerateCustomReportQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GenerateCustomReportQueryHandler.cs`

### Task 4.2: Create GetForecastingDataQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Implement trend analysis
- [ ] Implement revenue prediction
- [ ] Implement success rate projection
- [ ] Return ForecastingDataDto

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GetForecastingDataQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GetForecastingDataQueryHandler.cs`

### Task 4.3: Create GetAuditComplianceReportQuery
- [ ] Create query class
- [ ] Create query handler
- [ ] Query all changes log
- [ ] Query approval history
- [ ] Query payment history
- [ ] Query user activity
- [ ] Return AuditReportDto

**Files**: 
- `src/Backend/CRM.Application/Reports/Queries/GetAuditComplianceReportQuery.cs`
- `src/Backend/CRM.Application/Reports/Queries/Handlers/GetAuditComplianceReportQueryHandler.cs`

---

## Phase 5: Command Handlers

### Task 5.1: Create GenerateReportCommand
- [ ] Create command class
- [ ] Create command handler
- [ ] Generate report data
- [ ] Cache results (optional)
- [ ] Handle large datasets
- [ ] Return ReportData

**Files**: 
- `src/Backend/CRM.Application/Reports/Commands/GenerateReportCommand.cs`
- `src/Backend/CRM.Application/Reports/Commands/Handlers/GenerateReportCommandHandler.cs`

### Task 5.2: Create ExportReportCommand
- [ ] Create command class
- [ ] Create command handler
- [ ] Call export service
- [ ] Save file to storage
- [ ] Create ExportedReport record
- [ ] Return file path

**Files**: 
- `src/Backend/CRM.Application/Reports/Commands/ExportReportCommand.cs`
- `src/Backend/CRM.Application/Reports/Commands/Handlers/ExportReportCommandHandler.cs`

### Task 5.3: Create ScheduleReportCommand
- [ ] Create command class
- [ ] Create command handler
- [ ] Validate recurrence pattern
- [ ] Calculate NextScheduledAt
- [ ] Create ScheduledReport record
- [ ] Return ScheduledReport

**Files**: 
- `src/Backend/CRM.Application/Reports/Commands/ScheduleReportCommand.cs`
- `src/Backend/CRM.Application/Reports/Commands/Handlers/ScheduleReportCommandHandler.cs`

### Task 5.4: Create CancelScheduledReportCommand
- [ ] Create command class
- [ ] Create command handler
- [ ] Set IsActive = false
- [ ] Update record

**Files**: 
- `src/Backend/CRM.Application/Reports/Commands/CancelScheduledReportCommand.cs`
- `src/Backend/CRM.Application/Reports/Commands/Handlers/CancelScheduledReportCommandHandler.cs`

### Task 5.5: Create SaveDashboardBookmarkCommand
- [ ] Create command class
- [ ] Create command handler
- [ ] Validate dashboard config
- [ ] Save or update bookmark
- [ ] Handle IsDefault flag
- [ ] Return DashboardBookmark

**Files**: 
- `src/Backend/CRM.Application/Reports/Commands/SaveDashboardBookmarkCommand.cs`
- `src/Backend/CRM.Application/Reports/Commands/Handlers/SaveDashboardBookmarkCommandHandler.cs`

### Task 5.6: Create DeleteDashboardBookmarkCommand
- [ ] Create command class
- [ ] Create command handler
- [ ] Delete bookmark
- [ ] Verify ownership

**Files**: 
- `src/Backend/CRM.Application/Reports/Commands/DeleteDashboardBookmarkCommand.cs`
- `src/Backend/CRM.Application/Reports/Commands/Handlers/DeleteDashboardBookmarkCommandHandler.cs`

### Task 5.7: Create Validators for Commands
- [ ] Create validators for all commands
- [ ] Add validation rules
- [ ] Register in DI

**Files**: 
- `src/Backend/CRM.Application/Reports/Validators/GenerateReportCommandValidator.cs`
- `src/Backend/CRM.Application/Reports/Validators/ExportReportCommandValidator.cs`
- `src/Backend/CRM.Application/Reports/Validators/ScheduleReportCommandValidator.cs`
- `src/Backend/CRM.Application/Reports/Validators/SaveDashboardBookmarkCommandValidator.cs`

---

## Phase 6: Export Services

### Task 6.1: Create IReportExportService Interface
- [ ] Define interface
- [ ] Add methods: ExportToPdf, ExportToExcel, ExportToCsv
- [ ] Add XML documentation

**File**: `src/Backend/CRM.Application/Reports/Services/IReportExportService.cs`

### Task 6.2: Implement PdfExportService
- [ ] Install QuestPDF or similar library
- [ ] Implement ExportToPdf method
- [ ] Format report data
- [ ] Add charts (if applicable)
- [ ] Generate PDF file
- [ ] Return byte array

**File**: `src/Backend/CRM.Application/Reports/Services/PdfExportService.cs`

### Task 6.3: Implement ExcelExportService
- [ ] Install EPPlus or ClosedXML
- [ ] Implement ExportToExcel method
- [ ] Create workbook
- [ ] Add multiple sheets
- [ ] Format cells
- [ ] Generate .xlsx file
- [ ] Return byte array

**File**: `src/Backend/CRM.Application/Reports/Services/ExcelExportService.cs`

### Task 6.4: Implement CsvExportService
- [ ] Implement ExportToCsv method
- [ ] Format data as CSV
- [ ] Handle special characters
- [ ] Generate .csv file
- [ ] Return byte array

**File**: `src/Backend/CRM.Application/Reports/Services/CsvExportService.cs`

### Task 6.5: Create File Storage Service
- [ ] Create IFileStorageService interface
- [ ] Implement file storage (local or cloud)
- [ ] Add methods: SaveFile, GetFile, DeleteFile
- [ ] Handle file paths

**Files**: 
- `src/Backend/CRM.Application/Reports/Services/IFileStorageService.cs`
- `src/Backend/CRM.Infrastructure/Services/FileStorageService.cs`

---

## Phase 7: Background Jobs

### Task 7.1: Create DailyMetricsCalculationJob
- [ ] Create job class
- [ ] Calculate daily metrics
- [ ] Store in AnalyticsMetricsSnapshot
- [ ] Schedule to run at 2 AM daily
- [ ] Add error handling
- [ ] Add logging

**File**: `src/Backend/CRM.Infrastructure/Jobs/DailyMetricsCalculationJob.cs`

### Task 7.2: Create ScheduledReportExecutionJob
- [ ] Create job class
- [ ] Query reports where IsActive=true and NextScheduledAt <= now
- [ ] Generate report
- [ ] Send email via notification service
- [ ] Update LastSentAt
- [ ] Calculate and update NextScheduledAt
- [ ] Schedule to run every hour
- [ ] Add error handling

**File**: `src/Backend/CRM.Infrastructure/Jobs/ScheduledReportExecutionJob.cs`

### Task 7.3: Create ReportCleanupJob
- [ ] Create job class
- [ ] Find exports >90 days old
- [ ] Delete files from storage
- [ ] Archive or delete records
- [ ] Schedule to run weekly
- [ ] Add error handling

**File**: `src/Backend/CRM.Infrastructure/Jobs/ReportCleanupJob.cs`

### Task 7.4: Register Jobs in DI
- [ ] Register all jobs in Program.cs
- [ ] Configure Quartz.NET
- [ ] Set up job scheduling

**File**: `src/Backend/CRM.Api/Program.cs`

---

## Phase 8: API Controllers

### Task 8.1: Create ReportsController - Dashboard Endpoints
- [ ] Create controller
- [ ] Add GET /api/v1/reports/dashboard/sales endpoint
- [ ] Add GET /api/v1/reports/dashboard/manager endpoint
- [ ] Add GET /api/v1/reports/dashboard/finance endpoint
- [ ] Add GET /api/v1/reports/dashboard/admin endpoint
- [ ] Add authorization attributes
- [ ] Add error handling

**File**: `src/Backend/CRM.Api/Controllers/ReportsController.cs`

### Task 8.2: Create ReportsController - Report Generation
- [ ] Add GET /api/v1/reports/custom endpoint
- [ ] Add GET /api/v1/reports/forecasting endpoint
- [ ] Add GET /api/v1/reports/audit endpoint
- [ ] Add query parameter handling
- [ ] Add validation

**File**: `src/Backend/CRM.Api/Controllers/ReportsController.cs`

### Task 8.3: Create ReportsController - Export Endpoints
- [ ] Add POST /api/v1/reports/export endpoint
- [ ] Add GET /api/v1/reports/export-history endpoint
- [ ] Add file download handling
- [ ] Add authorization

**File**: `src/Backend/CRM.Api/Controllers/ReportsController.cs`

### Task 8.4: Create ScheduledReportsController
- [ ] Create controller
- [ ] Add POST /api/v1/reports/scheduled endpoint
- [ ] Add GET /api/v1/reports/scheduled endpoint
- [ ] Add DELETE /api/v1/reports/scheduled/{reportId} endpoint
- [ ] Add POST /api/v1/reports/send-test endpoint
- [ ] Add authorization
- [ ] Add validation

**File**: `src/Backend/CRM.Api/Controllers/ScheduledReportsController.cs`

### Task 8.5: Create DashboardBookmarksController
- [ ] Create controller
- [ ] Add POST /api/v1/dashboard/bookmarks endpoint
- [ ] Add GET /api/v1/dashboard/bookmarks endpoint
- [ ] Add DELETE /api/v1/dashboard/bookmarks/{bookmarkId} endpoint
- [ ] Add authorization
- [ ] Add validation

**File**: `src/Backend/CRM.Api/Controllers/DashboardBookmarksController.cs`

---

## Phase 9: AutoMapper & Validators

### Task 9.1: Create ReportProfile
- [ ] Create AutoMapper profile
- [ ] Map entities to DTOs
- [ ] Map request DTOs to queries/commands
- [ ] Register in DI

**File**: `src/Backend/CRM.Application/Mapping/ReportProfile.cs`

### Task 9.2: Register Services in DI
- [ ] Register all query handlers
- [ ] Register all command handlers
- [ ] Register export services
- [ ] Register file storage service
- [ ] Register validators
- [ ] Register AutoMapper profile

**File**: `src/Backend/CRM.Api/Program.cs`

---

## Phase 10: Frontend - TypeScript Types & API Client

### Task 10.1: Create TypeScript Types
- [ ] Create `reportTypes.ts` with all report-related types
- [ ] Create `dashboardTypes.ts` with dashboard types
- [ ] Create `chartTypes.ts` with chart data types
- [ ] Export all types

**Files**: 
- `src/Frontend/web/src/types/reports.ts`
- `src/Frontend/web/src/types/dashboards.ts`
- `src/Frontend/web/src/types/charts.ts`

### Task 10.2: Create API Service Layer
- [ ] Create `reportService.ts` with all report API methods
- [ ] Create `chartsService.ts` for chart data transformation
- [ ] Create `exportService.ts` for export handling
- [ ] Create `forecastingService.ts` for forecasting API
- [ ] Update `api.ts` with new endpoints

**Files**: 
- `src/Frontend/web/src/lib/services/reportService.ts`
- `src/Frontend/web/src/lib/services/chartsService.ts`
- `src/Frontend/web/src/lib/services/exportService.ts`
- `src/Frontend/web/src/lib/services/forecastingService.ts`
- `src/Frontend/web/src/lib/api.ts`

---

## Phase 11: Frontend - Dashboard Components

### Task 11.1: Create Sales Rep Dashboard Page
- [ ] Create `/dashboard` page
- [ ] Add KPI cards component
- [ ] Add line chart (quotations trend)
- [ ] Add pie chart (status breakdown)
- [ ] Add bar chart (top clients)
- [ ] Add recent activity list
- [ ] Add action buttons
- [ ] Make responsive

**File**: `src/Frontend/web/src/app/(protected)/dashboard/page.tsx`

### Task 11.2: Create Manager Dashboard Page
- [ ] Create `/dashboard/manager` page
- [ ] Add team KPI cards
- [ ] Add team performance charts
- [ ] Add funnel chart (pipeline stages)
- [ ] Add heatmap (discount compliance)
- [ ] Add team member cards
- [ ] Add approval queue
- [ ] Make responsive

**File**: `src/Frontend/web/src/app/(protected)/dashboard/manager/page.tsx`

### Task 11.3: Create Finance Dashboard Page
- [ ] Create `/dashboard/finance` page
- [ ] Add payment KPI cards
- [ ] Add payment trend charts
- [ ] Add payment method distribution chart
- [ ] Add payment list table
- [ ] Add filters and search
- [ ] Make responsive

**File**: `src/Frontend/web/src/app/(protected)/dashboard/finance/page.tsx`

### Task 11.4: Create Admin Dashboard Page
- [ ] Create `/admin/dashboard` page
- [ ] Add system KPI cards
- [ ] Add growth charts
- [ ] Add usage charts
- [ ] Add system metrics
- [ ] Make responsive

**File**: `src/Frontend/web/src/app/(protected)/admin/dashboard/page.tsx`

---

## Phase 12: Frontend - Report Builder & Reports

### Task 12.1: Create Custom Report Builder Page
- [ ] Create `/reports/custom` page
- [ ] Add metric selector component
- [ ] Add filter panel
- [ ] Add date range picker
- [ ] Add group by selector
- [ ] Add sort options
- [ ] Add preview table
- [ ] Add chart preview
- [ ] Add export buttons
- [ ] Implement "Run Report" functionality

**File**: `src/Frontend/web/src/app/(protected)/reports/custom/page.tsx`

### Task 12.2: Create Pre-built Report Pages
- [ ] Create Sales Pipeline Report page
- [ ] Create Team Performance Report page
- [ ] Create Discount Analysis Report page
- [ ] Create Approval Metrics Report page
- [ ] Create Payment Status Report page
- [ ] Create Client Engagement Report page
- [ ] Create Forecasting Report page
- [ ] Create Audit Trail Report page
- [ ] Each with metrics, filters, export, drill-down

**Files**: 
- `src/Frontend/web/src/app/(protected)/reports/pipeline/page.tsx`
- `src/Frontend/web/src/app/(protected)/reports/team-performance/page.tsx`
- `src/Frontend/web/src/app/(protected)/reports/discount-analysis/page.tsx`
- `src/Frontend/web/src/app/(protected)/reports/approval-metrics/page.tsx`
- `src/Frontend/web/src/app/(protected)/reports/payment-status/page.tsx`
- `src/Frontend/web/src/app/(protected)/reports/client-engagement/page.tsx`
- `src/Frontend/web/src/app/(protected)/reports/forecasting/page.tsx`
- `src/Frontend/web/src/app/(protected)/reports/audit/page.tsx`

### Task 12.3: Create Export History Page
- [ ] Create `/reports/exports` page
- [ ] Add export list table
- [ ] Add download links
- [ ] Add delete functionality
- [ ] Add filters
- [ ] Make responsive

**File**: `src/Frontend/web/src/app/(protected)/reports/exports/page.tsx`

### Task 12.4: Create Scheduled Reports Page
- [ ] Create `/reports/scheduled` page
- [ ] Add scheduled report list
- [ ] Add create/edit modal
- [ ] Add pause/resume actions
- [ ] Add test email button
- [ ] Add delivery history view
- [ ] Make responsive

**File**: `src/Frontend/web/src/app/(protected)/reports/scheduled/page.tsx`

---

## Phase 13: Frontend - Custom Hooks & Utilities

### Task 13.1: Create useReport Hook
- [ ] Create hook for fetching report data
- [ ] Implement caching with React Query
- [ ] Handle loading and error states
- [ ] Add refetch functionality

**File**: `src/Frontend/web/src/hooks/useReport.ts`

### Task 13.2: Create useCharts Hook
- [ ] Create hook for chart data transformation
- [ ] Transform data for different chart types
- [ ] Handle chart configuration

**File**: `src/Frontend/web/src/hooks/useCharts.ts`

### Task 13.3: Create useDashboardBookmarks Hook
- [ ] Create hook for dashboard bookmarks
- [ ] Implement save/load functionality
- [ ] Handle default bookmark

**File**: `src/Frontend/web/src/hooks/useDashboardBookmarks.ts`

### Task 13.4: Create useReportExport Hook
- [ ] Create hook for report export
- [ ] Handle export triggering
- [ ] Handle download
- [ ] Show progress

**File**: `src/Frontend/web/src/hooks/useReportExport.ts`

### Task 13.5: Create useScheduledReports Hook
- [ ] Create hook for scheduled reports CRUD
- [ ] Implement list, create, update, delete
- [ ] Handle test email

**File**: `src/Frontend/web/src/hooks/useScheduledReports.ts`

### Task 13.6: Create useForecast Hook
- [ ] Create hook for forecasting data
- [ ] Fetch forecast data
- [ ] Handle trend line

**File**: `src/Frontend/web/src/hooks/useForecast.ts`

---

## Phase 14: Frontend - Visualization Components

### Task 14.1: Install and Configure ApexCharts
- [ ] Install react-apexcharts
- [ ] Install apexcharts
- [ ] Create chart configuration utilities

**Files**: Package.json updates

### Task 14.2: Create Chart Components
- [ ] Create `KPICard.tsx` component
- [ ] Create `LineChart.tsx` component
- [ ] Create `BarChart.tsx` component
- [ ] Create `PieChart.tsx` component
- [ ] Create `FunnelChart.tsx` component
- [ ] Create `HeatmapChart.tsx` component
- [ ] Add tooltips and interactivity

**Files**: 
- `src/Frontend/web/src/components/reports/KPICard.tsx`
- `src/Frontend/web/src/components/reports/LineChart.tsx`
- `src/Frontend/web/src/components/reports/BarChart.tsx`
- `src/Frontend/web/src/components/reports/PieChart.tsx`
- `src/Frontend/web/src/components/reports/FunnelChart.tsx`
- `src/Frontend/web/src/components/reports/HeatmapChart.tsx`

### Task 14.3: Create UI Components
- [ ] Create `DataTable.tsx` (sortable, filterable, paginated)
- [ ] Create `MetricSelector.tsx` (multi-checkbox)
- [ ] Create `DateRangePicker.tsx`
- [ ] Create `FilterPanel.tsx`
- [ ] Create `ReportPreview.tsx`
- [ ] Create `ExportButton.tsx`
- [ ] Create `ChartTooltip.tsx`
- [ ] Create `DrillDown.tsx`
- [ ] Create `TrafficLightIndicator.tsx`

**Files**: 
- `src/Frontend/web/src/components/reports/DataTable.tsx`
- `src/Frontend/web/src/components/reports/MetricSelector.tsx`
- `src/Frontend/web/src/components/reports/DateRangePicker.tsx`
- `src/Frontend/web/src/components/reports/FilterPanel.tsx`
- `src/Frontend/web/src/components/reports/ReportPreview.tsx`
- `src/Frontend/web/src/components/reports/ExportButton.tsx`
- `src/Frontend/web/src/components/reports/ChartTooltip.tsx`
- `src/Frontend/web/src/components/reports/DrillDown.tsx`
- `src/Frontend/web/src/components/reports/TrafficLightIndicator.tsx`

### Task 14.4: Create Component Index
- [ ] Create `index.ts` to export all components
- [ ] Organize exports

**File**: `src/Frontend/web/src/components/reports/index.ts`

---

## Phase 15: Integration & Testing

### Task 15.1: Backend Unit Tests
- [ ] Write tests for query handlers
- [ ] Write tests for command handlers
- [ ] Write tests for export services
- [ ] Achieve ≥85% coverage

**Files**: Test files in `src/Backend/CRM.Application.Tests/`

### Task 15.2: Backend Integration Tests
- [ ] Write tests for API endpoints
- [ ] Write tests for background jobs
- [ ] Write performance tests

**Files**: Test files in `src/Backend/CRM.Api.Tests/`

### Task 15.3: Frontend Component Tests
- [ ] Write tests for dashboard components
- [ ] Write tests for report components
- [ ] Write tests for chart components
- [ ] Achieve ≥80% coverage

**Files**: Test files in `src/Frontend/web/src/__tests__/`

### Task 15.4: Frontend Integration Tests
- [ ] Write tests for API integration
- [ ] Write tests for hooks
- [ ] Write E2E tests for report flows

**Files**: Test files in `src/Frontend/web/src/__tests__/`

### Task 15.5: Performance Optimization
- [ ] Optimize database queries
- [ ] Implement caching
- [ ] Optimize frontend rendering
- [ ] Lazy load charts
- [ ] Optimize export generation

---

## Phase 16: Documentation & Deployment

### Task 16.1: Update API Documentation
- [ ] Document all endpoints
- [ ] Add request/response examples
- [ ] Generate OpenAPI contract

**File**: `docs/api/reports.md`

### Task 16.2: Create User Guides
- [ ] Create dashboard user guide
- [ ] Create report builder guide
- [ ] Create scheduled reports guide

**Files**: `docs/user-guides/`

### Task 16.3: Update Quickstart Guide
- [ ] Add reporting section
- [ ] Add dashboard setup instructions

**File**: `docs/quickstart.md`

### Task 16.4: Deployment Checklist
- [ ] Create deployment checklist
- [ ] Document environment variables
- [ ] Document file storage configuration

**File**: `docs/deployment/reports.md`

---

## Priority Markers

- **P0 (Critical)**: Tasks 1.1-1.13, 3.1-3.6, 8.1-8.5, 11.1-11.4
- **P1 (High)**: Tasks 2.1-2.7, 4.1-4.3, 5.1-5.6, 6.1-6.5, 12.1-12.4
- **P2 (Medium)**: Tasks 7.1-7.4, 9.1-9.2, 10.1-10.2, 13.1-13.6, 14.1-14.4
- **P3 (Low)**: Tasks 15.1-15.5, 16.1-16.4

---

## Checkpoints

1. **Checkpoint 1** (End of Phase 1): Database schema complete, entities created
2. **Checkpoint 2** (End of Phase 5): All command handlers complete
3. **Checkpoint 3** (End of Phase 8): All API endpoints complete
4. **Checkpoint 4** (End of Phase 12): All frontend pages complete
5. **Checkpoint 5** (End of Phase 15): All tests passing, integration complete

---

**Total Tasks**: 141+ tasks  
**Estimated Duration**: 10 weeks  
**Team**: 2-3 developers

