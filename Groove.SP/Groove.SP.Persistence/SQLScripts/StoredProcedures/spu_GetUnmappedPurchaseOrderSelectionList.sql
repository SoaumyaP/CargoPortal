SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetUnmappedPurchaseOrderSelectionList', 'P') IS NOT NULL
DROP PROC dbo.spu_GetUnmappedPurchaseOrderSelectionList
GO

-- =============================================
-- Author:		Hau Ng
-- Create date: 18 Nov 2021
-- Description:	This method to get all POs selections which unmapped with supplier belonging to selected Principal organization as multi-POs selections
-- It works for both Internal & External users
-- =============================================
CREATE PROCEDURE spu_GetUnmappedPurchaseOrderSelectionList
	@PrincipalOrganizationId BIGINT, -- to filter po
	@SupplierOrganizationId BIGINT, -- if @PrincipalOrganizationId is null -> using this to get customers via CustomerRelationship then filter po by those customers.
	@SearchTerm NVARCHAR(255) = NULL,
	@Skip INT,
	@Take INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	--SET @PrincipalOrganizationId = 456
	--SET @SearchTerm = ''
	--SET @Skip = 40
	--SET @Take = 20

	-- Variables

	DECLARE @RowCount INT;

	DECLARE @PrincipalOrganizationIds TABLE (
		Id INT
	);

	IF (@PrincipalOrganizationId IS NOT NULL AND @PrincipalOrganizationId != 0)
	BEGIN
		INSERT INTO @PrincipalOrganizationIds
		SELECT @PrincipalOrganizationId
	END
	-- else, get customers by supplierOrgId via CustomerRelationship then filter po by those customers.
	ELSE IF (@SupplierOrganizationId IS NOT NULL AND @SupplierOrganizationId != 0)
	BEGIN
		INSERT INTO @PrincipalOrganizationIds
		SELECT cr.CustomerId
		FROM CustomerRelationship cr
		WHERE cr.SupplierId = @SupplierOrganizationId AND cr.ConnectionType = 1
	END

	-- Count available records
	-- It is just to count a number of records, not need to select all columns
	SELECT @RowCount = COUNT(t1.Id)
			FROM
			(
				SELECT
					PO.Id AS [Id]
					,CONCAT(PO.PONumber, ' - ', PO.ShipFrom, IIF(T.CompanyName IS NOT NULL AND T.CompanyName <> '', CONCAT(' - ', T.CompanyName), '')) as [DisplayText]
				FROM [PurchaseOrders] PO WITH (NOLOCK)
				INNER JOIN [PurchaseOrderContacts] POC WITH (NOLOCK)
					ON PO.Id = POC.PurchaseOrderId AND POC.OrganizationId IN (SELECT Id FROM @PrincipalOrganizationIds) AND POC.OrganizationRole = 'Principal'
				OUTER APPLY (
					SELECT TOP(1) SC.CompanyName
					FROM [PurchaseOrderContacts] SC WITH (NOLOCK)
					WHERE SC.PurchaseOrderId = PO.Id AND SC.OrganizationRole = 'Supplier'
				) T
				WHERE PO.[Status] = 1
					AND NOT EXISTS(
							SELECT 1
							FROM [PurchaseOrderContacts] POCS (NOLOCK)
							WHERE POCS.OrganizationRole = 'Supplier' AND POCS.PurchaseOrderId = PO.Id AND POCS.OrganizationId <> 0
					)
			) t1
			WHERE ( @SearchTerm IS NULL OR @SearchTerm = '' OR t1.[DisplayText] LIKE '%' + @SearchTerm + '%')
			
	-- Return data here
	-- PLEASE make sure order of column is matched to C# mapping
	SELECT
		PO.Id AS [Id]
		,PO.PONumber as [PONumber]
		,PO.ShipFrom as [ShipFrom]
		,CAST(@RowCount AS BIGINT) AS [RecordCount]
		,T1.[DisplayText]
	FROM [PurchaseOrders] PO WITH (NOLOCK)
	INNER JOIN [PurchaseOrderContacts] POC WITH (NOLOCK) 
		ON PO.Id = POC.PurchaseOrderId AND POC.OrganizationId IN (SELECT Id FROM @PrincipalOrganizationIds) AND POC.OrganizationRole = 'Principal'
	OUTER APPLY (
		SELECT TOP(1) SC.CompanyName
		FROM [PurchaseOrderContacts] SC WITH (NOLOCK)
		WHERE SC.PurchaseOrderId = PO.Id AND SC.OrganizationRole = 'Supplier'
	) T
	OUTER APPLY (
		SELECT CONCAT(PO.PONumber, ' - ', PO.ShipFrom, IIF(T.CompanyName IS NOT NULL AND T.CompanyName <> '', CONCAT(' - ', T.CompanyName), '')) AS [DisplayText]
	) T1
	WHERE PO.[Status] = 1
		AND (@SearchTerm IS NULL OR @SearchTerm = '' OR T1.[DisplayText] LIKE '%' + @SearchTerm + '%')
		AND NOT EXISTS(
				SELECT 1
				FROM [PurchaseOrderContacts] POCS (NOLOCK)
				WHERE POCS.OrganizationRole = 'Supplier' AND POCS.PurchaseOrderId = PO.Id AND POCS.OrganizationId <> 0
		)
	ORDER BY PO.Id
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
END
GO