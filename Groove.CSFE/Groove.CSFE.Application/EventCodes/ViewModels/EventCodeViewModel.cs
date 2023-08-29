using Groove.CSFE.Application.Common;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using System;

namespace Groove.CSFE.Application.EventCodes.ViewModels
{
    public class EventCodeViewModel : ViewModelBase<EventCodeModel>
    {
        public string ActivityCode { get; set; }

        public string ActivityType { get; set; }

        public string ActivityTypeDescription { get; set; }

        public int ActivityTypeLevel { get; set; }

        public string ActivityTypeLevelDescription { get; set; }

        public string ActivityDescription { get; set; }

        public bool LocationRequired { get; set; }

        public bool RemarkRequired { get; set; }

        public EventCodeStatus Status { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }
}
