-- create encryption key
CREATE MASTER KEY ENCRYPTION BY PASSWORD = '<put-your-encryption-password-here>' ;
go

-- create credential for masterdatadb
CREATE DATABASE SCOPED CREDENTIAL MasterDataCred
WITH
     IDENTITY = '<sql-login>',
     SECRET = '<sql-login-password>' ;
go

-- create external data source for Masterdatadb
CREATE EXTERNAL DATA SOURCE [MasterDataDataSrc] WITH 
(
	TYPE = RDBMS, 
	LOCATION = N'cargofe-csportal.database.windows.net', 
	CREDENTIAL = MasterDataCred, 
	DATABASE_NAME = N'Masterdatadb'
);
GO

-- create external data soruce for csportaldb
CREATE EXTERNAL DATA SOURCE [CsPortDataSrc] WITH 
(
	TYPE = RDBMS, 
	LOCATION = N'cargofe-csportal.database.windows.net', 
	CREDENTIAL = MasterDataCred, 
	DATABASE_NAME = N'csportaldb'
);
GO

-- create external table 
CREATE EXTERNAL TABLE [dbo].[Locations]
(
	[Id] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [varchar](128) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [varchar](128) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Name] [nvarchar](128) NULL,
	[CountryId] [bigint] NOT NULL,
	[EdiSonPortCode] [nvarchar](128) NULL,
	[LocationDescription] [nvarchar](128) NOT NULL
)
WITH (DATA_SOURCE = [MasterDataDataSrc]);
GO

-- create external table Organizations
CREATE EXTERNAL TABLE [dbo].[Organizations]
(
	[Id] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [varchar](128) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [varchar](128) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Code] [varchar](35) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[ContactEmail] [varchar](128) NULL,
	[ContactName] [nvarchar](256) NULL,
	[ContactNumber] [varchar](32) NULL,
	[Address] [nvarchar](500) NULL,
	[EdisonInstanceId] [nvarchar](32) NULL,
	[EdisonCompanyCodeId] [nvarchar](32) NULL,
	[CustomerPrefix] [varchar](5) NULL,
	[LocationId] [bigint] NULL,
	[OrganizationType] [int] NOT NULL,
	[ParentId] [varchar](256) NULL,
	[Status] [int] NOT NULL,
	[IsBuyer] [bit] NOT NULL,
	[AdminUser] [nvarchar](max) NULL,
	[WebsiteDomain] [nvarchar](max) NULL,
	[AddressLine2] [nvarchar](50) NULL,
	[AddressLine3] [nvarchar](50) NULL,
	[AddressLine4] [nvarchar](50) NULL,
	[OrganizationLogo] [nvarchar](max) NULL
)
WITH (DATA_SOURCE = [MasterDataDataSrc]);
GO

-- create external table WarehouseAssignments
CREATE EXTERNAL TABLE [dbo].[WarehouseAssignments]
(
	[WarehouseLocationId] [bigint] NOT NULL,
	[OrganizationId] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [varchar](128) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [varchar](128) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ContactEmail] [nvarchar](128) NULL,
	[ContactPerson] [nvarchar](256) NULL,
	[ContactPhone] [nvarchar](32) NULL
)
WITH (DATA_SOURCE = [MasterDataDataSrc]);
GO

-- create external table WarehouseLocations
CREATE EXTERNAL TABLE [dbo].[WarehouseLocations]
(
	[Id] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [varchar](128) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [varchar](128) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Code] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[AddressLine1] [nvarchar](256) NOT NULL,
	[AddressLine2] [nvarchar](256) NULL,
	[AddressLine3] [nvarchar](256) NULL,
	[AddressLine4] [nvarchar](256) NULL,
	[ContactPerson] [nvarchar](256) NULL,
	[ContactPhone] [nvarchar](32) NULL,
	[ContactEmail] [nvarchar](128) NULL,
	[LocationId] [bigint] NOT NULL,
	[OrganizationId] [bigint] NOT NULL,
	[Remarks] [nvarchar](512) NULL,
	[WorkingHours] [nvarchar](512) NULL
)
WITH (DATA_SOURCE = [MasterDataDataSrc]);
GO

