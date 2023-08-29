SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetWarehouseBooking_Confirm', 'P') IS NOT NULL
DROP PROC dbo.spu_GetWarehouseBooking_Confirm
GO

-- =============================================
-- Author:		Dong Tran
-- Create date: 2 Dec 2021
-- Description:	Get list of warehouse booking to confirm
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetWarehouseBooking_Confirm]
	@JsonFilterSet NVARCHAR(MAX) = '',
	@Affiliates NVARCHAR(MAX) = '',
	@IsInternal BIT = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @SelectedCustomerId BIGINT
	DECLARE @SelectedSupplier VARCHAR (512)
	DECLARE @BookingNoFrom VARCHAR (512) 
	DECLARE @BookingNoTo VARCHAR (512)
	DECLARE @ExpectedHubArrivalDateFrom DATETIME2
	DECLARE @ExpectedHubArrivalDateTo DATETIME2

	SELECT	@SelectedCustomerId = SelectedCustomerId,
			@SelectedSupplier = SelectedSupplier,
			@BookingNoFrom = BookingNoFrom,
			@BookingNoTo = BookingNoTo,
			@ExpectedHubArrivalDateFrom = ExpectedHubArrivalDateFrom,
			@ExpectedHubArrivalDateTo = ExpectedHubArrivalDateTo

	FROM OPENJSON(@JsonFilterSet)
	WITH (
		[SelectedCustomerId] BIGINT '$.SelectedCustomerId',
		[SelectedSupplier] [VARCHAR] (512) '$.SelectedSupplier',
		[BookingNoFrom] [VARCHAR] (512) '$.BookingNoFrom',
		[BookingNoTo] [VARCHAR] (512) '$.BookingNoTo',
		[ExpectedHubArrivalDateFrom] [DATETIME2] '$.ExpectedHubArrivalDateFrom',
		[ExpectedHubArrivalDateTo] [DATETIME2] '$.ExpectedHubArrivalDateTo'
	);

	SELECT 
		POF.Id,
		POF.Number AS BookingNumber,
		CONCAT(POF.Number,'01') AS ShipmentNumber,
		POF.ExpectedDeliveryDate,
		T1.OrganizationId AS CustomerOrgId,
		T2.OrganizationId AS SupplierOrgId,
		T4.Name AS WarehouseLocation
	FROM POFulfillments POF(NOLOCK)
	CROSS APPLY 
		(
			SELECT PC.*
			FROM POFulfillmentContacts PC (NOLOCK)
			INNER JOIN BuyerCompliances BC (NOLOCK) ON BC.OrganizationId = PC.OrganizationId AND BC.Status = 1 AND BC.ServiceType >= 20
			WHERE 
				PC.POFulfillmentId = POF.Id 
				AND PC.OrganizationRole = 'Principal'
				AND (@SelectedCustomerId IS NULL OR PC.OrganizationId = @SelectedCustomerId)
		)T1

	CROSS APPLY
		(
			SELECT PC.OrganizationId
			FROM POFulfillmentContacts PC (NOLOCK)
			WHERE 
				POF.Id = PC.POFulfillmentId 
				AND PC.OrganizationRole = 'Supplier'
				AND (@SelectedSupplier IS NULL OR PC.CompanyName = @SelectedSupplier)
		) T2

	OUTER APPLY
		(
			SELECT TOP 1 WA.OrganizationId,WA.WarehouseLocationId
			FROM WarehouseAssignments WA
			WHERE WA.OrganizationId = T1.OrganizationId
		) T3

	OUTER APPLY
		(
			SELECT WL.Name
			FROM WarehouseLocations WL
			WHERE WL.Id = T3.WarehouseLocationId
		) T4

	OUTER APPLY
		(
			SELECT BA.Stage AS PendingStage
			FROM BuyerApprovals BA
			WHERE BA.POFulfillmentId = POF.Id
		) T5

	WHERE Status = 10 AND Stage = 20 AND FulfillmentType = 3
		AND (@BookingNoFrom IS NULL OR POF.Number >= @BookingNoFrom) 
		AND (@BookingNoTo IS NULL OR POF.Number <= @BookingNoTo)
		AND (@ExpectedHubArrivalDateFrom IS NULL OR POF.ExpectedDeliveryDate >= @ExpectedHubArrivalDateFrom) 
		AND (@ExpectedHubArrivalDateTo IS NULL OR POF.ExpectedDeliveryDate <= @ExpectedHubArrivalDateTo)
		AND (@IsInternal = 1 OR @IsInternal IS NULL OR (@IsInternal = 0 AND EXISTS (
			SELECT 1
			FROM POFulfillmentContacts PC (NOLOCK)
			WHERE PC.POFulfillmentId = POF.Id AND PC.OrganizationId IN (SELECT [VALUE] FROM dbo.fn_SplitStringToTable(@Affiliates, ','))
		)))
		AND (PendingStage IS NULL OR PendingStage !=10)
END
GO


