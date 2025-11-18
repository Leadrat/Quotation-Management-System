# Spec-015: Data Model - Reporting, Analytics & Business Intelligence

## Database Schema

### AnalyticsMetricsSnapshot

Caches pre-calculated metrics for performance optimization.

```sql
CREATE TABLE "AnalyticsMetricsSnapshot" (
    "SnapshotId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "MetricType" VARCHAR(100) NOT NULL,
    "UserId" UUID NULL,
    "MetricData" JSONB NOT NULL,
    "CalculatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "PeriodDate" DATE NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_AnalyticsMetricsSnapshot_User" 
        FOREIGN KEY ("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE
);

CREATE INDEX "IX_AnalyticsMetricsSnapshot_MetricType_PeriodDate" 
    ON "AnalyticsMetricsSnapshot"("MetricType", "PeriodDate");
CREATE INDEX "IX_AnalyticsMetricsSnapshot_UserId" 
    ON "AnalyticsMetricsSnapshot"("UserId");
CREATE INDEX "IX_AnalyticsMetricsSnapshot_CalculatedAt" 
    ON "AnalyticsMetricsSnapshot"("CalculatedAt");
```

**MetricType Values:**
- `DailySales` - Daily sales metrics
- `TeamPerformance` - Team performance metrics
- `PaymentStatus` - Payment status summary
- `ApprovalMetrics` - Approval workflow metrics
- `DiscountAnalytics` - Discount analysis
- `ClientEngagement` - Client engagement metrics
- `RevenueForecast` - Revenue forecasting data

**MetricData JSONB Structure:**
```json
{
  "quotationsCreated": 10,
  "quotationsSent": 8,
  "quotationsAccepted": 3,
  "conversionRate": 37.5,
  "totalPipelineValue": 150000.00,
  "averageDiscount": 5.2,
  "pendingApprovals": 2
}
```

### DashboardBookmarks

Saves user dashboard configurations for quick access.

```sql
CREATE TABLE "DashboardBookmarks" (
    "BookmarkId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "DashboardName" VARCHAR(200) NOT NULL,
    "DashboardConfig" JSONB NOT NULL,
    "IsDefault" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_DashboardBookmarks_User" 
        FOREIGN KEY ("UserId") REFERENCES "Users"("UserId") ON DELETE CASCADE
);

CREATE INDEX "IX_DashboardBookmarks_UserId" 
    ON "DashboardBookmarks"("UserId");
CREATE UNIQUE INDEX "IX_DashboardBookmarks_UserId_IsDefault" 
    ON "DashboardBookmarks"("UserId", "IsDefault") 
    WHERE "IsDefault" = TRUE;
```

**DashboardConfig JSONB Structure:**
```json
{
  "layout": "grid",
  "widgets": [
    {
      "id": "kpi-quotations",
      "type": "kpi",
      "metric": "quotationsCreated",
      "position": { "row": 0, "col": 0, "width": 3, "height": 1 }
    },
    {
      "id": "chart-pipeline",
      "type": "lineChart",
      "metric": "pipelineTrend",
      "position": { "row": 1, "col": 0, "width": 6, "height": 3 }
    }
  ],
  "filters": {
    "dateRange": { "from": "2024-01-01", "to": "2024-01-31" },
    "teamId": null,
    "status": ["SENT", "VIEWED", "ACCEPTED"]
  }
}
```

### ScheduledReports

Manages scheduled report delivery via email.

```sql
CREATE TABLE "ScheduledReports" (
    "ReportId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CreatedByUserId" UUID NOT NULL,
    "ReportName" VARCHAR(200) NOT NULL,
    "ReportType" VARCHAR(100) NOT NULL,
    "ReportConfig" JSONB NOT NULL,
    "RecurrencePattern" VARCHAR(20) NOT NULL,
    "EmailRecipients" TEXT NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "LastSentAt" TIMESTAMPTZ NULL,
    "NextScheduledAt" TIMESTAMPTZ NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_ScheduledReports_User" 
        FOREIGN KEY ("CreatedByUserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
    CONSTRAINT "CK_ScheduledReports_RecurrencePattern" 
        CHECK ("RecurrencePattern" IN ('daily', 'weekly', 'monthly'))
);

CREATE INDEX "IX_ScheduledReports_CreatedByUserId" 
    ON "ScheduledReports"("CreatedByUserId");
CREATE INDEX "IX_ScheduledReports_IsActive_NextScheduledAt" 
    ON "ScheduledReports"("IsActive", "NextScheduledAt");
```

**RecurrencePattern Values:**
- `daily` - Every day at specified time
- `weekly` - Every week on specified day
- `monthly` - Every month on specified date

**ReportConfig JSONB Structure:**
```json
{
  "filters": {
    "dateRange": { "from": "2024-01-01", "to": "2024-01-31" },
    "teamId": "uuid-here",
    "status": ["SENT", "ACCEPTED"]
  },
  "groupBy": "date",
  "sortBy": "date",
  "format": "pdf",
  "includeCharts": true
}
```

