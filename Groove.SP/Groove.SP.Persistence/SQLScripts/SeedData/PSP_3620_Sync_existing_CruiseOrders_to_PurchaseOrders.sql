BEGIN tran t;

DECLARE @InsertedPOTbl table
(
   [Id] bigint,
   [PONumber] varchar(512),
   [CruiseOrderId] bigint
)

-- INSERT into dbo.PurchaseOrders

insert into PurchaseOrders
([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [PONumber], [POIssueDate], [Status], [Stage], [POKey], [POType], [CruiseOrderId])
OUTPUT inserted.Id, inserted.PONumber, inserted.CruiseOrderId INTO @InsertedPOTbl
SELECT	CASE WHEN cro.CreatedBy is null THEN ''
			 ELSE cro.CreatedBy
		END,
		cro.CreatedDate,
		cro.UpdatedBy,
		cro.UpdatedDate,
		cro.PONumber,
		cro.PODate,
		CASE WHEN cro.POStatus = 'active' THEN 1
			 ELSE 0
		END,
		20, --Released
		concat(t.Code, '|', cro.PONumber), --CustomerCode|PONumber
		10, --Bulk
		cro.Id
FROM cruise.CruiseOrders cro
OUTER APPLY (
			SELECT top(1) org.code
			FROM cruise.CruiseOrderContacts croc INNER JOIN Organizations org ON org.Id = croc.OrganizationId
			WHERE croc.OrderId = cro.Id AND croc.OrganizationRole = 'principal') t
WHERE cro.POStatus = 'active'



-- INSERT into dbo.PurchaseOrderContacts

insert into PurchaseOrderContacts ( [CreatedBy],
									[CreatedDate],
									[UpdatedBy],
									[UpdatedDate],
									[PurchaseOrderId],
									[OrganizationId],
									[OrganizationCode],
									[OrganizationRole],
									[CompanyName],
									[AddressLine1],
									[AddressLine2],
									[AddressLine3],
									[AddressLine4],
									[ContactName],
									[ContactNumber],
									[ContactEmail])
SELECT  croc.CreatedBy,
		croc.CreatedDate,
		croc.UpdatedBy,
		croc.UpdatedDate,
		ins.Id,
		croc.OrganizationId,
		CASE WHEN org.Code is null THEN '0'
			 ELSE org.Code
		END,
		croc.OrganizationRole,
		croc.CompanyName,
		org.[Address], --line 1
		croc.[Address], --line 2
		org.AddressLine3, --line 3
		org.AddressLine4, --line 4
		croc.ContactName,
		croc.ContactNumber,
		croc.ContactEmail

FROM cruise.CruiseOrderContacts croc inner join @InsertedPOTbl ins on croc.OrderId = ins.CruiseOrderId
left join Organizations org on org.Id = croc.OrganizationId



-- INSERT into dbo.POLineItems

insert into POLineItems([CreatedBy],
						[CreatedDate],
						[UpdatedBy],
						[UpdatedDate],
						[PurchaseOrderId],
						[LineOrder],
						[OrderedUnitQty],
						[BookedUnitQty],
						[BalanceUnitQty],
						[ProductCode],
						[ProductName],
						[UnitUOM],
						[UnitPrice],
						[CurrencyCode],
						[POLineKey])
SELECT croit.CreatedBy,
		croit.CreatedDate,
		croit.UpdatedBy,
		croit.UpdatedDate,
		ins.Id,
		croit.POLine,
		CASE WHEN croit.OrderQuantity is null THEN 0
			 ELSE croit.OrderQuantity
		END,
		0,
		CASE WHEN croit.OrderQuantity is null THEN 0
			 ELSE croit.OrderQuantity
		END,
		croit.ItemId,
		croit.ItemName,
		CASE WHEN croit.UOM = 10 OR croit.UOM = 50 THEN 10 --Each | Unit => Each
			 WHEN croit.UOM = 20 OR croit.UOM = 30 THEN 30 --Cartons | Packages => Set 
			 WHEN croit.UOM = 40 THEN 40 --Pieces => Piece
			 ELSE 0
		END,
		CASE WHEN croit.NetUnitPrice is null THEN 0
			 ELSE croit.NetUnitPrice
		END,
		croit.CurrencyCode,
		concat(t.Code, '|', ins.PONumber, '|', croit.POLine, '|', croit.ItemId)--CustomerCode|PONumber|Line|ProductCode

FROM cruise.CruiseOrderItems croit inner join @InsertedPOTbl ins on croit.OrderId = ins.CruiseOrderId
outer apply (
			SELECT top(1) org.code
			FROM cruise.CruiseOrderContacts croc inner join Organizations org on org.Id = croc.OrganizationId
			WHERE croc.OrderId = ins.CruiseOrderId and croc.OrganizationRole = 'principal') t

COMMIT tran t;
go