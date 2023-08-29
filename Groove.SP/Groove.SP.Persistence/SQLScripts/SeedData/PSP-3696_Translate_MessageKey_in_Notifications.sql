SET NOCOUNT ON;

BEGIN TRANSACTION;

DECLARE @fromDate DATETIME2(7) = DATEADD(DAY, -7, GETUTCDATE())
DECLARE @toDate DATETIME2(7) = GETUTCDATE()
DECLARE @translationKey NVARCHAR(MAX)
DECLARE @english NVARCHAR(MAX)

DECLARE @translationTbl TABLE (
	[Key] NVARCHAR(MAX),
	[English] NVARCHAR(MAX)
)
-- In order of priority
INSERT INTO @translationTbl Values('notification.msg.tellYourMindWith', 'Tell your mind with')
INSERT INTO @translationTbl Values('notification.msg.hasBeenApproved', 'has been approved')
INSERT INTO @translationTbl Values('notification.msg.hasBeenDelegatedToYou', 'has been delegated to you')
INSERT INTO @translationTbl Values('notification.msg.hasBeenRemovedYourAccess', 'has been removed from your access')
INSERT INTO @translationTbl Values('notification.msg.hasBeenUploaded', 'has been uploaded')
INSERT INTO @translationTbl Values('notification.msg.needYourUpdateProgress', 'need your update the progress')

INSERT INTO @translationTbl Values('notification.msg.hasBeenSubmittedWithNewShippingDocument', 'has been submitted with new shipping document')
INSERT INTO @translationTbl Values('notification.msg.hasBeenSubmitted', 'has been submitted')

INSERT INTO @translationTbl Values('notification.msg.hasBeenConfirmed', 'has been confirmed')
INSERT INTO @translationTbl Values('notification.msg.needsYourApproval', 'needs your approval')
INSERT INTO @translationTbl Values('notification.msg.hasBeenRejected', 'has been rejected')
INSERT INTO @translationTbl Values('notification.msg.hasBeenCancelled', 'has been cancelled')
INSERT INTO @translationTbl Values('notification.msg.hasBeenAmended', 'has been amended')
INSERT INTO @translationTbl Values('notification.msg.shipmentNo', 'Shipment#')
INSERT INTO @translationTbl Values('notification.msg.poNo', 'PO#')

INSERT INTO @translationTbl Values('notification.msg.missingPOOfBookingNo', 'Missing PO of Booking#')
INSERT INTO @translationTbl Values('notification.msg.bookingNo', 'Booking#')

DECLARE translation_cursor CURSOR FOR 
SELECT [Key], [English]
FROM @translationTbl

OPEN translation_cursor
FETCH NEXT FROM translation_cursor
INTO @translationKey, @english

WHILE @@FETCH_STATUS = 0 
BEGIN  
      UPDATE n
	  SET n.MessageKey = REPLACE(n.MessageKey, @english, '~' + @translationKey + '~')
	  FROM Notifications n
	  WHERE n.MessageKey like '%' + @english + '%'
			AND n.CreatedDate >= @fromDate
			AND n.CreatedDate <= @toDate

      FETCH NEXT FROM translation_cursor
	  INTO @translationKey, @english
END 

CLOSE translation_cursor
DEALLOCATE translation_cursor

COMMIT TRANSACTION
GO