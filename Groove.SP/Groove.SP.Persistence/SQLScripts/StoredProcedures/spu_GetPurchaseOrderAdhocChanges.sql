SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetPurchaseOrderAdhocChanges', 'P') IS NOT NULL
DROP PROC spu_GetPurchaseOrderAdhocChanges
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 01 July 2020
-- Description:	Get ad-hoc changes on PO which may impact to bookings by booking id
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetPurchaseOrderAdhocChanges]
	@POFFId BIGINT,
	@GetTopPriority BIT = 1
AS
BEGIN
	 --SET NOCOUNT ON added to prevent extra result sets from
	 --interfering with SELECT statements.

	SET NOCOUNT ON;

	IF (@GetTopPriority = 1)
	BEGIN

		SELECT TOP(1) POAH.Id, POAH.PurchaseOrderId, POAH.POFulfillmentId, POAH.[Priority], POAH.[Message]
		FROM PurchaseOrderAdhocChanges POAH
		WHERE POAH.POFulfillmentId = @POFFId
		ORDER BY [Priority] ASC, CreatedDate DESC

	END
	ELSE
	BEGIN
		SELECT POAH.Id, POAH.PurchaseOrderId, POAH.POFulfillmentId, POAH.[Priority], POAH.[Message]
		FROM PurchaseOrderAdhocChanges POAH
		WHERE POAH.POFulfillmentId = @POFFId
		ORDER BY CreatedDate DESC
	END


END
GO

