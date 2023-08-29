/****** Object:  StoredProcedure [dbo].[usp_UpdateBackgroundJobStatus]    Script Date: 7/26/2019 11:29:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[spu_UpdateImportDataProgressStatus]
	@id INT,
	@status INT,
	@result NVARCHAR(MAX) = NULL,
	@log NVARCHAR(MAX) = NULL
AS
BEGIN
	UPDATE 
		dbo.ImportDataProgresses
	SET 
		[Status] = @status,
		Result = @result,
		[Log] = @log,
		UpdatedDate = GETUTCDATE(),
		UpdatedBy = N'System',
		EndDate = GETUTCDATE()
	WHERE
		Id = @id
END


GO


