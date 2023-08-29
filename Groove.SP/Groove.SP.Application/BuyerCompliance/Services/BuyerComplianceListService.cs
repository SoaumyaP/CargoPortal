using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Groove.SP.Application.BuyerCompliance.Services.Interfaces;
using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;


using Newtonsoft.Json;

namespace Groove.SP.Application.BuyerCompliance.Services
{
    public class BuyerComplianceListService : ServiceBase<BuyerComplianceModel, BuyerComplianceListViewModel>, IBuyerComplianceListService
    {
        private readonly IDataQuery _dataQuery;
        public BuyerComplianceListService(IUnitOfWorkProvider unitOfWorkProvider, IDataQuery dataQuery) : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }
        
        protected override IDictionary<string, string> SortMap => new Dictionary<string, string>() {
            { "statusName", "status" },
            { "stageName", "stage" }
        };

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request)
        {
            IQueryable<BuyerComplianceQueryModel> query;
            string sql = @" SELECT BC.Id, BC.CreatedDate, BC.CreatedBy, BC.[Name], BC.OrganizationName, BC.Stage, STG.StageName, BC.[Status], STA.StatusName
                            FROM BuyerCompliances bc
                            OUTER APPLY 
							(
								SELECT CASE		WHEN Stage = 0 THEN 'label.cancelled'
												WHEN Stage = 1 THEN 'label.activated'
												WHEN Stage = 2 THEN 'label.draft'
												ELSE '' END AS [StageName]

							) STG
							OUTER APPLY 
							(
								SELECT CASE		WHEN Status = 1 THEN 'label.active'
												WHEN Status = 0 THEN 'label.cancel'
												ELSE '' END AS [StatusName]

							) STA";
           
            query = _dataQuery.GetQueryable<BuyerComplianceQueryModel>(sql);
            return await query.ToDataSourceResultAsync(request);
        }        
    }
}
