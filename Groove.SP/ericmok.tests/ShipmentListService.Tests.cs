using Groove.SP.Application.Shipments.Services;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Data;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Groove.SP.Application.Shipments.Mappers;
using Groove.SP.Core.Entities;

namespace ericmok.tests
{
    public class Tests
    {
        ShipmentListService service;
        SpContext sp;

        [OneTimeSetUp]
        public void Setup()
        {
            Mock<IDataQuery> dataQuery = new Mock<IDataQuery>();
            Mock<IOptions<AppConfig>> appConfig = new Mock<IOptions<AppConfig>>();
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();

            appConfig.Setup(x => x.Value).Returns(new AppConfig()
            {

            });

            AppDbConnections appDbConnections = new AppDbConnections()
            {
                CsPortalDb = constr
            };

            DbContextOptions<SpContext> op = new DbContextOptionsBuilder<SpContext>()
                .UseSqlServer(constr)
                .Options;
            sp = new SpContext(op);

            serviceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns(sp);

            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ShipmentMilestoneSingleQueryModel, ShipmentQueryModel>();
            }).CreateMapper();
            serviceProvider.Setup(x => x.GetService(typeof(IMapper))).Returns(mapper);

            EfDataQuery dq = new EfDataQuery(serviceProvider.Object, appDbConnections);
            service = new ShipmentListService(dq, appConfig.Object, serviceProvider.Object);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            sp.Dispose();
            service = null;
        }

        const string constr = "Server=tcp:cargofe-csportal.database.windows.net,1433;Initial Catalog=csportaldb;Persist Security Info=False;User ID=ericmok@cargofe.com;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Authentication=\"Active Directory Integrated\";";

        [Theory]
        [TestCase("2005", "Shipment Booked")]
        [TestCase("2014", "Handover from Shipper")]
        [TestCase("7001,7003", "Departure from Port")]
        [TestCase("7003,7001", "In Transit")]
        [TestCase("7002,7004", "Arrival at Port")]
        [TestCase("2054", "Handover to Consignee")]
        public async Task GetListShipmentAsync_Test_ShouldSuccess(string value, string label)
        {
            DataSourceRequest request = new DataSourceRequest()
            {
                Page = 1,
                PageSize = 10,
                Skip = 0,
                Take = 10,
                Filters = new List<IFilterDescriptor>
                { 
                    new FilterDescriptor()
                    {
                        Operator = FilterOperator.MultiSelect,
                        Member = "activitycode",
                        Value = value, 
                    }
                }
            };
            var result = await service.GetListShipmentAsync(request, true, "");
            Assert.IsTrue(result != null, "result null");
            Assert.IsTrue(result.Data.AsQueryable().Count() > 0, "count == 0");
        }

        [Test]
        public void IsInteractToColumn_Test()
        {
            DataSourceRequest request = new DataSourceRequest()
            {
                Page = 0,
                PageSize = 10,
                Skip = 0,
                Take = 10,
                Filters = new List<IFilterDescriptor>
                {
                    new FilterDescriptor()
                    {
                        Operator = FilterOperator.MultiSelect,
                        Member = "activitycode"
                    }
                }
            };
            var result = request.IsInteractToColumn("activitycode");
            Assert.IsTrue(result);
        }
    }
}