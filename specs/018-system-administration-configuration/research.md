# Research for Spec-018: System Administration & Configuration Console

## Decisions and Rationale

### Encryption for Integration Keys

**Decision**: Use AES-256-GCM encryption via .NET `System.Security.Cryptography.AesGcm` class for integration keys at rest.

**Rationale**: 
- AES-256-GCM provides authenticated encryption (confidentiality + integrity)
- Built into .NET 8.0, no external dependencies
- GCM mode prevents tampering and provides authentication tag
- Industry-standard encryption algorithm
- Performance: ~100MB/s encryption/decryption on modern hardware

**Alternatives considered**:
- AES-256-CBC: Less secure (no authentication), vulnerable to padding oracle attacks
- RSA encryption: Slower, key size limitations, not suitable for bulk data
- Database-level encryption (TDE): Less flexible, harder to rotate keys, vendor-specific
- External key management (Azure Key Vault, AWS KMS): Overkill for current scale, adds complexity and cost

**Implementation**:
- Encryption key stored in environment variable or Azure Key Vault (production)
- Key rotation strategy: Support multiple keys with versioning
- Decryption only when explicitly requested (via "Show Key" action)
- Keys masked in API responses (show only last 4 characters)

---

### File Storage for Logos

**Decision**: Use local filesystem storage initially, with S3-compatible interface for future migration.

**Rationale**:
- Simpler initial implementation (no external service dependency)
- Easy migration path to S3 via interface abstraction
- Sufficient for MVP (single organization, moderate file count)
- Can leverage existing S3 infrastructure from Spec-014 (Payment Processing)

**Alternatives considered**:
- S3 from start: Adds complexity and cost for MVP
- Database BLOB storage: Inefficient for large files, impacts DB performance
- CDN (CloudFront/Cloudflare): Overkill for initial implementation, can be added later

**Implementation**:
- `IFileStorageService` interface with `LocalFileStorageService` implementation
- Files stored in `wwwroot/uploads/branding/` (or configurable path)
- File naming: `{brandingId}_{timestamp}.{ext}` to prevent conflicts
- Future: `S3FileStorageService` implementing same interface
- File validation: Type (PNG/JPG/SVG), size (max 5MB), content verification

---

### HTML Sanitization

**Decision**: Use `HtmlSanitizer` NuGet package (Ganss.Xss) for sanitizing user-provided HTML in footer and banner messages.

**Rationale**:
- Mature, well-maintained library (used by ASP.NET Core)
- Configurable allowlist of HTML tags and attributes
- Prevents XSS attacks while allowing safe HTML
- Lightweight and performant
- Supports CSS sanitization for style attributes

**Alternatives considered**:
- Manual regex-based sanitization: Error-prone, difficult to maintain, security risks
- `AntiXss` library: Deprecated, not actively maintained
- Server-side rendering only (no HTML): Too restrictive, limits customization
- DOMPurify (JavaScript): Client-side only, not sufficient (need server-side validation)

**Implementation**:
- Allowlist: `<p>`, `<br>`, `<strong>`, `<em>`, `<a>` (with href validation), `<ul>`, `<ol>`, `<li>`
- Strip: `<script>`, `<iframe>`, `<object>`, event handlers, `javascript:` URLs
- Sanitize CSS in style attributes (remove dangerous properties)
- Apply sanitization in `BrandingService` and `NotificationSettingsService`

---

### Audit Log Storage and Archival

**Decision**: Store audit logs in PostgreSQL with append-only pattern, implement archival strategy for old entries.

**Rationale**:
- PostgreSQL provides ACID guarantees and efficient querying
- JSONB column for flexible change tracking
- Indexes on `PerformedBy`, `Timestamp`, `Entity` for fast filtering
- Archival to separate table or external storage after 1 year (configurable)
- Maintains referential integrity with Users table

**Alternatives considered**:
- Event sourcing: Overkill for audit logs, adds complexity
- External logging service (Datadog, Splunk): Cost, vendor lock-in, harder to query
- File-based logging: Difficult to query, no relational integrity
- Separate audit database: Operational overhead, synchronization complexity

**Implementation**:
- `AuditLog` table with indexes on frequently queried columns
- Background job to archive entries older than retention period
- Archived entries moved to `AuditLogArchive` table or exported to cold storage
- Query API supports filtering by date range, user, action, entity

---

### Settings Caching Strategy

**Decision**: Cache system settings in-memory with 5-minute TTL, invalidate on update.

**Rationale**:
- System settings are read frequently but updated rarely
- Reduces database load for high-traffic scenarios
- 5-minute TTL balances freshness with performance
- Immediate invalidation on update ensures consistency
- Simple implementation using `IMemoryCache` (built into ASP.NET Core)

**Alternatives considered**:
- No caching: Acceptable for low traffic, but scales poorly
- Redis distributed cache: Overkill for single-instance deployment
- Database query caching: Less control, harder to invalidate
- Long TTL (1 hour+): Risk of stale data after updates

