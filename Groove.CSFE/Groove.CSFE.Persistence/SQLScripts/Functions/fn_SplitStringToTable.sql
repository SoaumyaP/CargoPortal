SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT *
           FROM   sys.objects
           WHERE  object_id = OBJECT_ID(N'[dbo].[fn_SplitStringToTable]')
                  AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
  DROP FUNCTION [dbo].[fn_SplitStringToTable]
GO 


-- =============================================
-- Author:			Cuong Duong Duy
-- Created date:	10 Mar 2020
-- Description:		To split string into table ([Value]) with delimiter ','
-- =============================================
CREATE FUNCTION [dbo].[fn_SplitStringToTable]
(	
	@source	NVARCHAR(MAX),
	@delimiter VARCHAR(10) = ','
)

RETURNS 

@outPut TABLE 
(	
	[RowId]			INT IDENTITY(1, 1) NOT NULL,
	[Value]			VARCHAR(255) NOT NULL
)
AS
BEGIN

	DECLARE @pos      	INT,
         	@textpos  	INT,
          	@chunklen 	SMALLINT,
          	@str      	NVARCHAR(4000),
          	@tmpstr   	NVARCHAR(4000),
          	@leftover 	NVARCHAR(4000)

	SELECT @textpos = 1, @leftover = ''
    
  	WHILE @textpos <= DATALENGTH(@source) / 2
  	BEGIN
  	
  		SELECT	@chunklen = 4000 - DATALENGTH(@leftover) / 2,
 				@tmpstr	  = LTRIM(@leftover + SUBSTRING(@source, @textpos, @chunklen)),
 				@textpos  = @textpos + @chunklen,
 				@pos	  = CHARINDEX(@delimiter, @tmpstr)

		WHILE @pos > 0
		BEGIN
		
			SET @str = SUBSTRING(@tmpstr, 1, @pos - 1)
			INSERT @outPut([Value]) VALUES(@str)		
				
			SELECT	@tmpstr = LTRIM(SUBSTRING(@tmpstr, @pos + 1, LEN(@tmpstr))),
					@pos	= CHARINDEX(@delimiter, @tmpstr)
		END

		SET @leftover = @tmpstr
  	END

	IF LTRIM(RTRIM(@leftover)) <> ''
        INSERT @outPut([Value]) VALUES(@leftover)

    RETURN
END
GO


