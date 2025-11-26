# Data Model: Company Details Admin Configuration (Spec-022)

## Entities

### CompanyDetails (Singleton)

**Table**: `CompanyDetails`  
**Primary Key**: `CompanyDetailsId` (UUID)  
**Constraint**: Only one record should exist (enforced at application level)

| Column | Type | Constraints | Description |
|--------|------|------------|-------------|
| CompanyDetailsId | UUID | PK, NOT NULL | Unique identifier (constant: `00000000-0000-0000-0000-000000000001`) |
| PanNumber | VARCHAR(10) | NULL | Permanent Account Number (India) |
| TanNumber | VARCHAR(10) | NULL | Tax Deduction and Collection Account Number (India) |
| GstNumber | VARCHAR(15) | NULL | Goods and Services Tax Identification Number (India) |
| CompanyName | VARCHAR(255) | NULL | Company legal name |
| CompanyAddress | TEXT | NULL | Full company address |
| City | VARCHAR(100) | NULL | City |
| State | VARCHAR(100) | NULL | State/Province |
| PostalCode | VARCHAR(20) | NULL | Postal/ZIP code |
| Country | VARCHAR(100) | NULL | Country |
| ContactEmail | VARCHAR(255) | NULL | Primary contact email |
| ContactPhone | VARCHAR(20) | NULL | Primary contact phone |
| Website | VARCHAR(255) | NULL | Company website URL |
| LegalDisclaimer | TEXT | NULL | Legal disclaimer text for quotations |
| LogoUrl | VARCHAR(500) | NULL | Relative path to company logo file |
| CreatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Record creation timestamp |
| UpdatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Last update timestamp |
| UpdatedBy | UUID | NOT NULL, FK -> Users.UserId | User who last updated the record |

**Indexes**:
- PK(CompanyDetailsId)
- IX_CompanyDetails_UpdatedAt
- FK_CompanyDetails_Users_UpdatedBy

**Notes**:
- Singleton pattern: Application ensures only one record exists
- Use upsert pattern (INSERT ... ON CONFLICT UPDATE) or check existence before insert
- All fields except timestamps and UpdatedBy are nullable (flexible configuration)

---

### BankDetails

**Table**: `BankDetails`  
**Primary Key**: `BankDetailsId` (UUID)  
**Relationship**: Many BankDetails to one CompanyDetails

| Column | Type | Constraints | Description |
|--------|------|------------|-------------|
| BankDetailsId | UUID | PK, NOT NULL | Unique identifier |
| CompanyDetailsId | UUID | NOT NULL, FK -> CompanyDetails.CompanyDetailsId | Reference to company details |
| Country | VARCHAR(50) | NOT NULL | Country code/name ("India", "Dubai") |
| AccountNumber | VARCHAR(50) | NOT NULL | Bank account number |
| IfscCode | VARCHAR(11) | NULL | IFSC Code (India only) |
| Iban | VARCHAR(34) | NULL | IBAN (Dubai/International) |
| SwiftCode | VARCHAR(11) | NULL | SWIFT/BIC Code (Dubai/International) |
| BankName | VARCHAR(255) | NOT NULL | Bank name |
| BranchName | VARCHAR(255) | NULL | Branch name |
| CreatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Record creation timestamp |
| UpdatedAt | TIMESTAMPTZ | NOT NULL, DEFAULT CURRENT_TIMESTAMP | Last update timestamp |
| UpdatedBy | UUID | NOT NULL, FK -> Users.UserId | User who last updated the record |

**Indexes**:
- PK(BankDetailsId)
- IX_BankDetails_CompanyDetailsId
- UQ_BankDetails_CompanyDetailsId_Country (unique constraint: one bank detail per country per company)
- FK_BankDetails_CompanyDetails_CompanyDetailsId
- FK_BankDetails_Users_UpdatedBy

**Notes**:
- Country-specific fields:
  - **India**: AccountNumber, IfscCode, BankName, BranchName (required)
  - **Dubai**: AccountNumber, Iban, SwiftCode, BankName, BranchName (required)
- Unique constraint ensures only one bank detail per country per company
- Can add more countries in future by adding new records with different Country values

---

