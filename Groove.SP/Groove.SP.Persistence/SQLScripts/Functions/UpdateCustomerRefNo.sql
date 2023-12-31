
DECLARE @PONumbers varchar(3000)
DECLARE @NewPONumbers varchar(3000)
DECLARE @PONumberEachRow nvarchar(3000)
DECLARE @index int
DECLARE @count int
DECLARE @countDistinct int
DECLARE @shipmentId bigint

SET @NewPONumbers = ''
SET @index = 0
SET @count = 0
SET @countDistinct = 0

DECLARE MY_CURSOR CURSOR 
  LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR 
SELECT Id, CustomerReferenceNo
FROM Shipments

-- Loop table
OPEN MY_CURSOR
FETCH NEXT FROM MY_CURSOR INTO @shipmentId, @PONumbers
WHILE @@FETCH_STATUS = 0
BEGIN 
	-- If CustomerReferenceNo have ","
	IF (CHARINDEX(',', @PONumbers) > 0)
	BEGIN
		select @count = count(*) from string_split(@PONumbers, ',')
		select @countDistinct = count(*) from (select distinct * from string_split(@PONumbers, ',')) as T

		-- If duplicate
		IF (@count <> @countDistinct)
		BEGIN
			print '-----------------'
			PRINT CONCAT('ShipmentId: ', @shipmentId)
			PRINT CONCAT('Old PONumbers: ', @PONumbers)
			DECLARE MY_OTHER_CURSOR CURSOR 
			  LOCAL STATIC READ_ONLY FORWARD_ONLY
			FOR 
			SELECT distinct *
			FROM string_split(@PONumbers, ',')
			
			-- Loop substring in CustomerReferenceNo
			OPEN MY_OTHER_CURSOR
			FETCH NEXT FROM MY_OTHER_CURSOR INTO @PONumberEachRow
			WHILE @@FETCH_STATUS = 0
			BEGIN 
				SET @NewPONumbers = CONCAT(',', @PONumberEachRow)
				FETCH NEXT FROM MY_OTHER_CURSOR INTO @PONumberEachRow
			END
			CLOSE MY_OTHER_CURSOR
			DEALLOCATE MY_OTHER_CURSOR
			-- END Loop substring in CustomerReferenceNo

			-- Update script, please make sure it correct before uncomment and run it
			--Update Shipments SET CustomerReferenceNo = @NewPONumbers WHERE Id = @shipmentId

			SET @NewPONumbers = SUBSTRING(@NewPONumbers, 2, LEN(@NewPONumbers))
			PRINT CONCAT('New PONumbers: ', @NewPONumbers)
			SET @NewPONumbers = ''

		END
	END
    FETCH NEXT FROM MY_CURSOR INTO @shipmentId, @PONumbers
END
CLOSE MY_CURSOR
DEALLOCATE MY_CURSOR
-- End Loop table
