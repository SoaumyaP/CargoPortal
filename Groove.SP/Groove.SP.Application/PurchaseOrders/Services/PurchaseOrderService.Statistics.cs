using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.Services
{
    /// <summary>
    /// To contain services/methods to handle PO Statistics on Dashboard
    /// </summary>
    public partial class PurchaseOrderService : ServiceBase<PurchaseOrderModel, PurchaseOrderViewModel>, IPurchaseOrderService
    {
        #region Statistics
        #region Statistics
        public async Task<StatisticsPOViewModel> GetBookedPOStatistics(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "spu_POStatistics_Booked";
            if (!isInternal)
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
            }
            var filterParameter = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@isInternal",
                    Value = isInternal,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@affiliates",
                    Value = affiliates,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                 new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
            };

            StatisticsPOViewModel mappingCallback(DbDataReader reader)
            {
                var viewModel = new StatisticsPOViewModel();
                while (reader.Read())
                {
                    viewModel.TotalPO = (int)reader[0];
                }
                return viewModel;
            }

            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
            return result;
        }

        public async Task<StatisticsPOViewModel> GetUnbookedPOStatistics(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "spu_POStatistics_Unbooked";
            if (!isInternal)
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
            }
            var filterParameter = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@isInternal",
                    Value = isInternal,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@affiliates",
                    Value = affiliates,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
            };

            StatisticsPOViewModel mappingCallback(DbDataReader reader)
            {
                var viewModel = new StatisticsPOViewModel();
                while (reader.Read())
                {
                    viewModel.TotalPO = (int)reader[0];
                }
                return viewModel;
            }

            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
            return result;
        }

        public async Task<StatisticsPOInOriginDCViewModel> GetInOriginDCPOStatistics(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "";
            StatisticsPOInOriginDCViewModel mappingCallback(DbDataReader reader)
            {
                var viewModel = new StatisticsPOInOriginDCViewModel();
                while (reader.Read())
                {
                    viewModel.Total = (int)reader[0];
                }
                return viewModel;
            }

            if (isInternal)
            {
                storedProcedureName = "spu_POStatistics_InOriginDC_Internal";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@FromDate",
                        Value = dates["FromDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ToDate",
                        Value = dates["ToDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
            else
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
                storedProcedureName = "spu_POStatistics_InOriginDC_External";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@Affiliates",
                        Value = affiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FromDate",
                        Value = dates["FromDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ToDate",
                        Value = dates["ToDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
        }

        public async Task<StatisticsPOInTransitViewModel> GetInTransitPOStatistics(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "";
            StatisticsPOInTransitViewModel mappingCallback(DbDataReader reader)
            {
                var viewModel = new StatisticsPOInTransitViewModel();
                while (reader.Read())
                {
                    viewModel.Total = (int)reader[0];
                }
                return viewModel;
            }

            if (isInternal)
            {
                storedProcedureName = "spu_POStatistics_InTransit_Internal";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@FromDate",
                        Value = dates["FromDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ToDate",
                        Value = dates["ToDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
            else
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
                storedProcedureName = "spu_POStatistics_InTransit_External";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@Affiliates",
                        Value = affiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@FromDate",
                        Value = dates["FromDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@ToDate",
                        Value = dates["ToDate"],
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
        }

        public async Task<StatisticsPOCustomsClearedViewModel> GetCustomsClearedPOStatistics(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "";
            StatisticsPOCustomsClearedViewModel mappingCallback(DbDataReader reader)
            {
                var viewModel = new StatisticsPOCustomsClearedViewModel();
                while (reader.Read())
                {
                    viewModel.Total = (int)reader[0];
                }
                return viewModel;
            }

            if (isInternal)
            {
                storedProcedureName = "spu_POStatistics_CustomsCleared_Internal";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
            else
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
                storedProcedureName = "spu_POStatistics_CustomsCleared_External";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@Affiliates",
                        Value = affiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
        }

        public async Task<StatisticsPOPendingDCDeliveryViewModel> GetPendingDCDeliveryPOStatistics(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "";
            StatisticsPOPendingDCDeliveryViewModel mappingCallback(DbDataReader reader)
            {
                var viewModel = new StatisticsPOPendingDCDeliveryViewModel();
                while (reader.Read())
                {
                    viewModel.Total = (int)reader[0];
                }
                return viewModel;
            }

            if (isInternal)
            {
                storedProcedureName = "spu_POStatistics_PendingDCDelivery_Internal";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
            else
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
                storedProcedureName = "spu_POStatistics_PendingDCDelivery_External";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@Affiliates",
                        Value = affiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
        }

        public async Task<StatisticsPODCDeliveryConfirmedViewModel> GetDCDeliveryConfirmedPOStatistics(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "";
            StatisticsPODCDeliveryConfirmedViewModel mappingCallback(DbDataReader reader)
            {
                var viewModel = new StatisticsPODCDeliveryConfirmedViewModel();
                while (reader.Read())
                {
                    viewModel.Total = (int)reader[0];
                }
                return viewModel;
            }

            if (isInternal)
            {
                storedProcedureName = "spu_POStatistics_DCDeliveryConfirmed_Internal";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
            else
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
                storedProcedureName = "spu_POStatistics_DCDeliveryConfirmed_External";
                var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@Affiliates",
                        Value = affiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
                };
                var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
                return result;
            }
        }

        #endregion

        public async Task<StatisticsPOManagedToDateViewModel> GetManagedToDatePOStatistics(bool isInternal, string affiliates, string statisticFilter)
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);
            var storedProcedureName = "spu_POStatistics_ManagedToDate";
            if (!isInternal)
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
            }
            var filterParameter = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@IsInternal",
                    Value = isInternal,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@Affiliates",
                    Value = affiliates,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = dates["FromDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = dates["ToDate"],
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
            };

            StatisticsPOManagedToDateViewModel mappingCallback(DbDataReader reader)
            {
                var viewModel = new StatisticsPOManagedToDateViewModel();
                while (reader.Read())
                {
                    viewModel.NumberOfPO = (long)reader[0];
                    viewModel.CBM = (decimal)reader[1];
                    viewModel.Units = (long)reader[2];
                    viewModel.FOBPrice = (decimal)reader[3];
                }
                return viewModel;
            }

            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
            return result;
        }

        public async Task<ListPagingViewModel<string>> SearchCategorizedPOAsync(CategorizedPOType type, string searchTearm, int page, int pageSize, bool isInternal, string affiliates, string supplierCustomerRelationships, long? delegatedOrganizationId = 0)
        {
            if (!string.IsNullOrEmpty(affiliates))
            {
                var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                affiliates = string.Join(",", listOfAffiliates);
            }

            var storedProcedureName = "";

            switch (type)
            {
                case CategorizedPOType.Supplier:
                    storedProcedureName = "spu_SearchCategorizedSupplier_PurchaseOrder";
                    break;
                case CategorizedPOType.Consignee:
                    storedProcedureName = "spu_SearchCategorizedConsignee_PurchaseOrder";
                    break;
                case CategorizedPOType.Destination:
                    storedProcedureName = "spu_SearchCategorizedDestination_PurchaseOrder";
                    break;
                case CategorizedPOType.Stage:
                    break;
                case CategorizedPOType.Status:
                    break;
                default:
                    break;
            }
            
            
            var filterParameter = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@IsInternal",
                    Value = isInternal,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@OrganizationId",
                    Value = delegatedOrganizationId,
                    DbType = DbType.Int64,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@SupplierCustomerRelationships",
                    Value = supplierCustomerRelationships,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@Affiliates",
                    Value = affiliates,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@SearchTerm",
                    Value = searchTearm,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@Page",
                    Value = page,
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@PageSize",
                    Value = pageSize,
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Input
                }
            };

            ListPagingViewModel<string> mappingCallback(DbDataReader reader)
            {
                var mappedData = new ListPagingViewModel<string>
                {
                    Page = page,
                    PageSize = pageSize,
                    Items = new()
                };

                while (reader.Read())
                {
                    var recordCount = (int)reader[0];
                    mappedData.TotalItem = recordCount;
                }

                mappedData.PageTotal = (int)Math.Ceiling((double)mappedData.TotalItem / pageSize);

                reader.NextResult();

                while (reader.Read())
                {
                    var displayName = reader["Name"].ToString();
                    mappedData.Items.Add(displayName);
                }

                return mappedData;
            }

            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
            return result;
        }

        #endregion
    }
}