# Spec-015: Implementation Plan - Reporting, Analytics & Business Intelligence

## Overview

This plan outlines the phased implementation of comprehensive reporting and analytics capabilities for the CRM system. The implementation will be done in parallel for backend and frontend to ensure seamless integration.

## Implementation Phases

### Phase 1: Database & Entities (Week 1)
**Goal**: Set up database schema and domain entities

**Tasks**:
1. Create database migrations for:
   - `AnalyticsMetricsSnapshot` table
   - `DashboardBookmarks` table
   - `ScheduledReports` table
   - `ExportedReports` table
2. Create C# entity classes:
   - `AnalyticsMetricsSnapshot.cs`
   - `DashboardBookmark.cs`
   - `ScheduledReport.cs`
   - `ExportedReport.cs`
3. Create enums:
   - `MetricType` (constants)
   - `RecurrencePattern` enum
   - `ExportFormat` enum
4. Configure Entity Framework mappings
5. Update `AppDbContext` and `IAppDbContext`
6. Run migrations and verify schema

**Deliverables**:
- 4 database tables created
- 4 entity classes
- 3 enum definitions
- EF Core configurations

---

### Phase 2: DTOs & Request Models (Week 1)
**Goal**: Define all data transfer objects and request models

**Tasks**:
1. Create DTOs:
   - `ReportGenerationRequest.cs`
   - `ReportData.cs`
   - `DashboardConfig.cs`
   - `ScheduleReportRequest.cs`
   - `DashboardMetricsDto.cs`
   - `TeamPerformanceDto.cs`
   - `ApprovalMetricsDto.cs`
   - `DiscountAnalyticsDto.cs`
   - `PaymentAnalyticsDto.cs`
   - `ClientEngagementDto.cs`
   - `ForecastingDataDto.cs`
   - `AuditReportDto.cs`
2. Create response DTOs for each dashboard type
3. Create export request DTOs

**Deliverables**:
- 15+ DTO classes
- Request/Response models for all endpoints

---

### Phase 3: Query Handlers - Dashboard Metrics (Week 2)
**Goal**: Implement query handlers for dashboard metrics

**Tasks**:
1. `GetSalesDashboardMetricsQueryHandler`:
   - Calculate pipeline value
   - Calculate conversion rate
   - Count quotations created/sent
   - Count pending approvals
2. `GetTeamPerformanceMetricsQueryHandler`:
   - Aggregate per-user metrics
   - Calculate team averages
   - Performance rankings
3. `GetApprovalWorkflowMetricsQueryHandler`:
   - Calculate approval TAT
   - Calculate rejection rate
   - Calculate escalation %
   - Count pending approvals
4. `GetDiscountAnalyticsQueryHandler`:
   - Discount % by rep
   - Approval rates
   - Margin impact calculation
5. `GetPaymentAnalyticsQueryHandler`:
   - Collection rate
   - Failed payment count
   - Refund statistics
   - Payment method distribution
6. `GetClientEngagementMetricsQueryHandler`:
   - View statistics
   - Response rate
   - Conversion rate
   - Response time average

**Deliverables**:
- 6 query handlers
- Accurate metric calculations
- Performance optimized queries

---

### Phase 4: Query Handlers - Advanced Reports (Week 2-3)
**Goal**: Implement advanced reporting queries

**Tasks**:
1. `GenerateCustomReportQueryHandler`:
   - Flexible filtering
   - Dynamic grouping
   - Sorting capabilities
   - Pagination
2. `GetForecastingDataQueryHandler`:
   - Trend analysis
   - Revenue prediction
   - Success rate projection
3. `GetAuditComplianceReportQueryHandler`:
   - All changes log
   - Approval history
   - Payment history
   - User activity

**Deliverables**:
- 3 advanced query handlers
- Custom report generation logic
- Forecasting algorithms

---

### Phase 5: Command Handlers (Week 3)
**Goal**: Implement command handlers for report operations

**Tasks**:
1. `GenerateReportCommandHandler`:
   - Generate report data
   - Cache results
   - Handle large datasets
2. `ExportReportCommandHandler`:
   - PDF generation (using QuestPDF or similar)
   - Excel generation (using EPPlus or ClosedXML)
   - CSV generation
   - File storage
3. `ScheduleReportCommandHandler`:
   - Create scheduled report
   - Update scheduled report
   - Validate recurrence pattern
4. `CancelScheduledReportCommandHandler`:
   - Deactivate scheduled report
5. `SaveDashboardBookmarkCommandHandler`:
   - Save dashboard configuration
   - Validate config
6. `DeleteDashboardBookmarkCommandHandler`:
   - Delete bookmark

