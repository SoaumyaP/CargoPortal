using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Groove.SP.Infrastructure.CSFE.Configs;
using Groove.SP.Infrastructure.CSFE.Models;
using Microsoft.Extensions.Options;
using IdentityModel.Client;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace Groove.SP.Infrastructure.CSFE
{
    public class CSFEApiClient : ICSFEApiClient
    {
        #region Fields
        private readonly CSFEApiSettings _csfeApiSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenRequest _tokenRequest;
        private readonly TokenResponse _tokenResponse;
        #endregion

        #region ctors
        public CSFEApiClient(IOptions<CSFEApiSettings> csfeApiSettings, IHttpClientFactory httpClientFactory)
        {
            _csfeApiSettings = csfeApiSettings.Value;
            _httpClientFactory = httpClientFactory;
            _tokenRequest = new TokenRequest
            {
                Address = _csfeApiSettings.TokenEndpoint,
                ClientId = _csfeApiSettings.ClientId,
                ClientSecret = _csfeApiSettings.ClientSecret,
                GrantType = "client_credentials",

            };
            _tokenResponse = httpClientFactory.CreateClient().RequestTokenAsync(_tokenRequest).Result;
        }
        #endregion

        #region Methods

        private async Task<HttpResponseMessage> HttpRequest(HttpMethod method, string url, string accessToken, string jsonContent = "")
        {
            var client = _httpClientFactory.CreateClient();

            HttpRequestMessage request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (!string.IsNullOrWhiteSpace(jsonContent) && method == HttpMethod.Post)
            {
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(request);
            }
            catch (HttpRequestException exp)
            {
                throw new ServiceException("An error occurred sending the request.", exp);
            }

            if (!response.IsSuccessStatusCode)
            {
                if (response.Content?.Headers.ContentType?.MediaType == "application/json")
                {
                    string rawResponseBody = await response.Content.ReadAsStringAsync();

                    throw new ServiceException("Unexpected exception returned from the service.",
                        response.Headers, response.StatusCode, rawResponseBody);
                }
                else
                {
                    throw new ServiceException("Unexpected exception returned from the service.",
                        response.Headers, response.StatusCode);
                }
            }

            return response;
        }

        public async Task<IEnumerable<Organization>> GetOrganizationsByCodesAsync(IEnumerable<string> codes)
        {
            var url = _csfeApiSettings.APIEndpoint + "/organizations/code";
            var response = await HttpRequest(HttpMethod.Post, url, _tokenResponse.AccessToken, JsonConvert.SerializeObject(codes));

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Organization>>(jsonResponse);

            return result;
        }

        public async Task<Organization> GetOrganizationsByCodeAsync(string code)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/organizations/getByCode/{code}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Organization>(jsonResponse);

            return result;
        }

        public async Task<Organization> GetOrganizationByNameAsync(string name)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/organizations/getByName/{name}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Organization>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Organization>> GetOrganizationsByEdisonCompanyIdCodesAsync(params string[] codes)
        {
            var url = _csfeApiSettings.APIEndpoint + "/organizations/edisonCompanyCodeIds";
            var response = await HttpRequest(HttpMethod.Post, url, _tokenResponse.AccessToken, JsonConvert.SerializeObject(codes));

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Organization>>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Organization>> GetActiveOrganizationsAsync()
        {
            var url = _csfeApiSettings.APIEndpoint + "/organizations/activecodes";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Organization>>(jsonResponse);

            return result;
        }

        public async Task<Organization> GetOrganizationByIdAsync(long id)
        {
            var url = _csfeApiSettings.APIEndpoint + "/organizations/" + id;
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Organization>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Organization>> GetOrganizationByIdsAsync(IEnumerable<long> ids)
        {
            var url = _csfeApiSettings.APIEndpoint + "/organizations/OrganizationIds";
            var response = await HttpRequest(HttpMethod.Post, url, _tokenResponse.AccessToken, JsonConvert.SerializeObject(ids));

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Organization>>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Organization>> GetWarehouseProviderByOrgIdAsync(long id)
        {
            var url = _csfeApiSettings.APIEndpoint + "/organizations/" + id + "/warehouseProviders";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Organization>>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<long>> GetAffiliateIdsAsync(long organizationId)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/organizations/{organizationId}/affiliatecodes";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<long>>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<OrganizationRole>> GetAllOrganizationRolesAsync()
        {
            var url = _csfeApiSettings.APIEndpoint + "/organizationroles";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<OrganizationRole>>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Port>> GetAllPortsAsync()
        {
            var url = _csfeApiSettings.APIEndpoint + "/ports";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Port>>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
        {
            var url = _csfeApiSettings.APIEndpoint + "/currencies";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Currency>>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Country>> GetAllCountriesAsync()
        {
            var url = _csfeApiSettings.APIEndpoint + "/countries";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Country>>(jsonResponse);

            return result;
        }

        public async Task<Location> GetLocationByIdAsync(long id)
        {
            var url = _csfeApiSettings.APIEndpoint + "/locations/" + id;
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Location>(jsonResponse);

            return result;
        }

        public async Task<Location> GetLocationByCodeAsync(string code)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/locations/code:{code}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Location>(jsonResponse);

            return result;
        }
        public async Task<Location> GetLocationByDescriptionAsync(string description)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/locations/description:{description}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Location>(jsonResponse);

            return result;
        }

        public async Task<Location> GetLocationByDescriptionAsync(string description, long countryId)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/locations/byLocationName?locationName={description}&countryId={countryId}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Location>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Location>> GetLocationsByCodesAsync(IEnumerable<string> codes)
        {
            var url = _csfeApiSettings.APIEndpoint + "/locations/code";
            var response = await HttpRequest(HttpMethod.Post, url, _tokenResponse.AccessToken, JsonConvert.SerializeObject(codes));

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Location>>(jsonResponse);

            return result;
        }

        public async Task<Country> GetCountryByCodeAsync(string code)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/countries/code:{code}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Country>(jsonResponse);

            return result;
        }


        protected IEnumerable<Location> locations = null;
        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            if (locations != null)
            {
                return locations;
            }

            var url = _csfeApiSettings.APIEndpoint + "/locations";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Location>>(jsonResponse);

            if (locations == null)
            {
                locations = result;
            }

            return result;
        }

        public async Task<IEnumerable<CountryLocations>> GetAllCountryLocationsAsync()
        {
            var url = _csfeApiSettings.APIEndpoint + "/countries/AllCountryLocations";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<CountryLocations>>(jsonResponse);

            return result;
        }

        public async Task<Carrier> GetCarrierByCodeAsync(string code)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/carriers/code?code={code}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Carrier>(jsonResponse);

            return result;
        }

        public async Task<Carrier> GetCarrierByIdAsync(long id)
        {
            if (id == 0)
            {
                return await Task.FromResult<Carrier>(null);
            }

            var url = _csfeApiSettings.APIEndpoint + $"/carriers/{id}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Carrier>(jsonResponse);

            return result;
        }


        protected IEnumerable<Carrier> carriers = null;
        public async Task<IEnumerable<Carrier>> GetAllCarriesAsync()
        {
            if (carriers != null)
            {
                return carriers;
            }

            var url = _csfeApiSettings.APIEndpoint + "/carriers";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Carrier>>(jsonResponse);

            if (carriers == null)
            {
                carriers = result;
            }
            return result;
        }

        public async Task<IEnumerable<AlternativeLocation>> GetAllAlternativeLocationsAsync()
        {
            var url = _csfeApiSettings.APIEndpoint + "/alternativelocations";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<AlternativeLocation>>(jsonResponse);

            return result;
        }

        public async Task<Event> GetEventByCodeAsync(string code)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/eventCodes?code={code}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<Event>(jsonResponse);

            return result;
        }

        public async Task<IEnumerable<Event>> GetEventByCodesAsync(IEnumerable<string> codes)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/eventCodes/listByCodes?codes={string.Join(",", codes)}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Event>>(jsonResponse);

            return result;
        }

        public async Task<bool> CheckValidReportPrincipalAsync(long customerId, long roleId, long requestingOrganizationId)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/organizations/{customerId}/CheckValidReportPrincipal?roleId={roleId}&requestingOrganizationId={requestingOrganizationId}";

            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<CheckValidReportPrincipalResult>(jsonResponse);

            return result.IsValidReportPrincipal;
        }

        public async Task<IEnumerable<Location>> GetLocationsAsync(IEnumerable<long> countryIds)
        {
            var url = _csfeApiSettings.APIEndpoint + "/locations";

            if (countryIds != null && countryIds.Any())
            {
                url += $"?countryIds={string.Join(",", countryIds)}";
            }

            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<IEnumerable<Location>>(jsonResponse);
            return result;
        }

        protected IEnumerable<Vessel> allVessels = null;
        public async Task<IEnumerable<Vessel>> GetAllVesselsAsync()
        {
            if (allVessels != null)
            {
                return allVessels;
            }

            var url = _csfeApiSettings.APIEndpoint + "/vessels";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Vessel>>(jsonResponse);

            if (allVessels == null)
            {
                allVessels = result;
            }
            return result;
        }

        protected IEnumerable<Vessel> realActiveVessels = null;
        public async Task<IEnumerable<Vessel>> GetRealActiveVesselsAsync()
        {
            if (realActiveVessels != null)
            {
                return realActiveVessels;
            }

            var url = _csfeApiSettings.APIEndpoint + "/vessels?filterType=realActive";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Vessel>>(jsonResponse);

            if (realActiveVessels == null)
            {
                realActiveVessels = result;
            }
            return result;
        }

        protected IEnumerable<Vessel> activeVessels = null;
        public async Task<IEnumerable<Vessel>> GetActiveVesselsAsync()
        {
            if (activeVessels != null)
            {
                return activeVessels;
            }

            var url = _csfeApiSettings.APIEndpoint + "/vessels/internal?filterType=active";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);

            string jsonResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<IEnumerable<Vessel>>(jsonResponse);

            if (activeVessels == null)
            {
                activeVessels = result;
            }
            return result;
        }

        public async Task<bool> CheckAgentDomainAsync(string emailDomain)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/organizations/agents?domain={emailDomain}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<bool>(jsonResponse);
            return result;
        }

        public async Task<IEnumerable<WarehouseAssignment>> GetWarehouseAssignmentsByOrgIdAsync(long organizationId)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/organizations/{organizationId}/WarehouseAssignments";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<IEnumerable<WarehouseAssignment>>(jsonResponse);
            return result;
        }

        public async Task<EmailNotification> GetEmailNotificationAsync(long organizationId, long customerId, long locationId)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/EmailNotifications?organizationId={organizationId}&customerId={customerId}&locationId={locationId}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<EmailNotification>(jsonResponse);
            return result;
        }

        public async Task<List<EmailNotification>> GetEmailNotificationsAsync(long organizationId, long customerId, long locationId)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/EmailNotifications/list?organizationId={organizationId}&customerId={customerId}&locationId={locationId}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<EmailNotification>>(jsonResponse);
            return result;
        }

        public async Task<int> BulkInsertOrganizationsAsync(IEnumerable<BulkInsertOrganization> toImportOrganizations)
        {
            var url = _csfeApiSettings.APIEndpoint + "/Organizations/Bulk";
            var bodyContent = JsonConvert.SerializeObject(toImportOrganizations);
            var response = await HttpRequest(HttpMethod.Post, url, _tokenResponse.AccessToken, bodyContent);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            if (int.TryParse(jsonResponse, out int insertedCount))
            {
                return insertedCount;
            }
            else
            {
                return 0;
            }
        }

        public async Task<IEnumerable<CustomerRelationshipModel>> GetRelationshipAsync(string ids, string orgType)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/CustomerRelationships/getByOrgType/{orgType}?idList={ids}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<IEnumerable<CustomerRelationshipModel>>(jsonResponse);
            return result;
        }


        public async Task<IEnumerable<Organization>> GetAffiliatesByOrgIdsAsync(string orgIds)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/Organizations/AffiliatesByOrgIds?orgIds={orgIds}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<IEnumerable<Organization>>(jsonResponse);
            return result;
        }


        public async Task<UserOffice> GetUserOfficeByLocationAsync(string location, long countryId)
        {
            var url = _csfeApiSettings.APIEndpoint + $"/UserOffices/byLocationName?location={location}&countryId={countryId}";
            var response = await HttpRequest(HttpMethod.Get, url, _tokenResponse.AccessToken);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserOffice>(jsonResponse);
            return result;
        }
        #endregion
    }
}
