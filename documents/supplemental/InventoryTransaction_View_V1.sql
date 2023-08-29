SELECT m.principleid,
       o.code                                        AS PrincipleCode,
       o.NAME                                        AS PrincipleName,
       m.warehouseid,
       w.code                                        AS WarehouseCode,
       w.NAME                                        AS WarehouseName,
       w.locationid,
       l.NAME                                        AS LocationName,
       m.articleid,
       a.itemno                                      AS ArticleCode,
       a.itemdesc                                    AS ArticleName,
       m.received - m.shipped - m.damaged + m.adjust AS AvailableQuantity,
       m.received                                    AS ReceivedQuantity,
       m.shipped                                     AS ShippedQuantity,
       m.adjust                                      AS AdjustQuantity,
       m.damaged                                     AS DamagedQuantity
FROM   (SELECT principleid,
               warehouseid,
               articleid,
               Sum(CASE
                     WHEN transtypecode IN ( 1, 4 )
                          AND IsIn = 1 THEN quantity
                     ELSE 0
                   END) Received,
               Sum(CASE
                     WHEN transtypecode IN ( 1, 4 )
                          AND IsIn = 0 THEN quantity
                     ELSE 0
                   END) Shipped,
               Sum(CASE transtypecode
                     WHEN 2 THEN quantity
                     ELSE 0
                   END) Damaged,
               Sum(CASE transtypecode
                     WHEN 3 THEN quantity
                     ELSE 0
                   END) Adjust
        FROM   (SELECT toorgid    AS PrincipleId,
                       toinvlocid AS WarehouseId,
                       articleid,
                       transtypecode,
                       quantity,
                       1        IsIn
                FROM   inventorytransactions where TransTypeCode in (1, 4)
                UNION
                SELECT fromorgid    AS PrincipleId,
                       frominvlocid AS WarehouseId,
                       articleid,
                       transtypecode,
                       quantity,
                       0          IsIn
                FROM   inventorytransactions where TransTypeCode in (1, 4)
				UNION
				SELECT fromorgid    AS PrincipleId,
                       frominvlocid AS WarehouseId,
                       articleid,
                       transtypecode,
                       quantity,
                       0          IsIn
                FROM   inventorytransactions where TransTypeCode in (2, 3)			
				) Tmp
        GROUP  BY principleid,
                  warehouseid,
                  articleid) m
       LEFT OUTER JOIN articlemaster a
                    ON m.articleid = a.id
       LEFT OUTER JOIN warehouselocations w
                    ON m.warehouseid = w.id
       LEFT OUTER JOIN organizations o
                    ON m.principleid = o.id
       LEFT OUTER JOIN locations l
                    ON w.locationid = l.id 
					order by articleid, principleid