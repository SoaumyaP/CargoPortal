using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Groove.SP.Application.Consignment.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Consignment.Services
{
    public class ConsignmentListService : IConsignmentListService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        protected readonly IServiceProvider ServiceProvider;

        public ConsignmentListService(IDataQuery dataQuery,
                                IOptions<AppConfig> appConfig,
                                IServiceProvider serviceProvider)
        {
            _appConfig = appConfig.Value;
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> GetListConsignmentAsync(DataSourceRequest request, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();

            if (!string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            IQueryable<ConsignmentQueryModel> query;
            string sql;
            if (isInternal)
            {
                sql = @"SELECT C.[Id]
                              ,C.[ShipmentId]
	                          ,S.ShipmentNo
                              ,C.[ConsignmentDate]
                              ,C.[ShipFrom]
                              ,C.[ShipTo]
                              ,C.[Status]
                              ,C.[ExecutionAgentName] 
                      FROM [Consignments] C 
                          JOIN Shipments S ON S.Id = C.ShipmentId 
                      WHERE C.IsDeleted = 0 ";
            }
            else
            {
                sql = @"SELECT C.[Id]
                              ,C.[ShipmentId]
	                          ,S.ShipmentNo
                              ,C.[ConsignmentDate]
                              ,C.[ShipFrom]
                              ,C.[ShipTo]
                              ,C.[Status]
                              ,C.[ExecutionAgentName]
                      FROM [Consignments] C 
                          JOIN Shipments S ON S.Id = C.ShipmentId 
                      WHERE C.[ExecutionAgentId] IN (" + $"{string.Join(",", listOfAffiliates)}" + @") 
                          AND C.IsDeleted = 0 ";
            }

            query = _dataQuery.GetQueryable<ConsignmentQueryModel>(sql);

            return await query.ToDataSourceResultAsync(request);
        }

    }
}
