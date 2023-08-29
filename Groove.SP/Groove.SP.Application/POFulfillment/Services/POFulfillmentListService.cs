using System;
using System.Linq;
using System.Threading.Tasks;

using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Core.Models;
using Groove.SP.Core.Data;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using Groove.SP.Application.POFulfillment.ViewModels;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Groove.SP.Application.Common;
using Newtonsoft.Json;

namespace Groove.SP.Application.POFulfillment.Services
{
    public class POFulfillmentListService : IPOFulfillmentListService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        protected readonly IServiceProvider ServiceProvider;

        public POFulfillmentListService(IDataQuery dataQuery,
                                IOptions<AppConfig> appConfig,
                                IServiceProvider serviceProvider)
        {
            _appConfig = appConfig.Value;
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> GetListPOFulfillmentAsync(DataSourceRequest request, bool isInternal, string affiliates, long? organizationId = 0, string userRole = "", string statisticKey = "")
        {
            IQueryable<POFulfillmentQueryModel> query;
            string sql;
            if (isInternal)
            {
                sql = @"SELECT  POFF.[Id]
                                ,POFF.[Number]
                                ,POFF.[Status]
                                ,POFF.[Stage]
                                ,POFF.CreatedBy
                                ,POFF.[FulfillmentType]
                                ,POFF.[OrderFulfillmentPolicy]
                                ,CAST(POFF.[BookingDate] AS DATE) AS BookingDate
                                ,POFF.[CargoReadyDate]
                                ,POFF.[ShipFromName]
                                ,POFF.[IsRejected]
                                ,POFF.[CreatedDate]
                                ,PRIN.Customer
                                ,SUP.Supplier
                                ,STG.StageName
                                ,STA.StatusName
                                ,CASE WHEN BAV.Stage = {2} THEN CAST(1 AS BIT) ELSE CAST (0 AS BIT) END IsPending
                                ,CASE WHEN POAH.[Priority] IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST (0 AS BIT) END IsPOAdhocChanged 
                                FROM [POFulfillments] POFF

                                OUTER APPLY
                                (
	                                SELECT TOP(1) POFFC.CompanyName AS Supplier
	                                FROM POFulfillmentContacts POFFC
	                                WHERE POFF.Id = POFFC.POFulfillmentId AND POFFC.OrganizationRole = 'Supplier'
                                ) SUP
                                OUTER APPLY
                                (
                                    SELECT TOP(1) POFFC.CompanyName AS Customer
                                    FROM POFulfillmentContacts POFFC
                                    WHERE POFF.Id = POFFC.POFulfillmentId AND POFFC.OrganizationRole = 'Principal'
                                ) PRIN
                                OUTER APPLY
                                (
	                                SELECT MIN(B.Stage) AS Stage
                                    FROM BuyerApprovals B
                                    WHERE B.POFulfillmentId = POFF.Id
                                ) BAV
                                OUTER APPLY
                                (
	                                SELECT MIN(PA.[Priority]) AS [Priority]
                                    FROM PurchaseOrderAdhocChanges PA
                                    WHERE PA.POFulfillmentId = POFF.Id
                                ) POAH 
                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN POFF.Stage = 10 THEN 'label.draft'
					                                WHEN POFF.Stage = 20 THEN 'label.booked'
					                                WHEN POFF.Stage = 30 THEN 'label.bookingConfirmed'
					                                WHEN POFF.Stage = 35 THEN 'label.cargoReceived'
					                                WHEN POFF.Stage = 40 THEN 'label.shipmentDispatch'
					                                WHEN POFF.Stage = 50 THEN 'label.closed'
					                                ELSE '' END AS [StageName]

                                ) STG
                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN POFF.[Status] = 10 THEN 'label.active'
					                                WHEN POFF.[Status] = 20 THEN 'label.inactive'
					                                ELSE '' END AS [StatusName]

                                ) STA
                                ";
                if (statisticKey.Equals("draftBooking", StringComparison.OrdinalIgnoreCase))
                {
                    var email = _appConfig.CSREmailDomain;
                    switch (userRole)
                    {
                        case "CSR":
                            sql +=  @$"
                                        WHERE POFF.Stage = {(int)POFulfillmentStage.Draft} AND POFF.status = {(int)POFulfillmentStatus.Active} AND (POFF.CreatedBy LIKE '%{email}')";
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                sql = @"SELECT  POFF.[Id]
                                ,POFF.[Number]
                                ,POFF.[Status]
                                ,POFF.[Stage]
                                ,POFF.[FulfillmentType]
                                ,POFF.[OrderFulfillmentPolicy]
                                ,CAST(POFF.[BookingDate] AS DATE) AS BookingDate
                                ,POFF.[CargoReadyDate]
                                ,POFF.[ShipFromName]
                                ,POFF.[IsRejected]
                                ,POFF.[CreatedDate]
                                ,PRIN.Customer
                                ,SUP.Supplier
                                ,STG.StageName
                                ,STA.StatusName
                                ,CASE WHEN BAV.Stage = {2} THEN CAST(1 AS BIT) ELSE CAST (0 AS BIT) END IsPending
                                ,CASE WHEN POAH.[Priority] IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST (0 AS BIT) END IsPOAdhocChanged 
                                FROM [POFulfillments] POFF

                                OUTER APPLY
                                (
	                                SELECT TOP(1) POFFC.CompanyName AS Supplier
	                                FROM POFulfillmentContacts POFFC
	                                WHERE POFF.Id = POFFC.POFulfillmentId AND POFFC.OrganizationRole = 'Supplier'
                                ) SUP
                                OUTER APPLY
                                (
                                    SELECT TOP(1) POFFC.CompanyName AS Customer
                                    FROM POFulfillmentContacts POFFC
                                    WHERE POFF.Id = POFFC.POFulfillmentId AND POFFC.OrganizationRole = 'Principal'
                                ) PRIN
                                OUTER APPLY
                                (
	                                SELECT MIN(B.Stage) AS Stage
                                    FROM BuyerApprovals B
                                    WHERE B.POFulfillmentId = POFF.Id
                                ) BAV
                                OUTER APPLY
                                (
	                                SELECT MIN(PA.[Priority]) AS [Priority]
                                    FROM PurchaseOrderAdhocChanges PA
                                    WHERE PA.POFulfillmentId = POFF.Id
                                ) POAH 
                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN POFF.Stage = 10 THEN 'label.draft'
					                                WHEN POFF.Stage = 20 THEN 'label.booked'
					                                WHEN POFF.Stage = 30 THEN 'label.bookingConfirmed'
					                                WHEN POFF.Stage = 35 THEN 'label.cargoReceived'
					                                WHEN POFF.Stage = 40 THEN 'label.shipmentDispatch'
					                                WHEN POFF.Stage = 50 THEN 'label.closed'
					                                ELSE '' END AS [StageName]

                                ) STG
                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN POFF.[Status] = 10 THEN 'label.active'
					                                WHEN POFF.[Status] = 20 THEN 'label.inactive'
					                                ELSE '' END AS [StatusName]

                                ) STA

								WHERE EXISTS (
										  SELECT 1 
										  FROM POFulfillmentContacts POFC
										  WHERE POFF.Id = POFC.POFulfillmentId AND POFC.OrganizationId = {3}
									  )";
                if (statisticKey.Equals("draftBooking", StringComparison.OrdinalIgnoreCase))
                {
                    switch (userRole)
                    {
                        case "Shipper":
                            sql += @$" AND POFF.Status = {(int)POFulfillmentStatus.Active} AND POFF.Stage = {(int)POFulfillmentStage.Draft} AND (poff.CreatedBy NOT LIKE ('%{_appConfig.CSREmailDomain}'))";
                            break;
                        default:
                            break;
                    }
                }
            }

