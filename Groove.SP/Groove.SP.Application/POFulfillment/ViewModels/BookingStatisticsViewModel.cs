using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class BookingStatisticsViewModel
    {
        public int TotalAwaitingForSubmission { get; set; }
        public int TotalPendingForApproval { get; set; }
    }
}
