using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Core.Entities
{
    public class SurveyQueryModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Participants { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? PublishedDate { get; set; }
        public SurveyStatus Status { get; set; }
        public string StatusName { get; set; }
    }
}
