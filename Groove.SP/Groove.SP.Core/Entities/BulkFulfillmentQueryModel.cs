using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Core.Entities
{
    public class BulkFulfillmentQueryModel
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public DateTime? BookingDate { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public string ShipFromName { get; set; }

        public string ShipToName { get; set; }
    }
}
