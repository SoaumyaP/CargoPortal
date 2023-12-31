
-- This script can be run many times
-- Append a logic to seed more data at the end of this file
;
IF (NOT EXISTS (SELECT 1 FROM ViewRoleSettings))
BEGIN
	BEGIN TRAN

	-- SEED dbo.[ViewSettings]

	SET IDENTITY_INSERT [dbo].[ViewSettings] ON 

	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (1, N'id', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (2, N'poNumber', N'PO Number', 10, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (3, N'customerReferences', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (4, N'poIssueDate', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (5, N'status', N'Status', 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (6, N'statusName', NULL, 30, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (7, N'stage', N'Stage', 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (8, N'stageName', NULL, 60, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (9, N'poType', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (10, N'cargoReadyDate', N'Ex-work Date', 40, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (11, N'createdDate', NULL, 20, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (12, N'createdBy', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (13, N'supplier', N'Supplier', 50, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (14, N'isProgressCargoReadyDates', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (15, N'progressNotifyDay', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (16, N'productionStarted', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (17, N'modeOfTransport', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (18, N'shipFrom', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (19, N'shipTo', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (20, N'incoterm', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (21, N'expectedDeliveryDate', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (22, N'expectedShipDate', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (23, N'containerType', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (24, N'poRemark', NULL, 0, N'PO.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (25, N'id', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (26, N'poKey', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (27, N'poNumber', N'PO Number', 10, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (28, N'modeOfTransport', N'Mode of Transport', 80, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (29, N'numberOfLineItems', N'Number of Line Items', 30, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (30, N'poIssueDate', N'PO Issue Date', 20, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (31, N'shipFrom', N'Ship From', 140, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (32, N'shipFromId', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (33, N'shipTo', N'Ship To', 160, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (34, N'shipToId', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (35, N'incoterm', N'Incoterm', 60, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (36, N'carrierCode', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (37, N'gatewayCode', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (38, N'paymentCurrencyCode', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (39, N'earliestDeliveryDate', N'Earliest Delivery Date', 200, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (40, N'earliestShipDate', N'Earliest Ship Date', 180, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (41, N'expectedDeliveryDate', N'Expected Delivery Date', 190, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (42, N'expectedShipDate', N'Expected Ship Date', 170, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (43, N'latestDeliveryDate', N'Latest Delivery Date', 230, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (44, N'latestShipDate', N'Latest Ship Date', 210, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (45, N'customerReferences', N'Customer References', 70, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (46, N'department', N'Department', 100, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (47, N'season', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (48, N'poRemark', N'PO Remark', 70, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (49, N'poTerms', N'PO Terms', 10, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (50, N'hazardousMaterialsInstruction', N'Hazardous Materials Instruction', 80, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (51, N'specialHandlingInstruction', N'Special Handling Instruction', 90, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (52, N'carrierName', N'Carrier Name', 30, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (53, N'gatewayName', N'Gateway Name', 50, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (54, N'shipFromName', N'Ship From Name', 40, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (55, N'shipToName', N'Ship To Name', 60, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (56, N'blanketPOId', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (57, N'status', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (58, N'isProgressCargoReadyDates', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (59, N'isCompulsory', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (60, N'isAllowMissingPO', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (61, N'progressNotifyDay', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (62, N'productionStarted', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (63, N'qcRequired', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (64, N'shortShip', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (65, N'splitShipment', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (66, N'proposeDate', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (67, N'remark', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (68, N'volume', N'Volume', 150, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (69, N'grossWeight', N'Gross Weight', 130, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (70, N'contractShipmentDate', N'Contract Shipment Date', 220, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (71, N'containerType', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (72, N'containerTypeName', N'Equipment Type', 20, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (73, N'poType', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (74, N'statusName', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (75, N'stage', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (76, N'stageName', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (77, N'cargoReadyDate', N'Ex-work Date', 40, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (78, N'allowToBookIn', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (79, N'customerServiceType', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (80, N'supplier', N'Supplier', 90, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (81, N'customer', N'Customer', 50, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (82, N'shipper', N'Shipper', 110, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (83, N'consignee', N'Consignee', 120, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (84, N'createdBy', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (85, N'createdDate', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (86, N'updatedBy', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (87, N'updatedDate', N'', 0, N'PO.Detail', 10, N'System', GETUTCDATE(), N'System', GETUTCDATE())

	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (88, N'id', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (89, N'purchaseOrderId', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (90, N'organizationId', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (91, N'organizationCode', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (92, N'organizationRole', N'Organization Role', 10, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (93, N'companyName', N'Company', 20, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (94, N'addressLine1', N'Address', 30, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (95, N'addressLine2', N'Address', 30, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (96, N'addressLine3', N'Address', 30, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (97, N'addressLine4', N'Address', 30, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (98, N'department', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (99, N'contactName', N'Contact Name', 40, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())

	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (100, N'name', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (101, N'contactNumber', N'Contact Number', 50, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (102, N'contactEmail', N'Contact Email', 60, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (103, N'references', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (104, N'createdBy', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (105, N'createdDate', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (106, N'updatedBy', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (107, N'updatedDate', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (108, N'weChatOrWhatsApp', N'', 0, N'PO.Detail.Contacts', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())

	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (109, N'id', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (110, N'poLineKey', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (111, N'lineOrder', N'Line Order', 10, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (112, N'orderedUnitQty', N'Ordered Qty', 40, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (113, N'productCode', N'Product Code', 20, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (114, N'productName', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (115, N'unitUOM', N'Unit UOM', 50, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (116, N'bookedUnitQty', N'Booked Qty', 60, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (117, N'balanceUnitQty', N'Balance Qty', 70, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (118, N'unitPrice', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (119, N'currencyCode', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (120, N'productFamily', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (121, N'hsCode', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (122, N'chineseDescription', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (123, N'supplierProductCode', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (124, N'minPackageQty', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (125, N'minOrderQty', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (126, N'packageUOM', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (127, N'countryCodeOfOrigin', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (128, N'commodity', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (129, N'referenceNumber1', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (130, N'referenceNumber2', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (131, N'shippingMarks', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (132, N'descriptionOfGoods', N'Description Of Goods', 30, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (133, N'packagingInstruction', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (134, N'productRemark', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (135, N'seasonCode', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (136, N'styleNo', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (137, N'colourCode', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (138, N'size', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (139, N'volume', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (140, N'grossWeight', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (141, N'createdBy', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (142, N'createdDate', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (143, N'updatedBy', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (144, N'updatedDate', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (145, N'colourName', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (146, N'innerQuantity', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (147, N'outerQuantity', N'', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())

	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (148, N'id', N'', 0, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (149, N'number', N'Booking No.', 10, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (150, N'fulfillmentUnitQty', N'Booked Qty', 30, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (151, N'shipFromName', N'Ship From Location', 40, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (152, N'shipToName', N'Ship To Location', 60, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (153, N'expectedShipDate', N'Expected Ship Date', 50, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (154, N'expectedDeliveryDate', N'Expected Delivery Date', 70, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (155, N'stage', N'', 0, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (156, N'orderFulfillmentPolicy', N'', 0, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (157, N'soNo', N'', 0, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())

	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (158, N'id', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (159, N'poNumber', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (160, N'shipTo', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (161, N'shipToId', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (162, N'expectedDeliveryDate', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (163, N'shipToName', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (164, N'blanketPOId', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (165, N'status', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (166, N'statusName', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (167, N'stage', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (168, N'stageName', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (169, N'totalBlanketPOQty', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (170, N'totalAllocatedQty', N'', 0, N'PO.Detail.AllocatedPOs', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	
	SET IDENTITY_INSERT [dbo].[ViewSettings] OFF


	-- SEED dbo.[ViewRoleSettings]

	INSERT INTO ViewRoleSettings(
		[ViewId],
		[RoleId],
		[CreatedBy],
		[CreatedDate],
		[UpdatedBy],
		[UpdatedDate]
	)
	SELECT
		vw.[ViewId],
		rl.[Id],
		N'System' as [CreatedBy],
		GETUTCDATE() as [CreatedDate],
		N'System' as [UpdatedBy],
		GETUTCDATE() as [UpdatedDate]
	FROM ViewSettings vw JOIN Roles rl ON 1 = 1


	COMMIT TRAN
END
GO

-- Additional seeding data

-- PSP-3822: [PO Detail] 'Shipment No.', 'Booked Qty', 'Port of Loading', and 'Port of Discharge' columns are hidden in PO Detail - Booking section if PO is not linked to Booking

BEGIN TRANSACTION

SET IDENTITY_INSERT [dbo].[ViewSettings] ON;

IF (NOT EXISTS (SELECT 1 FROM [dbo].[ViewSettings] WHERE ViewId = 171))
BEGIN
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (171, N'shipmentNo', N'Shipment No. (link to 157)', 0, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT INTO ViewRoleSettings(
		[ViewId],
		[RoleId],
		[CreatedBy],
		[CreatedDate],
		[UpdatedBy],
		[UpdatedDate]
	)
	SELECT
		171,
		rl.[Id],
		N'System' as [CreatedBy],
		GETUTCDATE() as [CreatedDate],
		N'System' as [UpdatedBy],
		GETUTCDATE() as [UpdatedDate]
	FROM Roles rl
END

IF (NOT EXISTS (SELECT 1 FROM [dbo].[ViewSettings] WHERE ViewId = 172))
BEGIN
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (172, N'bookedQuantity', N'Booked Qty (link to 150)', 0, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT INTO ViewRoleSettings(
		[ViewId],
		[RoleId],
		[CreatedBy],
		[CreatedDate],
		[UpdatedBy],
		[UpdatedDate]
	)
	SELECT
		172,
		rl.[Id],
		N'System' as [CreatedBy],
		GETUTCDATE() as [CreatedDate],
		N'System' as [UpdatedBy],
		GETUTCDATE() as [UpdatedDate]
	FROM Roles rl
END

IF (NOT EXISTS (SELECT 1 FROM [dbo].[ViewSettings] WHERE ViewId = 173))
BEGIN
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (173, N'shipFrom', N'Ship From Location (link to 151)', 0, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT INTO ViewRoleSettings(
		[ViewId],
		[RoleId],
		[CreatedBy],
		[CreatedDate],
		[UpdatedBy],
		[UpdatedDate]
	)
	SELECT
		173,
		rl.[Id],
		N'System' as [CreatedBy],
		GETUTCDATE() as [CreatedDate],
		N'System' as [UpdatedBy],
		GETUTCDATE() as [UpdatedDate]
	FROM Roles rl
END

IF (NOT EXISTS (SELECT 1 FROM [dbo].[ViewSettings] WHERE ViewId = 174))
BEGIN
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (174, N'shipTo', N'Ship To Location (link to 152)', 0, N'PO.Detail.Bookings', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT INTO ViewRoleSettings(
		[ViewId],
		[RoleId],
		[CreatedBy],
		[CreatedDate],
		[UpdatedBy],
		[UpdatedDate]
	)
	SELECT
		174,
		rl.[Id],
		N'System' as [CreatedBy],
		GETUTCDATE() as [CreatedDate],
		N'System' as [UpdatedBy],
		GETUTCDATE() as [UpdatedDate]
	FROM Roles rl
END

SET IDENTITY_INSERT [dbo].[ViewSettings] OFF;
COMMIT TRANSACTION


-- PSP-3823: Missing ‘Style Name’ field on 'Product Details' popup

BEGIN TRANSACTION

SET IDENTITY_INSERT [dbo].[ViewSettings] ON;

IF (NOT EXISTS (SELECT 1 FROM [dbo].[ViewSettings] WHERE ViewId = 175))
BEGIN
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) 
	VALUES (175, N'styleName', N'Style Name', 0, N'PO.Detail.LineItems', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())

	INSERT INTO ViewRoleSettings(
		[ViewId],
		[RoleId],
		[CreatedBy],
		[CreatedDate],
		[UpdatedBy],
		[UpdatedDate]
	)
	SELECT
		175,
		rl.[Id],
		N'System' as [CreatedBy],
		GETUTCDATE() as [CreatedDate],
		N'System' as [UpdatedBy],
		GETUTCDATE() as [UpdatedDate]
	FROM Roles rl
END
SET IDENTITY_INSERT [dbo].[ViewSettings] OFF;
COMMIT TRANSACTION
	
IF (NOT EXISTS (SELECT 1 FROM ViewRoleSettings WHERE [ViewId] BETWEEN 176 AND 200))
BEGIN
	BEGIN TRAN

	SET IDENTITY_INSERT [dbo].[ViewSettings] ON 

	-- Quick search Item No.
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (176, N'id', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (177, N'poNumber', N'PO Number', 10, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (178, N'customerReferences', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (179, N'poIssueDate', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (180, N'status', N'Status', 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (181, N'statusName', NULL, 30, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (182, N'stage', N'Stage', 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (183, N'stageName', NULL, 60, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (184, N'containerTypeName', NULL, 60, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (185, N'poType', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (186, N'cargoReadyDate', N'Ex-work Date', 40, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (187, N'createdDate', NULL, 20, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (188, N'createdBy', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (189, N'supplier', N'Supplier', 50, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (190, N'isProgressCargoReadyDates', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (191, N'progressNotifyDay', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (192, N'productionStarted', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (193, N'modeOfTransport', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (194, N'shipFrom', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (195, N'shipTo', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (196, N'incoterm', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (197, N'expectedDeliveryDate', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (198, N'expectedShipDate', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (199, N'containerType', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())
	INSERT [dbo].[ViewSettings] ([ViewId], [Field], [Title], [Sequence], [ModuleId], [ViewType], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (200, N'poRemark', NULL, 0, N'PO.ItemQuickSearch.List', 20, N'System', GETUTCDATE(), N'System', GETUTCDATE())

	SET IDENTITY_INSERT [dbo].[ViewSettings] OFF

	INSERT INTO ViewRoleSettings(
		[ViewId],
		[RoleId],
		[CreatedBy],
		[CreatedDate],
		[UpdatedBy],
		[UpdatedDate]
	)
	SELECT
		VS.[ViewId],
		RL.[Id],
		N'System' as [CreatedBy],
		GETUTCDATE() as [CreatedDate],
		N'System' as [UpdatedBy],
		GETUTCDATE() as [UpdatedDate]
	FROM [Roles] RL 
	JOIN [ViewSettings] VS ON VS.[ViewId] BETWEEN 176 AND 200


	COMMIT TRAN
END

