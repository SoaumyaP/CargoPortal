using Groove.SP.Application.Common;
using Groove.SP.Application.SurveyQuestion.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Survey.ViewModels
{
    public class SurveyViewModel : ViewModelBase<SurveyModel>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SurveyParticipantType ParticipantType { get; set; }
        public Role? UserRole { get; set; }
        public OrganizationType? OrganizationType { get; set; }
        public string SpecifiedOrganization { get; set; }
        public string SpecifiedUser { get; set; }
        public SurveySendToUserType? SendToUser { get; set; }
        public bool IsIncludeAffiliate { get; set; }
        public SurveyStatus Status { get; set; }
        public string StatusName => EnumHelper<SurveyStatus>.GetDisplayName(Status);
        public DateTime? PublishedDate { get; set; }
        public DateTime? ClosedDate { get; set; }

        public IEnumerable<SurveyQuestionViewModel> Questions { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
