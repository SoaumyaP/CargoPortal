using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Vessels.Services.Interfaces;
using Groove.CSFE.Application.Vessels.ViewModels;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Groove.CSFE.Application.Exceptions;

namespace Groove.CSFE.Application.Vessels.Services
{
    public class VesselService : ServiceBase<VesselModel, VesselViewModel>, IVesselService
    {
        private readonly IDataQuery _dataQuery;

        public VesselService(IUnitOfWorkProvider unitOfWorkProvider, IDataQuery dataQuery) : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0)
        {
            IQueryable<VesselQueryModel> query;
            string sql;

            sql =
                @"SELECT Id, Code, Name, Status, IsRealVessel
                  FROM Vessels WITH (NOLOCK)
                ";
            query = _dataQuery.GetQueryable<VesselQueryModel>(sql);
            return await query.ProjectTo<VesselListViewModel>(Mapper.ConfigurationProvider).ToDataSourceResultAsync(request);
        }

        public override async Task<IEnumerable<VesselViewModel>> GetAllAsync()
        {
            var models = await Repository.QueryAsNoTracking().ToListAsync();
            return Mapper.Map<IEnumerable<VesselViewModel>>(models);
        }

        public async Task<IEnumerable<VesselViewModel>> GetRealActiveListAsync()
        {
            var models = await Repository.QueryAsNoTracking(v => v.Status == VesselStatus.Active && v.IsRealVessel, null, null).ToListAsync();
            return Mapper.Map<IEnumerable<VesselViewModel>>(models);
        }

        public async Task<IEnumerable<VesselViewModel>> GetActiveListAsync()
        {
            var models = await Repository.QueryAsNoTracking(v => v.Status == VesselStatus.Active, null, null).ToListAsync();
            return Mapper.Map<IEnumerable<VesselViewModel>>(models);
        }

        public async Task<IEnumerable<DropDownListItem<string>>> SearchRealActiveByNameAsync(string name)
        {
            var searchParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@name",
                        Value = name,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            List<DropDownListItem<string>> mappingCallback(DbDataReader reader)
            {
                var vessels = new List<DropDownListItem<string>>();
                while (reader.Read())
                {
                    var row = new DropDownListItem<string>
                    {
                        Value = reader[0] as string,
                    };
                    vessels.Add(row);
                }
                return vessels;
            }

            var data = await _dataQuery.GetDataByStoredProcedureAsync("spu_GetVessels_Searching", mappingCallback, searchParameter.ToArray());

            return data;
        }

        public async Task<VesselViewModel> UpdateStatusAsync(long id, VesselStatus status, string userName)
        {
            var model = await Repository.FindAsync(id);
            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }
            model.Status = status;
            model.Audit(userName);
            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();
            return Mapper.Map<VesselViewModel>(model);
        }
    }
}
