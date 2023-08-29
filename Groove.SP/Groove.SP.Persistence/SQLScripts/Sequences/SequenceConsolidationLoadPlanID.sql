-- =============================================
-- Author:		Hau Nguyen
-- Created date: 3 September 2020
-- Description:	#PSP-1320 - Add New Consolidation
-- =============================================

IF NOT EXISTS
(
	SELECT [name]
	FROM sys.sequences
	WHERE [name] = 'SequenceConsolidationLoadPlanID'
)
BEGIN
	CREATE SEQUENCE [DBO].SequenceConsolidationLoadPlanID AS INT 
	START WITH 1
	INCREMENT BY 1
END
GO