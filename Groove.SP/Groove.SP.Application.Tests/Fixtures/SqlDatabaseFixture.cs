using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using Groove.SP.Persistence.Contexts;
using Groove.SP.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.IO;
using System.Reflection;

namespace Groove.SP.Application.Tests.Fixtures
{
    public class SqlDatabaseFixture : IDisposable
    {
        protected string ScriptFolderPath;
        protected IDataQuery EfDataQuery;
        protected Mock<IServiceProvider> ServiceProviderMock;
        protected Mock<IUnitOfWorkProvider>UnitOfWorkProviderMock;


        public SqlDatabaseFixture(AppDbConnections dataConnections)
        {
            // ... initialize data in the test database ...

            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ScriptFolderPath = Path.Combine(baseDir, "App_Data", "Scripts");

            var optionsBuilder = new DbContextOptionsBuilder<SpContext>();
            optionsBuilder.UseSqlServer(dataConnections.CsPortalDb);
            var spDbContext = new SpContext(optionsBuilder.Options);
            UnitOfWorkProviderMock = new Mock<IUnitOfWorkProvider>();
            var unitOfWork = Mock.Of<IUnitOfWork>();
            UnitOfWorkProviderMock.Setup(x => x.CreateUnitOfWork(It.IsAny<bool>(), It.IsAny<bool>())).Returns(unitOfWork);
            ServiceProviderMock = new Mock<IServiceProvider>();
            ServiceProviderMock.Setup(x => x.GetService(It.Is<Type>(x => x.Equals(typeof(SpContext))))).Returns(spDbContext);

            EfDataQuery = new EfDataQuery(ServiceProviderMock.Object, dataConnections);
        }

        public virtual void Dispose()
        {
            // ... clean up database ...
            
        }
    }
}
