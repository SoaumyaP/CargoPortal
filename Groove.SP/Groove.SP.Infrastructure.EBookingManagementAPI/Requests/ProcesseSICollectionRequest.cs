using Groove.Infrastructure.APIClientCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.EBookingManagementAPI.Requests
{
    public class ProcesseSICollectionRequest : CollectionRequest
    {
        public ProcesseSICollectionRequest(string requestUrl,
            BaseClient client,
            IDictionary<string, string> headers) : base(requestUrl, client, headers)
        {
        }

        public Task<ResponseResult> PostAsync(eSI.Booking booking)
        {
            this.ContentType = "application/json";
            this.Method = "POST";
            return this.SendAsync<ResponseResult>(booking);
        }

        /// <summary>
        /// It is to send silent request.
        /// In case there is exception, it will be wrapped into response with some information
        /// </summary>
        /// <param name="booking"></param>
        /// <returns></returns>
        public Task<ResponseResult> PostSilentAsync(eSI.Booking booking)
        {
            this.ContentType = "application/json";
            this.Method = "POST";
            return this.SendSilentAsync<ResponseResult>(booking);
        }
    }
}
