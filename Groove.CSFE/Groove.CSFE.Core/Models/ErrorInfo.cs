using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Core.Models
{
    public class ErrorInfo
    {
        public string Errors { get; set; }

        public string Title { get; set; }

        public int Status { get; set; }

        public string Message { get; set; }
    }
}
