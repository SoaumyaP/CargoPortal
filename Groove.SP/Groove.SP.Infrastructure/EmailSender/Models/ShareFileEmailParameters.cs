using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Infrastructure.EmailSender.Models
{
    public class ShareFileEmailParameters
    {
        public string CustomerName { get; set; }
        public string ShareFileExpiredTime { get; set; }
        public string ShareFileLink { get; set; }
        public string FileName { get; set; }
        public string SupportEmail { get; set; }
    }
}
