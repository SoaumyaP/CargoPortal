using System;

namespace Groove.SP.Core.Entities
{
    public class UserAuditLogQueryModel
    {
        public long Id { get; set; }
        public string OperatingSystem { get; set; }
        public string Browser { get; set; }
        public string ScreenSize { get; set; }
        public string Feature { get; set; }
        public DateTime AccessDateTime { get; set; }
    }
}
