/****** Object:  StoredProcedure [dbo].[usp_UpdateBackgroundJobProgress]    Script Date: 7/26/2019 11:29:57 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[spu_UpdateImportDataProgress]
	@id BIGINT,
	@completedSteps INT,
	@totalSteps INT = NULL
AS
BEGIN
	UPDATE 
		dbo.ImportDataProgresses 
	SET 
		CompletedSteps = @completedSteps,
		TotalSteps = CASE WHEN @totalSteps IS NULL THEN TotalSteps ELSE @totalSteps END,
		UpdatedDate = GETUTCDATE(),
		UpdatedBy = N'System'
	WHERE
		Id = @id
END


GO


