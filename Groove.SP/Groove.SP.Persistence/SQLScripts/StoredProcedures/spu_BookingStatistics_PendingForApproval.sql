
-- =============================================
-- Author:		Dong Tran
-- Create date: 11 Aug 2022
-- Description:	Count number of booking pending for approval
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_BookingStatistics_PendingForApproval]
	@userRole nvarchar(50),
	@organizationIds nvarchar(250),
	@userEmail nvarchar(250)
AS
BEGIN
	DECLARE @result INT;
	SET @result = 0;

	IF @userRole = 'CSR'
		BEGIN
			SELECT @result = COUNT (1)
			FROM POFulfillments POF
			INNER JOIN BuyerApprovals BA ON POF.Id = BA.POFulfillmentId AND BA.Stage = 10
			WHERE POF.Stage = 20 AND POF.Status = 10
		END
	IF @userRole = 'Principal'
		BEGIN
			SELECT @result = COUNT (1)
			FROM POFulfillments POF
			INNER JOIN BuyerApprovals BA ON POF.Id = BA.POFulfillmentId 
			WHERE POF.Status = 10 AND POF.Stage = 20 AND BA.Stage = 10
				AND ((BA.ApproverSetting = 10 AND BA.ApproverOrgId IN (SELECT value FROM [dbo].[fn_SplitStringToTable](@organizationIds,',')))
		                            OR (BA.ApproverSetting = 20 AND @userEmail IN (SELECT TRIM(value) FROM [dbo].[fn_SplitStringToTable](BA.ApproverUser, ','))))
		END
	RETURN @result
END

GO

