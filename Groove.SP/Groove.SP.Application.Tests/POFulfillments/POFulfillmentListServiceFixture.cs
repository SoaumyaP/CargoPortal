using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.Tests.Fixtures;
using Groove.SP.Core.Models;
using Microsoft.Extensions.Options;
using Moq;
using System.IO;

namespace Groove.SP.Application.Tests.POFulfillments
{
    public class POFulfillmentListServiceFixture : SqlDatabaseFixture
    {
        public IPOFulfillmentListService POFulfillmentListService { get; private set; }

        public POFulfillmentListServiceFixture(AppDbConnections dataConnections) : base(dataConnections)
        {
            var appConfig = new Mock<IOptions<AppConfig>>();
            POFulfillmentListService = new POFulfillmentListService(EfDataQuery, appConfig.Object, ServiceProviderMock.Object);

            InitializeSchema();
            SeedData();
        }
        

        private void InitializeSchema()
        {
            string sqlScript = "";
            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.POFulfillments.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.POFulfillmentContacts.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.BuyerApprovals.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.PurchaseOrderAdhocChanges.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
        }

        private void SeedData()
        {
            var sqlScript = "";

            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Data", "Test_POFulfillment.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);

            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Data", "Test_POFulfillmentContact.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);

            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Data", "Test_BuyerApproval.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);

            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Data", "Test_PurchaseOrderAdhocChanges.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
        }

        public override void Dispose()
        {
            var dropTable = @"
                        IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BuyerApprovals]') AND type in (N'U'))
                        DROP TABLE [dbo].[BuyerApprovals]

                        IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseOrderAdhocChanges]') AND type in (N'U'))
                        DROP TABLE [dbo].[PurchaseOrderAdhocChanges]

                        IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POFulfillmentContacts]') AND type in (N'U'))
                        DROP TABLE [dbo].[POFulfillmentContacts]

                        IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POFulfillments]') AND type in (N'U'))
                        DROP TABLE [dbo].[POFulfillments]
            ";

            EfDataQuery.ExecuteSqlCommand(dropTable);

            base.Dispose();
        }
    }
}
