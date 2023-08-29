SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetAttachmentCrossModule', 'P') IS NOT NULL
DROP PROC spu_GetAttachmentCrossModule
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 15 July 2021
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetAttachmentCrossModule]
	@roleId BIGINT = 0,
	@organizationId BIGINT = 0,
	@poFulfillmentId BIGINT = 0,
	@shipmentId BIGINT = 0,
	@containerId BIGINT = 0,
	@houseBLId BIGINT = 0,
	@masterBLId BIGINT = 0
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

	DECLARE @attachmentTbl TABLE(
		[Id] BIGINT,
        [FileName] NVARCHAR(256),
		[AttachmentType] NVARCHAR(64),
        [BlobId] NVARCHAR(512),
		[Description] NVARCHAR(MAX),
		[ReferenceNo] NVARCHAR(128),
		[UploadedBy] NVARCHAR(256),
		[UploadedDate] DATETIME2(7),
		[CreatedDate] DATETIME2(7),
		[CreatedBy] NVARCHAR(256),
		[UpdatedBy]  NVARCHAR(256),
		[UpdatedDate] DATETIME2(7),
		[EntityType] NVARCHAR(MAX)
	)
	DECLARE @globalIdAttachmentPartTbl TABLE(
		[EntityType] NVARCHAR(MAX),
		[EntityId] BIGINT
	)
	DECLARE @relatedShipmentIdTbl TABLE([Id] BIGINT)
	DECLARE @relatedMasterBLIdTbl TABLE([Id] BIGINT)

	--AttachmentType allowed in dbo.AttachmentTypePermissions
	DECLARE @allowedAttachmentTypeTbl TABLE (AttachmentType NVARCHAR(64),Alias NVARCHAR(100), CheckContractHolder BIT)
	INSERT INTO @allowedAttachmentTypeTbl
	SELECT 
		AttachmentType, Alias, CheckContractHolder
	FROM AttachmentTypePermissions (NOLOCK)
	WHERE RoleId = @roleId

	IF (@poFulfillmentId IS NOT NULL AND @poFulfillmentId > 0)
	BEGIN
		-- Obtain GlobalId parts from POFulfillment
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @poffEntityType, @poFulfillmentId

		-- Obtain GlobalId parts from related Shipment(s)
		INSERT INTO @globalIdAttachmentPartTbl
		OUTPUT inserted.EntityId
		INTO @relatedShipmentIdTbl
		SELECT @shipmentEntityType, Id
		FROM Shipments (NOLOCK)
		WHERE POFulfillmentId = @poFulfillmentId

		-- Obtain GlobalId parts from related Container(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @containerEntityType, c.Id
		FROM ShipmentLoads sl (NOLOCK) JOIN Containers c (NOLOCK) ON sl.ContainerId = c.Id
		WHERE sl.ShipmentId IN (
			SELECT * FROM @relatedShipmentIdTbl
		)

		-- Obtain GlobalId parts from related HouseBL(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT DISTINCT @houseBLEntityType, bol.Id
		FROM BillOfLadingShipmentLoads (NOLOCK) bsl JOIN ShipmentLoads sl (NOLOCK) ON bsl.ShipmentLoadId = sl.Id JOIN BillOfLadings bol (NOLOCK) ON bsl.BillOfLadingId = bol.Id
		WHERE sl.ShipmentId IN (SELECT * FROM @relatedShipmentIdTbl)

		-- Obtain GlobalId parts from related MasterBL(s)
		INSERT INTO @globalIdAttachmentPartTbl
		OUTPUT inserted.EntityId
		INTO @relatedMasterBLIdTbl
		SELECT DISTINCT @masterBLEntityType, c.MasterBillId
		FROM Consignments c (NOLOCK)
		WHERE c.ShipmentId IN (SELECT * FROM @relatedShipmentIdTbl) AND c.MasterBillId IS NOT NULL AND c.MasterBillId != 0
	END
	ELSE IF (@shipmentId IS NOT NULL AND @shipmentId > 0)
	BEGIN
		-- Obtain GlobalId parts from Shipment
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @shipmentEntityType, @shipmentId

		-- Obtain GlobalId parts from related POFulfillment(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @poffEntityType, POFulfillmentId
		FROM Shipments (NOLOCK)
		WHERE Id = @shipmentId AND POFulfillmentId is not null and POFulfillmentId != 0

		-- Obtain GlobalId parts from related Container(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @containerEntityType, c.Id
		FROM ShipmentLoads sl (NOLOCK) JOIN Containers c (NOLOCK) ON sl.ContainerId = c.Id
		WHERE sl.ShipmentId = @shipmentId

		-- Obtain GlobalId parts from related HouseBL(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT DISTINCT @houseBLEntityType, bol.Id
		FROM BillOfLadingShipmentLoads (NOLOCK) bsl JOIN ShipmentLoads sl (NOLOCK) ON bsl.ShipmentLoadId = sl.Id JOIN BillOfLadings bol (NOLOCK) ON bsl.BillOfLadingId = bol.Id
		WHERE sl.ShipmentId = @shipmentId

		-- Obtain GlobalId parts from related MasterBL(s)
		INSERT INTO @globalIdAttachmentPartTbl
		OUTPUT inserted.EntityId
		INTO @relatedMasterBLIdTbl
		SELECT DISTINCT @masterBLEntityType, c.MasterBillId
		FROM Consignments c (NOLOCK)
		WHERE c.ShipmentId = @shipmentId AND c.MasterBillId IS NOT NULL AND c.MasterBillId != 0
	END
	ELSE IF (@containerId IS NOT NULL AND @containerId > 0)
	BEGIN
		-- Obtain GlobalId parts from Container
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @containerEntityType, @containerId

		-- Obtain GlobalId parts from related Shipment(s)
		INSERT INTO @globalIdAttachmentPartTbl
		OUTPUT inserted.EntityId
		INTO @relatedShipmentIdTbl
		SELECT @shipmentEntityType, sl.ShipmentId
		FROM ShipmentLoads sl (NOLOCK) JOIN Containers c (NOLOCK) ON sl.ContainerId = c.Id
		WHERE c.Id = @containerId

		-- Obtain GlobalId parts from related POFulfillment(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT DISTINCT @poffEntityType, POFulfillmentId
		FROM Shipments (NOLOCK)
		WHERE Id IN (SELECT * FROM @relatedShipmentIdTbl) AND POFulfillmentId IS NOT NULL AND POFulfillmentId != 0

		-- Obtain GlobalId parts from related HouseBL(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT DISTINCT @houseBLEntityType, bol.Id
		FROM BillOfLadingShipmentLoads (NOLOCK) bsl JOIN ShipmentLoads sl (NOLOCK) ON bsl.ShipmentLoadId = sl.Id JOIN BillOfLadings bol (NOLOCK) ON bsl.BillOfLadingId = bol.Id
		WHERE sl.ShipmentId IN (SELECT * FROM @relatedShipmentIdTbl)

		-- Obtain GlobalId parts from related MasterBL(s)
		INSERT INTO @globalIdAttachmentPartTbl
		OUTPUT inserted.EntityId
		INTO @relatedMasterBLIdTbl
		SELECT DISTINCT @masterBLEntityType, c.MasterBillId
		FROM Consignments c (NOLOCK)
		WHERE c.ShipmentId IN (SELECT * FROM @relatedShipmentIdTbl) AND c.MasterBillId IS NOT NULL AND c.MasterBillId != 0
	END
	ELSE IF (@houseBLId IS NOT NULL AND @houseBLId > 0)
	BEGIN
	    -- BillOfLadingShipmentLoads related to HouseBLId
		DECLARE @relatedBillOfLadingShipmentLoadTbl TABLE ([ShipmentLoadId] BIGINT, [MasterBillOfLoadingId] BIGINT)
		INSERT INTO @relatedBillOfLadingShipmentLoadTbl
		SELECT ShipmentLoadId, MasterBillOfLadingId
		FROM BillOfLadingShipmentLoads (NOLOCK)
		WHERE BillOfLadingId = @houseBLId

		-- ShipmentLoads related to HouseBLId
		DECLARE @relatedShipmentLoadTbl TABLE ([ShipmentId] BIGINT, [ContainerId] BIGINT)
		INSERT INTO @relatedShipmentLoadTbl
		SELECT ShipmentId, ContainerId
		FROM ShipmentLoads (NOLOCK)
		WHERE Id IN (SELECT ShipmentLoadId FROM @relatedBillOfLadingShipmentLoadTbl)

		-- Obtain GlobalId parts from HouseBL
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @houseBLEntityType, @houseBLId

		-- Obtain GlobalId parts from related MasterBL(s)
		INSERT INTO @globalIdAttachmentPartTbl
		OUTPUT inserted.EntityId
		INTO @relatedMasterBLIdTbl
		SELECT DISTINCT @masterBLEntityType, MasterBillOfLoadingId
		FROM @relatedBillOfLadingShipmentLoadTbl
		WHERE MasterBillOfLoadingId IS NOT NULL AND MasterBillOfLoadingId != 0

		-- Obtain GlobalId parts from related Shipment(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT DISTINCT @shipmentEntityType, ShipmentId
		FROM @relatedShipmentLoadTbl

		-- Obtain GlobalId parts from related Container(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT DISTINCT @containerEntityType, ContainerId
		FROM @relatedShipmentLoadTbl
		WHERE ContainerId IS NOT NULL AND ContainerId != 0

		-- Obtain GlobalId parts from related POFulfillment(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT DISTINCT @poffEntityType, POFulfillmentId
		FROM Shipments (NOLOCK)
		WHERE Id IN (SELECT ShipmentId FROM @relatedShipmentLoadTbl) AND POFulfillmentId IS NOT NULL AND POFulfillmentId != 0
	END
	ELSE IF (@masterBLId IS NOT NULL AND @masterBLId > 0)
	BEGIN
		-- Obtain GlobalId parts from MasterBL
		INSERT INTO @globalIdAttachmentPartTbl
		OUTPUT inserted.EntityId
		INTO @relatedMasterBLIdTbl
		SELECT @masterBLEntityType, @masterBLId

		INSERT INTO @relatedShipmentIdTbl
		SELECT DISTINCT c.ShipmentId AS Id
		FROM Consignments c (NOLOCK)
		WHERE c.MasterBillId = @masterBLId

		-- Obtain GlobalId parts from related Shipment(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @shipmentEntityType, Id
		FROM @relatedShipmentIdTbl

		-- Obtain GlobalId parts from related POFulfillment(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @poffEntityType, POFulfillmentId
		FROM Shipments (NOLOCK)
		WHERE Id IN (SELECT Id from @relatedShipmentIdTbl) AND POFulfillmentId IS NOT NULL AND POFulfillmentId != 0

		-- Obtain GlobalId parts from related Container(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @containerEntityType, ContainerId
		FROM ShipmentLoads (NOLOCK)
		WHERE ShipmentId IN (SELECT Id from @relatedShipmentIdTbl) AND ContainerId IS NOT NULL AND ContainerId != 0

		-- Obtain GlobalId parts from related MasterBL(s)
		INSERT INTO @globalIdAttachmentPartTbl
		SELECT @houseBLEntityType, BillOfLadingId
		FROM BillOfLadingShipmentLoads (NOLOCK)
		WHERE MasterBillOfLadingId = @masterBLId
	END

	SELECT TOP 1 @relatedContractHolderOrgId = cm.contractHolder
	FROM MasterBills mb (NOLOCK) INNER JOIN ContractMaster cm (NOLOCK) ON mb.CarrierContractNo = cm.CarrierContractNo AND cm.ContractHolder IS NOT NULL AND cm.ContractHolder != '' AND ISNUMERIC(cm.ContractHolder) = 1
	WHERE mb.Id IN (SELECT * FROM @relatedMasterBLIdTbl)

	INSERT INTO @attachmentTbl
	SELECT DISTINCT a.Id,
		a.[FileName],
		a.AttachmentType,
		a.BlobId,
		a.[Description],
		a.ReferenceNo,
		a.UploadedBy,
		a.UploadedDate,
		a.CreatedDate,
		a.CreatedBy,
		a.UpdatedBy,
		a.UpdatedDate,
		t.EntityType
	FROM Attachments a (NOLOCK) JOIN GlobalIdAttachments g (NOLOCK) ON a.Id = g.AttachemntId JOIN @globalIdAttachmentPartTbl t ON g.GlobalId = CONCAT(t.EntityType, '_', t.EntityId)

	--Output
	SELECT
		Id,
		[FileName],
		CASE WHEN att.Alias IS NOT NULL AND @roleId = 13 THEN att.Alias ELSE t.AttachmentType END AttachmentType,
		BlobId,
		[Description],
		ReferenceNo,
		UploadedBy,
		UploadedDate,
		CreatedDate,
		CreatedBy,
		UpdatedBy,
		UpdatedDate,
		EntityType,
		att.Alias
	FROM @attachmentTbl t
	CROSS APPLY 
	(
		SELECT TOP 1 Alias,att.AttachmentType,att.CheckContractHolder
		FROM @allowedAttachmentTypeTbl att
		WHERE @roleId = 0 OR (att.AttachmentType = t.AttachmentType AND (@roleId != 8 OR att.CheckContractHolder = 0 OR @organizationId = @relatedContractHolderOrgId)) 
	)att
	--Do not show duplicate attachment between house bill and shipment (Service BUS task)
	WHERE (t.EntityType != @shipmentEntityType OR (t.EntityType = @shipmentEntityType AND NOT EXISTS (SELECT 1 FROM @attachmentTbl t1 WHERE t1.Id =  t.Id AND t1.EntityType = @houseBLEntityType)))

	ORDER BY UploadedDate DESC
END

GO