-- create exteranl table ArticleMaster
CREATE EXTERNAL TABLE [dbo].[ArticleMaster]
(
	[CompanyCode] [varchar](10) NOT NULL,
	[CompanyType] [varchar](1) NOT NULL,
	[PONo] [varchar](40) NOT NULL,
	[ItemNo] [varchar](50) NOT NULL,
	[ShipmentNo] [varchar](5) NOT NULL,
	[POSeq] [bigint] NOT NULL,
	[DestCode] [varchar](5) NOT NULL,
	[OrderDetailKey] [varchar](30) NULL,
	[CategoryCode] [varchar](30) NULL,
	[ItemDepth] [numeric](18, 3) NULL,
	[ItemHeight] [numeric](18, 3) NULL,
	[ItemWidth] [numeric](18, 3) NULL,
	[ItemDesc] [varchar](256) NULL,
	[UnitWeight] [decimal](18, 3) NULL,
	[CartonType] [varchar](20) NULL,
	[AssignedSupplier] [varchar](3999) NULL,
	[SupplierType] [varchar](3999) NULL,
	[Barcode] [varchar](3999) NULL,
	[BarcodeType] [varchar](3999) NULL,
	[Seller] [bigint] NULL,
	[InnerQuantity] [int] NULL,
	[InnerDepth] [numeric](18, 3) NULL,
	[InnerHeight] [numeric](18, 3) NULL,
	[InnerWidth] [numeric](18, 3) NULL,
	[InnerGrossWeight] [numeric](18, 3) NULL,
	[OuterQuantity] [int] NULL,
	[OuterDepth] [numeric](18, 3) NULL,
	[OuterHeight] [numeric](18, 3) NULL,
	[OuterWidth] [numeric](18, 3) NULL,
	[OuterGrossWeight] [decimal](18, 3) NULL,
	[DisplaySetFlat] [varchar](30) NULL,
	[MembersQuantity] [varchar](3999) NULL,
	[MembersItemId] [varchar](3999) NULL,
	[ItemPrice] [numeric](18, 3) NULL,
	[ProcurementRule] [varchar](10) NULL,
	[Status] [varchar](1) NULL,
	[CreatedBy] [varchar](10) NULL,
	[CreatedOn] [datetime] NULL,
	[UpdatedBy] [varchar](10) NULL,
	[UpdatedOn] [datetime] NULL,
	[StyleNo] [nvarchar](256) NULL,
	[ColourCode] [nvarchar](256) NULL,
	[Size] [nvarchar](256) NULL,
	[StyleName] [nvarchar](256) NULL,
	[ColourName] [nvarchar](256) NULL,
	[Id] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL
)
WITH (DATA_SOURCE = [CsPortDataSrc]);
GO

-- create table TransTypes
CREATE TABLE [dbo].[TransTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
GO

-- create table InventoryTransactions
CREATE TABLE [dbo].[InventoryTransactions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ArticleId] [bigint] NOT NULL,
	[POId] [bigint] NULL,
	[ShippingOrderNo] [varchar](50) NULL,
	[BillOfLadingNo] [varchar](50) NULL,
	[DocumentNo] [varchar](50) NULL,
	[FromOrgId] [bigint] NULL,
	[FromInvLocId] [bigint] NOT NULL,
	[ToOrgId] [bigint] NULL,
	[ToInvLocId] [bigint] NOT NULL,
	[TransTypeCode] [int] NOT NULL,
	[TransDate] [datetime2](7) NOT NULL,
	[NoOfPackage] [int] NULL,
	[PackageUOM] [int] NULL,
	[Quantity] [int] NOT NULL,
	[QuantityUOM] [int] NULL,
	[VolumeCBM] [decimal](18, 4) NULL,
	[GrossWeightKgs] [decimal](18, 2) NULL,
	[Remarks] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](256) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_InventoryTransactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
GO

