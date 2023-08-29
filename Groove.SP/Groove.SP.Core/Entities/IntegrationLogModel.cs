using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class IntegrationLogModel : Entity
    {
        public long Id { get; set; }
        public string Profile { get; set; }
        public string APIName { get; set; }
        public string APIMessage { get; set; }
        public string EDIMessageType { get; set; }
        public string EDIMessageRef { get; set; }
        public DateTime PostingDate { get; set; }
        public IntegrationStatus Status { get; set; }
        public string Remark { get; set; }
        public string Response { get; set; }
    }
}
