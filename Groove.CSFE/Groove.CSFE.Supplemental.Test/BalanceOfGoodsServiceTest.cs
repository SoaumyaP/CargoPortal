using System.Data.SqlClient;
using Groove.CSFE.Supplemental.Models;
using Groove.CSFE.Supplemental.Services;

namespace Groove.CSFE.Supplemental.Test
{
    public class BalanceOfGoodsServiceTest
    {
        BalanceOfGoodsService service;
        SqlConnection sql;

        [SetUp]
        public void Setup()
        {
            sql = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=csps-supplemental;Integrated Security=SSPI;");
            Mock<IDbConnections> conn = new Mock<IDbConnections>();
            conn.Setup(x => x.Supplemental).Returns(sql);
            service = new BalanceOfGoodsService(conn.Object);
        }

        [TearDown]
        public void TearDown()
        {
            sql.Dispose();
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
                Keyword = "cargo"
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
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
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_AccessiblePrinciples_ShouldSuccess()
        {
            BalanceOfGoodSearchModel model = new BalanceOfGoodSearchModel()
            {
                Skip = 0,
                Take = 10,
                AccessiblePrinciples = new long[] {1, 2},
                Order = "PrincipleCode",
                Direction = "asc",
                Keyword = "cargo"
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
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
                }
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_Filter2_ShouldSuccess()
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
                            @operator = "contain",
                            value = "cargo"
                        }
                    }
                }
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_Filter3_ShouldSuccess()
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
                            @operator = "startswith",
                            value = "cargo"
                        }
                    }
                }
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task GetBalanceOfGoodAsync_Filter4_ShouldSuccess()
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
                        }
                    }
                }
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
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
                }
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
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
                }
            };
            var result = await service.GetBalanceOfGoodAsync(model);
            Assert.IsNotNull(result);
        }
    }
}