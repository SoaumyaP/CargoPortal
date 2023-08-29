using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Groove.SP.Application.Tests.PurchaseOrders
{
    public class PurchaseOrderListServiceTest : IClassFixture<PurchaseOrderListServiceFixture>
    {
        private readonly PurchaseOrderListServiceFixture _purchaseOrderListServiceFixture;
        public PurchaseOrderListServiceTest(PurchaseOrderListServiceFixture purchaseOrderListServiceFixture)
        {
            _purchaseOrderListServiceFixture = purchaseOrderListServiceFixture;
        }

        [Trait("Category", "Purchase Order List")]
        [Fact(DisplayName = "Internal user + paging")]
        public async Task Internal_Paging()
        {
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Sorts = new SortDescriptor[]
                {
                    new SortDescriptor
                    {
                        Member = "createdDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Descending
                    }
                }
            };
            var isInternal = true;
            string affiliate = null;
            string supplierCustomerRelationships = "";
            var delegatedOrganizationId = 0;
            var statisticKey = "";

            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(request, isInternal, affiliate, supplierCustomerRelationships, delegatedOrganizationId, statisticKey);
            var data = (IList<PurchaseOrderQueryModel>)result.Data;

            Assert.True(condition: result.Total == 200 && data.Count() == 20, "Total records should be 20 and current page should be 20.");
        }

        [Trait("Category", "Purchase Order List")]
        [Theory(DisplayName = "Internal user + filtering for all column")]
        [MemberData(nameof(PurchaseOrderListServiceTestData.FilteringIsEqualTo), MemberType = typeof(PurchaseOrderListServiceTestData))]
        public async Task Internal_Filtering_AllColumn(DataSourceRequest dataSource)
        {
            // Arrange
            var isInternal = true;
            var affiliate = "";
            string supplierCustomerRelationships = "";

            // Act
            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(dataSource, isInternal, affiliate, supplierCustomerRelationships);
            var data = result.Data as IList<PurchaseOrderQueryModel>;

            // Assert
            var filterDescriptor = dataSource.Filters[0] as FilterDescriptor;
            Action<PurchaseOrderQueryModel> action = null;
            switch (filterDescriptor.Member)
            {
                case "poNumber":
                    action = (x) => Assert.Equal(filterDescriptor.Value.ToString(), x.PONumber, ignoreCase: true);
                    break;
                case "createdDate":
                    action = (x) => Assert.Equal(DateTime.Parse(filterDescriptor.Value.ToString()), x.CreatedDate);
                    break;
                case "status":
                    action = (x) => Assert.Equal(filterDescriptor.Value, (int)x.Status);
                    break;
                case "cargoReadyDate":
                    action = (x) => Assert.Equal(DateTime.Parse(filterDescriptor.Value.ToString()), x.CargoReadyDate);
                    break;
                case "supplier":
                    action = (x) => Assert.Equal(filterDescriptor.Value.ToString(), x.Supplier, ignoreCase: true);
                    break;
                case "stage":
                    action = (x) => Assert.Equal(filterDescriptor.Value, (int)x.Stage);
                    break;
            }
            Assert.All(data, action);
        }

        [Trait("Category", "Purchase Order List")]
        [Theory(DisplayName = "Internal user + sorting for all column")]
        [MemberData(nameof(PurchaseOrderListServiceTestData.SortingDescending), MemberType = typeof(PurchaseOrderListServiceTestData))]
        public async Task Internal_Sorting_Descending_AllColumn(DataSourceRequest dataSource)
        {
            // Arrange
            var isInternal = true;
            var affiliates = "";
            var supplierCustomerRelationship = "";

            // Act
            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(dataSource, isInternal, affiliates, supplierCustomerRelationship);
            var data = result.Data as IList<PurchaseOrderQueryModel>;

            // Assert
            var expectedList = data;
            switch (dataSource.Sorts[0].Member)
            {
                case "poNumber":
                    expectedList = data.OrderByDescending(x => x.PONumber, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "createdDate":
                    expectedList = data.OrderByDescending(x => x.CreatedDate).ToList();
                    break;
                case "statusName":
                    expectedList = data.OrderByDescending(x => x.StatusName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "cargoReadyDate":
                    expectedList = data.OrderByDescending(x => x.CargoReadyDate).ToList();
                    break;
                case "supplier":
                    expectedList = data.OrderByDescending(x => x.Supplier, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "stageName":
                    expectedList = data.OrderByDescending(x => x.StageName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
            }
            Assert.True(expectedList.SequenceEqual(data));
        }

        [Trait("Category", "Purchase Order List")]
        [Fact]
        public async Task Principal_DataAccessRight_GivenPONumberOfCurrentOrg()
        {
            // Arrange
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Filters = new FilterDescriptor[]
                {
                    new FilterDescriptor
                    {
                        Member = "poNumber",
                        Operator = FilterOperator.IsEqualTo,
                        Value = "MEL-APR3001"
                    }
                },
                Sorts = new SortDescriptor[]
                {
                    new SortDescriptor
                    {
                        Member = "createdDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
                }
            };
            var isInternal = false;
            var affiliates = "[461]"; //MLS Principal 1
            var supplierCustomerRelationships = "";

            // Act
            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(request, isInternal, affiliates, supplierCustomerRelationships);
            var data = result.Data as IList<PurchaseOrderQueryModel>;

            // Assert
            Assert.Equal(1, data.Count);
            Assert.All(data, x => Assert.Equal("MEL-APR3001", x.PONumber));
        }

        [Trait("Category", "Purchase Order List")]
        [Fact]
        public async Task Principal_DataAccessRight_GivenPONumberOfOtherOrg()
        {
            // Arrange
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Filters = new FilterDescriptor[]
                {
                    new FilterDescriptor
                    {
                        Member = "poNumber",
                        Operator = FilterOperator.IsEqualTo,
                        Value = "PO-HN-033"
                    }
                },
                Sorts = new SortDescriptor[]
                {
                    new SortDescriptor
                    {
                        Member = "createdDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
                }
            };
            var isInternal = false;
            var affiliates = "[461]"; //MLS Principal 1
            var supplierCustomerRelationships = "";

            // Act
            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(request, isInternal, affiliates, supplierCustomerRelationships);
            var data = result.Data as IList<PurchaseOrderQueryModel>;

            // Assert
            Assert.Equal(0, data.Count);
        }

        [Trait("Category", "Purchase Order List")]
        [Fact]
        public async Task Supplier_DataAccessRight_InCustomerRelationship()
        {
            // Arrange
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Filters = new FilterDescriptor[]
                {
                    new FilterDescriptor
                    {
                        Member = "poNumber",
                        Operator = FilterOperator.IsEqualTo,
                        Value = "MEL-APR3001"
                    }
                },
                Sorts = new SortDescriptor[]
                {
                    new SortDescriptor
                    {
                        Member = "createdDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
                }
            };
            var isInternal = false;
            var affiliates = "";
            var delegatedOrganizationId = 462; //MLS Suplier 1
            var supplierCustomerRelationships = ";462,461;";

            // Act
            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(request, isInternal, affiliates, supplierCustomerRelationships, delegatedOrganizationId);
            var data = result.Data as IList<PurchaseOrderQueryModel>;

            // Assert
            Assert.Equal(1, data.Count);
        }

        [Trait("Category", "Purchase Order List")]
        [Fact]
        public async Task Supplier_DataAsscessRight_NotInCustomerRelationship()
        {
            // Arrange
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Filters = new FilterDescriptor[]
                {
                    new FilterDescriptor
                    {
                        Member = "poNumber",
                        Operator = FilterOperator.IsEqualTo,
                        Value = "MEL-APR3001"
                    }
                },
                Sorts = new SortDescriptor[]
                {
                    new SortDescriptor
                    {
                        Member = "createdDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
                }
            };
            var isInternal = false;
            var affiliates = "";
            var delegatedOrganizationId = 466; //ORG_HN_04
            var supplierCustomerRelationships = ";463;466;";

            // Act
            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(request, isInternal, affiliates, supplierCustomerRelationships, delegatedOrganizationId);
            var data = result.Data as IList<PurchaseOrderQueryModel>;

            // Assert
            Assert.Equal(0, data.Count);
        }

        [Trait("Category", "Purchase Order List")]
        [Theory(DisplayName = "External user + filtering for all column")]
        [MemberData(nameof(PurchaseOrderListServiceTestData.FilteringIsEqualTo), MemberType = typeof(PurchaseOrderListServiceTestData))]
        public async Task External_Filtering_AllColumn(DataSourceRequest dataSource)
        {
            // Arrange
            var isInternal = false;
            var affiliate = "[461]"; //MLS Principal 1
            string supplierCustomerRelationships = "";

            // Act
            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(dataSource, isInternal, affiliate, supplierCustomerRelationships);
            var data = result.Data as IList<PurchaseOrderQueryModel>;

            // Assert
            var filterDescriptor = dataSource.Filters[0] as FilterDescriptor;
            Action<PurchaseOrderQueryModel> action = null;
            switch (filterDescriptor.Member)
            {
                case "poNumber":
                    action = (x) => Assert.Equal(filterDescriptor.Value.ToString(), x.PONumber, ignoreCase: true);
                    break;
                case "createdDate":
                    action = (x) => Assert.Equal(DateTime.Parse(filterDescriptor.Value.ToString()), x.CreatedDate);
                    break;
                case "status":
                    action = (x) => Assert.Equal(filterDescriptor.Value, (int)x.Status);
                    break;
                case "cargoReadyDate":
                    action = (x) => Assert.Equal(DateTime.Parse(filterDescriptor.Value.ToString()), x.CargoReadyDate);
                    break;
                case "supplier":
                    action = (x) => Assert.Equal(filterDescriptor.Value.ToString(), x.Supplier, ignoreCase: true);
                    break;
                case "stage":
                    action = (x) => Assert.Equal(filterDescriptor.Value, (int)x.Stage);
                    break;
            }
            Assert.All(data, action);
        }

        [Trait("Category", "Purchase Order List")]
        [Theory(DisplayName = "External user + sorting for all column")]
        [MemberData(nameof(PurchaseOrderListServiceTestData.SortingDescending), MemberType = typeof(PurchaseOrderListServiceTestData))]
        public async Task External_Sorting_Descending_AllColumn(DataSourceRequest dataSource)
        {
            // Arrange
            var isInternal = false;
            var affiliates = "[461]"; //MLS principal 1
            var supplierCustomerRelationship = "";

            // Act
            var result = await _purchaseOrderListServiceFixture._purchaseOrderListService.ListAsync(dataSource, isInternal, affiliates, supplierCustomerRelationship);
            var data = result.Data as IList<PurchaseOrderQueryModel>;

            // Assert
            var expectedList = data;
            switch (dataSource.Sorts[0].Member)
            {
                case "poNumber":
                    expectedList = data.OrderByDescending(x => x.PONumber, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "createdDate":
                    expectedList = data.OrderByDescending(x => x.CreatedDate).ToList();
                    break;
                case "statusName":
                    expectedList = data.OrderByDescending(x => x.StatusName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "cargoReadyDate":
                    expectedList = data.OrderByDescending(x => x.CargoReadyDate).ToList();
                    break;
                case "supplier":
                    expectedList = data.OrderByDescending(x => x.Supplier, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "stageName":
                    expectedList = data.OrderByDescending(x => x.StageName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
            }
            Assert.True(expectedList.SequenceEqual(data));
        }
    }
}