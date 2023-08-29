using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;
using System;

namespace Groove.CSFE.Application.WarehouseLocations.ViewModels
{
    public class WarehouseLocationViewModel : ViewModelBase<WarehouseLocationModel>
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Address = concatenate values from database (address line 1 -> 4)
        /// </summary>
        public string Address { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }

        public string ContactPerson { get; set; }

        public string ContactPhone { get; set; }

        public string ContactEmail { get; set; }

        public long LocationId { get; set; }

        public long OrganizationId { get; set; }

        public string WorkingHours { get; set; }

        public string Remarks { get; set; }

        public virtual LocationModel Location { get; set; }

        public virtual OrganizationModel Organization { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            
        }
    }
}