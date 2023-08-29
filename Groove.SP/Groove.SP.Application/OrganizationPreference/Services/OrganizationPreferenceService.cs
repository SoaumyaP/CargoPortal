using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.OrganizationPreference.Services.Interfaces;
using Groove.SP.Application.OrganizationPreference.ViewModels;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Note.Services
{
    public class OrganizationPreferenceService : ServiceBase<OrganizationPreferenceModel, OrganizationPreferenceViewModel>, IOrganizationPreferenceService
    {
        private readonly IOrganizationPreferenceRepository _organizationPreferenceRepository;

        public OrganizationPreferenceService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IOrganizationPreferenceRepository organizationPreferenceRepository
            )
            : base(unitOfWorkProvider)
        {
            _organizationPreferenceRepository = organizationPreferenceRepository;
        }

        public async Task<OrganizationPreferenceViewModel> GetAsNoTrackingAsync(long organizationId, string productCode)
        {
            var orgPreference = await _organizationPreferenceRepository.GetAsNoTrackingAsync(c => c.OrganizationId == organizationId && c.ProductCode == productCode);
            var result = Mapper.Map<OrganizationPreferenceViewModel>(orgPreference);
            return result;
        }

        public async Task<IEnumerable<OrganizationPreferenceViewModel>> GetAsNoTrackingAsync(long organizationId, IEnumerable<string> productCodeList)
        {
            var orgPreferences = _organizationPreferenceRepository.QueryAsNoTracking(c => c.OrganizationId == organizationId && productCodeList.Contains(c.ProductCode));
            var result = Mapper.Map<IEnumerable<OrganizationPreferenceViewModel>>(orgPreferences);
            return result;
        }

        public async Task<IEnumerable<OrganizationPreferenceViewModel>> GetAsNoTrackingAsync(long organizationId)
        {
            var orgPreferences = _organizationPreferenceRepository.QueryAsNoTracking(c => c.OrganizationId == organizationId);
            var result = Mapper.Map<IEnumerable<OrganizationPreferenceViewModel>>(orgPreferences);
            return result;
        }

        public async Task InsertOrUpdateRangeAsync(IEnumerable<OrganizationPreferenceViewModel> viewModels, long organizationId, string userName)
        {
            // early return if data not valid
            if (viewModels == null || !viewModels.Any())
            {
                return;
            }
            var filteredData = viewModels
                    .Where(x => !string.IsNullOrEmpty(x.ProductCode))
                    .ToList();
            if (filteredData == null || !viewModels.Any())
            {
                return;
            }

            // get list of unique product code
            var productCodes = filteredData.Select(c => c.ProductCode).Distinct();

            // filter data to get final
            var insertOrUpdateData = new List<OrganizationPreferenceViewModel>();

            foreach (var productCode in productCodes)
            {
                insertOrUpdateData.Add(filteredData.Last(x => x.ProductCode == productCode));
            }

            // store organization preference if either HS code / Chinese description available
            insertOrUpdateData = insertOrUpdateData.Where(x => !string.IsNullOrEmpty(x.HSCode) || !string.IsNullOrEmpty(x.ChineseDescription)).ToList();

            // load data from database
            var storedData = _organizationPreferenceRepository.Query(x => x.OrganizationId == organizationId && productCodes.Contains(x.ProductCode));

            var newDataList = new List<OrganizationPreferenceModel>();
            foreach (var insertOrUpdate in insertOrUpdateData)
            {
                var stored = storedData.FirstOrDefault(x => x.ProductCode == insertOrUpdate.ProductCode);
                if (stored != null)
                {
                    // ignore update if new value is NULL/empty
                    stored.HSCode = !string.IsNullOrEmpty(insertOrUpdate.HSCode) ? insertOrUpdate.HSCode : stored.HSCode;
                    // ignore update if new value is NULL/empty
                    stored.ChineseDescription = !string.IsNullOrEmpty(insertOrUpdate.ChineseDescription) ? insertOrUpdate.ChineseDescription : stored.ChineseDescription;
                    stored.Audit(userName);
                }
                else
                {
                    var newData = Mapper.Map<OrganizationPreferenceModel>(insertOrUpdate);
                    newData.OrganizationId = organizationId;

                    // empty string/ null -> NULL
                    newData.HSCode = !string.IsNullOrEmpty(newData.HSCode) ? newData.HSCode : null;
                    newData.ChineseDescription = !string.IsNullOrEmpty(newData.ChineseDescription) ? newData.ChineseDescription : null;

                    newData.Audit(userName);
                    newDataList.Add(newData);
                }
            }
            await _organizationPreferenceRepository.AddRangeAsync(newDataList.ToArray());
            await UnitOfWork.SaveChangesAsync();
        }

    }
}
