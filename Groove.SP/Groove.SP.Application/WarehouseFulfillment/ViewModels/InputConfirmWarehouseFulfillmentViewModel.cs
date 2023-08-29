using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class InputConfirmWarehouseFulfillmentViewModel
    {
        public long Id { set; get; }
        public DateTime ConfirmedHubArrivalDate { get; set; }
        public string Time { set; get; }
        public string LoadingBay { set; get; }
    }
}