-- create view vw_BalanceOfGoods_Transactions
CREATE view [dbo].[vw_BalanceOfGoods_Transactions]
As
WITH trans AS
(
                SELECT          a.id,
                                a.articleid,
                                a.fromorgid,
                                a.frominvlocid,
                                a.toorgid,
                                a.toinvlocid,
                                a.transtypecode,
                                d.code AS transcode,
                                a.quantity,
                                b.itemno,
                                b.itemdesc,
                                Isnull(forg.code, '') AS fromorgcode,
                                Isnull(forg.NAME, '') AS fromorgname,
                                Isnull(torg.code, '') AS toorgcode,
                                Isnull(torg.NAME, '') AS toorgname,
                                fw.code               AS fromwarehousecode,
                                fw.NAME               AS fromwarehousename,
                                rw.code               AS towarehousecode,
                                rw.NAME               AS towarehousename,
                                fl.NAME               AS fromlocationname,
                                rl.NAME               AS tolocationname,
                                fl.id                 AS fromlocationid,
                                rl.id                 AS tolocationid
                FROM            inventorytransactions a
                INNER JOIN      articlemaster b
                ON              a.articleid = b.id
                LEFT OUTER JOIN organizations fOrg
                ON              a.fromorgid = forg.id
                LEFT OUTER JOIN organizations tOrg
                ON              a.toorgid = torg.id
                INNER JOIN      transtypes d
                ON              a.transtypecode = d.id
                INNER JOIN      warehouselocations fw
                ON              a.frominvlocid = fw.id
                INNER JOIN      warehouselocations rw
                ON              a.toinvlocid = rw.id
                INNER JOIN      locations fl
                ON              fw.locationid = fl.id
                INNER JOIN      locations rl
                ON              rw.locationid = rl.id), 
received AS
(
         SELECT   articleid,
                  toorgid    AS principleid,
                  toinvlocid AS warehouseid,
                  transtypecode,
                  Sum(quantity) AS receivedqty
         FROM     trans
         WHERE    transtypecode = 1
         GROUP BY articleid,
                  toorgid,
                  toinvlocid,
                  transtypecode), 
shipped AS
(
         SELECT   articleid,
                  fromorgid    AS principleid,
                  frominvlocid AS warehouseid,
                  transtypecode,
                  Sum(quantity) AS shippedqty
         FROM     trans
         WHERE    transtypecode = 1
         GROUP BY articleid,
                  fromorgid,
                  frominvlocid,
                  transtypecode), adjusted AS
(
         SELECT   articleid,
                  fromorgid    AS principleid,
                  frominvlocid AS warehouseid,
                  transtypecode,
                  Sum(quantity) AS adjustedqty
         FROM     trans
         WHERE    transtypecode = 2
         GROUP BY articleid,
                  fromorgid,
                  frominvlocid,
                  transtypecode), damaged AS
(
         SELECT   articleid,
                  fromorgid    AS principleid,
                  frominvlocid AS warehouseid,
                  transtypecode,
                  Sum(quantity) AS damagedqty
         FROM     trans
         WHERE    transtypecode = 3
         GROUP BY articleid,
                  fromorgid,
                  frominvlocid,
                  transtypecode), 
masterrow AS
(
         SELECT   principleid,
                  principlecode,
                  principlename,
                  warehouseid,
                  warehousecode,
                  warehousename,
                  locationid,
                  locationname,
                  articleid,
                  itemno,
                  itemdesc
         FROM     (
                           SELECT   fromorgid         AS principleid,
                                    fromorgcode       AS principlecode,
                                    fromorgname       AS principlename,
                                    frominvlocid      AS warehouseid,
                                    fromwarehousecode AS warehousecode,
                                    fromwarehousename AS warehousename,
                                    fromlocationname  AS locationname,
                                    fromlocationid    AS locationid,
                                    articleid,
                                    itemno,
                                    itemdesc
                           FROM     trans
                           GROUP BY fromorgid,
                                    fromorgcode,
                                    fromorgname,
                                    frominvlocid,
                                    fromwarehousecode,
                                    fromwarehousename,
									fromlocationid,
                                    fromlocationname,
                                    articleid,
                                    itemno,
                                    itemdesc
                           UNION
                           SELECT   toorgid         AS principleid,
                                    toorgcode       AS principlecode,
                                    toorgname       AS principlename,
                                    toinvlocid      AS warehouseid,
                                    towarehousecode AS warehousecode,
                                    towarehousename AS warehousename,
                                    tolocationname  AS locationname,
                                    tolocationid    AS locationid,
                                    articleid,
                                    itemno,
                                    itemdesc
                           FROM     trans
                           GROUP BY toorgid,
                                    toorgcode,
                                    toorgname,
                                    toinvlocid,
                                    towarehousecode,
                                    towarehousename,
									tolocationid,
                                    tolocationname,
                                    articleid,
                                    itemno,
                                    itemdesc) a
         GROUP BY principleid,
                  principlecode,
                  principlename,
                  warehouseid,
                  warehousecode,
                  warehousename,
                  locationid,
				  locationname,
                  articleid,
                  itemno,
                  itemdesc)
