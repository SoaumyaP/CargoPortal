---------------------------
--Set up view settings for booking module (Freight and NVO).
--This script support for running multiple times, but it's not good for the performance because the data will be deleted and re-added. (id will not increment in sequence)
--We need to refactor it later
---------------------------

BEGIN TRAN

--Temporary delete to make this script can run multiple times as we must refactor it later.
DELETE vw
FROM ViewSettings vw
WHERE (vw.ModuleId LIKE 'Booking.%' OR vw.ModuleId LIKE 'BulkBooking.%' OR vw.ModuleId LIKE 'FreightBooking.%') 


-- SEED dbo.[ViewSettings]

--SET IDENTITY_INSERT [dbo].[ViewSettings] ON 

--List of Booking
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'number', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'statusName', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'stage', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'stageName', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'fulfillmentType', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'orderFulfillmentPolicy', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'bookingDate', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cargoReadyDate', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipFromName', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'customer', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'supplier', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isRejected', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isPending', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isPOAdhocChanged', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'createdDate', NULL, 0, N'Booking.List', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())

--Bulk booking copy list
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'BulkBooking.CopyList', 20 , N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'number', NULL, 0, N'BulkBooking.CopyList', 20 , N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'bookingDate', NULL, 0, N'BulkBooking.CopyList', 20 , N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cargoReadyDate', NULL, 0, N'BulkBooking.CopyList', 20 , N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipFromName', NULL, 0, N'BulkBooking.CopyList', 20 , N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipToName', NULL, 0, N'BulkBooking.CopyList', 20 , N'System', GETUTCDATE(), NULL, GETUTCDATE())

--Bulk booking General
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'BulkBooking.Detail', 10 , N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'number', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'owner', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'statusName', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'stage', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'stageName', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cargoReadyDate', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'incoterm', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isGeneratePlanToShip', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isPartialShipment', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isContainDangerousGoods', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isAllowMixedCarton', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'modeOfTransport', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'preferredCarrier', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'logisticsServiceName', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'movementTypeName', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'logisticsService', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'movementType', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipFrom', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipTo', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipFromName', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipToName', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'expectedShipDate', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'expectedDeliveryDate', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'remarks', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'PoRemark', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'vesselName', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'voyageNo', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isRejected', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isForwarderBookingItineraryReady', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isNeedToPlanToShipAgain', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isFulfilledFromPO', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'fulfilledFromPOType', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isShipperPickup', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isNotifyPartyAsConsignee', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isCIQOrFumigation', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isBatteryOrChemical', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isExportLicence', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'receiptPortId', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'deliveryPortId', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'receiptPort', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'deliveryPort', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cyEmptyPickupTerminalCode', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cyEmptyPickupTerminalDescription', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cfSWarehouseCode', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cfSWarehouseDescription', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cyClosingDate', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cfSClosingDate', NULL, 0, N'BulkBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

--Bulk booking Planned Schedule
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'poFulfillmentId', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sequence', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'modeOfTransport', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'carrierId', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'carrierName', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'vesselFlight', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingPortId', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingPort', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'dischargePortId', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'dischargePort', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'etdDate', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'etaDate', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipmentNo', NULL, 0, N'BulkBooking.Detail.PlannedSchedule', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

-- Bulk booking Contacts
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'organizationId', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'organizationRole', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'companyName', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'address', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'contactName', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'contactNumber', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'contactEmail', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'contactSequence', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'weChatOrWhatsApp', NULL, 0, N'BulkBooking.Detail.Contacts', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

