using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.OrganizationPreference.ViewModels
{
    public class OrganizationPreferenceViewModel : ViewModelBase<OrganizationPreferenceModel>
    {
        public long Id { set; get; }
        public long OrganizationId { get; set; }
        public string ProductCode { get; set; }
        public string HSCode { get; set; }
        public string ChineseDescription { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
