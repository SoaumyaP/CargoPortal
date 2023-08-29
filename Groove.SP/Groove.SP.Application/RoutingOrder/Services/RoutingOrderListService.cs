using Groove.SP.Application.RoutingOrder.Services.Interfaces;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.RoutingOrder.Services
{
    public class RoutingOrderListService : IRoutingOrderListService
    {
        private readonly IDataQuery _dataQuery;
        public RoutingOrderListService(IDataQuery dataQuery)
        {
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates, string customerRelationships, long? organizationId)
        {
			IQueryable<RoutingOrderQueryModel> query;
			var sql = @"
                        SELECT 
							rod.Id,
							rod.RoutingOrderNumber,
							rod.RoutingOrderDate,
							t1.CompanyName as ShipperCompany,
							t2.CompanyName as ConsigneeCompany,
							rod.Incoterm,
							CASE
								WHEN rod.Incoterm =" + (int)IncotermType.EXW + "THEN '" + IncotermType.EXW.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.FCA + "THEN '" + IncotermType.FCA.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.CPT + "THEN '" + IncotermType.CPT.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.CIP + "THEN '" + IncotermType.CIP.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.DAT + "THEN '" + IncotermType.DAT.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.DAP + "THEN '" + IncotermType.DAP.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.DDP + "THEN '" + IncotermType.DDP.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.FAS + "THEN '" + IncotermType.FAS.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.FOB + "THEN '" + IncotermType.FOB.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.CFR + "THEN '" + IncotermType.CFR.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.CIF + "THEN '" + IncotermType.CIF.ToString() + @"'
								WHEN rod.Incoterm =" + (int)IncotermType.DPU + "THEN '" + IncotermType.DPU.ToString() + @"'
								ELSE ''
							END AS IncotermName,
							t3.LocationDescription as [ShipFromName],
							t4.LocationDescription as [ShipToName],
							rod.CargoReadyDate,
							rod.[Status],
							CASE
								WHEN rod.[Status] =" + (int)RoutingOrderStatus.Active + "THEN '" + RoutingOrderStatus.Active.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								WHEN rod.[Status] =" + (int)RoutingOrderStatus.Cancel + "THEN '" + RoutingOrderStatus.Cancel.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								ELSE ''
							END AS StatusName,
							rod.Stage,
							CASE
								WHEN rod.Stage =" + (int)RoutingOrderStageType.Released + "THEN '" + RoutingOrderStageType.Released.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								WHEN rod.Stage =" + (int)RoutingOrderStageType.RateAccepted + "THEN '" + RoutingOrderStageType.RateAccepted.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								WHEN rod.Stage =" + (int)RoutingOrderStageType.RateConfirmed + "THEN '" + RoutingOrderStageType.RateConfirmed.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								WHEN rod.Stage =" + (int)RoutingOrderStageType.ForwarderBookingRequest + "THEN '" + RoutingOrderStageType.ForwarderBookingRequest.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								WHEN rod.Stage =" + (int)RoutingOrderStageType.ForwarderBookingConfirmed + "THEN '" + RoutingOrderStageType.ForwarderBookingConfirmed.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								WHEN rod.Stage =" + (int)RoutingOrderStageType.ShipmentDispatch + "THEN '" + RoutingOrderStageType.ShipmentDispatch.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								WHEN rod.Stage =" + (int)RoutingOrderStageType.Closed + "THEN '" + RoutingOrderStageType.Closed.GetAttributeValue<DisplayAttribute, string>(x => x.Name) + @"'
								ELSE ''
							END AS StageName
						FROM RoutingOrders rod
						OUTER APPLY (
							SELECT TOP(1) roc.CompanyName FROM RoutingOrderContacts roc WHERE roc.RoutingOrderId = rod.Id AND roc.OrganizationRole = {1}
						) t1
						OUTER APPLY (
							SELECT TOP(1) roc.CompanyName FROM RoutingOrderContacts roc WHERE roc.RoutingOrderId = rod.Id AND roc.OrganizationRole = {2}
						) t2
						OUTER APPLY (
							SELECT loc.LocationDescription FROM Locations loc WHERE loc.Id = rod.ShipFromId
						) t3
						OUTER APPLY (
							SELECT loc.LocationDescription FROM Locations loc WHERE loc.Id = rod.ShipToId
						) t4
                 ";

			if (isInternal)
            {
				// Admin, CSR
            }
            else
            {

				sql += @"
                        WHERE rod.Id IN (
										SELECT roc.RoutingOrderId FROM RoutingOrderContacts roc
										WHERE rod.Id = roc.RoutingOrderId AND roc.OrganizationId IN (SELECT value FROM [dbo].[fn_SplitStringToTable]({0},',')))
					";
			}

			var affiliatesByComma = string.Join(",", GetListAffiliates(affiliates) ?? new List<long>());

			query = _dataQuery.GetQueryable<RoutingOrderQueryModel>(sql, affiliatesByComma, OrganizationRole.Shipper, OrganizationRole.Consignee);

			return await query.ToDataSourceResultAsync(request);
		}

		private List<long> GetListAffiliates(string affiliates)
        {
			var listOfAffiliates = new List<long>();

			if (!string.IsNullOrEmpty(affiliates))
			{
				listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
			}

			return listOfAffiliates;
		}
    }
}
