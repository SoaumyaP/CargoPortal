
-- =============================================
-- Author:		Dong Tran
-- Create date: 11 Aug 2022
-- Description:	Count number of booking awaiting for submission.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_BookingStatistics_AwaitingForSubmission]
	@userRole nvarchar(50),
	@organizationId bigint,
	@email nvarchar(50)
AS
BEGIN
	DECLARE @result INT;
	SET @result = 0;

	IF @userRole = 'Shipper'
		BEGIN
			SELECT @result = COUNT (1)
			FROM POFulfillments P
			WHERE Status = 10 AND Stage = 10
				AND EXISTS (
					 SELECT 1 
				     FROM POFulfillmentContacts POFC
				     WHERE P.Id = POFC.POFulfillmentId AND POFC.OrganizationId = @organizationId AND (p.CreatedBy NOT LIKE CONCAT('%',@email))
					 )
		END
	IF @userRole = 'CSR'
		BEGIN
			SELECT @result = COUNT (1)
			FROM POFulfillments P
			WHERE Status = 10 AND Stage = 10 AND (P.CreatedBy LIKE CONCAT('%',@email))
		END
	RETURN @result
END

GO

