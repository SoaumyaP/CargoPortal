-- It is view to link Master data localy
-- On Azure SQL cloud, it is external table


CREATE OR ALTER VIEW [dbo].Carriers
AS
SELECT *
FROM CSFEDDb..Carriers
GO