using Groove.SP.Application.Shipments.Services;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Application.Tests.Fixtures;
using Groove.SP.Core.Models;
using Microsoft.Extensions.Options;
using Moq;
using System.IO;

namespace Groove.SP.Application.Tests.Shipments
{
    public class ShipmentListServiceFixture : SqlDatabaseFixture
    {
        public IShipmentListService _shipmentListService;

        public ShipmentListServiceFixture(AppDbConnections dataConnections) : base(dataConnections)
        {
            var appConfig = new Mock<IOptions<AppConfig>>();
            _shipmentListService = new ShipmentListService(EfDataQuery, appConfig.Object, ServiceProviderMock.Object);

            var sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.ShipmentContacts.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Schema", "Table_dbo.Shipments.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);

            sqlScript = File.ReadAllText(Path.Combine(ScriptFolderPath, "Data", "Test_ShipmentList.sql"));
            EfDataQuery.ExecuteSqlCommand(sqlScript);
        }

        public override void Dispose()
        {
            // ... clean up test data from the database ...

            var sqlScript = @"
                            IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShipmentContacts]') AND type in (N'U'))
                                DROP TABLE [dbo].[ShipmentContacts]
                            IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Shipments]') AND type in (N'U'))
                                DROP TABLE [dbo].[Shipments]";
            EfDataQuery.ExecuteSqlCommand(sqlScript);

            base.Dispose();
        }
    }
}
