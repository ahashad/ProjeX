-- Script to re-enable all foreign key and check constraints
-- This restores database integrity after structural changes

PRINT 'Starting to re-enable all constraints...'

-- Re-enable all foreign key constraints
DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(13)
FROM sys.tables

EXEC sp_executesql @sql

PRINT 'All foreign key constraints have been re-enabled.'

-- Re-enable all check constraints
SET @sql = ''
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + '.' + QUOTENAME(t.name) + ' WITH CHECK CHECK CONSTRAINT ' + QUOTENAME(c.name) + ';' + CHAR(13)
FROM sys.check_constraints c
INNER JOIN sys.tables t ON c.parent_object_id = t.object_id

EXEC sp_executesql @sql

PRINT 'All check constraints have been re-enabled.'

-- Add foreign key constraint for PathId if it doesn't exist
IF NOT EXISTS (
    SELECT * FROM sys.foreign_keys
    WHERE name = 'FK_PlannedTeamSlots_Paths_PathId'
    AND parent_object_id = OBJECT_ID('PlannedTeamSlots')
)
BEGIN
    -- Only add if Paths table exists
    IF OBJECT_ID('Paths') IS NOT NULL
    BEGIN
        ALTER TABLE PlannedTeamSlots
        ADD CONSTRAINT FK_PlannedTeamSlots_Paths_PathId
        FOREIGN KEY (PathId) REFERENCES Paths(Id)
        ON DELETE SET NULL
        PRINT 'Added foreign key constraint for PathId'
    END
    ELSE
        PRINT 'Paths table not found - skipping PathId foreign key constraint'
END
ELSE
    PRINT 'Foreign key constraint for PathId already exists'

-- Verify constraint status
PRINT 'Verifying constraint status...'

SELECT
    'Foreign Key Constraints' as ConstraintType,
    COUNT(*) as Total,
    SUM(CASE WHEN is_disabled = 0 THEN 1 ELSE 0 END) as Enabled,
    SUM(CASE WHEN is_disabled = 1 THEN 1 ELSE 0 END) as Disabled
FROM sys.foreign_keys

UNION ALL

SELECT
    'Check Constraints' as ConstraintType,
    COUNT(*) as Total,
    SUM(CASE WHEN is_disabled = 0 THEN 1 ELSE 0 END) as Enabled,
    SUM(CASE WHEN is_disabled = 1 THEN 1 ELSE 0 END) as Disabled
FROM sys.check_constraints

PRINT 'Database constraints have been restored successfully.'