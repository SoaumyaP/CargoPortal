IF NOT EXISTS
(
	SELECT [name]
	FROM sys.sequences
	WHERE [name] = 'SequenceOrganizationNumber'
)
BEGIN
	CREATE SEQUENCE [DBO].SequenceOrganizationNumber AS INT
	START WITH 1
	INCREMENT BY 1
END
GO
