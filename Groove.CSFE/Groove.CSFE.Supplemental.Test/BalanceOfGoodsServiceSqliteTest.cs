using Dapper;
using Groove.CSFE.Supplemental.Services;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groove.CSFE.Supplemental.Models;

namespace Groove.CSFE.Supplemental.Test
{
    public class BalanceOfGoodsServiceSqliteTest
    {
        IDbConnection connection;
        IBalanceOfGoodsService service;

        [OneTimeSetUp]
        public void Setup()
        {
            connection = new SQLiteConnection("Data Source=:memory:");
            connection.Open();
            BuildAndSeedDb();

            Mock<IDbConnections> conn = new Mock<IDbConnections>();
            conn.Setup(x => x.Supplemental).Returns(connection);
            service = new BalanceOfGoodsService(conn.Object);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            connection.Close();
            connection.Dispose();
        }

        void BuildAndSeedDb()
        {
            string script = File.ReadAllText("sqlite_config.sql");
            connection.Execute(script);

            string script1 = File.ReadAllText("sqlite_view.sql");
            connection.Execute(script1);

            string script3 = File.ReadAllText("sqlite_view2.sql");
            connection.Execute(script3);

            string script2 = File.ReadAllText("sqlite_insert.sql");
            connection.Execute(script2);
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_Keyword_ShouldSuccess()
        {
            BalanceOfGoodSearchModel model = new BalanceOfGoodSearchModel()
            {
                Skip = 0,
                Take = 10,
                AccessiblePrinciples = null,
                Order = "PrincipleCode",
                Direction = "asc",
                Keyword = "cargo",
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TotalRecords == 1, $"Failed: Total Recordcount = {result.TotalRecords}, expect: 1");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");
            Assert.Pass();
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_Advanced_ShouldSuccess()
        {
            BalanceOfGoodSearchModel model = new BalanceOfGoodSearchModel()
            {
                Skip = 0,
                Take = 10,
                AccessiblePrinciples = null,
                Order = "PrincipleCode",
                Direction = "asc",
                Principle = "cargo",
                Warehouse = "gro",
                Location = "au",
                Article = "st"
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TotalRecords == 1, $"Failed: Total Recordcount = {result.TotalRecords}, expect: 1");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_AccessiblePrinciples_ShouldSuccess()
        {
            BalanceOfGoodSearchModel model = new BalanceOfGoodSearchModel()
            {
                Skip = 0,
                Take = 10,
                AccessiblePrinciples = new long[] { 1, 2 },
                Order = "PrincipleCode",
                Direction = "asc",
                Keyword = "cargo"
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsTrue(result.TotalRecords == 0, "Expect 0");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");


            model = new BalanceOfGoodSearchModel()
            {
                Skip = 0,
                Take = 10,
                AccessiblePrinciples = new long[] { 3, 4 },
                Order = "PrincipleCode",
                Direction = "asc",
                Keyword = "cargo"
            };
            result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsTrue(result.TotalRecords == 1, "Expect 1");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_Filter_ShouldSuccess()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "principlename",
                        dir = "asc"
                    }
                },
                filter = new QueryFilter()
                {
                    logic = "and",
                    filters = new List<FilterModel>()
                    {
                        new FilterModel()
                        {
                            field = "principlename",
                            @operator = "eq",
                            value = "cargo"
                        }
                    }
                },
                isInternal = true
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result.TotalRecords == 0, "Expect 0");

            model.filter.filters.ToList()[0].@operator = "contains";
            result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsTrue(result.TotalRecords == 1, "Expect 1");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");

            model.filter.filters.ToList()[0].@operator = "startswith";
            result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsTrue(result.TotalRecords == 1, "Expect 1, startswith");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"startswith Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");

            model.filter.filters.ToList()[0].@operator = "endswith";
            model.filter.filters.ToList()[0].value = "limited";
            result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsTrue(result.TotalRecords == 2, "Expect 2, endswith");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"endswith Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_Filter5_ShouldSuccess()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "principlename",
                        dir = "asc"
                    }
                },
                filter = new QueryFilter()
                {
                    logic = "and",
                    filters = new List<FilterModel>()
                    {
                        new FilterModel()
                        {
                            field = "principlename",
                            @operator = "endswith",
                            value = "limited"
                        },
                        new FilterModel()
                        {
                            field = "principlename",
                            @operator = "startswith",
                            value = "cargo"
                        }
                    }
                },
                isInternal = true
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsTrue(result.TotalRecords == 1, "Expect 1");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_Filter6_ShouldSuccess()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "principlename",
                        dir = "asc"
                    }
                },
                filter = new QueryFilter()
                {
                    logic = "and",
                    filters = new List<FilterModel>()
                    {
                        new FilterModel()
                        {
                            field = "availablequantity",
                            @operator = "gte",
                            value = "100"
                        }
                    }
                },
                isInternal = true
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsTrue(result.TotalRecords == 79, "Expect 79");
            Assert.That(result.Records.Count(), Is.EqualTo(10), $"Failed: for skip 10, Records {result.Records.Count()}");
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_WithPrinciple_ShouldSuccess()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "principlename",
                        dir = "asc"
                    }
                },
                filter = new QueryFilter()
                {
                    logic = "and",
                    filters = new List<FilterModel>()
                    {
                        new FilterModel()
                        {
                            field = "availablequantity",
                            @operator = "gte",
                            value = "100"
                        }
                    }
                },
                isInternal = false,
                principles = new List<int> { 1, 2 }
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsTrue(result.TotalRecords == 2, "Expect 2");
            Assert.That(result.TotalRecords, Is.EqualTo(result.Records.Count()), $"Failed: TotalRecords {result.TotalRecords}, Records {result.Records.Count()}");
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_WithPrinciple_ShouldThrow()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "principlename",
                        dir = "asc"
                    }
                },
                filter = new QueryFilter()
                {
                    logic = "and",
                    filters = new List<FilterModel>()
                    {
                        new FilterModel()
                        {
                            field = "availablequantity",
                            @operator = "gte",
                            value = "100"
                        }
                    }
                },
                isInternal = false
            };
            Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetBalanceOfGoodAsync(model));
        }

        [Test]
        public async Task GetBalanceOfGoodDetailAsync_LookupArticle_ShouldSuccess()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "principlename",
                        dir = "asc"
                    }
                },
                filter = new QueryFilter()
                {
                    logic = "and",
                    filters = new List<FilterModel>()
                },
                isInternal = true
            };
            string mode = "article";
            int principleId = 3;
            int articleId = 55945;

            var result = await service.GetBalanceOfGoodDetailAsync(mode, principleId, articleId, null, model);

            Assert.IsTrue(result.TotalRecords == 4, "total record 4");
            Assert.That(result.Records.Count, Is.EqualTo(result.TotalRecords), "total records");
        }

        [Test]
        public async Task GetBalanceOfGoodDetailAsync_LookupWarehouse_ShouldSuccess()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "principlename",
                        dir = "asc"
                    }
                },
                filter = new QueryFilter()
                {
                    logic = "and",
                    filters = new List<FilterModel>()
                },
                isInternal = true
            };
            string mode = "warehouse";
            int principleId = 3;
            int articleId = 55945;
            int warehouseId = 3;

            var result = await service.GetBalanceOfGoodDetailAsync(mode, principleId, null, warehouseId, model);

            Assert.IsTrue(result.TotalRecords == 4, "total record 4");
            Assert.That(result.Records.Count, Is.EqualTo(result.TotalRecords), "total records");
        }

        [Test]
        public async Task GetBalanceOfGoodDetailAsync_LookupWarehouseWithFilter_ShouldSuccess()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "principlename",
                        dir = "asc"
                    }
                },
                filter = new QueryFilter()
                {
                    logic = "and",
                    filters = new List<FilterModel>()
                    {
                        new FilterModel()
                        {
                            field = "ArticleName",
                            @operator = "contains",
                            value = "ITSI"
                        }
                    }
                },
                isInternal = true
            };
            string mode = "warehouse";
            int principleId = 91;
            int articleId = 55945;
            int warehouseId = 7;

            var result = await service.GetBalanceOfGoodDetailAsync(mode, principleId, null, warehouseId, model);

            Assert.IsTrue(result.TotalRecords == 30, "total record 30");
            Assert.IsTrue(result.Records.Count() == 10);
        }

        [Test]
        public async Task GetBalanceOfGoodDetailAsync_SortByQuantity_ShouldSuccess()
        {
            FilterRoot model = new FilterRoot()
            {
                skip = 0,
                take = 10,
                sort = new List<SortModel>()
                {
                    new SortModel()
                    {
                        field = "quantity",
                        dir = "desc"
                    }
                },
                filter = new QueryFilter()
                {
                    filters = new List<FilterModel>()
                },
                isInternal = true
            };
            string mode = "warehouse";
            int principleId = 91;
            int articleId = 55945;
            int warehouseId = 7;

            var result = await service.GetBalanceOfGoodDetailAsync(mode, principleId, null, warehouseId, model);

            Assert.IsTrue(result.TotalRecords == 231, "total record 231");
            Assert.IsTrue(result.Records.First().id == 61, "id order");
        }
    }
}
