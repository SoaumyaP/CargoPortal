
IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ArticleMaster'))
BEGIN
	CREATE TABLE [dbo].[ArticleMaster](
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
		[MembersQuantity] [varchar](256) NULL,
		[MembersItemId] [varchar](256) NULL,
		[ItemPrice] [numeric](18, 3) NULL,
		[ProcurementRule] [varchar](10) NULL,
		[StyleNo] [nvarchar](256) NULL,
		[StyleName] [nvarchar](256) NULL,
		[ColourCode] [nvarchar](256) NULL,
		[ColourName] [nvarchar](256) NULL,
		[Size] [nvarchar](256) NULL,
		[Status] [varchar](1) NULL,
		[CreatedBy] [varchar](10) NULL,
		[CreatedOn] [datetime] NULL,
		[UpdatedBy] [varchar](10) NULL,
		[UpdatedOn] [datetime] NULL,
	PRIMARY KEY CLUSTERED 
	(
		[CompanyCode] ASC,
		[CompanyType] ASC,
		[PONo] ASC,
		[ItemNo] ASC,
		[ShipmentNo] ASC,
		[POSeq] ASC,
		[DestCode] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
END
GO
IF (NOT EXISTS(SELECT *
				FROM   INFORMATION_SCHEMA.COLUMNS
				WHERE  TABLE_NAME = 'ArticleMaster'
						AND COLUMN_NAME = 'StyleNo'))
BEGIN
	ALTER TABLE dbo.ArticleMaster
	ADD [StyleNo] [nvarchar](256);
END
GO
IF (NOT EXISTS(SELECT *
				FROM   INFORMATION_SCHEMA.COLUMNS
				WHERE  TABLE_NAME = 'ArticleMaster'
						AND COLUMN_NAME = 'ColourCode'))
BEGIN
	ALTER TABLE dbo.ArticleMaster
	ADD [ColourCode] [nvarchar](256);
END
GO
IF (NOT EXISTS(SELECT *
				FROM   INFORMATION_SCHEMA.COLUMNS
				WHERE  TABLE_NAME = 'ArticleMaster'
						AND COLUMN_NAME = 'Size'))
BEGIN
	ALTER TABLE dbo.ArticleMaster
	ADD [Size] [nvarchar](256);
END
GO
IF (NOT EXISTS(SELECT *
				FROM   INFORMATION_SCHEMA.COLUMNS
				WHERE  TABLE_NAME = 'ArticleMaster'
						AND COLUMN_NAME = 'StyleName'))
BEGIN
	ALTER TABLE dbo.ArticleMaster
	ADD [StyleName] [nvarchar](256);
END
GO
IF (NOT EXISTS(SELECT *
				FROM   INFORMATION_SCHEMA.COLUMNS
				WHERE  TABLE_NAME = 'ArticleMaster'
						AND COLUMN_NAME = 'ColourName'))
BEGIN
	ALTER TABLE dbo.ArticleMaster
	ADD [ColourName] [nvarchar](256);
END


