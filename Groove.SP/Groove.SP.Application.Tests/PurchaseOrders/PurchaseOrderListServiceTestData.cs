using Groove.SP.Core.Data;
using System.Collections.Generic;

namespace Groove.SP.Application.Tests.PurchaseOrders
{
    public class PurchaseOrderListServiceTestData
    {
        public static IEnumerable<object[]> FilteringIsEqualTo
        {
            get
            {
                yield return new object[] { new DataSourceRequest
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
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Filters = new FilterDescriptor[]
                    {
                        new FilterDescriptor
                        {
                            Member = "createdDate",
                            Operator = FilterOperator.IsEqualTo,
                            Value = "2020-05-12"
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "createdDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Filters = new FilterDescriptor[]
                    {
                        new FilterDescriptor
                        {
                            Member = "status",
                            Operator = FilterOperator.IsEqualTo,
                            Value = 1 // Active
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "createdDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Filters = new FilterDescriptor[]
                    {
                        new FilterDescriptor
                        {
                            Member = "cargoReadyDate",
                            Operator = FilterOperator.IsEqualTo,
                            Value = "2020-05-07"
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "createdDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Filters = new FilterDescriptor[]
                    {
                        new FilterDescriptor
                        {
                            Member = "supplier",
                            Operator = FilterOperator.IsEqualTo,
                            Value = "MLS Supplier 1"
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "createdDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Filters = new FilterDescriptor[]
                    {
                        new FilterDescriptor
                        {
                            Member = "stage",
                            Operator = FilterOperator.IsEqualTo,
                            Value = 40
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "createdDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};
            }
        }

        public static IEnumerable<object[]> SortingDescending
        {
            get
            {
                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "poNumber",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
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
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "statusName",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "cargoReadyDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "supplier",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "stageName",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Descending
                        }
                    }
                }};
            }
        }
    }
}
