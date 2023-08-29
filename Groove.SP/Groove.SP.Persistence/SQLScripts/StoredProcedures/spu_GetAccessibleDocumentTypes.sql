SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetAccessibleDocumentTypes', 'P') IS NOT NULL
DROP PROC spu_GetAccessibleDocumentTypes
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 26 Aug 2021
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetAccessibleDocumentTypes]
	@roleId BIGINT = 0,
	@organizationId BIGINT = 0,
	@entityType NVARCHAR(MAX),
	@entityId BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;
	
	-- Global variable declarations
	DECLARE @poffEntityType NVARCHAR(20) = 'POF'
	DECLARE @shipmentEntityType NVARCHAR(20) = 'SHI'
	DECLARE @containerEntityType NVARCHAR(20) = 'CTN'
	DECLARE @houseBLEntityType NVARCHAR(20) = 'BOL'
	DECLARE @masterBLEntityType NVARCHAR(20) = 'MBL'

	DECLARE @relatedContractHolderOrgId BIGINT
	DECLARE @relatedMasterBLId TABLE (
		[Id] BIGINT
	)
	DECLARE @allowedAttachmentTypes TABLE (
		AttachmentType NVARCHAR(64),
		Alias NVARCHAR(100)
	)

	DECLARE @resultTbl TABLE(
		AttachmentType NVARCHAR(64),
		[Order] INT
	)
	-- External user
	IF (@roleId != 0)
	BEGIN
		-- External roles apply master contract holder checking.
		IF (@roleId IN (8)) --(Principal)
		BEGIN
			-- POFulfillment
			IF (@entityType = @poffEntityType)
			BEGIN
				-- Related shipments
				;with tmp as (
					SELECT Id
					FROM Shipments (NOLOCK)
					WHERE POFulfillmentId = @entityId
				)
				INSERT INTO @relatedMasterBLId
				SELECT DISTINCT c.MasterBillId
				FROM Consignments c (NOLOCK)
				WHERE c.ShipmentId IN (SELECT * FROM tmp) AND c.MasterBillId IS NOT NULL AND c.MasterBillId != 0
			END

			-- Shipment
			ELSE IF (@entityType = @shipmentEntityType)
			BEGIN
				INSERT INTO @relatedMasterBLId
				SELECT DISTINCT c.MasterBillId
				FROM Consignments c (NOLOCK)
				WHERE c.ShipmentId = @entityId AND c.MasterBillId IS NOT NULL AND c.MasterBillId != 0
			END

			-- Container
			ELSE IF (@entityType = @containerEntityType)
			BEGIN
				-- Related shipments
				;with tmp as (
					SELECT sl.ShipmentId
					FROM ShipmentLoads sl (NOLOCK) JOIN Containers c (NOLOCK) ON sl.ContainerId = c.Id
					WHERE c.Id = @entityId
				)
				INSERT INTO @relatedMasterBLId
				SELECT DISTINCT c.MasterBillId
				FROM Consignments c (NOLOCK)
				WHERE c.ShipmentId IN (SELECT * FROM tmp) AND c.MasterBillId IS NOT NULL AND c.MasterBillId != 0
			END

			-- House BL
			ELSE IF (@entityType = @houseBLEntityType)
			BEGIN
				INSERT INTO @relatedMasterBLId
				SELECT MasterBillOfLadingId
				FROM BillOfLadingShipmentLoads (NOLOCK)
				WHERE BillOfLadingId = @entityType AND MasterBillOfLadingId IS NOT NULL AND MasterBillOfLadingId != 0
			END

			-- Master BL
			ELSE IF (@entityType = @masterBLEntityType)
			BEGIN
				INSERT INTO @relatedMasterBLId
				SELECT @entityId
			END

			SELECT TOP 1 @relatedContractHolderOrgId = cm.ContractHolder
			FROM MasterBills mb (NOLOCK) JOIN ContractMaster cm (NOLOCK) ON mb.CarrierContractNo = cm.CarrierContractNo AND cm.ContractHolder IS NOT NULL AND cm.ContractHolder != '' AND ISNUMERIC(cm.ContractHolder) = 1
			WHERE mb.Id IN (SELECT * FROM @relatedMasterBLId)

			INSERT INTO @allowedAttachmentTypes
			SELECT atp.AttachmentType, atp.Alias
			FROM AttachmentTypePermissions atp (NOLOCK)
			WHERE atp.RoleId = @roleId AND (atp.CheckContractHolder = 0 OR @organizationId = @relatedContractHolderOrgId)

		END
		ELSE
		BEGIN
			INSERT INTO @allowedAttachmentTypes
			SELECT atp.AttachmentType,atp.Alias
			FROM AttachmentTypePermissions atp (NOLOCK)
			WHERE atp.RoleId = @roleId
		END

		INSERT INTO @resultTbl
		SELECT 
				CASE WHEN AT.Alias IS NOT NULL AND @roleId = 13 THEN AT.Alias ELSE  atc.AttachmentType END AttachmentType
				, atc.[Order]
		FROM AttachmentTypeClassifications atc (NOLOCK)
		CROSS APPLY
		(
			SELECT Alias, AttachmentType 
			FROM @allowedAttachmentTypes
			WHERE atc.AttachmentType IN (AttachmentType)
		)AT
		WHERE atc.EntityType = @entityType
	END
	-- Internal user
	ELSE
	BEGIN
		INSERT INTO @resultTbl
		SELECT atc.AttachmentType, atc.[Order]
		FROM AttachmentTypeClassifications atc (NOLOCK)
		WHERE atc.EntityType = @entityType
	END

	-- return
	SELECT AttachmentType FROM @resultTbl
	ORDER BY [Order]
END

GO