**Deliverables**:
- 6 command handlers
- Export service integration
- Scheduled report management

---

### Phase 6: Export Services (Week 3-4)
**Goal**: Implement export functionality

**Tasks**:
1. Create `IReportExportService` interface
2. Implement `PdfExportService`:
   - Format report data
   - Add charts (if applicable)
   - Generate PDF file
3. Implement `ExcelExportService`:
   - Create workbook
   - Multiple sheets
   - Format cells
   - Generate .xlsx file
4. Implement `CsvExportService`:
   - Format data as CSV
   - Generate .csv file
5. File storage service integration
6. Cleanup service for old exports

**Deliverables**:
- Export service interface
- 3 export implementations
- File storage integration

---

### Phase 7: Background Jobs (Week 4)
**Goal**: Implement background jobs for metrics and scheduled reports

**Tasks**:
1. `DailyMetricsCalculationJob`:
   - Calculate daily metrics
   - Store in `AnalyticsMetricsSnapshot`
   - Run at 2 AM daily
2. `ScheduledReportExecutionJob`:
   - Check `NextScheduledAt`
   - Generate report
   - Send email
   - Update `LastSentAt` and `NextScheduledAt`
   - Run every hour
3. `ReportCleanupJob`:
   - Find exports >90 days old
   - Delete files
   - Archive records
   - Run weekly

**Deliverables**:
- 3 background jobs
- Quartz.NET integration
- Email service integration

---

### Phase 8: API Controllers (Week 4-5)
**Goal**: Create REST API endpoints

**Tasks**:
1. `ReportsController`:
   - Dashboard endpoints (sales, manager, finance, admin)
   - Custom report generation
   - Export endpoints
   - Forecasting endpoint
   - Audit report endpoint
2. `ScheduledReportsController`:
   - Create/update scheduled report
   - List scheduled reports
   - Delete scheduled report
   - Send test email
3. `DashboardBookmarksController`:
   - Save bookmark
   - List bookmarks
   - Delete bookmark
4. Add authorization attributes
5. Add validation
6. Add error handling

**Deliverables**:
- 3 API controllers
- 15+ endpoints
- Authorization and validation

---

### Phase 9: AutoMapper & Validators (Week 5)
**Goal**: Configure mapping and validation

**Tasks**:
1. Create AutoMapper profiles:
   - `ReportProfile.cs`
   - `DashboardProfile.cs`
2. Create FluentValidation validators:
   - `ReportGenerationRequestValidator.cs`
   - `ScheduleReportRequestValidator.cs`
   - `DashboardConfigValidator.cs`
   - Query validators
3. Register in DI container

**Deliverables**:
- AutoMapper profiles
- Validation rules
- DI registration

---

### Phase 10: Frontend - TypeScript Types & API Client (Week 5)
**Goal**: Set up frontend types and API integration

**Tasks**:
1. Create TypeScript types:
   - `reportTypes.ts`
   - `dashboardTypes.ts`
   - `chartTypes.ts`
2. Create API service:
   - `reportService.ts`
   - `chartsService.ts`
   - `exportService.ts`
   - `forecastingService.ts`
3. Update `api.ts` with report endpoints

**Deliverables**:
- TypeScript type definitions
- API service layer
- Type-safe API calls

---

### Phase 11: Frontend - Dashboard Components (Week 6-7)
**Goal**: Build dashboard pages and components

**Tasks**:
1. **Sales Rep Dashboard** (`/dashboard`):
   - KPI cards component
   - Line chart (quotations trend)
   - Pie chart (status breakdown)
   - Bar chart (top clients)
   - Recent activity list
2. **Manager Dashboard** (`/dashboard/manager`):
   - Team KPI cards
   - Team performance charts
   - Funnel chart (pipeline stages)
   - Heatmap (discount compliance)
   - Team member cards
   - Approval queue
3. **Finance Dashboard** (`/dashboard/finance`):
   - Payment KPI cards
   - Payment trend charts
   - Payment method distribution
   - Payment list table
4. **Admin Dashboard** (`/admin/dashboard`):
   - System KPI cards
   - Growth charts
   - Usage charts
   - System metrics

**Deliverables**:
- 4 dashboard pages
- 10+ chart components
- KPI card components
- Responsive layouts

---

### Phase 12: Frontend - Report Builder & Reports (Week 7-8)
**Goal**: Build report generation and viewing pages

**Tasks**:
1. **Custom Report Builder** (`/reports/custom`):
   - Metric selector
   - Filter panel
   - Date range picker
   - Group by selector
   - Sort options
   - Preview table
   - Chart preview
   - Export buttons
