-- Drop the foreign key constraint that's causing issues
ALTER TABLE "Payments" DROP CONSTRAINT IF EXISTS "FK_Payments_Tenants_TenantId";

-- Show result
SELECT 
    'Foreign key constraint dropped' as status,
    'Payments table should now work without tenant FK' as message;
