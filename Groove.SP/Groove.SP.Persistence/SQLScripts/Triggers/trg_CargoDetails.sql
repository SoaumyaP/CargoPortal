SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_CargoDetails' AND [type] = 'TR')
BEGIN
	DROP TRIGGER [dbo].trg_CargoDetails;
END
GO

-- There are 2 trigger on table CargoDetails


IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_CargoDetails_AfterInsert' AND [type] = 'TR')
BEGIN
	DROP TRIGGER [dbo].trg_CargoDetails_AfterInsert;
END
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 2020 Dec 22
-- To fulfill OrderId/ItemId for Freight/Cruise
-- To fulfill shipment id to cruise order item
-- Notes: NOT link shipment to poff allocated orders as business not run yet
-- =============================================
CREATE TRIGGER [dbo].trg_CargoDetails_AfterInsert ON [dbo].[CargoDetails]
AFTER INSERT
AS
BEGIN

	SET NOCOUNT ON;
	
	DECLARE @CargoDetailsTbl TABLE (
		Id BIGINT NOT NULL,
		OrderId BIGINT NULL,
		ItemId BIGINT NULL
	)

	DECLARE @CruiseOrderItemLinkingCargoDetailTbl TABLE (
		ItemId BIGINT NOT NULL,
		OrderId BIGINT NOT NULL,
		POLine INT
	)

	DECLARE @FreightCargoDetailsLinkingTbl TABLE (
		OrderId BIGINT NULL,
		ItemId BIGINT NULL
	)
	-- Get information for PurchaseOrder
	DECLARE 
		@freightCargoDetailId BIGINT,
		@freightShipmentId BIGINT,
		@poNumber NVARCHAR(512),
		@lineOrder BIGINT,
		@scheduleLineNo BIGINT

	DECLARE cursorFreightCargoDetail CURSOR FOR 
		SELECT	INS.Id,
				INS.ShipmentId,
				REVERSE(PARSENAME(REPLACE(REVERSE(ProductNumber), '~', '.'), 1)),
				REVERSE(PARSENAME(REPLACE(REVERSE(ProductNumber), '~', '.'), 3)),
				REVERSE(PARSENAME(REPLACE(REVERSE(ProductNumber), '~', '.'), 4))
				
		FROM inserted INS 
		WHERE 
			INS.OrderType = 1
			AND INS.ProductNumber IS NOT NULL 
			AND INS.ProductNumber != ''

	OPEN cursorFreightCargoDetail
		FETCH NEXT FROM cursorFreightCargoDetail
			INTO
				@freightCargoDetailId,
				@freightShipmentId,
				@poNumber,
				@lineOrder,
				@scheduleLineNo

	WHILE @@FETCH_STATUS = 0
		BEGIN
			-- Must clean up on every cursor data fetching
			DELETE FROM @FreightCargoDetailsLinkingTbl

			-- Same columns on table and stored procedure
			INSERT INTO @FreightCargoDetailsLinkingTbl
			EXEC [dbo].[spu_GetPurchaseOrdersLinkingCargoDetail] @freightShipmentId, @poNumber, @lineOrder, @scheduleLineNo

			IF EXISTS (SELECT * FROM @FreightCargoDetailsLinkingTbl)
				BEGIN
					INSERT INTO @CargoDetailsTbl (Id, OrderId, ItemId)
					SELECT TOP(1) @freightCargoDetailId, tbl.OrderId, tbl.ItemId
					FROM @FreightCargoDetailsLinkingTbl tbl
				END
			ELSE 
				BEGIN
					INSERT INTO @CargoDetailsTbl (Id)
					SELECT @freightCargoDetailId
				END

		UPDATE CargoDetails
			SET OrderId = CDT.OrderId,
			ItemId = CDT.ItemId
			FROM CargoDetails CD
			INNER JOIN @CargoDetailsTbl CDT ON CD.Id = CDT.Id

		FETCH NEXT FROM cursorFreightCargoDetail
			INTO 
				@freightCargoDetailId,
				@freightShipmentId,
				@poNumber,
				@lineOrder,
				@scheduleLineNo
		END
	CLOSE cursorFreightCargoDetail
	DEALLOCATE cursorFreightCargoDetail 
	

	-- Get information for Cruise orders
	DECLARE 
			@cargoDetailId BIGINT,
			@shipmentId BIGINT,
			@cruiseOrderNumber NVARCHAR(512),
			@cruiseOrderItemPOLine INT,
			@cruiseOrderItemId NVARCHAR(100)


	-- Declare cursor to handle row by row for Cruise orders
	DECLARE cursorCargoDetail CURSOR FOR 
	SELECT	INS.Id,
			INS.ShipmentId,
			REVERSE(PARSENAME(REPLACE(REVERSE(ProductNumber), '~', '.'), 1)),
			REVERSE(PARSENAME(REPLACE(REVERSE(ProductNumber), '~', '.'), 2)),
			REVERSE(PARSENAME(REPLACE(REVERSE(ProductNumber), '~', '.'), 3))
	FROM inserted INS 
	WHERE INS.OrderType = 2
			-- If available Product number
			AND INS.ProductNumber IS NOT NULL AND INS.ProductNumber != ''

	OPEN cursorCargoDetail

	FETCH NEXT FROM cursorCargoDetail
		  INTO	@cargoDetailId,
				@shipmentId,
				@cruiseOrderNumber,
				@cruiseOrderItemId,
				@cruiseOrderItemPOLine

	WHILE @@FETCH_STATUS = 0 
	BEGIN

		-- Must clean up on every cursor data fetching
		DELETE FROM @CruiseOrderItemLinkingCargoDetailTbl

		-- Same columns on table and stored procedure
		INSERT INTO @CruiseOrderItemLinkingCargoDetailTbl
		-- Return data
		--SELECT COI.Id AS [Id], COI.OrderId AS [OrderId], COI.POLine AS [POLine]
		EXEC [dbo].[spu_GetCruiseOrderItemLinkingCargoDetail] @shipmentId, @cruiseOrderNumber, @cruiseOrderItemPOLine, @cruiseOrderItemId

		IF EXISTS (SELECT * FROM @CruiseOrderItemLinkingCargoDetailTbl)
		BEGIN
			-- Insert data to fulfill cargo detail
			-- Get only the first record
			INSERT INTO @CargoDetailsTbl (Id, OrderId, ItemId)
			SELECT TOP(1) @cargoDetailId, TMP.OrderId, TMP.ItemId
			FROM @CruiseOrderItemLinkingCargoDetailTbl TMP	

			-- Fulfill shipment id to cruise order item
			UPDATE cruise.CruiseOrderItems
			SET ShipmentId = @shipmentId,
				-- To don't create log for changes (trg_CruiseOrderItems_AfterUpdate Section #01)
				ItemUpdates = NULL
			FROM cruise.CruiseOrderItems COI
			WHERE COI.Id = (
				-- The first cruise order item matched
				SELECT TOP(1) COI.Id
				FROM cruise.CruiseOrderItems COI
				INNER JOIN (SELECT TOP(1) * FROM @CruiseOrderItemLinkingCargoDetailTbl) TBL ON TBL.ItemId = COI.Id
			)
		END
		ELSE			
		BEGIN
			INSERT INTO @CargoDetailsTbl (Id)
			SELECT @cargoDetailId
		END				

		FETCH NEXT FROM cursorCargoDetail
			   INTO	@cargoDetailId,	
					@shipmentId,
					@cruiseOrderNumber,
					@cruiseOrderItemId,
					@cruiseOrderItemPOLine
	END

	CLOSE cursorCargoDetail
	DEALLOCATE cursorCargoDetail 

	UPDATE CargoDetails
	SET OrderId = CDT.OrderId,
		ItemId = CDT.ItemId
	FROM CargoDetails CD
	INNER JOIN @CargoDetailsTbl CDT ON CD.Id = CDT.Id
	
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_CargoDetails_AfterUpdate' AND [type] = 'TR')
BEGIN
	DROP TRIGGER [dbo].trg_CargoDetails_AfterUpdate;
