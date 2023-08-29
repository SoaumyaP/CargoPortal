SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetShipmentSelectionList_Consolidation', 'P') IS NOT NULL
DROP PROC spu_GetShipmentSelectionList_Consolidation
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 4 May 2021
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetShipmentSelectionList_Consolidation]
	@shipmentNumber VARCHAR(50),
	@consolidationId BIGINT,
	@isInternal BIT,
	@organizationId BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;
	
	-- list of shipments that have been linked with this consolidation
	DECLARE @linkedShipmentTbl TABLE(
		Id BIGINT,
		IsFCL BIT,
		ShipFrom NVARCHAR(128) NOT NULL,
		ShipTo NVARCHAR(128) NULL,
		ModeOfTransport NVARCHAR(512) NULL,
		OrderType INT NOT NULL
	)

	INSERT INTO @linkedShipmentTbl
	SELECT s.Id, s.IsFCL, s.ShipFrom, s.ShipTo, s.ModeOfTransport, s.OrderType
	FROM ShipmentLoads sl (NOLOCK) INNER JOIN Shipments s (NOLOCK) ON sl.ShipmentId = s.Id
	WHERE sl.ConsolidationId = @consolidationId

	-- First linked shipment variable
	-- As the current, consolidation always has a linked consignment
	DECLARE @isFCL BIT = (SELECT TOP 1 IsFCL FROM @linkedShipmentTbl)
	DECLARE @shipFrom NVARCHAR(128) = (SELECT TOP 1 ShipFrom FROM @linkedShipmentTbl)
	DECLARE @shipTo NVARCHAR(128) = (SELECT TOP 1 ShipTo FROM @linkedShipmentTbl)
	DECLARE @modeOfTransport NVARCHAR(512) = (SELECT TOP 1 ModeOfTransport FROM @linkedShipmentTbl)
	DECLARE @orderType INT = (SELECT TOP 1 OrderType FROM @linkedShipmentTbl)

	IF (@isInternal = 1)
	BEGIN
		SELECT s.Id AS [Id], s.ShipmentNo AS [ShipmentNumber]
		FROM Shipments s (NOLOCK)
		WHERE s.ShipmentNo LIKE CONCAT('%', @shipmentNumber, '%')
			AND s.IsFCL = @isFCL
			AND s.ShipFrom = @shipFrom
			AND s.ShipTo = @shipTo
			AND s.ModeOfTransport = @modeOfTransport
			AND s.OrderType = @orderType
			AND s.[Status] = 'active'
			AND s.IsItineraryConfirmed = 1
			-- NO link with viewing Consolidation yet
			AND s.Id NOT IN (SELECT Id FROM @linkedShipmentTbl)
			-- Must have Cargo Details
			AND EXISTS (
				SELECT 1
				FROM CargoDetails cd (NOLOCK)
				WHERE cd.ShipmentId = s.Id AND cd.Package > (
					SELECT COALESCE(SUM(sld.Package), 0)
					FROM ShipmentLoadDetails sld (NOLOCK)
					WHERE sld.CargoDetailId = cd.Id
				)
			)
	END
	ELSE
	BEGIN
		SELECT s.Id AS [Id], s.ShipmentNo AS [ShipmentNumber]
		FROM Shipments s (NOLOCK)
		WHERE s.ShipmentNo LIKE CONCAT('%', @shipmentNumber, '%')
			AND s.IsFCL = @isFCL
			AND s.ShipFrom = @shipFrom
			AND s.ShipTo = @shipTo
			AND s.ModeOfTransport = @modeOfTransport
			AND s.OrderType = @orderType
			AND s.[Status] = 'active'
			AND s.IsItineraryConfirmed = 1
			-- NO link with viewing Consolidation yet
			AND s.Id NOT IN (SELECT Id FROM @linkedShipmentTbl)
			-- Must have Cargo Details
			AND EXISTS (
				SELECT 1
				FROM CargoDetails cd (NOLOCK)
				WHERE cd.ShipmentId = s.Id AND cd.Package > (
					SELECT COALESCE(SUM(sld.Package), 0)
					FROM ShipmentLoadDetails sld (NOLOCK)
					WHERE sld.CargoDetailId = cd.Id
				)
			)
			-- current external user organization must be matching with Consignment's Execution Agent.
			AND EXISTS (
				SELECT 1
				FROM Consignments cs (NOLOCK)
				WHERE cs.ShipmentId = s.Id
				AND cs.ExecutionAgentId = @organizationId
			)
	END
END