### ExportedReports

Tracks exported report files for download history.

```sql
CREATE TABLE "ExportedReports" (
    "ExportId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CreatedByUserId" UUID NOT NULL,
    "ReportType" VARCHAR(100) NOT NULL,
    "ExportFormat" VARCHAR(10) NOT NULL,
    "FilePath" VARCHAR(500) NOT NULL,
    "FileSize" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_ExportedReports_User" 
        FOREIGN KEY ("CreatedByUserId") REFERENCES "Users"("UserId") ON DELETE CASCADE,
    CONSTRAINT "CK_ExportedReports_ExportFormat" 
        CHECK ("ExportFormat" IN ('pdf', 'excel', 'csv'))
);

CREATE INDEX "IX_ExportedReports_CreatedByUserId" 
    ON "ExportedReports"("CreatedByUserId");
CREATE INDEX "IX_ExportedReports_CreatedAt" 
    ON "ExportedReports"("CreatedAt");
```

**ExportFormat Values:**
- `pdf` - PDF document
- `excel` - Excel workbook (.xlsx)
- `csv` - CSV file

---

## C# Entity Classes

### AnalyticsMetricsSnapshot.cs

```csharp
using System;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    public class AnalyticsMetricsSnapshot
    {
        public Guid SnapshotId { get; set; }
        public string MetricType { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public JsonDocument MetricData { get; set; } = null!;
        public DateTimeOffset CalculatedAt { get; set; }
        public DateTime PeriodDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}
```

### DashboardBookmark.cs

```csharp
using System;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    public class DashboardBookmark
    {
        public Guid BookmarkId { get; set; }
        public Guid UserId { get; set; }
        public string DashboardName { get; set; } = string.Empty;
        public JsonDocument DashboardConfig { get; set; } = null!;
        public bool IsDefault { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}
```

### ScheduledReport.cs

```csharp
using System;
using System.Text.Json;

namespace CRM.Domain.Entities
{
    public class ScheduledReport
    {
        public Guid ReportId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public JsonDocument ReportConfig { get; set; } = null!;
        public string RecurrencePattern { get; set; } = string.Empty; // "daily", "weekly", "monthly"
        public string EmailRecipients { get; set; } = string.Empty; // Comma-separated emails
        public bool IsActive { get; set; }
        public DateTimeOffset? LastSentAt { get; set; }
        public DateTimeOffset NextScheduledAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation
        public User CreatedByUser { get; set; } = null!;
    }
}
```

### ExportedReport.cs

```csharp
using System;

namespace CRM.Domain.Entities
{
    public class ExportedReport
    {
        public Guid ExportId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string ExportFormat { get; set; } = string.Empty; // "pdf", "excel", "csv"
        public string FilePath { get; set; } = string.Empty;
        public int FileSize { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        // Navigation
        public User CreatedByUser { get; set; } = null!;
    }
}
```

---

## Enums

### MetricType (string constants)

```csharp
namespace CRM.Domain.Enums
{
    public static class MetricType
    {
        public const string DailySales = "DailySales";
        public const string TeamPerformance = "TeamPerformance";
        public const string PaymentStatus = "PaymentStatus";
        public const string ApprovalMetrics = "ApprovalMetrics";
        public const string DiscountAnalytics = "DiscountAnalytics";
        public const string ClientEngagement = "ClientEngagement";
        public const string RevenueForecast = "RevenueForecast";
    }
}
```

### RecurrencePattern

```csharp
namespace CRM.Domain.Enums
{
    public enum RecurrencePattern
    {
        Daily,
        Weekly,
        Monthly
    }
}
```

### ExportFormat

```csharp
namespace CRM.Domain.Enums
{
    public enum ExportFormat
    {
        Pdf,
        Excel,
        Csv
    }
}
```

---

## Relationships

```
Users (1) ──→ (N) AnalyticsMetricsSnapshot
Users (1) ──→ (N) DashboardBookmarks
Users (1) ──→ (N) ScheduledReports
Users (1) ──→ (N) ExportedReports
```

---

## Indexes Summary

1. **AnalyticsMetricsSnapshot**:
   - `MetricType + PeriodDate` (for time-series queries)
   - `UserId` (for user-specific metrics)
   - `CalculatedAt` (for cleanup queries)

2. **DashboardBookmarks**:
   - `UserId` (for user's bookmarks)
   - `UserId + IsDefault` (unique, for default bookmark)

3. **ScheduledReports**:
   - `CreatedByUserId` (for user's scheduled reports)
   - `IsActive + NextScheduledAt` (for job execution)

4. **ExportedReports**:
   - `CreatedByUserId` (for user's exports)
   - `CreatedAt` (for cleanup queries)

---

## Data Retention

- **AnalyticsMetricsSnapshot**: Keep for 2 years, archive older
- **ExportedReports**: Keep for 90 days, then delete files
- **ScheduledReports**: Keep indefinitely (unless deleted by user)
- **DashboardBookmarks**: Keep indefinitely (unless deleted by user)

