CREATE view vw_BalanceOfGoods_Details
as
select
	m.Id,
	m.PrincipleId, o.Code as PrincipleCode, o.Name as PrincipleName,
	m.ArticleId, a.ItemNo as ArticleCode, a.ItemDesc as ArticleName,
	m.WarehouseId, w.Code as WarehouseCode, w.Name as WarehouseName, l.Name as LocationName,
	m.TransTypeId, t.Code as TransTypeCode, m.CreatedDate as TransactionDate,
	m.Quantity, ifnull(m.QuantityUOM, 0) as QuantityUOM,
	Ifnull(ma.PoNumber, '') as PoNumber,
	Ifnull(ma.SoNumber, '') as SoNumber,
	Ifnull(ma.BLNumber, '') as BlNumber,
	Ifnull(ma.DocumentNumber, '') as DocumentNumber,
	Ifnull(ma.NoOfPackage, 0) as NoOfPackage,
	Ifnull(ma.PackageUOM, 0) as PackageUOM,
	Ifnull(ma.VolumneCBM, 0) as VolumneCBM,
	Ifnull(ma.GrossWeightKgs, 0) as GrossWeightKgs,
	Ifnull(ma.Remarks, '') as Remarks
from invTransactions m
left outer join Organizations o on m.PrincipleId = o.Id
left outer join ArticleMaster a on m.ArticleId = a.Id
left outer join WarehouseLocations w on m.WarehouseId = w.Id
left outer join Locations l on w.LocationId = l.Id
left outer join TransTypes t on m.TransTypeId = t.Id
left outer join InvTransactionAdditional ma on m.id = ma.Id;