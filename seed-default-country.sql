-- Seed default country and tax framework
-- This creates India as the default country with GST framework

-- Insert India as default country if it doesn't exist
-- Using proper UUID format (uppercase hex)
INSERT INTO "Countries" (
    "CountryId",
    "CountryName",
    "CountryCode",
    "TaxFrameworkType",
    "DefaultCurrency",
    "IsActive",
    "IsDefault",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt"
)
SELECT 
    'A1B2C3D4-E5F6-47A8-B9C0-D1E2F3A4B5C6'::uuid,
           'India',
           'IN',
           0, -- GST (0 = GST, 1 = VAT)
    'INR',
    true,
    true,
    NOW(),
    NOW(),
    NULL
WHERE NOT EXISTS (
    SELECT 1 FROM "Countries" WHERE "CountryCode" = 'IN' AND "DeletedAt" IS NULL
);

-- Insert GST Tax Framework for India if it doesn't exist
INSERT INTO "TaxFrameworks" (
    "TaxFrameworkId",
    "CountryId",
    "FrameworkName",
    "FrameworkType",
    "Description",
    "TaxComponents",
    "IsActive",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt"
)
SELECT 
    'E5F6A7B8-C9D0-51E1-F2A3-B4C5D6E7F8A9'::uuid,
    'A1B2C3D4-E5F6-47A8-B9C0-D1E2F3A4B5C6'::uuid,
           'Goods and Services Tax',
           0, -- GST (0 = GST, 1 = VAT)
    'GST framework for India with CGST, SGST, and IGST components',
    '[{"name": "CGST", "code": "CGST", "isCentrallyGoverned": false, "description": "Central Goods and Services Tax"}, {"name": "SGST", "code": "SGST", "isCentrallyGoverned": false, "description": "State Goods and Services Tax"}, {"name": "IGST", "code": "IGST", "isCentrallyGoverned": true, "description": "Integrated Goods and Services Tax"}]'::jsonb,
    true,
    NOW(),
    NOW(),
    NULL
WHERE NOT EXISTS (
    SELECT 1 FROM "TaxFrameworks" 
    WHERE "CountryId" = 'A1B2C3D4-E5F6-47A8-B9C0-D1E2F3A4B5C6'::uuid 
    AND "DeletedAt" IS NULL
);

-- Insert some common Indian states as Jurisdictions
INSERT INTO "Jurisdictions" (
    "JurisdictionId",
    "CountryId",
    "ParentJurisdictionId",
    "JurisdictionName",
    "JurisdictionCode",
    "JurisdictionType",
    "IsActive",
    "CreatedAt",
    "UpdatedAt",
    "DeletedAt"
)
VALUES
    ('C3D4E5F6-A7B8-49C0-D1E2-F3A4B5C6D7E8'::uuid, 'A1B2C3D4-E5F6-47A8-B9C0-D1E2F3A4B5C6'::uuid, NULL, 'Maharashtra', '27', 'State', true, NOW(), NOW(), NULL),
    ('C4D5E6F7-B8C9-50D1-E2F3-A4B5C6D7E8F9'::uuid, 'A1B2C3D4-E5F6-47A8-B9C0-D1E2F3A4B5C6'::uuid, NULL, 'Karnataka', '29', 'State', true, NOW(), NOW(), NULL),
    ('D4E5F6A7-C9D0-51E1-F2A3-B4C5D6E7F8A9'::uuid, 'A1B2C3D4-E5F6-47A8-B9C0-D1E2F3A4B5C6'::uuid, NULL, 'Haryana', '06', 'State', true, NOW(), NOW(), NULL),
    ('E5F6A7B8-D0E1-52F2-A3B4-C5D6E7F8A9B0'::uuid, 'A1B2C3D4-E5F6-47A8-B9C0-D1E2F3A4B5C6'::uuid, NULL, 'Delhi', '07', 'State', true, NOW(), NOW(), NULL),
    ('F6A7B8C9-E1F2-53A3-B4C5-D6E7F8A9B0C1'::uuid, 'A1B2C3D4-E5F6-47A8-B9C0-D1E2F3A4B5C6'::uuid, NULL, 'Gujarat', '24', 'State', true, NOW(), NOW(), NULL)
ON CONFLICT DO NOTHING;

-- Insert default tax rate (18% GST) for India
INSERT INTO "TaxRates" (
    "TaxRateId",
    "JurisdictionId",
    "TaxFrameworkId",
    "ProductServiceCategoryId",
    "TaxRate",
    "EffectiveFrom",
    "EffectiveTo",
    "IsExempt",
    "IsZeroRated",
    "TaxComponents",
    "Description",
    "CreatedAt",
    "UpdatedAt"
)
SELECT 
    'F7A8B9C0-D1E2-54F3-A4B5-C6D7E8F9A0B1'::uuid,
    NULL, -- General rate, not jurisdiction-specific
    'E5F6A7B8-C9D0-51E1-F2A3-B4C5D6E7F8A9'::uuid,
    NULL, -- General rate, not category-specific
    18.00,
    CURRENT_DATE,
    NULL,
    false,
    false,
           '[{"Component": "CGST", "Rate": 9.0}, {"Component": "SGST", "Rate": 9.0}]'::jsonb,
    'Standard GST rate for India (18%)',
    NOW(),
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM "TaxRates" 
    WHERE "TaxFrameworkId" = 'E5F6A7B8-C9D0-51E1-F2A3-B4C5D6E7F8A9'::uuid
    AND "JurisdictionId" IS NULL
    AND "ProductServiceCategoryId" IS NULL
);

-- Verify the default country was created
SELECT 
    "CountryId",
    "CountryName",
    "CountryCode",
    "IsDefault",
    "IsActive"
FROM "Countries"
WHERE "IsDefault" = true AND "DeletedAt" IS NULL;

