
DECLARE @MyCursor CURSOR;
DECLARE @ActivityCode nvarchar(100);
DECLARE @Index BIGINT = 0 ;

BEGIN
    SET @MyCursor = CURSOR FOR
    SELECT ActivityCode 
	FROM EventCodes
    ORDER BY SortSequence

    OPEN @MyCursor 
    FETCH NEXT FROM @MyCursor 
    INTO @ActivityCode

	BEGIN TRANSACTION

    WHILE @@FETCH_STATUS = 0
    BEGIN
	  SET @Index = @Index + 1;

	  --PRINT(CONCAT(@ActivityCode,' - ',@Index))

	  UPDATE EventCodes
	  SET SortSequence = @Index
	  WHERE ActivityCode = @ActivityCode

      FETCH NEXT FROM @MyCursor 
      INTO @ActivityCode 
    END; 

    CLOSE @MyCursor ;
    DEALLOCATE @MyCursor;

	COMMIT TRANSACTION
END;
GO
