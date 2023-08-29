-- Author:			Dong Tran
-- Created date:	25 Nov 2021
-- Description:		[PSP-3025] Patch all existing value of 'Piece' by remmoving 's'

BEGIN TRANSACTION

UPDATE Shipments 
SET TotalUnitUOM = 'Piece'     
WHERE TotalUnitUOM = 'Pieces'

UPDATE Consignments        
SET UnitUOM = 'Piece'   
WHERE UnitUOM = 'Pieces'

UPDATE ShipmentLoadDetails	
SET UnitUOM = 'Piece' 
WHERE UnitUOM = 'Pieces'

UPDATE CargoDetails		
SET UnitUOM	= 'Piece' 
WHERE UnitUOM = 'Pieces'

COMMIT TRANSACTION