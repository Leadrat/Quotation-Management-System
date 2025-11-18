# Spec-018: System Administration & Configuration Console

## Overview

This specification defines the **System Administration & Configuration Console**, which centralizes all organization-wide settings and critical system management tools into an admin-only dashboard. It covers configurable parameters for user management, roles, permissions, system preferences, security controls, integration keys, data retention policies, custom branding, audit log viewing, and monitoring. The admin console must be accessible, secure, and enable changes with audit logs, and support company-specific overrides.

## Project Information

- **PROJECT_NAME**: CRM Quotation Management System
- **SPEC_NUMBER**: Spec-018
- **SPEC_NAME**: System Administration & Configuration Console
- **GROUP**: Administration & Settings (Group 8 of 11)
- **PRIORITY**: HIGH (Phase 2, after Advanced Features)
- **DEPENDENCIES**: Spec-017 (Localization), Spec-009 (User Management), Spec-014 (Payment Config)
- **RELATED_SPECS**: Spec-019 (User Management Enhancements)

---

## Key Features

### System Settings Management
- Centralized configuration for organization-wide settings
- Company name, date format, currencies, notification toggles
- System-wide preferences that affect all users
- Real-time updates across the application

### Integration Keys & API Management
- Secure storage and management of third-party API credentials
- Support for payment gateways, SMS, email, analytics services
- Key rotation and access control
- Last used tracking and audit trail

### Custom Branding
- Company logo upload and management
- Theme color customization (primary, secondary, accent)
- Custom footer HTML and notices
- Live preview of branding changes

### Audit Log System
- Comprehensive logging of all system and admin actions
- Filterable and searchable audit trail
- User, action, entity, date, and IP tracking
- Export capabilities for compliance

### Data Retention & Compliance
- Configurable retention periods for different entity types
- Automatic purging policies
- Compliance settings and warnings
- Data archiving capabilities

### Global System Messages
- System-wide banner messages
- Info/warning notifications for all users
- Toggle visibility controls
- Preview functionality

---

## JTBD Alignment

**Persona**: System Administrator, IT Manager

**JTBD**: "I want to centrally manage all system settings, security configurations, and monitor system activity so that I can maintain control, ensure compliance, and customize the system for our organization"

**Success Metric**: "Admins can configure all system settings from one place; all changes are logged and auditable; branding and preferences apply immediately across the application"

---

## Business Value

- Centralized administration reduces operational overhead
- Enhanced security through proper key management and audit trails
- Custom branding improves brand consistency and user experience
- Compliance support through data retention policies and audit logs
- Reduced deployment frequency by enabling configuration changes without code deploys
- Better visibility into system usage and security events
- Improved governance and control over system behavior

---

## User Scenarios & Testing

### User Story 1 - System Settings Management (Priority: P1)

As a system administrator, I want to view and update system-wide settings (company name, date formats, currencies, notification preferences) so that I can configure the system to match our organization's requirements without requiring code changes.

**Why this priority**: This is the foundation of the admin console. All other features depend on having a working settings management system. It enables immediate value by allowing admins to customize basic system behavior.

**Independent Test**: Can be fully tested by an admin logging in, navigating to settings, updating a setting (e.g., company name), saving, and verifying the change is reflected immediately in the UI and persisted after refresh.

**Acceptance Scenarios**:

1. **Given** I am logged in as an admin, **When** I navigate to `/admin/settings`, **Then** I see a form with all system settings displayed with current values
2. **Given** I am on the settings page, **When** I update the company name and click save, **Then** the change is saved, a success toast appears, and the company name updates across the application immediately
3. **Given** I am on the settings page, **When** I enter invalid data (e.g., empty company name), **Then** validation errors are shown and the form cannot be submitted
4. **Given** I am a non-admin user, **When** I try to access `/admin/settings`, **Then** I receive a 403 Forbidden error

---

### User Story 2 - Integration Keys Management (Priority: P1)

As a system administrator, I want to manage API keys and credentials for third-party services (payment gateways, SMS, email) so that I can securely configure integrations without exposing sensitive credentials.

**Why this priority**: Critical for security and enables payment processing and notification features. Must be implemented early to support other dependent features.

