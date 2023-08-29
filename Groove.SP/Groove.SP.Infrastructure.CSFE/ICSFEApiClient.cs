using Groove.SP.Infrastructure.CSFE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.CSFE
{
    public interface ICSFEApiClient
    {
        Task<IEnumerable<Organization>> GetOrganizationsByCodesAsync(IEnumerable<string> codes);
        Task<Organization> GetOrganizationsByCodeAsync(string code);
        Task<Organization> GetOrganizationByNameAsync(string name);
        Task<IEnumerable<Organization>> GetOrganizationsByEdisonCompanyIdCodesAsync(params string[] codes);
        Task<IEnumerable<Organization>> GetActiveOrganizationsAsync();
        Task<IEnumerable<OrganizationRole>> GetAllOrganizationRolesAsync();
        Task<Organization> GetOrganizationByIdAsync(long id);
        Task<IEnumerable<Organization>> GetOrganizationByIdsAsync(IEnumerable<long> ids);
        Task<IEnumerable<Organization>> GetWarehouseProviderByOrgIdAsync(long id);
        Task<IEnumerable<long>> GetAffiliateIdsAsync(long organizationId);
        Task<IEnumerable<Port>> GetAllPortsAsync();
        Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
        Task<IEnumerable<Country>> GetAllCountriesAsync();
        Task<Location> GetLocationByIdAsync(long id);
        Task<Location> GetLocationByCodeAsync(string code);
        Task<Location> GetLocationByDescriptionAsync(string desctiption);
        Task<Location> GetLocationByDescriptionAsync(string desctiption, long countryId);
        Task<IEnumerable<Location>> GetLocationsByCodesAsync(IEnumerable<string> codes);
        Task<Country> GetCountryByCodeAsync(string code);
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<IEnumerable<Carrier>> GetAllCarriesAsync();
        Task<IEnumerable<CountryLocations>> GetAllCountryLocationsAsync();
        Task<IEnumerable<AlternativeLocation>> GetAllAlternativeLocationsAsync();
        Task<Event> GetEventByCodeAsync(string code);
        Task<IEnumerable<Event>> GetEventByCodesAsync(IEnumerable<string> codes);
        Task<Carrier> GetCarrierByCodeAsync(string code);
        Task<Carrier> GetCarrierByIdAsync(long id);
        Task<bool> CheckValidReportPrincipalAsync(long customerId, long roleId, long requestingOrganizationId);
        Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<long> countryIds);
        Task<IEnumerable<Vessel>> GetAllVesselsAsync();
        Task<IEnumerable<Vessel>> GetRealActiveVesselsAsync();
        Task<IEnumerable<Vessel>> GetActiveVesselsAsync();
        Task<bool> CheckAgentDomainAsync(string emailDomain);
        Task<IEnumerable<WarehouseAssignment>> GetWarehouseAssignmentsByOrgIdAsync(long organizationId);
        Task<EmailNotification> GetEmailNotificationAsync(long organizationId, long customerId, long locationId);
        Task<List<EmailNotification>> GetEmailNotificationsAsync(long organizationId, long customerId, long locationId);

        Task<int> BulkInsertOrganizationsAsync(IEnumerable<BulkInsertOrganization> toImportOrganizations);
        Task<IEnumerable<CustomerRelationshipModel>> GetRelationshipAsync(string ids, string orgType);
        Task<IEnumerable<Organization>> GetAffiliatesByOrgIdsAsync(string orgIds);

        Task<UserOffice> GetUserOfficeByLocationAsync(string location, long countryId);
    }
}
