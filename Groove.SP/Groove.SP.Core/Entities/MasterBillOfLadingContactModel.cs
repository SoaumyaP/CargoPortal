﻿namespace Groove.SP.Core.Entities
{
    public class MasterBillOfLadingContactModel : Entity
    {
        public long Id { get; set; }

        public long MasterBillOfLadingId { get; set; }

        public long OrganizationId { get; set; }

        public string OrganizationRole { get; set; }

        public string CompanyName { get; set; }

        public string Address { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string ContactEmail { get; set; }



        public virtual MasterBillOfLadingModel MasterBillOfLading { get; set; }
    }
}