### Quotations (Modification)

**Table**: `Quotations` (existing table)  
**Modification**: Add optional column for historical company details snapshot

| Column | Type | Constraints | Description |
|--------|------|------------|-------------|
| ... (existing columns) | ... | ... | ... |
| CompanyDetailsSnapshot | JSONB | NULL | Snapshot of company details at quotation creation time |

**Indexes**:
- (No additional indexes needed - JSONB column for read-only snapshot)

**Notes**:
- Column added via migration
- Snapshot stored as JSON when quotation is created
- Format: `{ "panNumber": "...", "tanNumber": "...", "gstNumber": "...", "companyName": "...", "address": "...", "bankDetails": [{ "country": "India", ... }] }`
- If snapshot is NULL, use current company details when generating PDFs
- Historical accuracy: Quotations preserve company details from creation time

---

## Relationships

### CompanyDetails → Users
- `CompanyDetails.UpdatedBy` → `Users.UserId` (many-to-one)
- Foreign key: `FK_CompanyDetails_Users_UpdatedBy`
- On delete: RESTRICT (cannot delete user who updated company details)

### BankDetails → CompanyDetails
- `BankDetails.CompanyDetailsId` → `CompanyDetails.CompanyDetailsId` (many-to-one)
- Foreign key: `FK_BankDetails_CompanyDetails_CompanyDetailsId`
- On delete: CASCADE (if company details deleted, bank details deleted)

### BankDetails → Users
- `BankDetails.UpdatedBy` → `Users.UserId` (many-to-one)
- Foreign key: `FK_BankDetails_Users_UpdatedBy`
- On delete: RESTRICT

### Quotations → CompanyDetails (Indirect)
- Quotations reference company details via `CompanyDetailsSnapshot` JSONB column
- No foreign key (snapshot is historical data)
- Quotations can reference company details that may have been updated since creation

---

## Constraints

### CompanyDetails
- Singleton constraint: Application-level enforcement (only one record exists)
- UpdatedBy must reference valid User
- PanNumber format: `^[A-Z]{5}[0-9]{4}[A-Z]{1}$` (if provided)
- TanNumber format: `^[A-Z]{4}[0-9]{5}[A-Z]{1}$` (if provided)
- GstNumber format: `^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$` (if provided)
- LogoUrl must be valid relative path (if provided)

### BankDetails
- Unique constraint: One bank detail per country per company (`UQ_BankDetails_CompanyDetailsId_Country`)
- Country must be valid ("India", "Dubai", or future countries)
- Country-specific field validation:
  - **India**: IfscCode required, Iban and SwiftCode should be NULL
  - **Dubai**: Iban and SwiftCode required, IfscCode should be NULL
- AccountNumber required for all countries
- BankName required for all countries
- UpdatedBy must reference valid User

### Quotations
- CompanyDetailsSnapshot is optional (nullable)
- If provided, must be valid JSONB containing company details structure

---

## State Management

### CompanyDetails
- **Active**: Record exists (singleton)
- **Initialization**: First admin configuration creates the record
- **Updates**: Upsert pattern (update existing record or create if not exists)

### BankDetails
- **Active**: Record exists and is associated with CompanyDetails
- **Multiple Countries**: Can have multiple active bank details (one per country)
- **Updates**: Update existing record or create new if country doesn't exist

---

## Seed Data

### Initial CompanyDetails Record
- Create with constant ID: `00000000-0000-0000-0000-000000000001`
- All fields NULL initially (admin must configure)
- CreatedAt and UpdatedAt set to current timestamp
- UpdatedBy set to first admin user (if available)

### No Initial BankDetails Records
- Bank details created by admin during configuration
- No seed data needed

---

## Migration Strategy

