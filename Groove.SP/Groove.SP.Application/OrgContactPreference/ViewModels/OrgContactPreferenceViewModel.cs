using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.OrgContactPreference.ViewModels
{
    public class OrgContactPreferenceViewModel : ViewModelBase<OrgContactPreferenceModel>
    {
        public long Id { set; get; }
        public long OrganizationId { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }
        public string WeChatOrWhatsApp { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}