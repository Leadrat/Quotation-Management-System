-- Script to fix the Clients table foreign key constraint
-- This checks and fixes the FK_Clients_Users_CreatedByUserId constraint

-- First, check the current foreign key constraint
SELECT 
    tc.constraint_name, 
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM 
    information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
      AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
      ON ccu.constraint_name = tc.constraint_name
      AND ccu.table_schema = tc.table_schema
WHERE 
    tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name = 'Clients'
    AND tc.constraint_name = 'FK_Clients_Users_CreatedByUserId';

-- Drop the incorrect foreign key if it exists and points to wrong column
DO $$
BEGIN
    -- Drop the constraint if it exists
    IF EXISTS (
        SELECT 1 
        FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Clients_Users_CreatedByUserId' 
        AND table_name = 'Clients'
    ) THEN
        ALTER TABLE "Clients" DROP CONSTRAINT IF EXISTS "FK_Clients_Users_CreatedByUserId";
        RAISE NOTICE 'Dropped existing FK_Clients_Users_CreatedByUserId constraint';
    END IF;
END $$;

-- Recreate the foreign key with the correct column reference
ALTER TABLE "Clients"
ADD CONSTRAINT "FK_Clients_Users_CreatedByUserId" 
FOREIGN KEY ("CreatedByUserId") 
REFERENCES "Users" ("UserId") 
ON DELETE RESTRICT;

-- Verify the constraint was created correctly
SELECT 
    tc.constraint_name, 
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM 
    information_schema.table_constraints AS tc 
    JOIN information_schema.key_column_usage AS kcu
      ON tc.constraint_name = kcu.constraint_name
      AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage AS ccu
      ON ccu.constraint_name = tc.constraint_name
      AND ccu.table_schema = tc.table_schema
WHERE 
    tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name = 'Clients'
    AND tc.constraint_name = 'FK_Clients_Users_CreatedByUserId';

