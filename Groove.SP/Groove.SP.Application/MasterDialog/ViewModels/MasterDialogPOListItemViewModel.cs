using System.Collections.Generic;

namespace Groove.SP.Application.MasterDialog.ViewModels
{
    public class MasterDialogPOListItemViewModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string ParentId { get; set; }
        public string DialogItemNumber { get; set; }
        public IEnumerable<MasterDialogPOListItemViewModel> ChildrenItems { get; set; }

        public bool IsChecked { get; set; }
        public bool IsDisabled { get; set; }


        /// <summary>
        /// Total number of records
        /// </summary>
        public long RecordCount { get; set; }

    }
}
