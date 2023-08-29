SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetNotificationList', 'P') IS NOT NULL
DROP PROC dbo.spu_GetNotificationList
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 04 August 2022
-- Description:	Get notification list by username
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetNotificationList]
	@username NVARCHAR(MAX),
	@fromDate DATETIME2(7) = null,
	@toDate DATETIME2(7) = null,
	@Skip INT,
	@Take INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	-- Variables

	DECLARE @RowCount BIGINT;


	DECLARE @ResultTbl TABLE (
		[Id] BIGINT,
		[MessageKey] NVARCHAR(512),
		[IsRead] BIT,
		[ReadUrl] NVARCHAR(MAX),
		[CreatedDate] DATETIME2(7)
	)

	INSERT INTO @ResultTbl
	SELECT nf.[Id], nf.[MessageKey], unf.[IsRead], nf.[ReadUrl], unf.CreatedDate
	FROM UserNotifications unf JOIN Notifications nf ON unf.NotificationId = nf.Id
	WHERE unf.Username = @username 
		AND (@fromDate is null OR nf.CreatedDate >= @fromDate)
		AND (@toDate is null OR nf.CreatedDate <= @toDate)

	SET @RowCount = @@ROWCOUNT

	SELECT *, @RowCount AS [RecordCount] FROM @ResultTbl
	ORDER BY [CreatedDate] DESC
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
END
GO
