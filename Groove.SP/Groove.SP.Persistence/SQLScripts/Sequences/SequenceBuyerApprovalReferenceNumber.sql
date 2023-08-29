-- =============================================
-- Author:		Phuoc Le
-- Created date: 9 April 2020
-- Description:	#PSP-646 - Create sequence for Buyer Approval Reference Number format
-- =============================================

IF NOT EXISTS
(
	SELECT [name]
	FROM sys.sequences
	WHERE [name] = 'SequenceBuyerApprovalReferenceNumber'
)
BEGIN
	CREATE SEQUENCE [DBO].SequenceBuyerApprovalReferenceNumber AS INT 
	START WITH 1
	INCREMENT BY 1
END
GO