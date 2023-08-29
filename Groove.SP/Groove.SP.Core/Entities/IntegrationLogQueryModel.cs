using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Core.Entities
{
    public class IntegrationLogQueryModel
    {
        public long Id { get; set; }
        public string Profile { get; set; }
        public string APIName { get; set; }
        public string EDIMessageType { get; set; }
        public string EDIMessageRef { get; set; }
        public DateTime PostingDate { get; set; }
        public IntegrationStatus Status { get; set; }
        public string StatusName => Status == IntegrationStatus.Succeed ? "label.succeed" : "label.failed";
        public string Remark { get; set; }
    }
}
