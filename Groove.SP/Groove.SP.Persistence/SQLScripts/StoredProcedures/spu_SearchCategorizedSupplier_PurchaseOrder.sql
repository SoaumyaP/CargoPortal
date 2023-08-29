SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_SearchCategorizedSupplier_PurchaseOrder', 'P') IS NOT NULL
DROP PROC dbo.spu_SearchCategorizedSupplier_PurchaseOrder
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 17 Aug 2022
-- Description:	
-- =============================================
CREATE PROCEDURE spu_SearchCategorizedSupplier_PurchaseOrder
	@IsInternal BIT = 1,
	-- leave it empty if user is internal
	@OrganizationId BIGINT = null, 
	@SupplierCustomerRelationships NVARCHAR(255) = '',
	@Affiliates NVARCHAR(255) = '',

	@SearchTerm NVARCHAR(255) = NULL,
	@Page INT = 1,
	@PageSize INT = 8

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	-- Variables
	DECLARE @SkipRows int = (@Page - 1) * @PageSize;

	DECLARE @RowCount INT;


	DECLARE @CategorizedSuppliersTbl TABLE (
		[Name] NVARCHAR(250)
	)

	IF (@IsInternal = 1)
	BEGIN
		INSERT INTO @CategorizedSuppliersTbl
		SELECT
			DISTINCT SUP.Supplier AS [Name]
		FROM PurchaseOrders po WITH (NOLOCK)
		OUTER APPLY
		(
			SELECT TOP 1 pc.CompanyName AS Supplier
			FROM PurchaseOrderContacts pc WITH (NOLOCK) WHERE po.Id = pc.PurchaseOrderId AND pc.OrganizationRole = 'Supplier'
		) SUP
		WHERE SUP.Supplier is not null AND SUP.Supplier <> ''
		AND (@SearchTerm is null OR @SearchTerm = '' OR Supplier like '%' + @SearchTerm + '%')
	END
	
	--user role = agent/ principal
	ELSE IF (@SupplierCustomerRelationships is null
				OR @SupplierCustomerRelationships = '')
	BEGIN
		INSERT INTO @CategorizedSuppliersTbl
		SELECT 
			DISTINCT SUP.Supplier AS [Name]
        FROM
        (
	        SELECT 
                po.Id
	        FROM PurchaseOrders po WITH (NOLOCK)
	        WHERE po.Id IN (
		        SELECT pc.PurchaseOrderId FROM PurchaseOrderContacts pc
		        WHERE po.Id = pc.PurchaseOrderId AND pc.OrganizationId IN (SELECT tmp.[Value]
		                            FROM dbo.fn_SplitStringToTable(@Affiliates, ',') tmp))
        ) PO
        OUTER APPLY
        (
	        SELECT TOP(1) sc.CompanyName AS Supplier
	        FROM PurchaseOrderContacts sc WITH (NOLOCK)
	        WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
        ) SUP
		WHERE (@SearchTerm is null OR @SearchTerm = '' OR Supplier like '%' + @SearchTerm + '%')
		AND SUP.Supplier is not null AND SUP.Supplier <> ''
	END

	--user role = shipper
	ELSE
	BEGIN
		INSERT INTO @CategorizedSuppliersTbl
		SELECT DISTINCT SUP.Supplier AS [Name]
        FROM
        (
	        SELECT 
                po.Id,
				pc.OrganizationRole
	        FROM PurchaseOrders po WITH (NOLOCK)
            INNER JOIN PurchaseOrderContacts pc WITH (NOLOCK) on po.Id = pc.PurchaseOrderId
            WHERE pc.OrganizationId = @OrganizationId
        ) PO
        CROSS APPLY
        (
			SELECT TOP(1) sc.CompanyName AS Supplier, sc.OrganizationId AS SupplierId
			FROM PurchaseOrderContacts sc WITH (NOLOCK)
			WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
        ) SUP
        CROSS APPLY
        (
            SELECT TOP(1) sc.OrganizationId AS CustomerId
            FROM PurchaseOrderContacts sc WITH (NOLOCK)
            WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
        ) PRIN
        CROSS APPLY
        (
		    SELECT TOP(1)
			    BC.IsProgressCargoReadyDate AS IsProgressCargoReadyDates,
			    BC.ProgressNotifyDay
		    FROM BuyerCompliances BC (NOLOCK)
		    WHERE BC.OrganizationId = PRIN.CustomerId
                AND BC.Stage = 1
        ) BCOM
        WHERE
	        PO.OrganizationRole = 'Delegation' 
	        OR (
		        PO.OrganizationRole = 'Supplier' 
		        AND CAST(SUP.SupplierID AS NVARCHAR(20)) + ',' + CAST(PRIN.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] 
		        FROM dbo.fn_SplitStringToTable(@SupplierCustomerRelationships, ';') tmp )
	        )
			AND SUP.Supplier is not null AND SUP.Supplier <> ''
			AND (@SearchTerm is null OR @SearchTerm = '' OR Supplier like '%' + @SearchTerm + '%')
	END
	

	SET @RowCount = @@ROWCOUNT
	select @RowCount

	SELECT *
	FROM @CategorizedSuppliersTbl rs
	ORDER BY rs.[Name]
	OFFSET @SkipRows ROWS FETCH NEXT @PageSize ROWS ONLY;

END
GO