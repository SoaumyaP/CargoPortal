using Groove.SP.API.Filters;
using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Groove.SP.Application.Tests.POFulfillments
{
    public class POFulfillmentListServiceTest : IClassFixture<POFulfillmentListServiceFixture>
    {
        private readonly POFulfillmentListServiceFixture _poFulfillmentListServiceFixture;
        private readonly IPOFulfillmentListService _poFulfillmentListService;


        public POFulfillmentListServiceTest(POFulfillmentListServiceFixture poFulfillmentListServiceFixture)
        {
           _poFulfillmentListServiceFixture = poFulfillmentListServiceFixture;
            _poFulfillmentListService = _poFulfillmentListServiceFixture.POFulfillmentListService;
        }

        #region Paging
        [Fact]
        public async Task Internal_GetListSuccessfully()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 1;
            dataSourceRequest.PageSize = 1;
            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task Internal_GetListOnly20Items()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 1;
            dataSourceRequest.PageSize = 20;
            var expectedNumberOfItem = 20;

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            var actualItems = (IEnumerable<POFulfillmentQueryModel>)data.Data;
            Assert.Equal(expectedNumberOfItem, actualItems.Count());
        }

        [Fact]
        public async Task Internal_AnyPage_AnyItems()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 5;
            dataSourceRequest.PageSize = 10;

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task Internal_NotFound_PageToMuch()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 99999999;
            dataSourceRequest.PageSize = 1;

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            var actualItems = (IEnumerable<POFulfillmentQueryModel>)data.Data;
            var expectedItems = 0;
            Assert.Equal(expectedItems, actualItems.Count());
        }
        #endregion Paging

        #region Filter
        [Fact]
        public async Task Internal_NotFound_InvalidBookingNumber()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 1;
            dataSourceRequest.PageSize = 1;

            var expectedItem = 0;

            var filter = FilterDescriptorFactory.Create("number~contains~'#$%#$'");
            dataSourceRequest.Filters = filter;
            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            var actualItems = (IEnumerable<POFulfillmentQueryModel>)data.Data;
            Assert.Equal(expectedItem, actualItems.Count());
        }

        [Fact]
        public async Task Internal_AtLeastOneActiveBooking()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 1;
            dataSourceRequest.PageSize = 1;
            var filter = FilterDescriptorFactory.Create("status~eq~10");
            dataSourceRequest.Filters = filter;
            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            var actualItems = (IEnumerable<POFulfillmentQueryModel>)data.Data;
            Assert.True(actualItems.Count() >= 1);
        }

        [Fact]
        public async Task Internal_FilterBookingDateSuccess()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 1;
            dataSourceRequest.PageSize = 20;
            var filter = FilterDescriptorFactory.Create("cargoReadyDate~gte~datetime'2022-05-17T00-00-00'");
            dataSourceRequest.Filters = filter;
            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            var actualItems = (IEnumerable<POFulfillmentQueryModel>)data.Data;
            Assert.NotNull(actualItems);
        }

        [Fact]
        public async Task Internal_Filter_ActiveAndBooked_Booking()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 1;
            dataSourceRequest.PageSize = 1000;
            var filter = FilterDescriptorFactory.Create("(status~eq~10~and~stage~eq~20)");
            dataSourceRequest.Filters = filter;
            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            var actualItems = (IEnumerable<POFulfillmentQueryModel>)data.Data;
            actualItems = actualItems.OrderBy(c => c.CreatedDate);
            var lowDate = actualItems.FirstOrDefault().CreatedDate;
            var heightDate = actualItems.LastOrDefault().CreatedDate;
            var expectedDate = new DateTime(2022, 1, 1);

            Assert.InRange(expectedDate, lowDate, heightDate);
        }
        #endregion Filter

        #region External
        [Fact]
        public async Task External_GetListSuccessfully()
        {
            var dataSourceRequest = new DataSourceRequest();
            dataSourceRequest.Page = 1;
            dataSourceRequest.PageSize = 1;
            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, true);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task External_OnlyOne()
        {
            var dataSourceRequest = new DataSourceRequest()
            {
                Page = 1,
                PageSize = 1,
            };

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, false, null, 461);
            var actualItems = (IList<POFulfillmentQueryModel>)data.Data;
            Assert.True(actualItems.Count() >= 1);
        }


        [Fact]
        public async Task External_Filter_InvalidPONumber()
        {
            var dataSourceRequest = new DataSourceRequest()
            {
                Page = 1,
                PageSize = 1,
                Filters = new FilterDescriptor[]
                {
                    new FilterDescriptor
                    {
                        Member = "number",
                        Operator = FilterOperator.Contains,
                        Value = "!!@#$%#"
                    }
                }
            };

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, false, null, 461);
            var actualItems = (IList<POFulfillmentQueryModel>)data.Data;
            Assert.True(actualItems.Count() == 0);
        }

        [Fact]
        public async Task External_Sort_PONumber()
        {
            var dataSourceRequest = new DataSourceRequest()
            {
                Page = 1,
                PageSize = 1,
                Sorts = new SortDescriptor[]
                {
                    new SortDescriptor
                    {
                        Member = "number",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
                },
            };

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, false, null, 461);
            var actualItems = (IList<POFulfillmentQueryModel>)data.Data;
            Assert.NotNull(actualItems);
        }


        [Fact]
        public async Task External_HaveAtLeast_One_BulkBooking()
        {
            var dataSourceRequest = new DataSourceRequest()
            {
                Page = 1,
                PageSize = 20,
                Filters = new FilterDescriptor[]
                {
                    new FilterDescriptor
                    {
                        Member = "number",
                        Operator = FilterOperator.Contains,
                        Value = "CS"
                    }
                }
            };

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, false, null, 461);
            var actualItems = (IList<POFulfillmentQueryModel>)data.Data;
            Assert.True(actualItems.Count() >= 1);
        }

        [Fact]
        public async Task External_HaveAtLeast_One_ActiveBookedBooking()
        {
            var dataSourceRequest = new DataSourceRequest()
            {
                Page = 1,
                PageSize = 20,
                Filters = new FilterDescriptor[]
                {
                    new FilterDescriptor
                    {
                        Member = "number",
                        Operator = FilterOperator.Contains,
                        Value = "mls"
                    },
                    new FilterDescriptor
                    {
                        Member = "stage",
                        Operator = FilterOperator.IsEqualTo,
                        Value = 10
                    },
                    new FilterDescriptor
                    {
                        Member = "status",
                        Operator = FilterOperator.IsEqualTo,
                        Value = 20
                    }
                }
            };

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, false, null, 461);
            var actualItems = (IList<POFulfillmentQueryModel>)data.Data;
            Assert.True(actualItems.Count() >= 1);
        }

        [Fact]
        public async Task External_FullSortAndFilter_NotFound()
        {
            var dataSourceRequest = new DataSourceRequest()
            {
                Page = 1,
                PageSize = 20,
                Sorts = new SortDescriptor[]
                {
                    new SortDescriptor
                    {
                        Member = "number",
                        SortCompare = null,
                        SortDirection = ListSortDirection.Ascending
                    }
                },
                Filters = new FilterDescriptor[]
                {
                    new FilterDescriptor
                    {
                        Member = "number",
                        Operator = FilterOperator.Contains,
                        Value = "mls"
                    },
                    new FilterDescriptor
                    {
                        Member = "customer",
                        Operator = FilterOperator.IsEqualTo,
                        Value = "David"
                    },
                    new FilterDescriptor
                    {
                        Member = "shipFromName",
                        Operator = FilterOperator.IsEqualTo,
                        Value = "US"
                    },
                    new FilterDescriptor
                    {
                        Member = "bookingDate",
                        Operator = FilterOperator.IsEqualTo,
                        Value = "1-1-2020"
                    },
                    new FilterDescriptor
                    {
                        Member = "stage",
                        Operator = FilterOperator.IsEqualTo,
                        Value = 10
                    },
                    new FilterDescriptor
                    {
                        Member = "status",
                        Operator = FilterOperator.IsEqualTo,
                        Value = 20
                    }
                }
            };

            var data = await _poFulfillmentListService.GetListPOFulfillmentAsync(dataSourceRequest, false, null, 461);
            var actualItems = (IList<POFulfillmentQueryModel>)data.Data;
            Assert.True(!actualItems.Any());
        }

        #endregion External

    }
}
