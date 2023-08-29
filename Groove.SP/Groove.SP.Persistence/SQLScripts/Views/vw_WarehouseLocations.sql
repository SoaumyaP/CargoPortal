-- It is view to link Master data localy
-- On Azure SQL cloud, it is external table


CREATE OR ALTER VIEW [dbo].WarehouseLocations
AS

SELECT
   *
FROM [CSFEDatabase].[dbo].WarehouseLocations
GO