-- Bulk booking Cargo Details
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadReferenceNumber', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'equipmentType', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'plannedVolume', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'plannedNetWeight', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'plannedGrossWeight', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'plannedPackageQuantity', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalPackageQuantity', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalUnitQuantity', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalNetWeight', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalGrossWeight', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalVolume', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageUOM', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'containerNumber', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sealNumber', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sealNumber2', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingDate', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'gateInDate', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'totalGrossWeight', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'totalNetWeight', NULL, 0, N'BulkBooking.Detail.Loads', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'purchaseOrderId', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'poLineItemId', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'customerPONumber', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'productCode', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'productName', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'fulfillmentUnitQty', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'bookedPackage', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'grossWeight', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'volume', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'unitUOM', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageUOM', NULL, 0, N'BulkBooking.Detail.CargoDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'purchaseOrderId', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'poLineItemId', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'customerPONumber', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'productCode', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'productName', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'orderedUnitQty', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'fulfillmentUnitQty', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'balanceUnitQty', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadedQty', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'openQty', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'unitUOM', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageUOM', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'commodity', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'hsCode', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'countryNameOfOrigin', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'bookedPackage', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'volume', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'grossWeight', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'netWeight', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'innerQuantity', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'outerQuantity', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shippingMarks', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'descriptionOfGoods', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'chineseDescription', NULL, 0, N'BulkBooking.Detail.CargoDetails.Item', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

-- Bulk booking load details
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'poFulfillmentLoadId', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'customerPONumber', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'productCode', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageQuantity', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageUOM', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'height', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'width', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'length', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'dimensionUnit', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'unitQuantity', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'volume', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'grossWeight', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'netWeight', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'poFulfillmentOrderId', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shippingMarks', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageDescription', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sequence', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ( [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loads', NULL, 0, N'BulkBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

-- FreightBooking.Detail
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'number', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'owner', N'Booked By', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'statusName', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'stage', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'stageName', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cargoReadyDate', N'Cargo Ready Date', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'customer', N'Customer', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'supplier', N'Supplier', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'incoterm', N'Incoterms', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isGeneratePlanToShip', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isPartialShipment', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isContainDangerousGoods', N'Dangerous Goods', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'modeOfTransport', N'Mode of Transport', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'preferredCarrier', N'Carrier Name', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'logisticsServiceName', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'movementTypeName', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'logisticsService', N'Service Type', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'movementType', N'Movement Type', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipFrom', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipTo', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipFromName', N'Port of Loading/Origin', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipToName', N'Port of Discharge/Destination', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'expectedShipDate', N'Expected Ship Date', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'expectedDeliveryDate', N'Expected Delivery Date', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'remarks', N'Remarks', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cyEmptyPickupTerminalCode', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cyEmptyPickupTerminalDescription', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
GO
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cfsWarehouseCode', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cfsWarehouseDescription', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cyClosingDate', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'cfsClosingDate', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isRejected', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isForwarderBookingItineraryReady', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isNeedToPlanToShipAgain', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isFulfilledFromPO', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'fulfilledFromPOType', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'fulfillmentType', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'orderFulfillmentPolicy', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipmentId', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipmentNumber', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipmentStatus', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isShipperPickup', N'Require Pickup?', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isNotifyPartyAsConsignee', N'Notify Party same as Consignee?', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isCIQOrFumigation', N'CIQ or Fumigation', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isBatteryOrChemical', N'Battery or Chemical', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isExportLicence', N'Export Licence', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'receiptPortId', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'deliveryPortId', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'receiptPort', N'Place of Receipt', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'deliveryPort', N'Place of Delivery', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'agentAssignmentMode', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'bookingRequestResult', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'createdBy', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'createdDate', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'updatedBy', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'updatedDate', N'', 0, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'customerPrefix', NULL, NULL, N'FreightBooking.Detail', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
GO
-- FreightBooking.Detail.Contacts
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'organizationId', NULL, NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'organizationRole', N'Organization Role', NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'companyName', N'Company Name', NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'address', N'Address', NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'contactName', N'Contact Name', NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'contactNumber', N'Contact Phone Number', NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'contactEmail', N'Contact Email', NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'contactSequence', NULL, NULL, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'weChatOrWhatsApp', N'WeChat ID/WhatsApp', 0, N'FreightBooking.Detail.Contacts', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())

