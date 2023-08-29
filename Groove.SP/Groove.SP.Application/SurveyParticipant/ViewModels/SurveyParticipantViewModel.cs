using Groove.SP.Application.Common;
using Groove.SP.Application.Survey.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.SurveyParticipant.ViewModels
{
    public class SurveyParticipantViewModel : ViewModelBase<SurveyParticipantModel>
    {
        public string Username { get; set; }
        public long SurveyId { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedOn { get; set; }

        public SurveyViewModel Survey { get; set; }


        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
