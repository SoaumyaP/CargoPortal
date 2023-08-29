DECLARE @dbName nvarchar(250);
SET @dbName = 'QC-SPDatabase-QC'

DECLARE @catalog nvarchar(250);

DECLARE @schema nvarchar(250);

DECLARE @tbl nvarchar(250);

DECLARE i
CURSOR LOCAL FAST_FORWARD
FOR
SELECT TABLE_CATALOG,
       TABLE_SCHEMA,
       TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_CATALOG = @dbName
  AND TABLE_NAME != 'sysdiagrams'
  AND TABLE_NAME != '__EFMigrationsHistory'
  AND TABLE_NAME != 'Translations'
  AND TABLE_NAME != 'Roles'
  AND TABLE_NAME != 'Permissions' 

OPEN i;

FETCH NEXT
FROM i INTO @catalog,
            @schema,
            @tbl;

WHILE @@FETCH_STATUS = 0 
BEGIN 

DECLARE @sql NVARCHAR(MAX)

SET @sql = N'DISABLE TRIGGER ALL ON [' + @catalog + '].[' + @schema + '].[' + @tbl + '];'
PRINT 'Executing statement: ' + @sql 
EXECUTE sp_executesql @sql 

SET @sql = N'ALTER TABLE  [' + @catalog + '].[' + @schema + '].[' + @tbl + '] NOCHECK CONSTRAINT ALL;'
PRINT 'Executing statement: ' + @sql 
EXECUTE sp_executesql @sql 

FETCH NEXT
FROM i INTO @catalog,
            @schema,
            @tbl;

END CLOSE i;

DEALLOCATE i;