# Data Model: Spec-008 Client History & Activity Log

## 1. ClientHistory (Table)
Immutable audit entry linked to a Client and the acting User.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| `HistoryId` | UUID | Primary key | PK |
| `ClientId` | UUID | FK to `Clients.ClientId` | NOT NULL, indexed |
| `ActorUserId` | UUID | FK to `Users.UserId` (nullable for system actions) | Indexed |
| `ActionType` | TEXT | `CREATED|UPDATED|DELETED|RESTORED|ACCESSED` | NOT NULL, check constraint |
| `ChangedFields` | TEXT[] | Names of fields touched | Defaults to empty array |
| `BeforeSnapshot` | JSONB | Field/value map prior to change | Nullable |
| `AfterSnapshot` | JSONB | Field/value map after change | Nullable |
| `Reason` | TEXT | User-supplied reason for delete/restore | Nullable |
| `Metadata` | JSONB | IP, user agent, automation flag, requestId | NOT NULL default `{}` |
| `SuspicionScore` | SMALLINT | 0–10 inline heuristic score | Default 0 |
| `CreatedAt` | TIMESTAMPTZ | When action logged | NOT NULL, indexed |

**Indexes**
- `(ClientId, CreatedAt DESC)` for timelines
- `(ActorUserId, CreatedAt DESC)` for user activity
- GIN index on `Metadata jsonb_path_ops` for IP searches

**Lifecycle**
- Append-only; no updates/deletes allowed.

## 2. ClientTimelineSummary (Read Model / View)
Materialized view or query projection for timeline endpoint.

| Field | Type | Description |
|-------|------|-------------|
| `ClientId` | UUID | Source client |
| `CompanyName` | TEXT | Snapshot from Clients |
| `IsDeleted` | BOOLEAN | Derived from Clients.DeletedAt |
| `CreatedAt` | TIMESTAMPTZ | Client creation time |
| `LastModifiedAt` | TIMESTAMPTZ | Most recent ClientHistory.CreatedAt |
| `LastModifiedBy` | UUID | Actor from latest entry |
| `TotalChangeCount` | BIGINT | Count of history rows |
| `DeletionInfo` | JSONB | Contains DeletedAt, DeletedBy, Reason |
| `RestorationWindowExpiresAt` | TIMESTAMPTZ | DeletedAt + 30 days, null when active |

**Notes**
- Backed by SQL view joining `Clients` + aggregated `ClientHistories`.
- Refreshed on demand per request; optional materialized cache refreshed hourly.

## 3. SuspiciousActivityFlag (Table)
Derived entity storing flagged events and review state.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| `FlagId` | UUID | Primary key | PK |
| `HistoryId` | UUID | FK to `ClientHistories.HistoryId` | NOT NULL |
| `ClientId` | UUID | Convenience FK | NOT NULL |
| `Score` | SMALLINT | 0–10 aggregated score | >= threshold |
| `Reasons` | TEXT[] | Triggered heuristics (e.g., `RAPID_CHANGES`) | NOT NULL |
| `DetectedAt` | TIMESTAMPTZ | When flag created | NOT NULL |
| `ReviewedBy` | UUID | Admin user who reviewed | Nullable |
| `ReviewedAt` | TIMESTAMPTZ | Review timestamp | Nullable |
| `Status` | TEXT | `OPEN|ACKNOWLEDGED|DISMISSED` | Default `OPEN` |

**Indexes**
- `(Status, DetectedAt DESC)` for dashboard
- `(ClientId, DetectedAt DESC)` for filtering

## 4. Supporting Enumerations
- `ActionType`: `CREATED`, `UPDATED`, `DELETED`, `RESTORED`, `ACCESSED`.
- `SuspiciousReason`: `RAPID_CHANGES`, `ODD_HOURS`, `UNKNOWN_IP`, `BULK_DELETES`, extensible.
- `SuspiciousStatus`: `OPEN`, `ACKNOWLEDGED`, `DISMISSED`.

## Validation & Business Rules
- Before/after snapshots required for `UPDATED`/`DELETED`; optional for other actions.
- Restore allowed only if `Clients.DeletedAt` not null and age ≤30 days.
- SuspicionScore ≥7 auto-creates `SuspiciousActivityFlag`.
- Export queries hard-limit to 5,000 rows and require Admin/Manager role.

