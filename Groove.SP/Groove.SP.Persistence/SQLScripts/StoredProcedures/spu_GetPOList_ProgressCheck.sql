SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetPOList_ProgressCheck', 'P') IS NOT NULL
DROP PROC dbo.spu_GetPOList_ProgressCheck
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 18 July 2021
-- Description:	Get list of purchase orders for progress check cargo ready date.
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetPOList_ProgressCheck]
	@JsonFilterSet NVARCHAR(MAX) = '',
	@Affiliates NVARCHAR(MAX) = '',
	@IsInternal BIT = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ResultTable TABLE
	(
		[Id] BIGINT,
		[PONumber] VARCHAR(512),
		[CargoReadyDate] DATETIME2(7),
		[ProductionStarted] BIT,
		[ProposeDate] DATETIME2(7),
		[QCRequired] BIT,
		[ShortShip] BIT,
		[SplitShipment] BIT,
		[Remark] NVARCHAR(MAX)
	)

 --   SET @JsonFilterSet = N'{
 --   "SelectedCustomerId": 461,
	--"SelectedSupplierId": null,
 --   "PONoFrom": null,
 --   "PONoTo": null,
 --   "CargoReadyDateFrom" : null,
 --   "CargoReadyDateTo": null
	--}';

	DECLARE @SelectedCustomerId BIGINT
	DECLARE @SelectedSupplierId BIGINT
	DECLARE @PONoFrom VARCHAR (512) 
	DECLARE @PONoTo VARCHAR (512)
	DECLARE @CargoReadyDateFrom DATETIME2
	DECLARE @CargoReadyDateTo DATETIME2

	-- PO Type allowed in buyer compliance
	DECLARE @SelectedPO_POType INT

	SELECT	@SelectedCustomerId = SelectedCustomerId,
			@SelectedSupplierId = SelectedSupplierId,
			@PONoFrom = PONoFrom,
			@PONoTo = PONoTo,
			@CargoReadyDateFrom = CargoReadyDateFrom,
			@CargoReadyDateTo = CargoReadyDateTo

	FROM OPENJSON(@JsonFilterSet)
	WITH (
		[SelectedCustomerId] BIGINT '$.SelectedCustomerId',
		[SelectedSupplierId] BIGINT '$.SelectedSupplierId',
		[PONoFrom] [VARCHAR] (512) '$.PONoFrom',
		[PONoTo] [VARCHAR] (512) '$.PONoTo',
		[CargoReadyDateFrom] [DATETIME2] '$.CargoReadyDateFrom',
		[CargoReadyDateTo] [DATETIME2] '$.CargoReadyDateTo'
	);

	;WITH TmpCTE AS 
	(
		SELECT 
			PO.*,
			T1.OrganizationId AS CustomerOrgId,
			T2.OrganizationId AS SupplierOrgId
		FROM PurchaseOrders PO (NOLOCK)

		CROSS APPLY
		(
			SELECT POC.*, BC.ProgressNotifyDay
			FROM PurchaseOrderContacts POC (NOLOCK) JOIN BuyerCompliances BC (NOLOCK) ON BC.OrganizationId = POC.OrganizationId AND BC.[Status] = 1
			WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationRole = 'Principal' AND BC.IsProgressCargoReadyDate = 1
		) T1

		CROSS APPLY
		(
			SELECT *
			FROM PurchaseOrderContacts POC (NOLOCK)
			WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationRole = 'Supplier'
		) T2

		WHERE (@IsInternal = 1 OR @IsInternal IS NULL OR (@IsInternal = 0 AND EXISTS (
			SELECT 1
			FROM PurchaseOrderContacts POC (NOLOCK)
			WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationId IN (SELECT [VALUE] FROM dbo.fn_SplitStringToTable(@Affiliates, ','))
		)))
		AND
		PO.ProductionStarted = 0
		AND
		PO.Stage < 30 --Stage < Forwarder Booking Request 
		AND
		PO.CargoReadyDate IS NOT NULL
		AND
		CAST(PO.CargoReadyDate AS DATE) <= CAST(DATEADD(DAY, T1.ProgressNotifyDay, GETDATE()) AS DATE)
	)

	INSERT INTO @ResultTable
	SELECT
		PO.Id,
		PO.PONumber,
		PO.CargoReadyDate,
		PO.ProductionStarted,
		PO.ProposeDate,
		PO.QCRequired,
		PO.ShortShip,
		PO.SplitShipment,
		PO.Remark
	FROM TmpCTE PO
	WHERE
	-- Purchase order status is active
	PO.[Status] = 1

	-- Purchase order number
	AND (@PONoFrom IS NULL OR PO.PONumber >= @PONoFrom) AND (@PONoTo IS NULL OR PO.PONumber <= @PONoTo)

	-- ETD
	AND (@CargoReadyDateFrom IS NULL OR PO.CargoReadyDate >= @CargoReadyDateFrom) AND (@CargoReadyDateTo IS NULL OR PO.CargoReadyDate <= @CargoReadyDateTo)

	AND (@SelectedCustomerId IS NULL OR PO.CustomerOrgId = @SelectedCustomerId)

	AND (@SelectedSupplierId IS NULL OR PO.SupplierOrgId = @SelectedSupplierId)

	--Return
	SELECT * FROM @ResultTable
END
GO

