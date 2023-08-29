using Groove.Infrastructure.APIClientCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    public class AuthenticationCollectionRequest : CollectionRequest
    {
        public AuthenticationCollectionRequest(string requestUrl,
            BaseClient client,
            IDictionary<string, string> headers) : base(requestUrl, client, headers)
        {
        }

        public Task<AuthenticationResult> PostAsync(UserAccount account)
        {
            this.ContentType = "application/json";
            this.Method = "POST";
            return this.SendAsync<AuthenticationResult>(account);
        }
        /// <summary>
        /// It is to send silent request.
        /// In case there is exception, it will be wrapped into response with some information
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public Task<AuthenticationResult> PostSilentAsync(UserAccount account)
        {
            this.ContentType = "application/json";
            this.Method = "POST";
            return this.SendSilentAsync<AuthenticationResult>(account);
        }
    }
}
