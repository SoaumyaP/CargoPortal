-- =============================================
-- Author:			Hau Nguyen
-- Created date:	28 May 2021
-- Description:		PSP-2542: Outport-Patching data for dbo.[Containers].ContainerType from equipment name to code
-- =============================================

DECLARE @EquipmentTypes TABLE(
	[Code] VARCHAR(128) NOT NULL,
	[Name] NVARCHAR(128) NOT NULL,
	[Description] NVARCHAR(MAX) NOT NULL,
	[Value] INT NOT NULL
)

INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('20GP', 'TwentyGP', '20'' Container', 10)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('20HC', 'TwentyHC', '20'' High Cube', 11)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('20NOR', 'TwentyNOR', '20'' Reefer Dry', 14)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('40GP', 'FourtyGP', '40'' Container', 20)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('40HC', 'FourtyHC', '40'' High Cube', 21)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('40NOR', 'FourtyNOR', '40'' Reefer Dry', 28)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('20RF', 'TwentyRF', '20'' Reefer', 30)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('40RF', 'FourtyRF', '40'' Reefer', 40)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('45HC', 'FourtyFiveHC', '45'' High Cube', 52)
INSERT INTO @EquipmentTypes ([Code], [Name], [Description], [Value])
VALUES ('LCL', 'LCL', 'LCL', 60)

UPDATE Containers
SET ContainerType = eqt.Code
FROM Containers c JOIN @EquipmentTypes eqt ON c.ContainerType = eqt.[Description]