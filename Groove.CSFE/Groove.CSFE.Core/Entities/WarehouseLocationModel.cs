using System.Collections.Generic;

namespace Groove.CSFE.Core.Entities
{
    public class WarehouseLocationModel : Entity
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string ContactPerson { get; set; }

        public string ContactPhone { get; set; }

        public string ContactEmail { get; set; }

        public long LocationId { get; set; }

        /// <summary>
        /// Remark: Provider Organization Id
        /// </summary>
        public long OrganizationId { get; set; }

        public string WorkingHours { get; set; }

        public string Remarks { get; set; }

        public virtual LocationModel Location { get; set; }

        /// <summary>
        /// Remark: Provider Organization.
        /// </summary>
        public virtual OrganizationModel Organization { get; set; }

        public virtual ICollection<WarehouseAssignmentModel> WarehouseAssignments { get; set; }
    }
}
