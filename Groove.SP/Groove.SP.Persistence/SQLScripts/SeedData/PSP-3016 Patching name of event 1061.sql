-- =============================================
-- Author:			Dong Tran
-- Created date:	21 Jan 2022
-- Description:	    Change name of event 1061
-- =============================================
BEGIN TRANSACTION

UPDATE A 
SET A.ActivityDescription = E.ActivityDescription
FROM Activities A
INNER JOIN EventCodes E ON A.ActivityCode = E.ActivityCode 
AND E.ActivityCode IN ('1061')

COMMIT TRANSACTION