-- FreightBooking.Detail.CustomerPOs
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'purchaseOrderId', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'poLineItemId', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'customerPONumber', N'Customer PO', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'productCode', N'Product Code', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'productName', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'orderedUnitQty', N'Ordered Qty', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'fulfillmentUnitQty', N'Booked Qty', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'balanceUnitQty', N'Balance Qty', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadedQty', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'openQty', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'unitUOM', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageUOM', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'commodity', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', N'Status', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'hsCode', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'countryCodeOfOrigin', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'bookedPackage', N'Booked Package', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'volume', N'Volume (CBM)', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'grossWeight', N'Gross Weight (KGS)', NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'netWeight', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'innerQuantity', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'outerQuantity', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shippingMarks', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'descriptionOfGoods', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'chineseDescription', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'isLatestUpdated', NULL, NULL, N'FreightBooking.Detail.CustomerPOs', 20, N'System', GETUTCDATE(), NULL, GETUTCDATE())
GO
--FreightBooking.Detail.Loads
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadReferenceNumber', N'Load Ref. No.', NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'equipmentType', N'Equipment Type', NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'plannedVolume', N'Planned Volume (CBM)', NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'plannedNetWeight', N'Planned Net Weight (KGS)', NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'plannedGrossWeight', N'Planned Gross Weight (KGS)', NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'plannedPackageQuantity', N'Planned Package Qty', NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalPackageQuantity', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalUnitQuantity', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalNetWeight', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalGrossWeight', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'subtotalVolume', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageUOM', N'Package UOM', NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'containerNumber', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sealNumber', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sealNumber2', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingDate', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'gateInDate', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'totalGrossWeight', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'totalNetWeight', NULL, NULL, N'FreightBooking.Detail.Loads', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())

--FreightBooking.Detail.Shipment.PlannedSchedule
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sequence', NULL, NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'modeOfTransport', N'Mode of Transport', NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'carrierId', NULL, NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'carrierName', N'Carrier', NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'vesselFlight', N'Vessel/Flight', NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingPortId', NULL, NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingPort', N'Loading Port', NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'dischargePortId', NULL, NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'dischargePort', N'Discharge Port', NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'etdDate', N'ETD', NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'etaDate', N'ETA', NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', NULL, NULL, N'FreightBooking.Detail.Shipment.PlannedSchedule', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
--FreightBooking.Detail.Shipment
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, NULL, N'FreightBooking.Detail.Shipment', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipmentNo',  N'Shipment No.', NULL, N'FreightBooking.Detail.Shipment', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipFrom',  N'Ship From', NULL, N'FreightBooking.Detail.Shipment', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipFromETDDate', N'Expected Ship Date', NULL, N'FreightBooking.Detail.Shipment', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipTo', N'Ship To', NULL, N'FreightBooking.Detail.Shipment', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shipToETADate', N'Expected Delivery Date', NULL, N'FreightBooking.Detail.Shipment', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'status', NULL, NULL, N'FreightBooking.Detail.Shipment', 20, N'System',GETUTCDATE(), NULL,GETUTCDATE())
GO
--FreightBooking.Detail.LoadDetails
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'poFulfillmentLoadId', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'customerPONumber', N'PO No.', 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'productCode', N'Product No.', 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageQuantity', N'Loaded Package', 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageUOM', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'height', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'width', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'length', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'dimensionUnit', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'unitQuantity', N'Loaded Qty', 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'volume', N'Volume', 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'grossWeight', N'Gross Weight', 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'netWeight', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'poFulfillmentOrderId', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'shippingMarks', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'packageDescription', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sequence', N'Seq.', 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loads', NULL, 0, N'FreightBooking.Detail.LoadDetails', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