**Independent Test**: Can be fully tested by an admin adding a new integration key, viewing it (masked), updating it, and verifying it's encrypted in the database. Non-admins should be blocked from accessing this feature.

**Acceptance Scenarios**:

1. **Given** I am an admin, **When** I navigate to `/admin/integrations`, **Then** I see a list of all configured integration keys with masked values
2. **Given** I am on the integrations page, **When** I click "Add New Key" and fill in the form, **Then** the key is saved encrypted and appears in the list
3. **Given** I am viewing an integration key, **When** I click "Show Key", **Then** the decrypted key is displayed temporarily (with a warning)
4. **Given** I am a non-admin, **When** I try to access `/admin/integrations`, **Then** I receive a 403 Forbidden error
5. **Given** I update an integration key, **When** the change is saved, **Then** an audit log entry is created and users are logged out if it's an authentication-related key

---

### User Story 3 - Audit Log Viewing (Priority: P2)

As a system administrator, I want to view and search audit logs of all system actions so that I can monitor security, troubleshoot issues, and maintain compliance.

**Why this priority**: Important for security and compliance, but can be implemented after core settings management. Provides visibility but doesn't block other features.

**Independent Test**: Can be fully tested by performing various admin actions (updating settings, managing keys), then viewing the audit log and verifying all actions are recorded with correct details (user, timestamp, IP, changes).

**Acceptance Scenarios**:

1. **Given** I am an admin, **When** I navigate to `/admin/audit-logs`, **Then** I see a paginated table of audit log entries with filters
2. **Given** I am viewing audit logs, **When** I filter by user and date range, **Then** the table shows only matching entries
3. **Given** I am viewing audit logs, **When** I click on an entry, **Then** a modal shows detailed information including the changes made
4. **Given** I am viewing audit logs, **When** I click "Export to CSV", **Then** a CSV file is downloaded with the filtered results

---

### User Story 4 - Custom Branding Management (Priority: P2)

As a system administrator, I want to customize the application's branding (logo, colors, footer) so that the system reflects our company's visual identity.

**Why this priority**: Enhances user experience and brand consistency, but is not critical for core functionality. Can be implemented after essential settings.

**Independent Test**: Can be fully tested by uploading a logo, changing theme colors, updating footer HTML, and verifying the changes appear immediately in a live preview and across the application.

**Acceptance Scenarios**:

1. **Given** I am an admin, **When** I navigate to `/admin/branding`, **Then** I see a form with logo upload, color pickers, and footer editor
2. **Given** I am on the branding page, **When** I upload a logo file, **Then** the logo is uploaded, previewed, and appears in the header immediately
3. **Given** I am on the branding page, **When** I change the primary color using the color picker, **Then** the live preview updates and the color is applied across the application after saving
4. **Given** I am on the branding page, **When** I update the footer HTML and save, **Then** the footer updates across all pages

---

### User Story 5 - Data Retention & Compliance Settings (Priority: P3)

As a system administrator, I want to configure data retention policies for different entity types so that we can comply with data protection regulations and manage storage costs.

**Why this priority**: Important for compliance but lower priority than core settings. Can be implemented after essential features are complete.

**Independent Test**: Can be fully tested by setting retention periods for different entities, enabling automatic purging, and verifying warnings appear before destructive actions.

**Acceptance Scenarios**:

1. **Given** I am an admin, **When** I navigate to `/admin/data-retention`, **Then** I see a table of all entity types with their current retention periods
2. **Given** I am on the data retention page, **When** I update a retention period and enable automatic purging, **Then** a confirmation dialog warns me about data loss and requires explicit confirmation
3. **Given** I enable automatic purging, **When** I confirm, **Then** the policy is saved and a scheduled job is created to enforce it

---

### User Story 6 - Global System Messages (Priority: P3)

As a system administrator, I want to set global banner messages that appear to all users so that I can communicate important system-wide announcements.

**Why this priority**: Useful feature but not critical. Can be implemented last as it's a nice-to-have communication tool.

**Independent Test**: Can be fully tested by setting a banner message, toggling visibility, and verifying it appears to all users (or disappears when toggled off).

**Acceptance Scenarios**:

