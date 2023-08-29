using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Terminals.ViewModels
{
    public class TerminalViewModel : ViewModelBase<TerminalModel>
    {
        public long Id { get; set; }
        public long LocationId { get; set; }
        public string TerminalCode { get; set; }
        public string TerminalName { get; set; }
        public string Address { get; set; }

        public virtual LocationModel Location { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
