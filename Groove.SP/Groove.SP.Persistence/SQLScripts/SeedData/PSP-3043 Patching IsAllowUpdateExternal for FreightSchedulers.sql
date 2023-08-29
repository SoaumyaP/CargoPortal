-- =============================================
-- Author:			Hau Nguyen
-- Created date:	31 Nov 2021
-- Description:	    PSP-3040 [Freight Scheduler] Add "Allow update from external" in Add/Edit/Update/View/Listing
-- =============================================
BEGIN TRANSACTION

UPDATE dbo.[FreightSchedulers]
SET [IsAllowExternalUpdate] = 0
WHERE [CreatedBy] IN ('System', 'FreightSystem', 'MANUALEDISON')
AND [UpdatedBy] LIKE '%@%'

COMMIT TRANSACTION