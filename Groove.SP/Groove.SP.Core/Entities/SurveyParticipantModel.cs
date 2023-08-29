using System;

namespace Groove.SP.Core.Entities
{
    public class SurveyParticipantModel : Entity
    {
        public string Username { get; set; }
        public long SurveyId { get; set; }
        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public SurveyModel Survey { get; set; }
    }
}