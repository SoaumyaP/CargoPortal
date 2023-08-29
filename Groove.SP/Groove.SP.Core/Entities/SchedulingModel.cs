using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class SchedulingModel : Entity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public SchedulingStatus Status { get; set; }

        /// <summary>
        /// To link Report Id on CS Portal
        /// </summary>
        public long CSPortalReportId { get; set; }

        /// <summary>
        /// To link to Scheduled Task Id on Telerik
        /// </summary>
        public string TelerikSchedulingId { get; set; }

        public long CreatedOrganizationId { get; set; }

        /// <summary>
        /// Ftp server config info to which the scheduling document will be uploaded once it is executed
        /// </summary>
        public long? FtpServerId { get; set; }

        public FtpServerModel FtpServer { get; set; }
    }
}