using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Core.Entities;
using Newtonsoft.Json;
using System;

namespace Groove.SP.Application.Users.ViewModels
{
    public class UserAuditLogViewModel : ViewModelBase<UserAuditLogModel>
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string OperatingSystem { get; set; }
        public string Browser { get; set; }
        public string ScreenSize { get; set; }
        public string UserAgent { get; set; }
        public string Feature { get; set; }
        [JsonConverter(typeof(UtcDateTimeConverter))]
        public DateTime AccessDateTime { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            return;
        }
    }
}
