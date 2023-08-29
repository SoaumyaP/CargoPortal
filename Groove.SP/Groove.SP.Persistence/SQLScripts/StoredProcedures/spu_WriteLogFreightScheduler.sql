SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.spu_WriteLogFreightScheduler', 'P') IS NOT NULL
DROP PROC dbo.spu_WriteLogFreightScheduler
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Created date:	15 Feb 2022
-- Description:	Write log to FreightSchedulerChangeLogs
-- =============================================
CREATE PROCEDURE [dbo].[spu_WriteLogFreightScheduler]
	@freightSchedulerId BIGINT,
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
	INSERT INTO [dbo].[FreightSchedulerChangeLogs]
		(
			[CreatedBy],
			[CreatedDate],
			[UpdatedBy],
			[UpdatedDate],
			[JsonCurrentData],
			[JsonNewData],
			[ScheduleId]
		)
	SELECT	@updatedBy,
			@curentUTC,
			@updatedBy,
			@curentUTC,
			@jsonCurrentData,
			@jsonNewData,
			@freightSchedulerId

END
GO

