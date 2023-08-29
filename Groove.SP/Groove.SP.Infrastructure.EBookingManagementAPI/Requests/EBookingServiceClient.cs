using System.Collections.Generic;
using System.Net.Http;

using Groove.Infrastructure.APIClientCore;
using Groove.SP.Infrastructure.EBookingManagementAPI.Requests;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    public class EBookingServiceClient : BaseClient
    {
        public string CurrSessionID { get; set; }

        public EBookingServiceClient(string baseUrl,
            IHttpClientFactory httpClientFactory) 
            : base (baseUrl, httpClientFactory)
        {
        }

        public AuthenticationCollectionRequest Authentication
        {
            get
            {
                IDictionary<string, string> headers = new Dictionary<string, string>
                {
                    { "User-Agent", "API" }
                };

                return new AuthenticationCollectionRequest(this.BaseUrl + "/AccountAuth.csfe",
                    this, headers);
            }
        }

        public BookingsCollectionRequest Bookings
        {
            get
            {
                IDictionary<string, string> headers = new Dictionary<string, string>
                {
                    { "currSessionID", CurrSessionID }
                };

                return new BookingsCollectionRequest(this.BaseUrl + "/API/ProcessBooking.csfe", 
                    this, headers);
            }
        }

        public ProcesseSICollectionRequest ProcesseSI
        {
            get
            {
                IDictionary<string, string> headers = new Dictionary<string, string>
                {
                    { "currSessionID", CurrSessionID }
                };

                return new ProcesseSICollectionRequest(this.BaseUrl + "/API/ProcesseSIBasic.csfe",
                    this, headers);
            }
        }
    }
}
