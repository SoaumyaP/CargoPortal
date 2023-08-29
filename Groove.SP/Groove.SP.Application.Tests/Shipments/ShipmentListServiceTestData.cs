using Groove.SP.Core.Data;
using System.Collections.Generic;

namespace Groove.SP.Application.Tests.Shipments
{
    public class ShipmentListServiceTestData
    {
        public static IEnumerable<object[]> SortingDescending
        {
            get
            {
                yield return new object[] { 
                    new DataSourceRequest 
                    {
                        Page = 1,
                        PageSize = 20,
                        Skip = 0,
                        Sorts = new SortDescriptor[]
                        {
                            new SortDescriptor
                            {
                                Member = "shipmentNo",
                                SortCompare = null,
                                SortDirection = ListSortDirection.Descending
                            }
                        }
                    }
                };

                yield return new object[] {
                    new DataSourceRequest
                    {
                        Page = 1,
                        PageSize = 20,
                        Skip = 0,
                        Sorts = new SortDescriptor[]
                        {
                            new SortDescriptor
                            {
                                Member = "shipper",
                                SortCompare = null,
                                SortDirection = ListSortDirection.Descending
                            }
                        }
                    }
                };

                yield return new object[] { 
                    new DataSourceRequest
                    {
                        Page = 1,
                        PageSize = 20,
                        Skip = 0,
                        Sorts = new SortDescriptor[]
                        {
                            new SortDescriptor
                            {
                                Member = "consignee",
                                SortCompare = null,
                                SortDirection = ListSortDirection.Descending
                            }
                        }
                    }
                };

                yield return new object[] {
                    new DataSourceRequest
                    {
                        Page = 1,
                        PageSize = 20,
                        Skip = 0,
                        Sorts = new SortDescriptor[]
                        {
                            new SortDescriptor
                            {
                                Member = "shipFrom",
                                SortCompare = null,
                                SortDirection = ListSortDirection.Descending
                            }
                        }
                    }
                };

                yield return new object[] {
                    new DataSourceRequest
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
                                SortDirection = ListSortDirection.Descending
                            }
                        }
                    }
                };

                yield return new object[] {
                    new DataSourceRequest
                    {
                        Page = 1,
                        PageSize = 20,
                        Skip = 0,
                        Sorts = new SortDescriptor[]
                        {
                            new SortDescriptor
                            {
                                Member = "shipTo",
                                SortCompare = null,
                                SortDirection = ListSortDirection.Descending
                            }
                        }
                    }
                };

                yield return new object[] {
                    new DataSourceRequest
                    {
                        Page = 1,
                        PageSize = 20,
                        Skip = 0,
                        Sorts = new SortDescriptor[]
                        {
                            new SortDescriptor
                            {
                                Member = "status",
                                SortCompare = null,
                                SortDirection = ListSortDirection.Descending
                            }
                        }
                    }
                };
            }
        }

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
                            Member = "shipmentNo",
                            Operator = FilterOperator.IsEqualTo,
                            Value = "MLSPX-OCT0501"
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "shipFromETDDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Ascending
                        }
                    }
                }, "shipmentNo", "MLSPX-OCT0501"};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Filters = new FilterDescriptor[]
                    {
                        new FilterDescriptor
                        {
                            Member = "shipFrom",
                            Operator = FilterOperator.IsEqualTo,
                            Value = "MELBOURNE"
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "shipFromETDDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Ascending
                        }
                    }
                }, "shipFrom", "MELBOURNE"};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Filters = new FilterDescriptor[]
                    {
                        new FilterDescriptor
                        {
                            Member = "shipFromETDDate",
                            Operator = FilterOperator.IsEqualTo,
                            Value = "03/22/2021"
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "shipFromETDDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Ascending
                        }
                    }
                }, "shipFromETDDate", "03/22/2021"};

                yield return new object[] { new DataSourceRequest
                {
                    Page = 1,
                    PageSize = 20,
                    Skip = 0,
                    Filters = new FilterDescriptor[]
                    {
                        new FilterDescriptor
                        {
                            Member = "shipTo",
                            Operator = FilterOperator.IsEqualTo,
                            Value = "NINGBO"
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "shipFromETDDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Ascending
                        }
                    }
                }, "shipTo", "NINGBO"};

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
                            Value = "Inactive"
                        }
                    },
                    Sorts = new SortDescriptor[]
                    {
                        new SortDescriptor
                        {
                            Member = "shipFromETDDate",
                            SortCompare = null,
                            SortDirection = ListSortDirection.Ascending
                        }
                    }
                }, "status", "Inactive"};
            }
        }
    }
}
