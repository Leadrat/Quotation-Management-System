-- Check what tenant IDs actually exist in the database
SELECT 'Tenants table:' as info;
SELECT TenantId, Identifier FROM Tenants LIMIT 5;

-- Check what tenant IDs are being used in Payments
SELECT 'Payments table tenant IDs:' as info;
SELECT DISTINCT TenantId, COUNT(*) as count FROM Payments GROUP BY TenantId;

-- Check what tenant IDs are being used in other tables
SELECT 'Quotations table tenant IDs:' as info;
SELECT DISTINCT TenantId, COUNT(*) as count FROM Quotations GROUP BY TenantId;

SELECT 'Users table tenant IDs:' as info;
SELECT DISTINCT TenantId, COUNT(*) as count FROM Users GROUP BY TenantId;

SELECT 'Clients table tenant IDs:' as info;
SELECT DISTINCT TenantId, COUNT(*) as count FROM Clients GROUP BY TenantId;
