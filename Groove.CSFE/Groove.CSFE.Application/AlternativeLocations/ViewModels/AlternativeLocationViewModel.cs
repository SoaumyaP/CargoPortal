using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;


namespace Groove.CSFE.Application.AlternativeLocations.ViewModels
{
    public class AlternativeLocationViewModel : ViewModelBase<AlternativeLocationModel>
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long LocationId { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
