using Groove.SP.Application.PurchaseOrders.Services;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.Tests.Fixtures;
using Groove.SP.Core.Models;
using System.IO;

namespace Groove.SP.Application.Tests.PurchaseOrders
{
    public class PurchaseOrderListServiceFixture : SqlDatabaseFixture
    {
        public IPurchaseOrderListService _purchaseOrderListService;
        public PurchaseOrderListServiceFixture(AppDbConnections dataConnections): base (dataConnections)
        {          
            _purchaseOrderListService = new PurchaseOrderListService(UnitOfWorkProviderMock.Object, EfDataQuery);

            var sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.BuyerCompliances.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.PurchaseOrderContacts.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.PurchaseOrders.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);

            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Data", "Test_PurchaseOrdersList.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);

            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Function", "fn_SplitStringToTable.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
        }

        public override void Dispose()
        {
            // ... clean up test data from the database ...

            var sqlScript = @"
                            IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BuyerCompliances]') AND type in (N'U'))
                                DROP TABLE [dbo].[BuyerCompliances]
                            IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseOrderContacts]') AND type in (N'U'))
                                DROP TABLE [dbo].[PurchaseOrderContacts]
                            IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseOrders]') AND type in (N'U'))
                                DROP TABLE [dbo].[PurchaseOrders]
                            IF EXISTS (SELECT * FROM   sys.objects WHERE  object_id = OBJECT_ID(N'[dbo].[fn_SplitStringToTable]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
                                DROP FUNCTION [dbo].[fn_SplitStringToTable]";
            EfDataQuery.ExecuteSqlCommand(sqlScript);

            base.Dispose();
        }
    }
}