1. **Given** I am an admin, **When** I navigate to `/admin/notifications`, **Then** I see a form to set global banner messages
2. **Given** I am on the notifications page, **When** I enter a message and enable it, **Then** the banner appears at the top of all pages for all users
3. **Given** I have an active banner, **When** I disable it, **Then** the banner disappears immediately for all users

---

## Requirements

### Functional Requirements

#### System Settings
- **FR-001**: System MUST provide an admin-only API endpoint to retrieve all system settings
- **FR-002**: System MUST provide an admin-only API endpoint to update system settings
- **FR-003**: System MUST validate all setting values before saving
- **FR-004**: System MUST log all setting changes to the audit log
- **FR-005**: System MUST apply setting changes immediately across the application without requiring restart

#### Integration Keys
- **FR-006**: System MUST encrypt all integration keys at rest in the database
- **FR-007**: System MUST provide admin-only endpoints to create, read, update, and delete integration keys
- **FR-008**: System MUST mask integration keys in API responses (show only last 4 characters)
- **FR-009**: System MUST track last used timestamp for each integration key
- **FR-010**: System MUST log all integration key changes to audit log
- **FR-011**: System MUST require admin confirmation before deleting integration keys

#### Audit Logs
- **FR-012**: System MUST log all admin actions including: settings changes, integration key changes, branding updates, retention policy changes
- **FR-013**: System MUST store audit log entries with: action type, entity, entity ID, user, IP address, timestamp, and changes (JSONB)
- **FR-014**: System MUST provide admin-only API to query audit logs with filters (user, action, date range, entity)
- **FR-015**: System MUST support pagination for audit log queries
- **FR-016**: System MUST provide CSV export functionality for audit logs

#### Custom Branding
- **FR-017**: System MUST allow admins to upload logo files (PNG, JPG, SVG) with size validation
- **FR-018**: System MUST store logo files securely and provide URLs for access
- **FR-019**: System MUST allow admins to set primary, secondary, and accent colors
- **FR-020**: System MUST allow admins to customize footer HTML
- **FR-021**: System MUST provide a live preview of branding changes before saving
- **FR-022**: System MUST apply branding changes immediately across the application

#### Data Retention
- **FR-023**: System MUST allow admins to configure retention periods (in months) per entity type
- **FR-024**: System MUST allow admins to enable/disable automatic purging per entity type
- **FR-025**: System MUST require explicit confirmation before enabling destructive purging actions
- **FR-026**: System MUST log all retention policy changes to audit log

#### Global Messages
- **FR-027**: System MUST allow admins to set global banner messages with type (info, warning, error)
- **FR-028**: System MUST allow admins to toggle banner visibility on/off
- **FR-029**: System MUST display active banners to all authenticated users
- **FR-030**: System MUST provide preview functionality for banner messages

#### Security & Access Control
- **FR-031**: System MUST enforce RBAC - only users with Admin role can access admin endpoints
- **FR-032**: System MUST return 403 Forbidden for non-admin users attempting to access admin features
- **FR-033**: System MUST validate all file uploads (type, size, content)
- **FR-034**: System MUST sanitize HTML content in footer and banner messages
- **FR-035**: System MUST use HTTPS for all admin API endpoints

### Key Entities

- **SystemSettings**: Key-value store for system-wide configuration. Key (string PK), value (JSONB), lastModifiedAt, lastModifiedBy (user ID)
- **IntegrationKeys**: Encrypted storage for third-party API credentials. ID (UUID PK), keyName, keyValueEncrypted, provider, createdAt, updatedAt, lastUsedAt
- **AuditLog**: Immutable log of all system actions. ID (UUID PK), actionType, entity, entityId, performedBy (user ID), ipAddress, timestamp, changes (JSONB)
- **CustomBranding**: Company-specific branding configuration. ID (PK), logoUrl, primaryColor, secondaryColor, accentColor, footerHtml, updatedBy, updatedAt
- **DataRetentionPolicy**: Rules for data archiving and purging. ID (PK), entityType, retentionPeriodMonths, isActive, autoPurgeEnabled, createdBy, updatedAt

---

## Technical Requirements

### Backend Requirements

#### Entities & Data Models
- SystemSettings (general configurations)
- IntegrationKeys (API credentials for third-party services)
- AuditLog (track all system/admin actions)
- CustomBranding (company logos, colors, templates)
- DataRetentionPolicy (rules for archiving/purging data)