2. **Pre-built Reports**:
   - Sales Pipeline Report page
   - Team Performance Report page
   - Discount Analysis Report page
   - Approval Metrics Report page
   - Payment Status Report page
   - Client Engagement Report page
   - Forecasting Report page
   - Audit Trail Report page
3. **Export History** (`/reports/exports`):
   - Export list table
   - Download links
   - Delete functionality
4. **Scheduled Reports** (`/reports/scheduled`):
   - Scheduled report list
   - Create/edit modal
   - Pause/resume actions
   - Test email button

**Deliverables**:
- Report builder page
- 8 pre-built report pages
- Export history page
- Scheduled reports management

---

### Phase 13: Frontend - Custom Hooks & Utilities (Week 8)
**Goal**: Create reusable hooks and utilities

**Tasks**:
1. Create custom hooks:
   - `useReport.ts`
   - `useCharts.ts`
   - `useDashboardBookmarks.ts`
   - `useReportExport.ts`
   - `useScheduledReports.ts`
   - `useForecast.ts`
2. Create chart utilities:
   - Data transformation functions
   - Chart configuration helpers
3. Create export utilities:
   - Download handling
   - File format helpers

**Deliverables**:
- 6 custom hooks
- Utility functions
- Reusable logic

---

### Phase 14: Frontend - Visualization Components (Week 8-9)
**Goal**: Build chart and visualization components

**Tasks**:
1. Install and configure ApexCharts
2. Create chart components:
   - `KPICard.tsx`
   - `LineChart.tsx`
   - `BarChart.tsx`
   - `PieChart.tsx`
   - `FunnelChart.tsx`
   - `HeatmapChart.tsx`
3. Create UI components:
   - `DataTable.tsx`
   - `MetricSelector.tsx`
   - `DateRangePicker.tsx`
   - `FilterPanel.tsx`
   - `ReportPreview.tsx`
   - `ExportButton.tsx`
   - `ChartTooltip.tsx`
   - `DrillDown.tsx`
   - `TrafficLightIndicator.tsx`

**Deliverables**:
- 6 chart components
- 10 UI components
- ApexCharts integration

---

### Phase 15: Integration & Testing (Week 9-10)
**Goal**: Integrate backend and frontend, write tests

**Tasks**:
1. **Backend Testing**:
   - Unit tests for query handlers
   - Unit tests for command handlers
   - Unit tests for export services
   - Integration tests for API endpoints
   - Performance tests
2. **Frontend Testing**:
   - Component tests
   - Hook tests
   - Integration tests
   - E2E tests for report flows
3. **Integration**:
   - Connect frontend to backend APIs
   - Test all dashboard flows
   - Test report generation
   - Test export functionality
   - Test scheduled reports
4. **Performance Optimization**:
   - Cache optimization
   - Query optimization
   - Frontend lazy loading
   - Chart rendering optimization

**Deliverables**:
- 20+ backend tests
- 25+ frontend tests
- Full integration verified
- Performance optimized

---

### Phase 16: Documentation & Deployment (Week 10)
**Goal**: Document and prepare for deployment

**Tasks**:
1. Update API documentation
2. Create user guide for dashboards
3. Create admin guide for scheduled reports
4. Update quickstart guide
5. Generate OpenAPI contract
6. Deployment checklist
7. Performance benchmarks

**Deliverables**:
- API documentation
- User guides
- Deployment documentation

---

## Critical Path Items

1. **Database schema** (Phase 1) - Blocks all other work
2. **Query handlers** (Phases 3-4) - Core functionality
3. **Export services** (Phase 6) - Required for exports
4. **Dashboard components** (Phase 11) - Main UI
5. **Report builder** (Phase 12) - Key feature

## Parallel Work Streams

- **Backend**: Phases 1-9 can be done in parallel with frontend setup
- **Frontend**: Phases 10-14 can start after Phase 1 (database) is complete
- **Testing**: Phase 15 runs in parallel with integration

## Risk Mitigation

1. **Performance**: Use caching, background jobs, database indexes
2. **Large datasets**: Implement pagination, async processing
3. **Export file size**: Compress PDFs, optimize Excel generation
4. **Chart rendering**: Lazy load, memoize components
5. **Scheduled reports**: Queue system, error handling

## Success Criteria

- All dashboards load in <2 seconds
- Reports generate in <5 seconds
- Exports complete successfully
- Scheduled reports execute on time
- All tests pass
- Mobile responsive
- Accessibility compliant

---

**Total Estimated Duration**: 10 weeks  
**Team Size**: 2-3 developers (1 backend, 1-2 frontend)

