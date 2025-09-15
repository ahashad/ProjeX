-- Script to add missing columns to PlannedTeamSlots table
-- This addresses the database schema mismatch identified earlier

PRINT 'Starting to add missing columns to PlannedTeamSlots table...'

-- Check if columns exist before adding them to avoid errors
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'PlannedMonthlyCost')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD PlannedMonthlyCost decimal(18,2) NOT NULL DEFAULT 0
    PRINT 'Added PlannedMonthlyCost column'
END
ELSE
    PRINT 'PlannedMonthlyCost column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'PlannedVendorCost')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD PlannedVendorCost decimal(18,2) NOT NULL DEFAULT 0
    PRINT 'Added PlannedVendorCost column'
END
ELSE
    PRINT 'PlannedVendorCost column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'Notes')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD Notes nvarchar(1000) NOT NULL DEFAULT ''
    PRINT 'Added Notes column'
END
ELSE
    PRINT 'Notes column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'Status')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD Status int NOT NULL DEFAULT 1
    PRINT 'Added Status column'
END
ELSE
    PRINT 'Status column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'IsVendorSlot')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD IsVendorSlot bit NOT NULL DEFAULT 0
    PRINT 'Added IsVendorSlot column'
END
ELSE
    PRINT 'IsVendorSlot column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'RequiredSkills')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD RequiredSkills nvarchar(500) NOT NULL DEFAULT ''
    PRINT 'Added RequiredSkills column'
END
ELSE
    PRINT 'RequiredSkills column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'Priority')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD Priority int NOT NULL DEFAULT 1
    PRINT 'Added Priority column'
END
ELSE
    PRINT 'Priority column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'RequiredStartDate')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD RequiredStartDate datetime2 NULL
    PRINT 'Added RequiredStartDate column'
END
ELSE
    PRINT 'RequiredStartDate column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'RequiredEndDate')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD RequiredEndDate datetime2 NULL
    PRINT 'Added RequiredEndDate column'
END
ELSE
    PRINT 'RequiredEndDate column already exists'

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PlannedTeamSlots') AND name = 'PathId')
BEGIN
    ALTER TABLE PlannedTeamSlots ADD PathId uniqueidentifier NULL
    PRINT 'Added PathId column'
END
ELSE
    PRINT 'PathId column already exists'

PRINT 'Completed adding missing columns to PlannedTeamSlots table.'