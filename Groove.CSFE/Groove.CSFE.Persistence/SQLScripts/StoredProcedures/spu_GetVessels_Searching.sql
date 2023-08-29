
-- =============================================
-- Author:		Dong Tran
-- Create date: 7 Jan 2021
-- Description:	To search vessel by name on dropdownlist, filtered on active and real vessels
-- =============================================
CREATE OR ALTER  PROCEDURE [dbo].[spu_GetVessels_Searching]	
	@name VARCHAR(512) = NULL
AS
BEGIN
	SET NOCOUNT ON;	

	SELECT [Name]
	FROM Vessels WITH (NOLOCK)
	WHERE 
		[Name] LIKE CONCAT('%', @name ,'%') 
		AND [Status] = 1 AND [IsRealVessel] = 1
END