            query = _dataQuery.GetQueryable<POFulfillmentQueryModel>(sql,
                   OrganizationRole.Principal,
                   OrganizationRole.Supplier,
                   BuyerApprovalStage.Pending,
                   organizationId);

            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<int> GetBookingAwaitingForSubmissionAsync(long? organizationId = 0, string userRole = "")
        {
            var storedProcedureName = "spu_BookingStatistics_AwaitingForSubmission";
            var sql = $@"
                        -- Default is 0
                        SET @result = 0;

                        EXEC @result = {storedProcedureName} @userRole, @organizationId, @email

                        ";

            var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@userRole",
                        Value = userRole,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@organizationId",
                        Value = organizationId,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@email",
                        Value = _appConfig.CSREmailDomain,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };
            var sqlResult = _dataQuery.GetValueFromVariable(sql, filterParameter.ToArray());
            if (int.TryParse(sqlResult, out var result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }

        public async Task<int> GetBookingPendingForApprovalAsync(string affiliates = "", IdentityInfo currentUser = null)
        {
            string organizationIds = string.Empty;
            if (!string.IsNullOrEmpty(affiliates))
            {
                var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                organizationIds = string.Join(",", listOfAffiliates);
            }
            var storedProcedureName = "spu_BookingStatistics_PendingForApproval";

            var sql = $@"
                        -- Default is 0
                        SET @result = 0;

                        EXEC @result = {storedProcedureName} @userRole, @organizationIds, @userEmail

                        ";

            var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@userRole",
                        Value = currentUser?.UserRoleName,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@organizationIds",
                        Value = organizationIds,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@userEmail",
                        Value = currentUser?.Email,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };
            var sqlResult = _dataQuery.GetValueFromVariable(sql, filterParameter.ToArray());
            if (int.TryParse(sqlResult, out var result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }
    }
}