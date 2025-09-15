-- Script to disable all foreign key constraints
-- This will help us modify the database structure without constraint conflicts

PRINT 'Starting to disable all foreign key constraints...'

-- Disable all foreign key constraints
DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
FROM sys.tables

EXEC sp_executesql @sql

PRINT 'All foreign key constraints have been disabled.'

-- Disable all check constraints
SET @sql = ''
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + '.' + QUOTENAME(t.name) + ' NOCHECK CONSTRAINT ' + QUOTENAME(c.name) + ';' + CHAR(13)
FROM sys.check_constraints c
INNER JOIN sys.tables t ON c.parent_object_id = t.object_id

EXEC sp_executesql @sql

PRINT 'All check constraints have been disabled.'
PRINT 'Database is ready for structural changes.'