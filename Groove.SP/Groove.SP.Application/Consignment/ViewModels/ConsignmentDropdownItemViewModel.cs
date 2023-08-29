using Groove.SP.Core.Models;

namespace Groove.SP.Application.Consignment.ViewModels
{
    public class ConsignmentDropdownItemViewModel : DropDownListItem<long>
    {
        public long ExecutionAgentId { get; set; }
    }
}
