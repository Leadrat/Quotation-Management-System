-- Check quotations table
SELECT 
    COUNT(*) as total_quotations,
    MIN("CreatedAt") as earliest_created,
    MAX("CreatedAt") as latest_created
FROM "Quotations";

-- Show sample quotations
SELECT 
    "QuotationId",
    "QuotationNumber", 
    "Status",
    "ClientId",
    "CreatedByUserId",
    "CreatedAt",
    "TenantId"
FROM "Quotations" 
ORDER BY "CreatedAt" DESC 
LIMIT 5;

-- Check if user has proper tenant access
SELECT DISTINCT "TenantId", COUNT(*) as count
FROM "Quotations" 
GROUP BY "TenantId";
