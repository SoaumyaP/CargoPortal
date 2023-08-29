-- =============================================
-- Author:		Phuoc Le
-- Created date: 9 April 2020
-- Description:	#PSP-646 - Create sequence for POFF Number format
-- =============================================

IF NOT EXISTS
(
	SELECT [name]
	FROM sys.sequences
	WHERE [name] = 'SequencePOFFNumber'
)
BEGIN
	CREATE SEQUENCE [DBO].SequencePOFFNumber AS SMALLINT 
	START WITH 1
	INCREMENT BY 1
END
GO
