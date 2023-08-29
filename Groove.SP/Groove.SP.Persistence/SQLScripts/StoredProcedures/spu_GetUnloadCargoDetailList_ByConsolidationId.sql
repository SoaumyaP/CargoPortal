SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetUnloadCargoDetailList_ByConsolidationId', 'P') IS NOT NULL
DROP PROC dbo.spu_GetUnloadCargoDetailList_ByConsolidationId
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 17 May 2021
-- Description:	Get list of unload cargo detail by consolidation id
-- =============================================
CREATE PROCEDURE spu_GetUnloadCargoDetailList_ByConsolidationId
	@consolidationId BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	-- Variables
	DECLARE @cargoDetailResultTbl TABLE (
		Id BIGINT NOT NULL,
		ShipmentId BIGINT NOT NULL,
		OrderId BIGINT NOT NULL,
		ItemId BIGINT NOT NULL,
		ConsignmentId BIGINT NOT NULL,
		ContainerId BIGINT NULL,
		ShipmentLoadId BIGINT NOT NULL,
		ShipmentNo VARCHAR(50) NOT NULL,
		PONumber VARCHAR(512) NOT NULL,
		ProductCode NVARCHAR(128) NOT NULL,
		UnitUOM NVARCHAR(20) NULL,
		PackageUOM NVARCHAR(20) NULL,
		VolumeUOM NVARCHAR(20) NULL,
		GrossWeightUOM NVARCHAR(20) NULL,
		NetWeightUOM NVARCHAR(20) NULL,
		Unit DECIMAL(18, 4) NOT NULL,
		Package DECIMAL(18, 4) NOT NULL,
		Volume DECIMAL(18, 4) NOT NULL,
		GrossWeight DECIMAL(18, 4) NOT NULL,
		NetWeight DECIMAL(18, 4) NULL
	)

	;WITH CargoDetailCTE AS (
		SELECT
			cd.Id,
			cd.ShipmentId,
			cd.OrderId,
			cd.ItemId,
			sl.ConsignmentId,
			sl.ContainerId,
			sl.Id AS ShipmentLoadId,
			s.ShipmentNo,
			pod.PONumber,
			poi.ProductCode,
			cd.UnitUOM,
			cd.PackageUOM,
			cd.VolumeUOM,
			cd.GrossWeightUOM,
			cd.NetWeightUOM,
			CASE
				WHEN t.LoadedUnitQty is null
					THEN cd.Unit
				ELSE (cd.Unit - t.LoadedUnitQty)
			END AS Unit,
			CASE
				WHEN t.LoadedPackageQty is null
					THEN cd.Package
				ELSE (cd.Package - t.LoadedPackageQty)
			END AS LoadedPackage,
			cd.Package as BookedPackage,
			CASE
				WHEN t.LoadedVolume is null
					THEN cd.Volume
				ELSE (cd.Volume - t.LoadedVolume)
			END AS Volume,
			CASE
				WHEN t.LoadedGrossWeight is null OR t.LoadedGrossWeight = 0
					THEN cd.GrossWeight
				ELSE (cd.GrossWeight - t.LoadedGrossWeight)
			END AS GrossWeight,
			CASE
				WHEN t.LoadedNetWeight is null
					THEN cd.NetWeight
				ELSE (cd.NetWeight - t.LoadedNetWeight)
			END AS NetWeight
		FROM ShipmentLoads sl (NOLOCK)
		INNER JOIN CargoDetails cd (NOLOCK) ON sl.ShipmentId = cd.ShipmentId AND cd.OrderType = 1
		INNER JOIN Shipments s (NOLOCK) ON cd.ShipmentId = s.id AND s.[Status] = 'active'
		INNER JOIN PurchaseOrders pod (NOLOCK) ON cd.OrderId = pod.id
		INNER JOIN POLineItems poi (NOLOCK) ON cd.ItemId = poi.Id
		OUTER APPLY (
			SELECT 
				SUM(sld.Unit) AS LoadedUnitQty,
				SUM(sld.Package) AS LoadedPackageQty,
				SUM(sld.Volume) AS LoadedVolume,
				SUM(sld.GrossWeight) AS LoadedGrossWeight,
				SUM(sld.NetWeight) AS LoadedNetWeight
			FROM ShipmentLoadDetails sld (NOLOCK)
			WHERE sld.CargoDetailId = cd.id AND sld.ConsolidationId != @consolidationId
		) AS t
		WHERE sl.ConsolidationId = @consolidationId
	)

	INSERT INTO @cargoDetailResultTbl
	SELECT
			cd.Id,
			cd.ShipmentId,
			cd.OrderId,
			cd.ItemId,
			cd.ConsignmentId,
			cd.ContainerId,
			cd.ShipmentLoadId,
			cd.ShipmentNo,
			cd.PONumber,
			cd.ProductCode,
			cd.UnitUOM,
			cd.PackageUOM,
			cd.VolumeUOM,
			cd.GrossWeightUOM,
			cd.NetWeightUOM,
			cd.Unit,
			cd.LoadedPackage as [Package],
			cd.Volume,
			cd.GrossWeight,
			cd.NetWeight
	FROM CargoDetailCTE cd
	WHERE (cd.BookedPackage > 0 AND cd.LoadedPackage > 0)
	OR
	(cd.BookedPackage = 0
		AND NOT EXISTS(
							SELECT 1
							FROM ShipmentLoadDetails sld
							WHERE sld.CargoDetailId = cd.Id AND sld.ConsolidationId <> @consolidationId
						)
	) --Mixed carton case

	-- Return
	SELECT * FROM @cargoDetailResultTbl
END
GO