#### APIs
- `GET /api/v1/admin/settings` - Retrieve system settings (admin only)
- `POST /api/v1/admin/settings` - Update system settings (admin only)
- `GET /api/v1/admin/integrations` - List all integration keys (admin only)
- `POST /api/v1/admin/integrations` - Create new integration key (admin only)
- `PUT /api/v1/admin/integrations/{id}` - Update integration key (admin only)
- `DELETE /api/v1/admin/integrations/{id}` - Delete integration key (admin only)
- `GET /api/v1/admin/integrations/{id}/show` - Show decrypted key (admin only, temporary)
- `GET /api/v1/admin/branding` - Get current branding (admin only)
- `POST /api/v1/admin/branding` - Update branding (admin only)
- `POST /api/v1/admin/branding/logo` - Upload logo file (admin only)
- `GET /api/v1/admin/data-retention` - Get retention policies (admin only)
- `POST /api/v1/admin/data-retention` - Update retention policies (admin only)
- `GET /api/v1/admin/audit-logs` - Query audit logs with filters (admin only)
- `GET /api/v1/admin/audit-logs/export` - Export audit logs to CSV (admin only)
- `GET /api/v1/admin/notification-settings` - Get global message settings (admin only)
- `POST /api/v1/admin/notification-settings` - Update global message settings (admin only)

#### Validation & Security
- Admins only (RBAC enforced throughout)
- Full validation of all config objects and files uploaded
- Logging all configuration and sensitive changes to audit logs
- Encryption of integration keys using secure encryption algorithms
- File upload validation (type, size, malware scanning if possible)
- HTML sanitization for user-provided HTML content

#### Events
- `SettingsUpdated` - Published when system settings are changed
- `IntegrationKeyChanged` - Published when integration keys are created/updated/deleted
- `BrandingChanged` - Published when branding is updated
- `RetentionPolicyChanged` - Published when retention policies are updated
- `SecurityAlert` - Published for security-sensitive actions (triggers notification/audit)

#### Unit/Integration Tests
- Coverage of CRUD for config, correct permissions, input validation, and events
- Test all API endpoints and edge cases
- Test encryption/decryption of integration keys
- Test audit log creation for all admin actions
- Test file upload validation and storage
- Test RBAC enforcement on all endpoints

### Database & Migrations

#### Tables

**SystemSettings**
- `Key` (string, PK) - Setting key identifier
- `Value` (JSONB) - Setting value (flexible structure)
- `LastModifiedAt` (timestamp)
- `LastModifiedBy` (UUID, FK to Users)

**IntegrationKeys**
- `Id` (UUID, PK)
- `KeyName` (string) - Human-readable name
- `KeyValueEncrypted` (string) - Encrypted key value
- `Provider` (string) - Service provider (e.g., "Stripe", "Razorpay")
- `CreatedAt` (timestamp)
- `UpdatedAt` (timestamp)
- `LastUsedAt` (timestamp, nullable)

**AuditLog**
- `Id` (UUID, PK)
- `ActionType` (string) - Type of action (e.g., "SettingsUpdated", "IntegrationKeyCreated")
- `Entity` (string) - Entity type affected
- `EntityId` (UUID, nullable) - ID of affected entity
- `PerformedBy` (UUID, FK to Users)
- `IpAddress` (string, nullable)
- `Timestamp` (timestamp)
- `Changes` (JSONB) - Before/after values or change description

**CustomBranding**
- `Id` (UUID, PK)
- `LogoUrl` (string, nullable) - URL to uploaded logo
- `PrimaryColor` (string) - Hex color code
- `SecondaryColor` (string) - Hex color code
- `AccentColor` (string, nullable) - Hex color code
- `FooterHtml` (text, nullable) - Custom footer HTML
- `UpdatedBy` (UUID, FK to Users)
- `UpdatedAt` (timestamp)

**DataRetentionPolicy**
- `Id` (UUID, PK)
- `EntityType` (string) - Type of entity (e.g., "Quotation", "Payment", "AuditLog")
- `RetentionPeriodMonths` (integer) - Retention period in months
- `IsActive` (boolean)
- `AutoPurgeEnabled` (boolean)
- `CreatedBy` (UUID, FK to Users)
- `UpdatedAt` (timestamp)

