CREATE view vw_BalanceOfGoods_Transactions
As
SELECT m.PrincipleId,
       o.code                                        AS PrincipleCode,
       o.NAME                                        AS PrincipleName,
       m.WarehouseId,
       w.code                                        AS WarehouseCode,
       w.NAME                                        AS WarehouseName,
       w.LocationId,
       l.NAME                                        AS LocationName,
       m.ArticleId,
       a.itemno                                      AS ArticleCode,
       a.itemdesc                                    AS ArticleName,
       m.received + m.shipped + m.damaged + m.adjust AS AvailableQuantity,
       m.received                                    AS ReceivedQuantity,
       m.shipped                                     AS ShippedQuantity,
       m.adjust                                      AS AdjustQuantity,
       m.damaged                                     AS DamageQuantity
FROM
(select articleid, principleid, warehouseid, 
	sum(case when transtypeid = 1 then quantity else 0 end) as received,
	sum(case when transtypeid = 4 then quantity else 0 end) as shipped,
	sum(case when transtypeid = 2 then quantity else 0 end) as damaged,
	sum(case when transtypeid = 3 then quantity else 0 end) as adjust
from invtransactions
	group by articleid, principleid, warehouseid) m
left outer join Organizations o on m.principleid = o.Id
left outer join WarehouseLocations w on m.warehouseid = w.Id
left outer join ArticleMaster a on m.articleid = a.Id
left outer join Locations l on w.LocationId = l.Id;
