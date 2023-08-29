	SET NOCOUNT ON;
	
	BEGIN TRANSACTION

	DECLARE @OriginCargoDetailsTbl TABLE (
		Id BIGINT NOT NULL,
		OrderType INT,
		ShipmentId BIGINT,
		ProductNumber NVARCHAR(128)
	)

	INSERT INTO @OriginCargoDetailsTbl
	SELECT Id, OrderType, ShipmentId, ProductNumber
	FROM CargoDetails
	WHERE OrderType = 1 AND (OrderId IS NULL OR OrderId = '')
	ORDER BY Id
	
	SELECT * FROM CargoDetails WHERE OrderType = 1

	DECLARE @CargoDetailsTbl TABLE (
		Id BIGINT NOT NULL,
		OrderId BIGINT NULL,
		ItemId BIGINT NULL
	)
	-- Get information for Freight purchase orders
	INSERT INTO @CargoDetailsTbl (Id, OrderId)
	SELECT	INS.Id, -- Cargo details id
			T1.Id -- OrderId
	FROM @OriginCargoDetailsTbl INS
	OUTER APPLY
	(	
		SELECT TOP(1) Id, PO.PONumber
		FROM PurchaseOrders PO
		WHERE PO.PONumber = REVERSE(PARSENAME(REPLACE(REVERSE(INS.ProductNumber), '~', '.'), 1))
	) T1
	-- For Freight purchase orders
	WHERE INS.OrderType = 1	

	UPDATE CargoDetails
	SET OrderId = CDT.OrderId,
		ItemId = CDT.ItemId
	FROM CargoDetails CD
	INNER JOIN @CargoDetailsTbl CDT ON CD.Id = CDT.Id

	SELECT * 
	FROM CargoDetails 
	WHERE OrderType = 1
	ORDER BY Id


	COMMIT TRANSACTION
