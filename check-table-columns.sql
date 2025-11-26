-- Check if TenantId columns exist in main tables
SELECT column_name, table_name 
FROM information_schema.columns 
WHERE column_name = 'TenantId' 
AND table_schema = 'public'
ORDER BY table_name;