END
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 2020 Dec 22
-- Description:	Trigger on [dbo].[CargoDetails]
-- To fulfill OrderId/ItemId for Freight/Cruise ONLY AS updating ProductNumber
-- Notes: NOT link shipment to poff allocated orders as business not run yet
-- =============================================
CREATE TRIGGER [dbo].trg_CargoDetails_AfterUpdate ON [dbo].[CargoDetails]
AFTER UPDATE
AS
BEGIN

	SET NOCOUNT ON;
	
	DECLARE @CargoDetailsTbl TABLE (
		Id BIGINT NOT NULL,
		OrderId BIGINT NULL,
		ItemId BIGINT NULL
	)

	DECLARE @FreightCargoDetailsLinkingTbl TABLE (
		OrderId BIGINT NULL,
		ItemId BIGINT NULL
	)

	DECLARE @CruiseOrderItemLinkingCargoDetailTbl TABLE (
		ItemId BIGINT NOT NULL,
		OrderId BIGINT NOT NULL,
		POLine INT
	)

	-- Get information for PurchaseOrder
	DECLARE 
		@freightCargoDetailId BIGINT,
		@freightShipmentId BIGINT,
		@poNumber NVARCHAR(512),
		@lineOrder BIGINT,
		@scheduleLineNo BIGINT

	DECLARE cursorUpdateFreightCargoDetail CURSOR FOR 
		SELECT	INS.Id,
				INS.ShipmentId,
				REVERSE(PARSENAME(REPLACE(REVERSE(INS.ProductNumber), '~', '.'), 1)),
				REVERSE(PARSENAME(REPLACE(REVERSE(INS.ProductNumber), '~', '.'), 3)),
				REVERSE(PARSENAME(REPLACE(REVERSE(INS.ProductNumber), '~', '.'), 4))
		FROM inserted INS
		INNER JOIN deleted DEL ON DEL.Id = INS.Id 
							-- If updating Product number
							AND INS.ProductNumber != DEL.ProductNumber 
							-- If available Product number
							AND INS.ProductNumber IS NOT NULL AND INS.ProductNumber != ''
							-- If CargoDetail is Freight
							AND INS.OrderType = 1 

	OPEN cursorUpdateFreightCargoDetail
		FETCH NEXT FROM cursorUpdateFreightCargoDetail
			INTO	
				@freightCargoDetailId,
				@freightShipmentId,
				@poNumber,
				@lineOrder,
				@scheduleLineNo

	WHILE @@FETCH_STATUS = 0
		BEGIN
			-- Must clean up on every cursor data fetching
			DELETE FROM @FreightCargoDetailsLinkingTbl

			-- Same columns on table and stored procedure
			INSERT INTO @FreightCargoDetailsLinkingTbl 
			EXEC [dbo].[spu_GetPurchaseOrdersLinkingCargoDetail] @freightShipmentId, @poNumber, @lineOrder, @scheduleLineNo

			IF EXISTS (SELECT * FROM @FreightCargoDetailsLinkingTbl)
				BEGIN
					INSERT INTO @CargoDetailsTbl (Id, OrderId, ItemId)
					SELECT TOP(1) @freightCargoDetailId, tbl.OrderId, tbl.ItemId
					FROM @FreightCargoDetailsLinkingTbl tbl
				END
			ELSE 
				BEGIN
					INSERT INTO @CargoDetailsTbl (Id)
					SELECT @freightCargoDetailId
				END

			UPDATE CargoDetails
				SET OrderId = CDT.OrderId,
					ItemId = CDT.ItemId
				FROM CargoDetails CD
				INNER JOIN @CargoDetailsTbl CDT ON CD.Id = CDT.Id
				
		FETCH NEXT FROM cursorUpdateFreightCargoDetail
			INTO 
				@freightCargoDetailId,
				@freightShipmentId,
				@poNumber,
				@lineOrder,
				@scheduleLineNo

		END
	CLOSE cursorUpdateFreightCargoDetail
	DEALLOCATE cursorUpdateFreightCargoDetail

	-- Get information for Cruise orders
	DECLARE 
			@cargoDetailId BIGINT,
			@shipmentId BIGINT,
			@cruiseOrderNumber NVARCHAR(512),
			@cruiseOrderItemPOLine INT,
			@cruiseOrderItemId NVARCHAR(100)


	-- Declare cursor to handle row by row for Cruise orders
	DECLARE cursorCargoDetail CURSOR FOR 
	SELECT	INS.Id,
			INS.ShipmentId,
			REVERSE(PARSENAME(REPLACE(REVERSE(INS.ProductNumber), '~', '.'), 1)),
			REVERSE(PARSENAME(REPLACE(REVERSE(INS.ProductNumber), '~', '.'), 2)),
			REVERSE(PARSENAME(REPLACE(REVERSE(INS.ProductNumber), '~', '.'), 3))
	FROM inserted INS 
	INNER JOIN deleted DEL ON DEL.Id = INS.Id 
							-- If updating Product number
							AND INS.ProductNumber != DEL.ProductNumber 
							-- If available Product number
							AND INS.ProductNumber IS NOT NULL AND INS.ProductNumber != ''
	WHERE INS.OrderType = 2

	OPEN cursorCargoDetail

	FETCH NEXT FROM cursorCargoDetail
		  INTO	@cargoDetailId,
				@shipmentId,
				@cruiseOrderNumber,
				@cruiseOrderItemId,
				@cruiseOrderItemPOLine

	WHILE @@FETCH_STATUS = 0 
	BEGIN
		-- Must clean up on every cursor data fetching
		DELETE FROM @CruiseOrderItemLinkingCargoDetailTbl

		-- Same columns on table and stored procedure
		INSERT INTO @CruiseOrderItemLinkingCargoDetailTbl
		-- Return data
		--SELECT COI.Id AS [Id], COI.OrderId AS [OrderId], COI.POLine AS [POLine]
		EXEC [dbo].[spu_GetCruiseOrderItemLinkingCargoDetail] @shipmentId, @cruiseOrderNumber, @cruiseOrderItemPOLine, @cruiseOrderItemId

		IF EXISTS (SELECT * FROM @CruiseOrderItemLinkingCargoDetailTbl)
		BEGIN
			-- Insert data to fulfill cargo detail
			-- Get only the first record
			INSERT INTO @CargoDetailsTbl (Id, OrderId, ItemId)
			SELECT TOP(1) @cargoDetailId, TMP.OrderId, TMP.ItemId
			FROM @CruiseOrderItemLinkingCargoDetailTbl TMP	

			--NOTE: FULFILL SHIPMENT ID TO CRUISE ORDER ITEM ONLY ON INSERT MODE
		END
		ELSE			
		BEGIN
			INSERT INTO @CargoDetailsTbl (Id)
			SELECT @cargoDetailId
		END				

		FETCH NEXT FROM cursorCargoDetail
			   INTO	@cargoDetailId,	
					@shipmentId,
					@cruiseOrderNumber,
					@cruiseOrderItemId,
					@cruiseOrderItemPOLine
	END

	CLOSE cursorCargoDetail
	DEALLOCATE cursorCargoDetail 

	UPDATE CargoDetails
	SET OrderId = CDT.OrderId,
		ItemId = CDT.ItemId
	FROM CargoDetails CD
	INNER JOIN @CargoDetailsTbl CDT ON CD.Id = CDT.Id
	
END