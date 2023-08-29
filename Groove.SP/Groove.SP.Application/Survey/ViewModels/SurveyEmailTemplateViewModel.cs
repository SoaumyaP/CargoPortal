using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Survey.ViewModels
{
    public class SurveyEmailTemplateViewModel
    {
        public long SurveyId { get; set; }
        public string SurveyName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
    }
}