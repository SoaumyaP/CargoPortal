using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class SurveyModel : Entity
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
        public DateTime? PublishedDate { get; set; }
        public DateTime? ClosedDate { get; set; }

        public ICollection<SurveyQuestionModel> Questions { get; set; }
        public ICollection<SurveyParticipantModel> Participants { get; set; }
    }
}