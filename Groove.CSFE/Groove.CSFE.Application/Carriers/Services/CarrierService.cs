using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using Groove.CSFE.Application.Carriers.ViewModels;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Groove.CSFE.Application.Carriers.Services
{
    public class CarrierService : ServiceBase<CarrierModel, CarrierViewModel>, ICarrierService
    {
        private readonly IDataQuery _dataQuery;
        public CarrierService(IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery)
           : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }

        public override async Task<DataSourceResult> ListAsync(DataSourceRequest request)
        {
            IQueryable<CarrierQueryModel> query;
            string sql;

            sql =
                @"SELECT 
                    Id,
	                ModeOfTransport,
	                [Name],
	                [Status],
                    [CarrierNumber],
                    [CarrierCode],
	                CAST(CASE
                            WHEN ModeOfTransport = {0} AND CarrierNumber IS NOT NULL
                                THEN CONCAT(CarrierCode, ' (', CarrierNumber, ')')
                            ELSE CarrierCode
                        END AS NVARCHAR) AS CarrierCodeNumber
                FROM Carriers WITH (NOLOCK)
                ";

            query = _dataQuery.GetQueryable<CarrierQueryModel>(sql, ModeOfTransport.Air);
            return await query.ProjectTo<CarrierListViewModel>(Mapper.ConfigurationProvider).ToDataSourceResultAsync(request);
        }

        public async Task<CarrierViewModel> GetByCodeAsync(string code)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.CarrierCode == code);

            return Mapper.Map<CarrierModel, CarrierViewModel>(model);
        }

        public async Task<CarrierViewModel> GetByIdAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.Id.Equals(id));

            return Mapper.Map<CarrierModel, CarrierViewModel>(model);
        }

        public IEnumerable<CarrierViewModel> GetAllCarriers(string code)
        {
            var model = this.Repository.GetListQueryable(null);

            if (!string.IsNullOrEmpty(code))
            {
                model = model.Where(c => c.CarrierCode.Equals(code));
            }

            return Mapper.Map<IEnumerable<CarrierViewModel>>(model);
        }

        public async Task<bool> CheckDuplicateCarrierCodeAsync(CarrierViewModel model)
        {
            string toCompareCarrierCode = null;
            if (!string.IsNullOrEmpty(model.CarrierCode))
            {
                toCompareCarrierCode = model.CarrierCode.Trim();
            }
            return await Repository.AnyAsync(c => !string.IsNullOrEmpty(c.CarrierCode) && c.CarrierCode == toCompareCarrierCode && c.Id != model.Id);
        }

        public async Task<bool> CheckDuplicateCarrierNameAsync(CarrierViewModel model)
        {
            string toCompareCarrierName = null;
            if (!string.IsNullOrEmpty(model.Name))
            {
                toCompareCarrierName = model.Name.Trim();
            }
            return await Repository.AnyAsync(c => !string.IsNullOrEmpty(c.Name) && c.Name == toCompareCarrierName && c.Id != model.Id);
        }

        public async Task<bool> CheckDuplicateCarrierNumberAsync(CarrierViewModel model)
        {
            return await Repository.AnyAsync(c
                => c.CarrierNumber != null && c.CarrierNumber == model.CarrierNumber && c.Id != model.Id);
        }

        public async Task<CarrierViewModel> UpdateStatusAsync(long id, CarrierStatus status, string userName)
        {
            var model = await Repository.FindAsync(id);
            model.Status = status;
            model.Audit(userName);
            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();
            return Mapper.Map<CarrierViewModel>(model);
        }
    }
}
