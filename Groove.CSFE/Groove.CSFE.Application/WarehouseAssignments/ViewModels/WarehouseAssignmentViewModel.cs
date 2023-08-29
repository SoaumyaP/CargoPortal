using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;
using System;

namespace Groove.CSFE.Application.WarehouseAssignments.ViewModels
{
    public class WarehouseAssignmentViewModel : ViewModelBase<WarehouseAssignmentModel>
    {
        public long WarehouseLocationId { get; set; }
        public long OrganizationId { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public WarehouseLocationModel WarehouseLocation { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }
}
