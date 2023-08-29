using Groove.CSFE.Application.Organizations.Services.Interfaces;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Organizations.Services
{
    public class OrganizationListService : IOrganizationListService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        protected readonly IServiceProvider ServiceProvider;

        public OrganizationListService(IDataQuery dataQuery,
                                IOptions<AppConfig> appConfig,
                                IServiceProvider serviceProvider)
        {
            _appConfig = appConfig.Value;
            _dataQuery = dataQuery;
        }

        public async Task<IList<CustomerRelationshipQueryModel>> GetListCustomerRelationshipAsync(long supplierRelationshipId)
        {
            IQueryable<CustomerRelationshipQueryModel> query;

            string sql = @"SELECT O.[Id]
                            ,O.[Code]
                            ,O.[Name]
	                        ,O.[ContactEmail]
                            ,O.[ContactName]
                            ,O.[OrganizationType]
                            ,O.[AdminUser]
                            ,O.[CreatedDate]
	                        ,C.[ConnectionType]
                            ,C.[IsConfirmConnectionType]
                            ,C.[CustomerRefId]
	                        ,CT.[Name] AS CountryName
                            FROM [Organizations] O
                                JOIN CustomerRelationship C ON O.Id = C.CustomerId
                                LEFT JOIN Locations L ON O.LocationId = L.Id
                                LEFT JOIN Countries CT ON CT.Id = L.CountryId
                            WHERE C.SupplierId = {0}";

            query = _dataQuery.GetQueryable<CustomerRelationshipQueryModel>(sql, supplierRelationshipId);

            return await query.ToListAsync();
        }

        public async Task<DataSourceResult> GetListSupplierRelationshipAsync(DataSourceRequest request, long customerRelationshipId)
        {
            IQueryable<CustomerRelationshipQueryModel> query;

            string sql = @"SELECT O.[Id]
                            ,O.[Code]
                            ,O.[Name]
	                        ,O.[ContactEmail]
                            ,O.[ContactName]
                            ,O.[OrganizationType]
                            ,O.[AdminUser]
                            ,O.[CreatedDate]
	                        ,C.[ConnectionType]
                            ,C.[IsConfirmConnectionType]
                            ,C.[CustomerRefId]
	                        ,CT.[Name] AS CountryName
                            FROM [Organizations] O
                                JOIN CustomerRelationship C ON O.Id = C.SupplierId
                                LEFT JOIN Locations L ON O.LocationId = L.Id
                                LEFT JOIN Countries CT ON CT.Id = L.CountryId
                            WHERE C.CustomerId = {0}";

            query = _dataQuery.GetQueryable<CustomerRelationshipQueryModel>(sql, customerRelationshipId);

            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<DataSourceResult> GetListOrganizationAsync(DataSourceRequest request)
        {
            IQueryable<OrganizationQueryModel> query;

            string sql = @"SELECT O.[Id]
                                ,O.[Code]
                                ,O.[Name]
                                ,O.[ContactEmail]
                                ,O.[ContactName]
                                ,O.[ContactNumber]
                                ,O.[WebsiteDomain]
	                            ,O.[Status]
	                            ,CASE WHEN [Status] = 0 THEN 'label.inactive'
		                                WHEN [Status] = 1 THEN 'label.active'
		                                WHEN [Status] = 2 THEN 'label.pending'
		                                ELSE '' END AS [StatusName]
                                ,O.[OrganizationType]
	                            ,CASE WHEN [OrganizationType] = 1 THEN 'label.general'
		                                WHEN [OrganizationType] = 2 THEN 'label.agent'
		                                WHEN [OrganizationType] = 4 THEN 'label.principal'
		                                ELSE '' END AS [OrganizationTypeName]
                                ,O.[CreatedDate]
	                            ,C.[Name] AS CountryName
                            FROM [Organizations] O
                            LEFT JOIN Locations L ON O.LocationId = L.Id
                            LEFT JOIN Countries C ON C.Id = L.CountryId";

            query = _dataQuery.GetQueryable<OrganizationQueryModel>(sql);

            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<IList<CustomerRelationshipQueryModel>> GetSuppliersAsync(long customerId)
        {
            IQueryable<CustomerRelationshipQueryModel> query;

            string sql = @"SELECT O.[Id]
                            ,O.[Code]
                            ,O.[Name]
	                        ,O.[ContactEmail]
                            ,O.[ContactName]
                            ,O.[OrganizationType]
                            ,O.[AdminUser]
                            ,O.[CreatedDate]
	                        ,C.[ConnectionType]
                            ,C.[IsConfirmConnectionType]
                            ,C.[CustomerRefId]
	                        ,CT.[Name] AS CountryName
                            FROM [Organizations] O
                                JOIN CustomerRelationship C ON O.Id = C.SupplierId
                                LEFT JOIN Locations L ON O.LocationId = L.Id
                                LEFT JOIN Countries CT ON CT.Id = L.CountryId
                            Where C.CustomerId = {0}";

            query = _dataQuery.GetQueryable<CustomerRelationshipQueryModel>(sql, customerId);

            return await query.ToListAsync();
        }
    }
}
