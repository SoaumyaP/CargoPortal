using Groove.SP.Application.POFulfillmentShortshipOrder.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillmentShortshipOrder.Services
{
    public class POFulfillmentShortshipOrderListService : IPOFulfillmentShortshipOrderListService
    {
        private readonly IDataQuery _dataQuery;
        public POFulfillmentShortshipOrderListService(IDataQuery dataQuery)
        {
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> SearchListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0, string userRole = "")
        {
            string sql = "";

            // Admin/CSR
            if (isInternal)
            {
                sql = @"SELECT 
	                    SSO.[Id],
	                    SSO.[POFulfillmentNumber],
	                    SSO.[POFulfillmentId],
	                    SSO.[CustomerPONumber],
	                    SSO.[PurchaseOrderId],
	                    SSO.[ProductCode],
	                    SSO.[OrderedUnitQty],
	                    SSO.[FulfillmentUnitQty],
	                    SSO.[BalanceUnitQty],
	                    SSO.[BookedPackage],
	                    SSO.[Volume],
	                    SSO.[GrossWeight],
	                    SSO.[IsRead],
	                    CAST(CONVERT(DATE, [ApprovedOn]) as DATE) as [ApprovedDate]
                    FROM POFulfillmentShortshipOrders SSO
					INNER JOIN POFulfillments POF ON SSO.POFulfillmentId = POF.Id AND POF.Stage = 20
					WHERE [IsRead] = 0";
            }
            else
            {
				sql = @"SELECT 
						SSO.[Id],
						SSO.[POFulfillmentNumber],
						SSO.[POFulfillmentId],
						SSO.[CustomerPONumber],
						SSO.[PurchaseOrderId],
						SSO.[ProductCode],
						SSO.[OrderedUnitQty],
						SSO.[FulfillmentUnitQty],
						SSO.[BalanceUnitQty],
						SSO.[BookedPackage],
						SSO.[Volume],
						SSO.[GrossWeight],
						SSO.[IsRead],
						CAST(CONVERT(DATE, SSO.[ApprovedOn]) as DATE) as [ApprovedDate]
					FROM POFulfillmentShortshipOrders SSO
					INNER JOIN POFulfillments POF ON SSO.POFulfillmentId = POF.Id AND POF.Stage = 20
					WHERE SSO.[IsRead] = 0 AND EXISTS (
						SELECT 1
						FROM POFulfillmentContacts POFC
						WHERE SSO.POFulfillmentId = POFC.POFulfillmentId AND POFC.OrganizationId = {0}
					)";

			}
            var query = _dataQuery.GetQueryable<POFulfillmentShortshipOrderQueryModel>(sql, organizationId);

            return await query.ToDataSourceResultAsync(request);
        }
    }
}