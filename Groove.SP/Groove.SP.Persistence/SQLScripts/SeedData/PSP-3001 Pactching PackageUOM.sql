-- =============================================
-- Author:			Dong Tran
-- Created date:	19 Nov 2021
-- Description:		[PSP-3001] patch all existing values 'Cartons' to 'Carton', 'Pieces' to 'Piece'
-- =============================================

SET NOCOUNT ON;

BEGIN TRANSACTION

UPDATE POLineItems
SET PackageUOM = 50
WHERE PackageUOM = 60

UPDATE POFulfillmentOrders
SET PackageUOM = 50
WHERE PackageUOM = 60

UPDATE POFulfillmentLoads
SET PackageUOM = 50
WHERE PackageUOM = 60

UPDATE POFulfillmentLoadDetails
SET PackageUOM = 50
WHERE PackageUOM = 60

UPDATE Shipments
SET TotalPackageUOM = CASE 
						WHEN TotalPackageUOM = 'Cartons' THEN 'Carton'
						WHEN TotalPackageUOM = 'Pieces' THEN 'Piece'
				      END
WHERE TotalPackageUOM = 'Cartons' OR TotalPackageUOM = 'Pieces'

UPDATE ShipmentLoadDetails
SET PackageUOM = CASE 
					WHEN PackageUOM = 'Cartons' THEN 'Carton'
					WHEN PackageUOM = 'Pieces' THEN 'Piece'
				 END
WHERE PackageUOM = 'Cartons' OR PackageUOM = 'Pieces'

UPDATE CargoDetails
SET PackageUOM = CASE 
					WHEN PackageUOM = 'Cartons' THEN 'Carton'
					WHEN PackageUOM = 'Pieces' THEN 'Piece'
				 END
WHERE PackageUOM = 'Cartons' OR PackageUOM = 'Pieces'

UPDATE Consignments
SET PackageUOM = CASE 
					WHEN PackageUOM = 'Cartons' THEN 'Carton'
					WHEN PackageUOM = 'Pieces' THEN 'Piece'
				 END
WHERE PackageUOM = 'Cartons' OR PackageUOM = 'Pieces'

UPDATE Containers
SET TotalPackageUOM = CASE 
					WHEN TotalPackageUOM = 'Cartons' THEN 'Carton'
					WHEN TotalPackageUOM = 'Pieces' THEN 'Piece'
				 END
WHERE TotalPackageUOM = 'Cartons' OR TotalPackageUOM = 'Pieces'

UPDATE Consolidations
SET TotalPackageUOM = CASE 
					WHEN TotalPackageUOM = 'Cartons' THEN 'Carton'
					WHEN TotalPackageUOM = 'Pieces' THEN 'Piece'
				 END
WHERE TotalPackageUOM = 'Cartons' OR TotalPackageUOM = 'Pieces'

UPDATE BillOfLadings
SET TotalPackageUOM = CASE 
					WHEN TotalPackageUOM = 'Cartons' THEN 'Carton'
					WHEN TotalPackageUOM = 'Pieces' THEN 'Piece'
				 END
WHERE TotalPackageUOM = 'Cartons' OR TotalPackageUOM = 'Pieces'

UPDATE POFulfillmentCargoDetails
SET PackageUOM = 50
WHERE PackageUOM = 60

COMMIT TRANSACTION