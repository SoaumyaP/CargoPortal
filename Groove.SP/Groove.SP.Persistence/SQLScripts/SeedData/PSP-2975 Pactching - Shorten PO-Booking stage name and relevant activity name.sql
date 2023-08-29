-- =============================================
-- Author:			Dong Tran
-- Created date:	12 Nov 2021
-- Description:	    PSP-2975 Shorten PO/Booking stage name and relevant activity name
							--patch value of ExceptionActivity to 'Booking Approval Request'
-- =============================================

BEGIN TRANSACTION

UPDATE A 
SET A.ActivityDescription = E.ActivityDescription
FROM Activities A
INNER JOIN EventCodes E ON A.ActivityCode = E.ActivityCode 
AND E.ActivityCode IN (
'1051',
'1052',
'1053',
'1054',
'1055',
'1056',
'1057',
'1058',
'1059',
'1061')

COMMIT TRANSACTION

BEGIN TRANSACTION

UPDATE BuyerApprovals
SET ExceptionActivity = 'Booking Approval Request'
WHERE ExceptionActivity = 'Forwarder Booking Approval Request'

COMMIT TRANSACTION