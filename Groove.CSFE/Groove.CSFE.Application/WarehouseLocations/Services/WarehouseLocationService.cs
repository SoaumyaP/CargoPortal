using GGroove.CSFE.Application;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Exceptions;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Application.WarehouseLocations.Mappers;
using Groove.CSFE.Application.WarehouseLocations.Services.Interfaces;
using Groove.CSFE.Application.WarehouseLocations.ViewModels;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.WarehouseLocations.Services
{
    public class WarehouseLocationService : ServiceBase<WarehouseLocationModel, WarehouseLocationViewModel>, IWarehouseLocationService
    {
        private readonly IDataQuery _dataQuery;
        public WarehouseLocationService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery)
            : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }

        public async Task<IEnumerable<WarehouseLocationViewModel>> GetByOrganizationAsync(long organizationId)
        {
            var warehouseLocationModels = await Repository.QueryAsNoTracking(w => w.WarehouseAssignments.Select(y => y.OrganizationId).Contains(organizationId), null, x => x.Include(y => y.Organization))
                .OrderBy(x => x.WarehouseAssignments.First(y => y.OrganizationId == organizationId).CreatedDate)
                .ToListAsync();

            return Mapper.Map<IEnumerable<WarehouseLocationViewModel>>(warehouseLocationModels);
        }

        public async Task<IEnumerable<WarehouseLocationDropDownViewModel>> GetDropdownAsync(string searchTerm = null)
        {
            var warehouseLocationModels = await Repository.QueryAsNoTracking(x => x.Code.Contains(searchTerm) || string.IsNullOrEmpty(searchTerm)).ToListAsync();

            return warehouseLocationModels?.Select(x => new WarehouseLocationDropDownViewModel
            {
                Label = x.Code,
                Value = x.Id,
                Name = x.Name,
                Address = ConcatenateCompanyAddressLinesResolver.ConcatenateCompanyAddressLines(x.AddressLine1, x.AddressLine2, x.AddressLine3, x.AddressLine4),
                ContactPerson = x.ContactPerson,
                ContactPhone = x.ContactPhone,
                ContactEmail = x.ContactEmail
            }) ?? new WarehouseLocationDropDownViewModel[] { };
        }

        public async Task<IEnumerable<OrganizationReferenceDataViewModel>> GetCustomersByWarehouseLocationIdAsync(long warehouseLocationId)
        {
            var sql = $@"
                    SELECT ORG.Id
	                     ,ORG.Code
	                     ,ORG.[Name]
	                     ,WHA.ContactPerson
	                     ,WHA.ContactPhone
	                     ,WHA.ContactEmail
                    FROM Organizations ORG WITH(NOLOCK)
                    INNER JOIN WarehouseAssignments WHA WITH(NOLOCK) ON ORG.Id = WHA.OrganizationId
                    WHERE WHA.WarehouseLocationId = {warehouseLocationId}
                    ORDER BY ORG.Code
                ";
            Func<DbDataReader, IEnumerable<OrganizationReferenceDataViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<OrganizationReferenceDataViewModel>();

                while (reader.Read())
                {
                    var newRow = new OrganizationReferenceDataViewModel
                    {
                       Id = (long)reader[0],
                       Code = reader[1].ToString(),
                       Name = reader[2].ToString()
                    };
                    var temp = reader[3];
                    newRow.ContactName = temp == DBNull.Value ? null : temp.ToString();
                    temp = reader[4];
                    newRow.ContactNumber = temp == DBNull.Value ? null : temp.ToString();
                    temp = reader[5];
                    newRow.ContactEmail = temp == DBNull.Value ? null : temp.ToString();

                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = _dataQuery.GetDataBySql(sql, mapping);
            return result;
        }

        public async Task<WarehouseLocationViewModel> CreateWarehouseLocationAsync(WarehouseLocationViewModel viewModel, string userName)
        {
            var isCodeExisting = await Repository.AnyAsync(c => c.Code == viewModel.Code);
            if (isCodeExisting == true)
            {
                throw new AppValidationException($"#WarehouseLocationCodeDuplicated# Warehouse Location Code {viewModel.Code} already exists.");
            }

            viewModel.Audit(userName);
            var warehouseLocation = Mapper.Map<WarehouseLocationModel>(viewModel);

            await Repository.AddAsync(warehouseLocation);
            await UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<WarehouseLocationViewModel>(warehouseLocation);
            return viewModel;
        }

        public async Task<WarehouseLocationViewModel> UpdateWarehouseLocationAsync(WarehouseLocationViewModel viewModel, string userName)
        {
            var isCodeExisting = await Repository.AnyAsync(c => c.Code == viewModel.Code & c.Id != viewModel.Id);
            if (isCodeExisting == true)
            {
                throw new AppValidationException($"#WarehouseLocationCodeDuplicated# Warehouse Location Code {viewModel.Code} already exists.");
            }

            viewModel.Audit(userName);
            var warehouseLocation = Mapper.Map<WarehouseLocationModel>(viewModel);

            Repository.Update(warehouseLocation);
            await UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<WarehouseLocationViewModel>(warehouseLocation);
            return viewModel;
        }
    }
}