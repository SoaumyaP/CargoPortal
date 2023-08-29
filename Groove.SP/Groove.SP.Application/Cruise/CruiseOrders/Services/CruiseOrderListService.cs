using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Application.CruiseOrders.ViewModels;
using Groove.SP.Application.CruiseOrders.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace Groove.SP.Application.CruiseOrders.Services
{
    public class CruiseOrderListService : ServiceBase<CruiseOrderModel, CruiseOrderListViewModel>, ICruiseOrderListService
    {
        private readonly IDataQuery _dataQuery;
        public CruiseOrderListService(IUnitOfWorkProvider unitOfWorkProvider, IDataQuery dataQuery) : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long? organizationId = 0)
        {
            IQueryable<CruiseOrderQueryModel> query;
            string sql;
            if (isInternal)
            {
                sql =
                    @"SELECT co.Id, co.PONumber, co.PODate, co.POStatus, t1.CompanyName AS Consignee, t2.CompanyName AS Supplier
                      FROM cruise.CruiseOrders co
                      OUTER APPLY 
                      (
	                        SELECT CompanyName
	                        FROM cruise.CruiseOrderContacts cc
	                        WHERE cc.OrderId = co.Id AND cc.OrganizationRole = 'Consignee'
                      ) t1
                      OUTER APPLY 
                      (
	                        SELECT CompanyName
	                        FROM cruise.CruiseOrderContacts cc
	                        WHERE cc.OrderId = co.Id AND cc.OrganizationRole = 'Supplier'
                      ) t2
                    ";
            }
            else
            {
                sql =
                    @"SELECT t1.*, t2.*, t3.*
                      FROM (SELECT co.Id, co.PONumber, co.PODate, co.POStatus
	                        FROM cruise.CruiseOrders co
	                        WHERE EXISTS (
		                        SELECT cc.OrderId
		                        FROM cruise.CruiseOrderContacts cc
		                        WHERE cc.OrderId = co.Id AND cc.OrganizationId = {0})
                      ) t1
                      OUTER APPLY 
                      (
	                        SELECT CompanyName AS Consignee
	                        FROM cruise.CruiseOrderContacts cc
	                        WHERE cc.OrderId = t1.Id AND cc.OrganizationRole = 'Consignee'
                      ) t2
                      OUTER APPLY 
                      (
	                        SELECT CompanyName AS Supplier
	                        FROM cruise.CruiseOrderContacts cc
	                        WHERE cc.OrderId = t1.Id AND cc.OrganizationRole = 'Supplier'
                      ) t3
                    ";
            }

            query = _dataQuery.GetQueryable<CruiseOrderQueryModel>(sql, organizationId);
            return await query.ProjectTo<CruiseOrderListViewModel>(Mapper.ConfigurationProvider)
                .ToDataSourceResultAsync(request);
        }

    }
}
