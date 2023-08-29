using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillmentShortshipOrder.ViewModels
{
    public class POFulfillmentShortshipOrderViewModel : ViewModelBase<POFulfillmentShortshipOrderModel>
    {
        public bool IsRead { set; get; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
