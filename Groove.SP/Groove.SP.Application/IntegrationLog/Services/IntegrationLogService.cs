using AutoMapper.QueryableExtensions;
using Groove.SP.Application.Common;
using Groove.SP.Application.IntegrationLog.Services.Interfaces;
using Groove.SP.Application.IntegrationLog.ViewModels;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.IntegrationLog.Services
{
    public class IntegrationLogService : ServiceBase<IntegrationLogModel, IntegrationLogViewModel>, IIntegrationLogService
    {
        private readonly IDataQuery _dataQuery;

        public IntegrationLogService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery)
            : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }

        public override async Task<DataSourceResult> GetListAsync(DataSourceRequest request, Func<IQueryable<IntegrationLogModel>, IQueryable<IntegrationLogModel>> includes = null, Expression<Func<IntegrationLogModel, bool>> customFilters = null)
        {
            IQueryable<IntegrationLogQueryModel> query;
            string sql = @"
                            SELECT 
                                   [Id]
                                  ,[Profile]
                                  ,[APIName]
                                  ,[EDIMessageType]
                                  ,[EDIMessageRef]
                                  ,[PostingDate]
                                  ,[Status]
                                  ,[Remark]
                              FROM IntegrationLogs (NOLOCK)
                            ";
            query = _dataQuery.GetQueryable<IntegrationLogQueryModel>(sql);

            ModifySorts(request.Sorts);
            ModifyFilters(request.Filters);
            return await query.ToDataSourceResultAsync(request);
        }

        public override async Task<IntegrationLogViewModel> CreateAsync(IntegrationLogViewModel viewModel)
        {
            using (var uow = UnitOfWorkProvider.CreateUnitOfWorkForBackgroundJob())
            {
                var model = Mapper.Map<IntegrationLogModel>(viewModel);
                model.Audit();
                var integrationLogRepository = uow.GetRepository<IntegrationLogModel>();
                await integrationLogRepository.AddAsync(model);
                await uow.SaveChangesAsync();
                viewModel = Mapper.Map<IntegrationLogViewModel>(model);
                return viewModel;
            }
        }
    }
}
