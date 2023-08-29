using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Groove.SP.Application.Tests.Shipments
{
    public class ShipmentListServiceTest : IClassFixture<ShipmentListServiceFixture>
    {
        private readonly ShipmentListServiceFixture _shipmentListServiceFixture;
        public ShipmentListServiceTest(ShipmentListServiceFixture shipmentListServiceFixture)
        {
            _shipmentListServiceFixture = shipmentListServiceFixture;
        }

        [Trait("Category", "Shipment List")]
        [Fact]
        public async Task Internal_Paging()
        {
            // Arrange
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Sorts = new SortDescriptor[]
               {
                    new SortDescriptor
                    {
                        Member = "shipFromETDDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
               }
            };
            var isInternal = true;

            // Act
            var result = await _shipmentListServiceFixture._shipmentListService.GetListShipmentAsync(request, isInternal);
            var data = result.Data as IList<ShipmentQueryModel>;

            // Assert
            Assert.Equal(20, data.Count);
        }

        [Trait("Category", "Shipment List")]
        [Theory]
        [MemberData(nameof(ShipmentListServiceTestData.FilteringIsEqualTo), MemberType = typeof(ShipmentListServiceTestData))]
        public async Task Internal_Filtering_IsEqualTo_AllColumn(DataSourceRequest dataSource, string member, string expectedValue)
        {
            // Arrange
            var isInternal = true;

            // Act
            var result = await _shipmentListServiceFixture._shipmentListService.GetListShipmentAsync(dataSource, isInternal);
            var data = result.Data as IList<ShipmentQueryModel>;

            // Assert
            Action<ShipmentQueryModel> action = null;
            switch (member)
            {
                case "shipmentNo":
                    action = (x) => Assert.Equal(expectedValue, x.ShipmentNo, ignoreCase: true);
                    break;
                case "shipper":
                    action = (x) => Assert.Equal(expectedValue, x.Shipper, ignoreCase: true);
                    break;
                case "consignee":
                    action = (x) => Assert.Equal(expectedValue, x.Consignee, ignoreCase: true);
                    break;
                case "shipFrom":
                    action = (x) => Assert.Equal(expectedValue, x.ShipFrom, ignoreCase: true);
                    break;
                case "shipFromETDDate":
                    action = (x) => Assert.Equal(DateTime.Parse(expectedValue), x.ShipFromETDDate);
                    break;
                case "shipTo":
                    action = (x) => Assert.Equal(expectedValue, x.ShipTo);
                    break;
                case "status":
                    action = (x) => Assert.Equal(expectedValue, x.Status, ignoreCase: true);
                    break;
            }
            Assert.All(data, action);
        }

        [Trait("Category", "Shipment List")]
        [Theory]
        [MemberData(nameof(ShipmentListServiceTestData.SortingDescending), MemberType = typeof(ShipmentListServiceTestData))]
        public async Task Internal_Sorting_Descending_AllColumn(DataSourceRequest dataSource)
        {
            // Arrange
            var isInternal = true;

            // Act
            var result = await _shipmentListServiceFixture._shipmentListService.GetListShipmentAsync(dataSource, isInternal);
            var data = result.Data as IList<ShipmentQueryModel>;

            // Assert
            var expectedList = data;
            switch (dataSource.Sorts[0].Member)
            {
                case "shipmentNo":
                    expectedList = data.OrderByDescending(x => x.ShipmentNo).ToList();
                    break;
                case "shipper":
                    expectedList = data.OrderByDescending(x => x.Shipper, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "consignee":
                    expectedList = data.OrderByDescending(x => x.Consignee, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "shipFrom":
                    expectedList = data.OrderByDescending(x => x.ShipFrom, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "shipFromETDDate":
                    expectedList = data.OrderByDescending(x => x.ShipFromETDDate).ToList();
                    break;
                case "shipTo":
                    expectedList = data.OrderByDescending(x => x.ShipTo, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "status":
                    expectedList = data.OrderByDescending(x => x.Status, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
            }
            Assert.True(expectedList.SequenceEqual(data));
        }

        [Trait("Category", "Shipment List")]
        [Theory]
        [InlineData(true, "")]
        [InlineData(false, "[462]")]
        public async Task QuickSearch_GivenCustomerRefNumber_ReturnsList(bool isInternal, string affiliates)
        {
            // Arrange
            var customerReferenceNo = "customerRefNo~MLSPXSO28";
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Sorts = new SortDescriptor[]
               {
                    new SortDescriptor
                    {
                        Member = "shipFromETDDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
               }
            };

            // Act
            var result = await _shipmentListServiceFixture._shipmentListService.GetListShipmentAsync(request, isInternal, affiliates, referenceNo: customerReferenceNo);
            var data = result.Data as IList<ShipmentQueryModel>;

            // Assert
            Assert.All(data, x => Assert.Equal("MLSPXSO28", x.CustomerReferenceNo, ignoreCase: true));
        }

        [Trait("Category", "Shipment List")]
        [Theory]
        [InlineData(true, "")]
        [InlineData(false, "[462]")]
        public async Task QuickSearch_GivenAgentRefNumber_ReturnsList(bool isInternal, string affiliates)
        {
            // Arrange
            var agentReferenceNo = "agentRefNo~Agent ref.0307";
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Sorts = new SortDescriptor[]
               {
                    new SortDescriptor
                    {
                        Member = "shipFromETDDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
               }
            };

            // Act
            var result = await _shipmentListServiceFixture._shipmentListService.GetListShipmentAsync(request, isInternal, affiliates, referenceNo: agentReferenceNo);
            var data = result.Data as IList<ShipmentQueryModel>;

            // Assert
            Assert.All(data, x => Assert.Equal("Agent ref.0307", x.AgentReferenceNo, ignoreCase: true));
        }

        [Trait("Category", "Shipment List")]
        [Fact]
        public async Task External_Data_Access_Right()
        {
            // Arrange
            var request = new DataSourceRequest
            {
                Page = 1,
                PageSize = 20,
                Skip = 0,
                Sorts = new SortDescriptor[]
               {
                    new SortDescriptor
                    {
                        Member = "shipFromETDDate",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
               }
            };
            var isInternal = false;
            var affiliates = "[462]"; //MLS Supplier 1

            // Act
            var result = await _shipmentListServiceFixture._shipmentListService.GetListShipmentAsync(request, isInternal, affiliates);
            var data = result.Data as IList<ShipmentQueryModel>;

            // Assert
            Assert.All(data, x => Assert.Equal("MLS Supplier 1", x.Shipper, ignoreCase: true));
        }

        [Trait("Category", "Shipment List")]
        [Theory]
        [MemberData(nameof(ShipmentListServiceTestData.SortingDescending), MemberType = typeof(ShipmentListServiceTestData))]
        public async Task External_Sorting_Descending_AllColumn(DataSourceRequest dataSource)
        {
            // Arrange
            var isInternal = false;
            var affiliates = "[461]"; //MLS Principal 1

            // Act
            var result = await _shipmentListServiceFixture._shipmentListService.GetListShipmentAsync(dataSource, isInternal, affiliates);
            var data = result.Data as IList<ShipmentQueryModel>;

            // Assert
            var expectedList = data;
            switch (dataSource.Sorts[0].Member)
            {
                case "shipmentNo":
                    expectedList = data.OrderByDescending(x => x.ShipmentNo).ToList();
                    break;
                case "shipper":
                    expectedList = data.OrderByDescending(x => x.Shipper, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "consignee":
                    expectedList = data.OrderByDescending(x => x.Consignee, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "shipFrom":
                    expectedList = data.OrderByDescending(x => x.ShipFrom, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "shipFromETDDate":
                    expectedList = data.OrderByDescending(x => x.ShipFromETDDate).ToList();
                    break;
                case "shipTo":
                    expectedList = data.OrderByDescending(x => x.ShipTo, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case "status":
                    expectedList = data.OrderByDescending(x => x.Status, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
            }
            Assert.True(expectedList.SequenceEqual(data));
        }

        [Trait("Category", "Shipment List")]
        [Theory]
        [MemberData(nameof(ShipmentListServiceTestData.FilteringIsEqualTo), MemberType = typeof(ShipmentListServiceTestData))]
        public async Task External_Filtering_IsEqualTo_AllColumn(DataSourceRequest dataSource, string member, string expectedValue)
        {
            // Arrange
            var isInternal = false;
            var affiliates = "[461]"; //MLS Principal 1

            // Act
            var result = await _shipmentListServiceFixture._shipmentListService.GetListShipmentAsync(dataSource, isInternal, affiliates);
            var data = result.Data as IList<ShipmentQueryModel>;

            // Assert
            Action<ShipmentQueryModel> action = null;
            switch (member)
            {
                case "shipmentNo":
                    action = (x) => Assert.Equal(expectedValue, x.ShipmentNo, ignoreCase: true);
                    break;
                case "shipper":
                    action = (x) => Assert.Equal(expectedValue, x.Shipper, ignoreCase: true);
                    break;
                case "consignee":
                    action = (x) => Assert.Equal(expectedValue, x.Consignee, ignoreCase: true);
                    break;
                case "shipFrom":
                    action = (x) => Assert.Equal(expectedValue, x.ShipFrom, ignoreCase: true);
                    break;
                case "shipFromETDDate":
                    action = (x) => Assert.Equal(DateTime.Parse(expectedValue), x.ShipFromETDDate);
                    break;
                case "shipTo":
                    action = (x) => Assert.Equal(expectedValue, x.ShipTo);
                    break;
                case "status":
                    action = (x) => Assert.Equal(expectedValue, x.Status, ignoreCase: true);
                    break;
            }
            Assert.All(data, action);
        }
    }
}
