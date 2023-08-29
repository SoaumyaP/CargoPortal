create table InvTransactionAdditional (
	Id bigint primary key references InvTransactions(Id),
	PoNumber nvarchar(50) null,
	SoNumber nvarchar(50) null,
	BLNumber nvarchar(50) null,
	DocumentNumber nvarchar(50) null,
	NoOfPackage int null,
	PackageUOM int null,
	VolumneCBM decimal(18, 4) null,
	GrossWeightKgs decimal(18, 2) null,
	Remarks nvarchar(max) null
)

Create view vw_BalanceOfGoods_Details 
as
select
	m.Id,
	m.PrincipleId, o.Code as PrincipleCode, o.Name as PrincipleName,
	m.ArticleId, a.ItemNo as ArticleCode, a.ItemDesc as ArticleName,
	m.WarehouseId, w.Code as WarehouseCode, w.Name as WarehouseName, l.Name as LocationName,
	m.TransTypeId, t.Code as TransTypeCode, 
	m.Quantity, isnull(m.QuantityUOM, 0) as QuantityUOM,
	Isnull(ma.PoNumber, '') as PoNumber,
	isnull(ma.SoNumber, '') as SoNumber,
	isnull(ma.BLNumber, '') as BlNumber,
	isnull(ma.DocumentNumber, '') as DocumentNumber,
	isnull(ma.NoOfPackage, 0) as NoOfPackage,
	isnull(ma.PackageUOM, 0) as PackageUOM,
	isnull(ma.VolumneCBM, 0) as VolumneCBM,
	isnull(ma.GrossWeightKgs, 0) as GrossWeightKgs,
	isnull(ma.Remarks, '') as Remarks
from invTransactions m
left outer join Organizations o on m.PrincipleId = o.Id
left outer join ArticleMaster a on m.ArticleId = a.Id
left outer join WarehouseLocations w on m.WarehouseId = w.Id
left outer join Locations l on w.LocationId = l.Id
left outer join TransTypes t on m.TransTypeId = t.Id
left outer join InvTransactionAdditional ma on m.id = ma.Id
