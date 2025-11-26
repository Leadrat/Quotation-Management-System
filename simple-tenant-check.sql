-- Simple tenant data check
SELECT 'Users Count:' as info, COUNT(*) as count FROM "Users";
SELECT 'Users with TenantId:' as info, COUNT(*) as count FROM "Users" WHERE "TenantId" IS NOT NULL;
SELECT 'Users TenantId values:' as info, "TenantId", COUNT(*) as count FROM "Users" GROUP BY "TenantId";

SELECT 'Clients Count:' as info, COUNT(*) as count FROM "Clients";
SELECT 'Clients with TenantId:' as info, COUNT(*) as count FROM "Clients" WHERE "TenantId" IS NOT NULL;
SELECT 'Clients TenantId values:' as info, "TenantId", COUNT(*) as count FROM "Clients" GROUP BY "TenantId";

SELECT 'Quotations Count:' as info, COUNT(*) as count FROM "Quotations";
SELECT 'Quotations with TenantId:' as info, COUNT(*) as count FROM "Quotations" WHERE "TenantId" IS NOT NULL;
SELECT 'Quotations TenantId values:' as info, "TenantId", COUNT(*) as count FROM "Quotations" GROUP BY "TenantId";

SELECT 'Payments Count:' as info, COUNT(*) as count FROM "Payments";
SELECT 'Payments with TenantId:' as info, COUNT(*) as count FROM "Payments" WHERE "TenantId" IS NOT NULL;
SELECT 'Payments TenantId values:' as info, "TenantId", COUNT(*) as count FROM "Payments" GROUP BY "TenantId";