#### Migrations
- Create tables for above models
- Add indexes on: `SystemSettings.Key`, `AuditLog.PerformedBy`, `AuditLog.Timestamp`, `AuditLog.Entity`, `IntegrationKeys.Provider`
- Seed default system settings if missing
- Seed default branding configuration if missing
- Ensure encrypted storage of sensitive keys (use database encryption or application-level encryption)

### Frontend Requirements

#### Pages & Components (Admin-Only, TailAdmin Next.js Theme)

**AD-P01: Admin Console Home** (`/admin/settings`)
- Dashboard with cards for each config area (Settings, API Keys, Branding, Retention, Audit Logs, Security)
- Quick links to each section
- Summary statistics (e.g., total integration keys, recent audit log entries)

**AD-P02: System Settings Editor**
- Form for updating system-wide settings (company name, date format, currencies, notification toggles)
- Save and reset buttons
- Field validation with error messages
- Toast notification on save
- Real-time preview of changes where applicable

**AD-P03: Integration Keys/Third Party API Manager**
- List view of all integration keys in a data table
- Add, edit, delete actions
- Show last used date, creation date
- Masked display for keys (show only last 4 characters)
- "Show Key" button with temporary display and warning
- Copy to clipboard functionality
- Confirmation dialog for delete actions

**AD-P04: Audit Log Browser**
- Filterable, searchable data table
- Filters: user, action type, entity, date range
- Columns: timestamp, user, action, entity, IP address
- Pagination support
- Export to CSV button
- View action details in modal/drawer
- Highlight security-sensitive actions

**AD-P05: Custom Branding Editor**
- Logo file upload with drag-and-drop
- Image preview with remove/replace options
- Color pickers for app theme (primary, secondary, accent)
- HTML editor for footer/custom notices (with preview)
- Live preview component showing how branding will look
- Save and reset buttons

**AD-P06: Data Retention & Compliance Settings**
- Table of retention periods for each entity type
- Form to update periods or toggle automatic purging
- Warnings/confirmations before enabling destructive actions
- Visual indicators for active policies
- Last updated timestamp

**AD-P07: Global System Message/Banner Settings**
- Form to set banner message text
- Message type selector (info, warning, error)
- Toggle visibility on/off
- Preview component showing how banner will appear
- Active/inactive status indicator

#### Components Needed
- `SettingsForm` - Reusable form component for settings
- `ApiKeyList` - Table component for integration keys
- `ApiKeyEditDialog` - Modal for creating/editing keys
- `BrandingUploader` - File upload component for logos
- `ColorPicker` - Color selection component
- `LivePreview` - Preview component for branding
- `AuditLogTable` - Data table for audit logs
- `AuditLogFilter` - Filter component for audit logs
- `LogDetailModal` - Modal showing audit log details
- `RetentionPolicyTable` - Table for retention policies
- `RetentionEditDialog` - Dialog for editing retention policies
- `SystemBannerSet` - Component for setting global banners
- `PreviewBanner` - Preview component for banners

#### UX Requirements
- All config changes require confirmation (especially destructive actions)
- Changes reflected immediately using React Query invalidate/refetch pattern
- Secure logout shown on API key updates (if authentication-related)
- Loading states for all async operations
- Error handling with user-friendly messages
- Success toasts for all save operations
- Responsive design for mobile/tablet access
- Accessibility compliance (WCAG 2.1 AA)

#### Tests
- Component/unit tests for all editors & tables
- Integration tests for forbidden access, settings propagation, audit log accuracy
- E2E: Admin logs in, updates config, sees change live across app
- Test file upload validation
- Test color picker functionality
- Test audit log filtering and export

---

## Security & Compliance

- All admin endpoints must enforce RBAC (Admin role required)
- Integration keys must be encrypted at rest
- Audit logs must be immutable (append-only)
- File uploads must be validated and scanned for malware
- HTML content must be sanitized to prevent XSS
- All admin actions must be logged to audit trail
- Rate limiting on admin endpoints to prevent abuse
- CSRF protection on all state-changing operations
- Secure session management for admin users

