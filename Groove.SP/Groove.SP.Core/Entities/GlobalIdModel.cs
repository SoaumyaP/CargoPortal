using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Core.Entities
{
    public class GlobalIdModel : Entity
    {
        public string Id { get; set; }

        public string EntityType { get; set; }

        public long EntityId { get; set; }

        public ICollection<GlobalIdActivityModel> GlobalIdActivities { get; set; }

        public ICollection<GlobalIdAttachmentModel> GlobalIdAttachments { get; set; }

        public ICollection<GlobalIdApprovalModel> GlobalIdApprovals { get; set; }

        public ICollection<GlobalIdMasterDialogModel> GlobalIdMasterDialogs { get; set; }
    }
}
