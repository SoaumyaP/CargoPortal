using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Application.EventTypes.ViewModels
{
    public class EventTypeViewModel : ViewModelBase<EventTypeModel>
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }
}