### Step 1: Create CompanyDetails Table
```sql
CREATE TABLE "CompanyDetails" (
    "CompanyDetailsId" UUID PRIMARY KEY DEFAULT '00000000-0000-0000-0000-000000000001',
    "PanNumber" VARCHAR(10),
    "TanNumber" VARCHAR(10),
    "GstNumber" VARCHAR(15),
    "CompanyName" VARCHAR(255),
    "CompanyAddress" TEXT,
    "City" VARCHAR(100),
    "State" VARCHAR(100),
    "PostalCode" VARCHAR(20),
    "Country" VARCHAR(100),
    "ContactEmail" VARCHAR(255),
    "ContactPhone" VARCHAR(20),
    "Website" VARCHAR(255),
    "LegalDisclaimer" TEXT,
    "LogoUrl" VARCHAR(500),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" UUID NOT NULL REFERENCES "Users"("UserId") ON DELETE RESTRICT
);

CREATE INDEX "IX_CompanyDetails_UpdatedAt" ON "CompanyDetails"("UpdatedAt");
```

### Step 2: Create BankDetails Table
```sql
CREATE TABLE "BankDetails" (
    "BankDetailsId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CompanyDetailsId" UUID NOT NULL REFERENCES "CompanyDetails"("CompanyDetailsId") ON DELETE CASCADE,
    "Country" VARCHAR(50) NOT NULL,
    "AccountNumber" VARCHAR(50) NOT NULL,
    "IfscCode" VARCHAR(11),
    "Iban" VARCHAR(34),
    "SwiftCode" VARCHAR(11),
    "BankName" VARCHAR(255) NOT NULL,
    "BranchName" VARCHAR(255),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" UUID NOT NULL REFERENCES "Users"("UserId") ON DELETE RESTRICT,
    CONSTRAINT "UQ_BankDetails_CompanyDetailsId_Country" UNIQUE ("CompanyDetailsId", "Country")
);

CREATE INDEX "IX_BankDetails_CompanyDetailsId" ON "BankDetails"("CompanyDetailsId");
```

### Step 3: Add CompanyDetailsSnapshot to Quotations
```sql
ALTER TABLE "Quotations" ADD COLUMN "CompanyDetailsSnapshot" JSONB;
```

---

## Entity Framework Configuration

### CompanyDetailsEntityConfiguration.cs
```csharp
public class CompanyDetailsEntityConfiguration : IEntityTypeConfiguration<CompanyDetails>
{
    public void Configure(EntityTypeBuilder<CompanyDetails> builder)
    {
        builder.ToTable("CompanyDetails");
        builder.HasKey(c => c.CompanyDetailsId);
        
        builder.Property(c => c.CompanyDetailsId)
            .HasDefaultValue(new Guid("00000000-0000-0000-0000-000000000001"));
        
        builder.Property(c => c.PanNumber).HasMaxLength(10);
        builder.Property(c => c.TanNumber).HasMaxLength(10);
        builder.Property(c => c.GstNumber).HasMaxLength(15);
        builder.Property(c => c.CompanyName).HasMaxLength(255);
        builder.Property(c => c.ContactEmail).HasMaxLength(255);
        builder.Property(c => c.LogoUrl).HasMaxLength(500);
        
        builder.HasOne(c => c.UpdatedByUser)
            .WithMany()
            .HasForeignKey(c => c.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(c => c.UpdatedAt);
    }
}
```

### BankDetailsEntityConfiguration.cs
```csharp
public class BankDetailsEntityConfiguration : IEntityTypeConfiguration<BankDetails>
{
    public void Configure(EntityTypeBuilder<BankDetails> builder)
    {
        builder.ToTable("BankDetails");
        builder.HasKey(b => b.BankDetailsId);
        
        builder.Property(b => b.Country).HasMaxLength(50).IsRequired();
        builder.Property(b => b.AccountNumber).HasMaxLength(50).IsRequired();
        builder.Property(b => b.IfscCode).HasMaxLength(11);
        builder.Property(b => b.Iban).HasMaxLength(34);
        builder.Property(b => b.SwiftCode).HasMaxLength(11);
        builder.Property(b => b.BankName).HasMaxLength(255).IsRequired();
        
        builder.HasOne(b => b.CompanyDetails)
            .WithMany(c => c.BankDetails)
            .HasForeignKey(b => b.CompanyDetailsId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(b => b.UpdatedByUser)
            .WithMany()
            .HasForeignKey(b => b.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(b => b.CompanyDetailsId);
        builder.HasAlternateKey(b => new { b.CompanyDetailsId, b.Country });
    }
}
```