SELECT          a.PrincipleId,
                a.PrincipleCode,
                a.PrincipleName,
                a.WarehouseId,
                a.WarehouseCode,
                a.WarehouseName,
                a.LocationId,
                a.LocationName,
                a.ArticleId,
                a.itemno as ArticleCode,
                a.itemdesc as ArticleName,
                isnull(b.receivedqty, 0) - isnull(c.shippedqty, 0) + isnull(d.adjustedqty, 0) - isnull(e.damagedqty, 0) AS AvailableQuantity,
                isnull(b.receivedqty, 0)  as ReceivedQuantity,
                isnull(c.shippedqty , 0) as ShippedQuantity,
                isnull(d.adjustedqty, 0)  as AdjustQuantity,
                isnull(e.damagedqty , 0) as DamageQuantity
FROM            masterrow a
LEFT OUTER JOIN received b
ON              a.articleid = b.articleid
AND             a.principleid = b.principleid
AND             a.warehouseid = b.warehouseid
LEFT OUTER JOIN shipped c
ON              a.articleid = c.articleid
AND             a.principleid = c.principleid
AND             a.warehouseid = c.warehouseid
LEFT OUTER JOIN adjusted d
ON              a.articleid = d.articleid
AND             a.principleid = d.principleid
AND             a.warehouseid = d.warehouseid
LEFT OUTER JOIN damaged e
ON              a.articleid = e.articleid
AND             a.principleid = e.principleid
AND             a.warehouseid = e.warehouseid
GO

-- seed transTypes
set identity_insert TransTypes On;
go

Merge into TransTypes as tgt
using (values 
	(1, N'StockIn', N'Stock in for ToOrgId, Stock out for FromOrgId'),
	(2, N'Damage', N'Damage adjustment, From and To OrgId should be same'),
	(3, N'Adjust', N'Adjustment, From and To OrgId should be same'))
as src (id, code, [description])
on tgt.id = src.id
when not matched by target then
insert (id, code, [description]) values (id, code, [description]);
go

set identity_insert TransTypes Off;
go

-- seed InventoryTransactions 
-- ******* for test and UAT only, don't run it on production ************
--set identity_insert InventoryTransactions On;
--go

--merge into InventoryTransactions as tgt
--using (values 
--	(1, 55927, 1, 1, 2, 2, 1, 1000, N'User'),
--	(2, 55927, 1, 1, 2, 2, 1, 1000, N'User'),
--	(3, 55945, 3, 3, 4, 4, 1, 1000, N'User'),
--	(4, 55945, 3, 3, 3, 3, 2, 200, N'User'),
--	(5, 55945, 3, 3, 3, 3, 3, 100, N'User'),
--	(6, 55945, 3, 3, 3, 3, 2, -105, N'User'))
--as src (id, articleid, fromorgid, frominvlocid, toorgid, toinvlocid, transtypecode, quantity, createdby)
--on tgt.id = src.id
--when not matched by target then
--insert (id, articleid, fromorgid, frominvlocid, toorgid, toinvlocid, transtypecode, transdate, quantity, createdby, createddate)
--values (id, articleid, fromorgid, frominvlocid, toorgid, toinvlocid, transtypecode, getdate(), quantity, N'User', getdate());
--go

--set identity_insert InventoryTransactions Off;
--go
