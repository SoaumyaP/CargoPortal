using Groove.CSFE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.EventCodes.ViewModels
{
    public class CreateEventCodeViewModel
    {
        public string ActivityCode { get; set; }
        public string ActivityTypeCode { get; set; }
        public string ActivityDescription { get; set; }
        public bool LocationRequired { get; set; }
        public bool RemarkRequired { get; set; }
        public long SortSequence { get; set; }
        public EventOrderType EventOrderType { get; set; }
        public string BeforeEvent { get; set; }
        public string AfterEvent { get; set; }
    }
}
