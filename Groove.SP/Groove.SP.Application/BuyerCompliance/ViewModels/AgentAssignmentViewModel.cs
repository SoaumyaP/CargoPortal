using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using FluentValidation;
using Groove.SP.Application.BuyerCompliance.Validations;
using System.Collections.Generic;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class AgentAssignmentViewModel : ViewModelBase<AgentAssignmentModel>
    {
        public int? AutoCreateShipment { get; set; }

        public AgentType AgentType { get; set; }

        public long? CountryId { get; set; }

        public IList<string> PortSelectionIds { get; set; }

        public long AgentOrganizationId { get; set; }

        public int Order { get; set; }

        public string ModeOfTransport { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new AgentAssignmentValidator().ValidateAndThrow(this);
        }
    }
}
