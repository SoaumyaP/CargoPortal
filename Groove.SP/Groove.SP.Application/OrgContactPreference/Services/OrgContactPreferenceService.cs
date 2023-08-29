using Groove.SP.Application.BulkFulfillment.Mappers;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.OrgContactPreference.Services.Interfaces;
using Groove.SP.Application.OrgContactPreference.ViewModels;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.OrgContactPreference.Services
{
    public class OrgContactPreferenceService : ServiceBase<OrgContactPreferenceModel, OrgContactPreferenceViewModel>, IOrgContactPreferenceService
    {
        private readonly IOrgContactPreferenceRepository orgContactPreferenceRepository;
        public OrgContactPreferenceService(IUnitOfWorkProvider unitOfWorkProvider,
            IOrgContactPreferenceRepository orgContactPreferenceRepository) : base(unitOfWorkProvider)
        {
            this.orgContactPreferenceRepository = orgContactPreferenceRepository;
        }

        public async Task<IEnumerable<OrgContactPreferenceViewModel>> GetAsNoTrackingAsync(long organizationId)
        {
            var orgContactPreferences = await orgContactPreferenceRepository.QueryAsNoTracking(x => x.OrganizationId == organizationId).ToListAsync();

            return Mapper.Map<IEnumerable<OrgContactPreferenceViewModel>>(orgContactPreferences);
        }

        public async Task InsertOrUpdateRangeAsync(IEnumerable<OrgContactPreferenceViewModel> viewModels, long organizationId, string userName)
        {
            // early return if data not valid
            if (viewModels == null || !viewModels.Any())
            {
                return;
            }
            var filteredData = viewModels
                    .Where(x => !string.IsNullOrEmpty(x.CompanyName))
                    .ToList();
            if (filteredData == null || !viewModels.Any())
            {
                return;
            }

            // get list of unique product code
            var companyNames = filteredData.Select(c => c.CompanyName).Distinct();

            // filter data to get final
            var insertOrUpdateData = new List<OrgContactPreferenceViewModel>();

            foreach (var companyName in companyNames)
            {
                insertOrUpdateData.Add(filteredData.Last(x => x.CompanyName == companyName));
            }

            // store organization preference if either HS code / Chinese description available
            //insertOrUpdateData = insertOrUpdateData.Where(x => !string.IsNullOrEmpty(x.con) || !string.IsNullOrEmpty(x.ChineseDescription)).ToList();

            // load data from database
            var storedData = orgContactPreferenceRepository.Query(x => x.OrganizationId == organizationId && companyNames.Contains(x.CompanyName));

            var newDataList = new List<OrgContactPreferenceModel>();
            foreach (var insertOrUpdate in insertOrUpdateData)
            {
                var stored = storedData.FirstOrDefault(x => x.CompanyName == insertOrUpdate.CompanyName);

                var addressLine1 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(insertOrUpdate.Address, 1);
                var addressLine2 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(insertOrUpdate.Address, 2);
                var addressLine3 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(insertOrUpdate.Address, 3);
                var addressLine4 = SplitCompanyAddressLinesResolver.SplitCompanyAddressLines(insertOrUpdate.Address, 4);
                if (stored != null)
                {
                    // ignore update if new value is NULL/empty
                    stored.Address = !string.IsNullOrEmpty(addressLine1) ? addressLine1 : stored.Address;
                    stored.AddressLine2 = !string.IsNullOrEmpty(addressLine2) ? addressLine2 : stored.AddressLine2;
                    stored.AddressLine3 = !string.IsNullOrEmpty(addressLine3) ? addressLine3 : stored.AddressLine3;
                    stored.AddressLine4 = !string.IsNullOrEmpty(addressLine4) ? addressLine4 : stored.AddressLine4;
                    stored.ContactName = !string.IsNullOrEmpty(insertOrUpdate.ContactName) ? insertOrUpdate.ContactName : stored.ContactName;
                    stored.ContactNumber = !string.IsNullOrEmpty(insertOrUpdate.ContactNumber) ? insertOrUpdate.ContactNumber : stored.ContactNumber;
                    stored.ContactEmail = !string.IsNullOrEmpty(insertOrUpdate.ContactEmail) ? insertOrUpdate.ContactEmail : stored.ContactEmail;
                    stored.WeChatOrWhatsApp = !string.IsNullOrEmpty(insertOrUpdate.WeChatOrWhatsApp) ? insertOrUpdate.WeChatOrWhatsApp : stored.WeChatOrWhatsApp;
                    stored.Audit(userName);
                }
                else
                {
                    var newData = Mapper.Map<OrgContactPreferenceModel>(insertOrUpdate);
                    newData.OrganizationId = organizationId;

                    // empty string/ null -> NULL
                    newData.Address = !string.IsNullOrEmpty(addressLine1) ? addressLine1 : null;
                    newData.AddressLine2 = !string.IsNullOrEmpty(addressLine2) ? addressLine2 : null;
                    newData.AddressLine3 = !string.IsNullOrEmpty(addressLine3) ? addressLine3 : null;
                    newData.AddressLine4 = !string.IsNullOrEmpty(addressLine4) ? addressLine4 : null;
                    newData.ContactNumber = !string.IsNullOrEmpty(newData.ContactNumber) ? newData.ContactNumber : null;
                    newData.WeChatOrWhatsApp = !string.IsNullOrEmpty(newData.WeChatOrWhatsApp) ? newData.WeChatOrWhatsApp : null;
                    newData.Audit(userName);
                    newDataList.Add(newData);
                }
            }
            await orgContactPreferenceRepository.AddRangeAsync(newDataList.ToArray());
            await UnitOfWork.SaveChangesAsync();
        }
    }
}
