SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_DeletePurchaseOrderAdhocChanges', 'P') IS NOT NULL
DROP PROC spu_DeletePurchaseOrderAdhocChanges
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 01 July 2020
-- Description:	Clean-up ad-hoc changes on PO which may impact to bookings by id/booking id/purchaseorder id
-- =============================================
CREATE PROCEDURE [dbo].[spu_DeletePurchaseOrderAdhocChanges]
	@Id BIGINT = 0,
	@POFFId BIGINT = 0,
	@PurchaseOrderId BIGINT = 0
AS
BEGIN
	 --SET NOCOUNT ON added to prevent extra result sets from
	 --interfering with SELECT statements.

	SET NOCOUNT ON;

	DELETE
	FROM PurchaseOrderAdhocChanges
	WHERE Id = @Id OR POFulfillmentId = @POFFId OR PurchaseOrderId = @PurchaseOrderId
	

END
GO

