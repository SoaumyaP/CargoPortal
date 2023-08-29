using Groove.SP.Infrastructure.CSFE.Models;
using System.Collections.Generic;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class WarehouseBookingEmailViewModel
    {
        public string CustomerName { get; set; }
        public WarehouseAssignment WarehouseAssignment { get; set; }
        public string FailureReasons { get; set; }
    }
}
