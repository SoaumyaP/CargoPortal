
using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;

namespace Groove.CSFE.Application.EmailNotification.ViewModel
{
    public class EmailNotificationViewModel: ViewModelBase<EmailNotificationModel>
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public long CustomerId { get; set; }
        public long? CountryId { get; set; }
        public IList<string> PortSelectionIds { get; set; }
        public string Email { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            
        }
    }
}
