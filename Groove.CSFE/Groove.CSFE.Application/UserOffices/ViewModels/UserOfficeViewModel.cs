using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.UserOffices.ViewModels
{
    public class UserOfficeViewModel : ViewModelBase<UserOfficeModel>
    {
        public long Id { get; set; }
        public long LocationId { set; get; }
        public string CorpMarketingContactName { set; get; }
        public string CorpMarketingContactEmail { set; get; }
        public string OPManagementContactName { set; get; }
        public string OPManagementContactEmail { set; get; }

        public virtual LocationModel Location { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
