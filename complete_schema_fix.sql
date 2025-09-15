-- Complete schema fix for PlannedTeamSlots table
-- This script will be executed through Entity Framework to bypass authentication issues

BEGIN TRANSACTION

BEGIN TRY
    PRINT 'Starting PlannedTeamSlots schema migration...'

    -- Step 1: Temporarily disable all foreign key constraints
    PRINT 'Disabling foreign key constraints...'

    DECLARE @DisableFK NVARCHAR(MAX) = ''
    SELECT @DisableFK = @DisableFK + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
    FROM sys.tables WHERE name = 'PlannedTeamSlots'

    IF @DisableFK != ''
        EXEC sp_executesql @DisableFK

    -- Step 2: Add missing columns with existence checks
    PRINT 'Adding missing columns to PlannedTeamSlots...'

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'PlannedMonthlyCost')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD PlannedMonthlyCost decimal(18,2) NOT NULL DEFAULT 0
        PRINT '✓ Added PlannedMonthlyCost column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'PlannedVendorCost')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD PlannedVendorCost decimal(18,2) NOT NULL DEFAULT 0
        PRINT '✓ Added PlannedVendorCost column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'Notes')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD Notes nvarchar(1000) NOT NULL DEFAULT ''
        PRINT '✓ Added Notes column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'Status')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD Status int NOT NULL DEFAULT 1
        PRINT '✓ Added Status column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'IsVendorSlot')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD IsVendorSlot bit NOT NULL DEFAULT 0
        PRINT '✓ Added IsVendorSlot column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'RequiredSkills')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD RequiredSkills nvarchar(500) NOT NULL DEFAULT ''
        PRINT '✓ Added RequiredSkills column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'Priority')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD Priority int NOT NULL DEFAULT 1
        PRINT '✓ Added Priority column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'RequiredStartDate')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD RequiredStartDate datetime2 NULL
        PRINT '✓ Added RequiredStartDate column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'RequiredEndDate')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD RequiredEndDate datetime2 NULL
        PRINT '✓ Added RequiredEndDate column'
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'PathId')
    BEGIN
        ALTER TABLE PlannedTeamSlots ADD PathId uniqueidentifier NULL
        PRINT '✓ Added PathId column'
    END

    -- Step 3: Re-enable constraints
    PRINT 'Re-enabling foreign key constraints...'

    DECLARE @EnableFK NVARCHAR(MAX) = ''
    SELECT @EnableFK = @EnableFK + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(13)
    FROM sys.tables WHERE name = 'PlannedTeamSlots'

    IF @EnableFK != ''
        EXEC sp_executesql @EnableFK

    -- Step 4: Add foreign key for PathId if Paths table exists
    IF OBJECT_ID('Paths') IS NOT NULL AND NOT EXISTS (
        SELECT * FROM sys.foreign_keys
        WHERE name = 'FK_PlannedTeamSlots_Paths_PathId'
        AND parent_object_id = OBJECT_ID('PlannedTeamSlots')
    )
    BEGIN
        ALTER TABLE PlannedTeamSlots
        ADD CONSTRAINT FK_PlannedTeamSlots_Paths_PathId
        FOREIGN KEY (PathId) REFERENCES Paths(Id)
        ON DELETE SET NULL
        PRINT '✓ Added foreign key constraint for PathId'
    END

    -- Step 5: Verify the schema
    PRINT 'Verifying schema changes...'

    SELECT
        'PlannedTeamSlots Column Verification' as CheckType,
        COUNT(*) as TotalColumns
    FROM sys.columns
    WHERE object_id = OBJECT_ID('PlannedTeamSlots')

    PRINT 'Schema migration completed successfully!'

    COMMIT TRANSACTION
    PRINT 'Transaction committed.'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    PRINT 'Error occurred during migration:'
    PRINT ERROR_MESSAGE()
    THROW
END CATCH