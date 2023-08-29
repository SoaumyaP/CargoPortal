using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Core.Entities
{
    public class OrganizationPreferenceModel : Entity
    {
        public long Id { set; get; }
        public long OrganizationId { get; set; }
        public string ProductCode { get; set; }
        public string HSCode { get; set; }
        public string ChineseDescription { get; set; }
    }
}