---

## Performance Considerations

- Cache system settings to minimize database queries
- Index audit log table for efficient filtering and querying
- Lazy load audit log entries with pagination
- Optimize file storage for logos (CDN, compression)
- Efficient encryption/decryption of integration keys
- Background jobs for data retention purging (if enabled)

---

## Scalability

- Support for multiple companies/tenants (if multi-tenant architecture)
- Efficient audit log storage and archival
- Horizontal scaling support for admin endpoints
- CDN support for branding assets
- Database partitioning for audit logs (if volume is high)

---

## Success Criteria

### Backend
- ✅ All APIs, models, and migrations created and tested
- ✅ All admin endpoints enforce RBAC correctly
- ✅ Integration keys are encrypted and secure
- ✅ Audit logs capture all admin actions accurately
- ✅ Settings changes are persisted and applied immediately
- ✅ File uploads are validated and stored securely
- ✅ Test coverage ≥85% for backend code

### Database
- ✅ All migrations succeed
- ✅ Encrypted storage for sensitive keys
- ✅ Proper indexes for performance
- ✅ Default seed data for settings/branding
- ✅ Data integrity constraints in place

### Frontend
- ✅ All admin console pages & components built using TailAdmin Next.js
- ✅ Immediate visual feedback for changes
- ✅ All forms have proper validation
- ✅ File uploads work correctly
- ✅ Audit log filtering and export functional
- ✅ Responsive design works on all devices
- ✅ Test coverage ≥80% for frontend code

### Integration
- ✅ Only admins can access config options, non-admins blocked
- ✅ UI reflects changes instantly across application
- ✅ Config changes affect user-facing UI (branding, banners, formats) without code deploy
- ✅ Audit trail stored and visible for all changes
- ✅ E2E flow works: admin makes settings change → change reflected across app → audit trail stored

---

## Deliverables

### Backend
- Entities: SystemSettings, IntegrationKeys, AuditLog, CustomBranding, DataRetentionPolicy
- DTOs for all admin operations
- Commands: UpdateSystemSettingsCommand, ManageIntegrationKeyCommand, UpdateBrandingCommand, UpdateRetentionPolicyCommand, UpdateNotificationSettingsCommand
- Queries: GetSystemSettingsQuery, GetIntegrationKeysQuery, GetAuditLogsQuery, GetBrandingQuery, GetRetentionPoliciesQuery
- Services: SystemSettingsService, IntegrationKeyService (with encryption), AuditLogService, BrandingService, RetentionPolicyService
- Controllers: AdminSettingsController, AdminIntegrationsController, AdminBrandingController, AdminAuditLogController, AdminRetentionController, AdminNotificationsController
- Event handlers for SettingsUpdated, IntegrationKeyChanged, BrandingChanged, RetentionPolicyChanged, SecurityAlert
- Validation for all inputs
- Logging and audit trail integration
- Migration scripts for all tables
- Unit/integration tests (≥85% coverage)

### Database
- New tables: SystemSettings, IntegrationKeys, AuditLog, CustomBranding, DataRetentionPolicy
- Default seed data for settings and branding
- Security settings and encrypted storage
- Optimized indexes for performance
- Full migration files with rollback support

### Frontend
- Pages: AdminConsoleHome, SystemSettingsEditor, IntegrationKeysManager, AuditLogBrowser, BrandingEditor, DataRetentionSettings, GlobalMessageSettings
- Components: SettingsForm, ApiKeyList, ApiKeyEditDialog, BrandingUploader, ColorPicker, LivePreview, AuditLogTable, AuditLogFilter, LogDetailModal, RetentionPolicyTable, RetentionEditDialog, SystemBannerSet, PreviewBanner
- API hooks/services for all admin endpoints
- State management with React Query
- Full styling using TailAdmin Next.js theme
- Robust validation and error handling
- Testing (≥80% coverage)

### Integration
- Full E2E flow: Admin makes settings change → change reflected across app → audit trail stored and visible
- Integration with existing RBAC system
- Integration with notification system (Spec-013) for security alerts
- Integration with localization system (Spec-017) for admin UI
- Integration with user management (Spec-009) for audit log user tracking

---

**End of Spec-018: System Administration & Configuration Console**

