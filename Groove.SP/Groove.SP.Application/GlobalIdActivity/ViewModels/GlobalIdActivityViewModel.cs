using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;

namespace Groove.SP.Application.GlobalIdActivity.ViewModels
{
    public class GlobalIdActivityViewModel : ViewModelBase<GlobalIdActivityModel>
    {
        public long Id { get; set; }

        public string GlobalId { get; set; }

        public long ActivityId { get; set; }

        public GlobalIdModel ReferenceEntity { get; set; }

        public ActivityViewModel Activity { get; set; }

        public string Location { get; set; }

        public string Remark { get; set; }

        public string POFulfillmentNos { get; set; }

        public string ShipmentNos { get; set; }

        public string ContainerNos { get; set; }

        public string VesselFlight { get; set; }

        public DateTime ActivityDate { get; set; }

        public string Actor { get; set; }


        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
