using Hangfire.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.ApplicationBackgroundJob.ViewModels
{
    public class AppScheduledJobViewModel
    {
        public Job Job { get; set; }

        public TimeSpan Delay { get; set; }        

    }
}
