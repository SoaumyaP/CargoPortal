SET NOCOUNT ON;
	
DECLARE 
	@organizationId BIGINT,
	@sequenceName NVARCHAR(MAX),
	@currentSequenceValue SMALLINT

-- set current poff sequence value
SELECT 
	@currentSequenceValue = cast(current_value AS NUMERIC)
FROM sys.sequences
WHERE [name] = 'SequencePOFFNumber'

DECLARE cursorOrganization CURSOR FOR 
	SELECT	ORG.Id
				
	FROM Organizations ORG
	WHERE 
		ORG.OrganizationType = 4 AND -- Principal
		ORG.IsBuyer = 1 -- has buyer compliance

OPEN cursorOrganization
	FETCH NEXT FROM cursorOrganization
		INTO	
			@organizationId

WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @sequenceName = CONCAT('ORG#', @organizationId, '_SequencePOFFNumber') -- sequence name by org id
		IF NOT EXISTS
		(
			SELECT [name]
			FROM sys.sequences
			WHERE [name] = @sequenceName
		)
		BEGIN

		EXEC ('CREATE SEQUENCE [dbo].' + @sequenceName + ' AS SMALLINT 
				START WITH ' + @currentSequenceValue + '
				INCREMENT BY 1
				
				SELECT NEXT VALUE FOR [dbo].' + @sequenceName)
		END

	FETCH NEXT FROM cursorOrganization
		INTO
			@organizationId
	END
CLOSE cursorOrganization
DEALLOCATE cursorOrganization
GO