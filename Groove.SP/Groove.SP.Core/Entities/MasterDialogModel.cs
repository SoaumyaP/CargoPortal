using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class MasterDialogModel : Entity
    {
        public long Id { set; get; }

        public string DisplayOn { get; set; }

        public string FilterCriteria { get; set; }

        public string FilterValue { get; set; }

        public string Message { get; set; }

        public string Category { get; set; }

        public string SelectedItems { get; set; }

        public string Owner { get; set; }

        public long? OrganizationId { get; set; }

        public virtual ICollection<GlobalIdMasterDialogModel> GlobalIdMasterDialogs { get; set; }
    }
}