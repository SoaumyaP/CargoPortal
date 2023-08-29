SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('cruise.spu_WriteLogCruiseOrderItem', 'P') IS NOT NULL
DROP PROC cruise.spu_WriteLogCruiseOrderItem
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Created date:	23 June 2021
-- Description:	Write log to CruiseOrderItemChangeLogs
-- =============================================
CREATE PROCEDURE [cruise].[spu_WriteLogCruiseOrderItem]
	@cruiseOrderItemId BIGINT,
	@jsonCurrentData NVARCHAR(MAX),
	@jsonNewData NVARCHAR(MAX),
	@updatedBy NVARCHAR(512)	
AS
BEGIN
	 --SET NOCOUNT ON added to prevent extra result sets from
	 --interfering with SELECT statements.

	SET NOCOUNT ON;

	DECLARE @curentUTC DATETIME2(7);
	SET @curentUTC = GETUTCDATE();

	-- Write log
	INSERT INTO [cruise].[CruiseOrderItemChangeLogs]
		(
			[CreatedBy],
			[CreatedDate],
			[UpdatedBy],
			[UpdatedDate],
			[JsonCurrentData],
			[JsonNewData],
			[CruiseOrderItemId]
		)
	SELECT	@updatedBy,
			@curentUTC,
			@updatedBy,
			@curentUTC,
			@jsonCurrentData,
			@jsonNewData,
			@cruiseOrderItemId

END
GO

