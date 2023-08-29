using System;
using System.Collections.Generic;
using System.Threading.Tasks;


using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using Groove.SP.Core.Entities;
using System.Linq.Expressions;
using System.Linq;
using Groove.SP.Application.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using AutoMapper;
using System.Text.RegularExpressions;

namespace Groove.SP.Application.Shipments.Services
{
    public class ShipmentListService : IShipmentListService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        private readonly IServiceProvider _serviceProvider;

        public ShipmentListService(IDataQuery dataQuery,
                                IOptions<AppConfig> appConfig,
                                IServiceProvider serviceProvider)
        {
            _appConfig = appConfig.Value;
            _dataQuery = dataQuery;
            _serviceProvider = serviceProvider;
        }

        public async Task<DataSourceResult> GetListShipmentAsync(DataSourceRequest request, bool isInternal, string affiliates, string referenceNo = "", string statisticKey = "", string statisticFilter = "")
        {
            var dates = CommonHelper.GetDateRange(statisticFilter);

            Expression<Func<ShipmentQueryModel, bool>> filter = null;
            var listOfAffiliates = new List<long>();

            if (!string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            if (!string.IsNullOrEmpty(referenceNo))
            {
                var referenceNoParts = referenceNo.Split("~");
                referenceNo = string.Join("~", referenceNoParts.Skip(1).ToArray());
            }

            // Access shipment list from left menu -> there is no statistic key + statisticFilter + no quick search
            // AND interacting to Milestone column
            if (string.IsNullOrEmpty(statisticKey) && string.IsNullOrEmpty(statisticFilter) && request.IsInteractToColumn("activitycode") && string.IsNullOrEmpty(referenceNo))
            {
                return await GetShipmentListMilestoneAsync(request, isInternal, listOfAffiliates);
            }

            IQueryable<ShipmentQueryModel> query;
            string sql = "";
            string viewNameSql = "";
            string otherFilterSql = "";

            #region to link current statistic key to correct view

            if (statisticKey.StartsWith("Top10Carrier", StringComparison.OrdinalIgnoreCase)
                || statisticKey.StartsWith("ShipmentByConsigneeThisWeek", StringComparison.OrdinalIgnoreCase)
                || statisticKey.StartsWith("ShipmentByShipperThisWeek", StringComparison.OrdinalIgnoreCase))
            {
                viewNameSql = $"vw_ShipmentList_ShipmentByThisWeek";
                var keys = statisticKey.Split('-', 2);
                switch (keys[0].ToLowerInvariant())
                {
                    case "top10carrier":
                        otherFilterSql = $" AND Carrier = '{keys[1]}'";
                        break;
                    case "shipmentbyconsigneethisweek":
                        otherFilterSql = $" AND Consignee = '{keys[1]}'";
                        break;
                    case "shipmentbyshipperthisweek":
                        otherFilterSql = $" AND Shipper = '{keys[1]}'";
                        break;
                }



            }
            else if (statisticKey.StartsWith("ShipmentVolumeByOrigin", StringComparison.OrdinalIgnoreCase)
                || statisticKey.StartsWith("ShipmentVolumeByDestination", StringComparison.OrdinalIgnoreCase))
            {
                viewNameSql = $"vw_ShipmentList_ShipmentVolumeByOriginDestination";
                var keys = statisticKey.Split('-', 2);
                switch (keys[0].ToLowerInvariant())
                {
                    case "shipmentvolumebyorigin":
                        otherFilterSql = $" AND ShipFrom = '{keys[1]}'";
                        break;
                    case "shipmentvolumebydestination":
                        otherFilterSql = $" AND ShipTo = '{keys[1]}'";
                        break;
                }
            }
            else if (statisticKey.EndsWith("ShipmentVolumeInThisWeek", StringComparison.OrdinalIgnoreCase))
            {
                viewNameSql = $"vw_ShipmentList_MovementShipmentVolumeInThisWeek";
                switch (statisticKey.ToLowerInvariant())
                {
                    case "cfscfsshipmentvolumeinthisweek":
                        otherFilterSql = $" AND Movement = 'CFS/CFS' ";
                        break;
                    case "cfscyshipmentvolumeinthisweek":
                        otherFilterSql = $" AND Movement = 'CFS/CY' ";
                        break;
                    case "cycyshipmentvolumeinthisweek":
                        otherFilterSql = $" AND Movement = 'CY/CY' ";
                        break;

                }
            }
            else if (new[] { "monthlyPortToDoorShipmentVolume", "monthlyPortToPortShipmentVolume", "monthlyDoorToPortShipmentVolume", "monthlyDoorToDoorShipmentVolume", "manualInputServiceTypeShipmentVolume" }
                .Any(c => statisticKey.Contains(c, StringComparison.OrdinalIgnoreCase)))
            {
                viewNameSql = $"vw_ShipmentList_MonthlyServiceTypeShipmentVolume";
                switch (statisticKey.ToLowerInvariant())
                {
                    case "monthlyporttodoorshipmentvolume":
                        otherFilterSql = $" AND ServiceType = 'Port-to-Door' ";
                        break;
                    case "monthlyporttoportshipmentvolume":
                        otherFilterSql = $" AND ServiceType = 'Port-to-Port' ";
                        break;
                    case "monthlydoortoportshipmentvolume":
                        otherFilterSql = $" AND ServiceType = 'Door-to-Port' ";
                        break;
                    case "monthlydoortodoorshipmentvolume":
                        otherFilterSql = $" AND ServiceType = 'Door-to-Door' ";
                        break;
                    default:
                        otherFilterSql = $" AND ServiceType = '{statisticKey.Split("=")[1]}' ";
                        break;
                }
            }
            else if (new[] { "monthlycfscfsshipmentvolume", "monthlycfscyshipmentvolume", "monthlycycyshipmentvolume", "manualInputMovementShipmentVolume" }
                .Any(c => statisticKey.Contains(c, StringComparison.OrdinalIgnoreCase)))
            {
                viewNameSql = $"vw_ShipmentList_MonthlyMovementShipmentVolume";
                switch (statisticKey.ToLowerInvariant())
                {
                    case "monthlycfscfsshipmentvolume":
                        otherFilterSql = $" AND Movement = 'CFS/CFS' ";
                        break;
                    case "monthlycfscyshipmentvolume":
                        otherFilterSql = $" AND Movement = 'CFS/CY' ";
                        break;
                    case "monthlycycyshipmentvolume":
                        otherFilterSql = $" AND Movement = 'CY/CY' ";
                        break;
                    default:
                        otherFilterSql = $" AND Movement = '{statisticKey.Split("=")[1]}' ";
                        break;
                }
            }
            else
            {
                viewNameSql = $"vw_ShipmentList_{statisticKey}";
            }
            #endregion to link current statistic key to correct view


            if (isInternal)
            {
                #region Internal users
                if (!string.IsNullOrEmpty(statisticKey))
                {
                    sql = $@"SELECT DISTINCT
                                Id,
			                    ShipmentNo,
			                    CustomerReferenceNo,
                                NULL AS AgentReferenceNo,
			                    ShipFromETDDate,
			                    BookingDate,
			                    ShipFrom,
			                    ShipTo,
			                    [Status],
			                    Shipper,
			                    Consignee,
                                ActivityCode, 
                                Milestone
                                FROM {viewNameSql}_InternalUsers "
                            + $"WHERE ShipFromETDDate >= '{dates["FromDate"]}' AND ShipFromETDDate <= '{dates["ToDate"]}'"
                            + $"{otherFilterSql}";
                }
                else
                {
                    if (string.IsNullOrEmpty(referenceNo))
                    {
                        sql =
                            @$"
                            SELECT t1.*, t2.*, t3.*,t4.*
                            FROM
                            (
                            SELECT s.Id, s.ShipmentNo, s.ShipFrom, s.ShipTo, s.ShipFromETDDate, s.BookingDate, s.CustomerReferenceNo, s.AgentReferenceNo, s.Status, ModeOfTransport
                            FROM Shipments s WITH(NOLOCK)
                            ) t1
                            OUTER APPLY
                            (
		                            SELECT TOP(1) sc.CompanyName AS Shipper
		                            FROM ShipmentContacts sc WITH(NOLOCK)
		                            WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Shipper'
                            ) t2
                            OUTER APPLY
                            (
		                            SELECT TOP(1) sc.CompanyName AS Consignee
		                            FROM ShipmentContacts sc WITH(NOLOCK)
		                            WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Consignee'
                            ) t3
					        OUTER APPLY
					        (
							        SELECT *
    						        FROM ShipmentMilestones sms WITH(NOLOCK)
							        WHERE sms.ShipmentId = t1.Id
					        ) t4
                            ";
                    }
                    else
                    {
                        sql = @$"
                            SELECT t1.*, t2.*, t3.*,t4.*
                            FROM
                            (
                            SELECT s.Id, s.ShipmentNo, s.ShipFrom, s.ShipTo, s.ShipFromETDDate, s.BookingDate, s.CustomerReferenceNo, s.AgentReferenceNo, s.Status, ModeOfTransport, s.POFulfillmentId
                            FROM Shipments s WITH(NOLOCK)
                            ) t1
                            OUTER APPLY
                            (
		                            SELECT TOP(1) sc.CompanyName AS Shipper
		                            FROM ShipmentContacts sc WITH(NOLOCK)
		                            WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Shipper'
                            ) t2
                            OUTER APPLY
                            (
		                            SELECT TOP(1) sc.CompanyName AS Consignee
		                            FROM ShipmentContacts sc WITH(NOLOCK)
		                            WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Consignee'
                            ) t3
                            OUTER APPLY
                            (
		                            SELECT *
    	                            FROM ShipmentMilestones sms WITH(NOLOCK)
		                            WHERE sms.ShipmentId = t1.Id
                            ) t4
                            WHERE
                            t1.CustomerReferenceNo like '%" + referenceNo + @"%'
						    OR t1.AgentReferenceNo like '%" + referenceNo + @"%'
						    OR EXISTS (
								    SELECT 1
								    FROM
								    (
									    SELECT POD.CustomerPONumber
									    FROM POFulfillments POF WITH(NOLOCK) INNER JOIN POFulfillmentOrders POD WITH(NOLOCK) ON POF.Id = POD.POFulfillmentId
									    WHERE POF.Id = t1.POFulfillmentId
									    UNION
									    SELECT OD.PONumber as CustomerPONumber
									    FROM CargoDetails CD WITH(NOLOCK) INNER JOIN PurchaseOrders OD WITH(NOLOCK) ON CD.OrderId = OD.Id
									    WHERE t1.POFulfillmentId is NULL AND CD.ShipmentId = t1.Id
								    ) t5
								    WHERE t5.CustomerPONumber like '%" + referenceNo + @"%'
						    )
                            ";
                    }

                }
                #endregion Internal users
            }
            else
            {
                #region External users
                if (!string.IsNullOrEmpty(statisticKey))
                {
                    sql = $@"SELECT DISTINCT
                                Id,
			                    ShipmentNo,
			                    CustomerReferenceNo,
                                NULL AS AgentReferenceNo,
			                    ShipFromETDDate,
			                    BookingDate,
			                    ShipFrom,
			                    ShipTo,
			                    [Status],
			                    Shipper,
			                    Consignee,
                                ActivityCode,
                                Milestone
                                FROM {viewNameSql}_ExternalUsers "
                            + $"WHERE [OrganizationId] IN ({affiliates.Replace("[", "").Replace("]", "")}) AND ShipFromETDDate >= '{dates["FromDate"]}' AND ShipFromETDDate <= '{dates["ToDate"]}'"
                            + $"{otherFilterSql}";

                }
                else
                {
                    if (string.IsNullOrEmpty(referenceNo))
                    {
                        sql =
                        @$"
                        SELECT t1.*, t2.*, t3.*, t4.*
                        FROM
                        (
                        SELECT s.Id, s.ShipmentNo, s.ShipFrom, s.ShipTo, s.ShipFromETDDate, s.BookingDate, s.CustomerReferenceNo, s.AgentReferenceNo, s.Status,ModeOfTransport
                        FROM Shipments s WITH(NOLOCK)
                        WHERE EXISTS (
			                    SELECT 1 
			                    FROM ShipmentContacts sc WITH(NOLOCK) 
                                WHERE s.Id = sc.ShipmentId AND sc.OrganizationId IN (" + string.Join(",", listOfAffiliates) + @$")
	                        )
                        ) t1
                        OUTER APPLY
                        (
		                        SELECT TOP(1) sc.CompanyName AS Shipper
		                        FROM ShipmentContacts sc WITH(NOLOCK)
		                        WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Shipper'
                        ) t2
                        OUTER APPLY
                        (
		                        SELECT TOP(1) sc.CompanyName AS Consignee
		                        FROM ShipmentContacts sc WITH(NOLOCK)
		                        WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Consignee'
                        ) t3
                        OUTER APPLY
					    (
						    SELECT *
    						    FROM ShipmentMilestones sms WITH(NOLOCK)
							    WHERE sms.ShipmentId = t1.Id
					    ) t4 ";
                    }
                    else
                    {
                        sql =
                        @$"
                        SELECT t1.*, t2.*, t3.*, t4.*
                        FROM
                        (
                        SELECT s.Id, s.ShipmentNo, s.ShipFrom, s.ShipTo, s.ShipFromETDDate, s.BookingDate, s.CustomerReferenceNo, s.AgentReferenceNo, s.Status, ModeOfTransport, s.POFulfillmentId
                        FROM Shipments s WITH(NOLOCK)
                        WHERE EXISTS (
			                    SELECT 1 
			                    FROM ShipmentContacts sc WITH(NOLOCK) 
                                WHERE s.Id = sc.ShipmentId AND sc.OrganizationId IN (" + string.Join(",", listOfAffiliates) + @$")
	                        )
                        ) t1
                        OUTER APPLY
                        (
		                        SELECT TOP(1) sc.CompanyName AS Shipper
		                        FROM ShipmentContacts sc WITH(NOLOCK)
		                        WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Shipper'
                        ) t2
                        OUTER APPLY
                        (
		                        SELECT TOP(1) sc.CompanyName AS Consignee
		                        FROM ShipmentContacts sc WITH(NOLOCK)
		                        WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Consignee'
                        ) t3
                        OUTER APPLY
					    (
						    SELECT *
    						    FROM ShipmentMilestones sms WITH(NOLOCK)
							    WHERE sms.ShipmentId = t1.Id
					    ) t4
                        WHERE
                        t1.CustomerReferenceNo like '%" + referenceNo + @"%'
						OR t1.AgentReferenceNo like '%" + referenceNo + @"%'
						OR EXISTS (
								SELECT 1
								FROM
								(
									SELECT POD.CustomerPONumber
									FROM POFulfillments POF WITH(NOLOCK) INNER JOIN POFulfillmentOrders POD WITH(NOLOCK) ON POF.Id = POD.POFulfillmentId
									WHERE POF.Id = t1.POFulfillmentId
									UNION
									SELECT OD.PONumber as CustomerPONumber
									FROM CargoDetails CD WITH(NOLOCK) INNER JOIN PurchaseOrders OD WITH(NOLOCK) ON CD.OrderId = OD.Id
									WHERE t1.POFulfillmentId is NULL AND CD.ShipmentId = t1.Id
								) t5
								WHERE t5.CustomerPONumber like '%" + referenceNo + @"%'
						    )";
                    }
                }
                #endregion External users
            }

            query = _dataQuery.GetQueryable<ShipmentQueryModel>(sql);

            var data = await query.ToDataSourceResultAsync(request);
            return data;
        }




        /// <summary>
        /// To get searching data for shipment list as interacting to Milestone column.
        /// <br></br>
        /// <b>Notice: Please use the method only as filtering/sorting on Milestone column with normal mode (accessing from the left menu)</b>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<DataSourceResult> GetShipmentListMilestoneAsync(DataSourceRequest request, bool isInternal, IEnumerable<long> listOfAffiliates)
        {
            var filterOnAffiliates = isInternal ? "" : @$"
                            WHERE EXISTS (
                                SELECT 1
                                FROM ShipmentContacts sc WITH(NOLOCK)
                                WHERE s.Id = sc.ShipmentId AND sc.OrganizationId IN(" + string.Join(",", listOfAffiliates) + @")
	                    )";

            var sql = @$"
                    SELECT t1.*, t2.*, t3.*, t4.*
                    FROM
                    (
                        SELECT s.Id, s.ShipmentNo, s.ShipFrom, s.ShipTo, s.ShipFromETDDate, s.BookingDate, s.CustomerReferenceNo, s.AgentReferenceNo, s.Status, ModeOfTransport
                        FROM Shipments s WITH(NOLOCK)

                        {filterOnAffiliates}
                  
                    ) t1
                    OUTER APPLY
                    (
		                    SELECT TOP(1) sc.CompanyName AS Shipper
		                    FROM ShipmentContacts sc WITH(NOLOCK)
		                    WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Shipper'
                    ) t2
                    OUTER APPLY
                    (
		                    SELECT TOP(1) sc.CompanyName AS Consignee
		                    FROM ShipmentContacts sc WITH(NOLOCK)
		                    WHERE t1.Id = sc.ShipmentId AND sc.OrganizationRole = 'Consignee'
                    ) t3
					OUTER APPLY
					(
						    SELECT *
    						FROM ShipmentMilestones sms WITH(NOLOCK)
							WHERE sms.ShipmentId = t1.Id
					) t4 ";

            var query = _dataQuery.GetQueryable<ShipmentMilestoneSingleQueryModel>(sql);
            var statement = query.ToQueryString();
            // Mappping: transform query model (from sql script) to data model (to grid/list)
            var mapper = _serviceProvider.GetService(typeof(IMapper)) as IMapper;
            Func<ShipmentMilestoneSingleQueryModel, ShipmentQueryModel> mappingData = (x) => { return mapper.Map<ShipmentMilestoneSingleQueryModel, ShipmentQueryModel>(x); };

            // Call to get grid data from current grid request
            var result = await _dataQuery.ToDataSourceResultAsSingleQueryAsync<ShipmentMilestoneSingleQueryModel, ShipmentQueryModel>(query, request, mappingData);
            return result;
        }


        public async Task<DataSourceResult> GetListByFreightSchedulerAsync(DataSourceRequest request, long freightSchedulerId)
        {
            IQueryable<ShipmentScheduleQueryModel> query;
            string sql = @"
                            SELECT DISTINCT
	                            SHI.Id,
	                            SHI.ShipmentNo,
	                            SCS.CompanyName as Shipper,
	                            SCC.CompanyName as Consignee,
	                            SHI.TotalPackage,
                                SHI.TotalPackageUOM,
	                            SHI.TotalVolume,
                                SHI.TotalVolumeUOM,
	                            SHI.CargoReadyDate,
	                            E.ActivityDescription as LatestMilestone
                            FROM ConsignmentItineraries CSMI (NOLOCk)
	                            INNER JOIN Itineraries ITI (NOLOCK) ON ITI.Id = CSMI.ItineraryId
	                            INNER JOIN Shipments SHI (NOLOCK) ON SHI.Id = CSMI.ShipmentId AND SHI.[Status] = 'active'
	                            LEFT JOIN ShipmentContacts SCS (NOLOCK) ON SCS.ShipmentId = SHI.Id AND SCS.OrganizationRole = 'shipper'
	                            LEFT JOIN ShipmentContacts SCC (NOLOCK) ON SCC.ShipmentId = SHI.Id AND SCC.OrganizationRole = 'consignee'
	                            -- EventName of latest activity in Shipment page (cross module)
	                            OUTER APPLY (
		                            SELECT TOP 1 ACT.ActivityDescription
		                            FROM Activities ACT (NOLOCK) INNER JOIN GlobalIdActivities GIDACT (NOLOCK) ON ACT.Id = GIDACT.ActivityId
		                            INNER JOIN (
			                            SELECT 
				                            CONCAT('SHI_', SHI.Id) as GlobalIdActivity
			                            UNION

			                            SELECT
				                            CONCAT('POF_', s.POFulfillmentId)
			                            FROM Shipments s (NOLOCK)
			                            WHERE s.Id = SHI.Id AND s.POFulfillmentId IS NOT NULL
			                            UNION

			                            SELECT DISTINCT
				                            CONCAT('CPO_', PurchaseOrderId)
			                            FROM POFulfillmentOrders (NOLOCK)
			                            WHERE POFulfillmentId IN (
				                            SELECT POFulfillmentId FROM Shipments s (NOLOCK)
				                            WHERE s.Id = SHI.Id AND s.POFulfillmentId IS NOT NULL)
			                            UNION

			                            SELECT 
				                            CONCAT('CTN_', '_', sl.ContainerId)
			                            FROM ShipmentLoads sl (NOLOCK)
			                            WHERE sl.ContainerId IS NOT NULL AND sl.ShipmentId = SHI.Id
			                            UNION

			                            SELECT 
				                            CONCAT('FSC_', i.ScheduleId)
			                            FROM ConsignmentItineraries csmi (NOLOCK) INNER JOIN Itineraries i (NOLOCK) ON csmi.ItineraryId = i.Id AND i.ModeOfTransport = 'sea' AND i.ScheduleId IS NOT NULL
			                            WHERE csmi.ShipmentId = SHI.Id

		                            ) T ON T.GlobalIdActivity = GIDACT.GlobalId
		                            INNER JOIN EventCodes EC ON EC.ActivityCode = ACT.ActivityCode
		                            INNER JOIN EventTypes ET ON ET.Code = EC.ActivityTypeCode AND ET.EventLevel >= 4 -- Shipment event level
		                            ORDER BY GIDACT.ActivityDate DESC
	                            ) E
                            WHERE ITI.ScheduleId = {0}
                        ";
            query = _dataQuery.GetQueryable<ShipmentScheduleQueryModel>(sql, freightSchedulerId);
            return await query.ToDataSourceResultAsync(request);
        }
    }
}
