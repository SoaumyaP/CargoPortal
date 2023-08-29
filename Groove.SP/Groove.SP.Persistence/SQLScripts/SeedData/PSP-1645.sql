-- =============================================
-- Author:		Phuoc Le
-- Created date: 26 Jun 2020
-- Description:	PSP-1645 [Purchase Order] Add new column: Container Type
-- =============================================


UPDATE PurchaseOrders 
SET ContainerType = (
	SELECT TOP 1 ContainerTypes.ContainerTypeValue
	FROM 
	 (VALUES 
			('20GP',10),
			('20HC',11),
			('20NOR',14),
			('20RE',30),
			('40GP',20),
			('40HC',21),
			('40NOR',23),
			('40RE',40),
			('45HC',52),
			('CFS',60),
			('LCL',60)
	) ContainerTypes(ContainerTypeCode,ContainerTypeValue)
	WHERE TRIM(PORemark) = ContainerTypes.ContainerTypeCode
)