**Implementation**:
- Cache key: `"system_settings_{key}"`
- Invalidate cache in `UpdateSystemSettingsCommandHandler` after save
- Frontend: React Query with stale-while-revalidate pattern

---

### Data Retention Policy Enforcement

**Decision**: Background job (Hangfire or .NET Background Service) to enforce retention policies on schedule.

**Rationale**:
- Retention policies need periodic execution (daily/weekly)
- Background job prevents blocking API requests
- Can be scheduled and monitored
- Supports retry logic for failed purges
- Aligns with existing background job infrastructure (email queue)

**Alternatives considered**:
- On-demand purging via API: Risk of accidental data loss, no automation
- Database triggers: Complex, harder to test and maintain
- External cron job: Operational overhead, less integrated
- Event-driven purging: Difficult to determine when data is "old enough"

**Implementation**:
- `DataRetentionJob` runs daily at 2 AM (configurable)
- Queries `DataRetentionPolicy` table for active policies
- For each policy, identifies records exceeding retention period
- Soft delete or hard delete based on policy configuration
- Logs all purging actions to audit log
- Sends notification to admins if purging fails

---

## Best Practices & Patterns

### Encryption
- Use `AesGcm` class from `System.Security.Cryptography` namespace
- Generate random IV (nonce) for each encryption operation
- Store IV alongside encrypted data (12 bytes for GCM)
- Use authenticated encryption (GCM mode) to prevent tampering
- Rotate encryption keys periodically (support key versioning)
- Never log or expose decrypted keys in logs or error messages

### File Uploads
- Validate file type by MIME type AND file extension
- Scan file content (magic bytes) to prevent extension spoofing
- Limit file size (5MB for logos)
- Generate unique filenames to prevent conflicts
- Store files outside web root or use secure URLs
- Implement virus scanning in production (optional but recommended)

### HTML Sanitization
- Sanitize on both client and server (defense in depth)
- Use allowlist approach (safer than blocklist)
- Strip all JavaScript and event handlers
- Validate URLs in `<a>` tags (no `javascript:`, `data:`)
- Sanitize CSS in style attributes
- Test with XSS payloads to verify effectiveness

### Audit Logging
- Log all admin actions (create, update, delete)
- Include before/after values in JSONB `Changes` column
- Mask sensitive data (passwords, keys) in audit logs
- Include IP address and user agent for security analysis
- Make audit logs immutable (no updates/deletes)
- Index frequently queried columns for performance

### Settings Management
- Use key-value store pattern for flexibility
- Validate setting values before saving
- Provide defaults for missing settings
- Cache frequently accessed settings
- Invalidate cache on updates
- Support nested JSON values in JSONB column

### RBAC Enforcement
- Use `[Authorize(Roles = "Admin")]` attribute on all admin controllers
- Verify role in handlers as additional check (defense in depth)
- Return 403 Forbidden (not 401) for unauthorized access
- Log unauthorized access attempts
- Use policy-based authorization for complex rules (future)

---

## Integration Points

### Existing Systems
- **Spec-009 (User Management)**: Audit logs reference Users table via `PerformedBy` FK
- **Spec-014 (Payment Config)**: Integration keys used by payment gateway services
- **Spec-017 (Localization)**: System settings include locale preferences
- **Spec-013 (Notifications)**: Security alerts trigger notifications via event handlers

### Future Enhancements
- Multi-tenant support: Add `CompanyId` to settings/branding tables
- Key rotation automation: Scheduled job to rotate integration keys
- Audit log analytics: Dashboard for security insights
- Settings versioning: Track history of setting changes
- Import/Export settings: Backup and restore configuration

---

## Security Considerations

1. **Encryption Key Management**: Store encryption keys in secure key vault (Azure Key Vault, AWS Secrets Manager) in production
2. **File Upload Security**: Validate file types, scan for malware, limit file size
3. **HTML Sanitization**: Always sanitize user-provided HTML to prevent XSS
4. **Audit Log Integrity**: Make audit logs append-only, use database constraints
5. **Rate Limiting**: Apply rate limiting to admin endpoints to prevent abuse
6. **CSRF Protection**: Use anti-forgery tokens for state-changing operations
7. **Access Control**: Enforce RBAC at multiple layers (controller, handler, service)

---

## Performance Optimizations

1. **Settings Caching**: Cache system settings with 5-minute TTL
2. **Audit Log Indexing**: Index `PerformedBy`, `Timestamp`, `Entity` columns
3. **Pagination**: Always paginate audit log queries (default 50 per page)
4. **Lazy Loading**: Load audit log details on-demand (modal view)
5. **File Compression**: Compress logo images on upload (optional)
6. **CDN**: Serve branding assets via CDN in production

---

## Testing Strategy

1. **Unit Tests**: Test encryption/decryption, HTML sanitization, file validation
2. **Integration Tests**: Test API endpoints with RBAC enforcement
3. **E2E Tests**: Test complete admin workflows (update settings, view audit logs)
4. **Security Tests**: Test XSS prevention, unauthorized access, file upload validation
5. **Performance Tests**: Test audit log query performance with large datasets