--FreightBooking.Detail.Shipment.ShipmentItineraries
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'id', NULL, NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'modeOfTransport', N'Mode of Transport', NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'carrierName', N'Carrier', NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'scheduleId', NULL, NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'vesselFlight', N'Vessel/Flight', NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingPort', N'Loading Port', NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'etdDate', N'ETD', NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'dischargePort', N'Discharge Port', NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'etaDate', N'ETA', NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sequence', NULL, NULL, N'FreightBooking.Detail.Shipment.ShipmentItineraries', 20, N'System', CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2), NULL, CAST(N'2023-04-06T00:00:00.0000000' AS DateTime2))
--SET IDENTITY_INSERT [dbo].[ViewSettings] OFF

GO

-- SEED dbo.[ViewRoleSettings]

CREATE TABLE #HideColumns (
	[Field] nvarchar(128) not null,
	[ModuleId] nvarchar(256) not null
)
INSERT INTO #HideColumns VALUES ('customerPrefix', 'FreightBooking.Detail')
INSERT INTO #HideColumns VALUES ('loadReferenceNumber', 'FreightBooking.Detail.Loads')


INSERT INTO ViewRoleSettings(
	[ViewId],
	[RoleId],
	[CreatedBy],
	[CreatedDate],
	[UpdatedDate]
)
SELECT
	vw.[ViewId],
	rl.[Id],
	'System' as [CreatedBy],
	GETUTCDATE() as [CreatedDate],
	GETUTCDATE() as [UpdatedDate]
FROM ViewSettings vw JOIN Roles rl ON 1 = 1
WHERE (vw.ModuleId LIKE 'Booking.%' OR vw.ModuleId LIKE 'BulkBooking.%' OR vw.ModuleId LIKE 'FreightBooking.%') 
AND NOT EXISTS (
	SELECT 1 FROM #HideColumns cte
	WHERE cte.Field = vw.Field and cte.ModuleId = vw.ModuleId
)
--IN (
--	'Booking.List',
--	'BulkBooking.Detail',
--	'BulkBooking.CopyList',
--	'BulkBooking.Detail.PlannedSchedule',
--	'BulkBooking.Detail.Contacts',
--	'BulkBooking.Detail.CargoDetails',
--	'BulkBooking.Detail.CargoDetails.Item',
--	'BulkBooking.Detail.Loads',
--	'BulkBooking.Detail.LoadDetails')

-- clean up
DROP TABLE #HideColumns

COMMIT TRAN

GO


BEGIN TRAN
DELETE vw
FROM ViewSettings vw
WHERE vw.ModuleId = 'FreightBooking.Detail.LoadDetails.ContainerInfo' OR vw.ModuleId = 'BulkBooking.Detail.LoadDetails.ContainerInfo' 

	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadReferenceNumber', NULL, 0, N'FreightBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'equipmentType', NULL, 0, N'FreightBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'containerNumber', NULL, 0, N'FreightBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sealNumber', NULL, 0, N'FreightBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingDate', NULL, 0, N'FreightBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sealNumber2', NULL, 0, N'FreightBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())

	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadReferenceNumber', NULL, 0, N'BulkBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'equipmentType', NULL, 0, N'BulkBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'containerNumber', NULL, 0, N'BulkBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sealNumber', NULL, 0, N'BulkBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'gateInDate', NULL, 0, N'BulkBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'sealNumber2', NULL, 0, N'BulkBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (N'loadingDate', NULL, 0, N'BulkBooking.Detail.LoadDetails.ContainerInfo', 10, N'System', GETUTCDATE(), NULL, GETUTCDATE())


INSERT INTO ViewRoleSettings(
	[ViewId],
	[RoleId],
	[CreatedBy],
	[CreatedDate],
	[UpdatedDate]
)
SELECT
	vw.[ViewId],
	rl.[Id],
	'System' as [CreatedBy],
	GETUTCDATE() as [CreatedDate],
	GETUTCDATE() as [UpdatedDate]
FROM ViewSettings vw JOIN Roles rl ON 1 = 1
WHERE vw.ModuleId like ('FreightBooking.Detail.LoadDetails.ContainerInfo%') or vw.ModuleId like ('BulkBooking.Detail.LoadDetails.ContainerInfo%')

COMMIT TRAN

GO

