using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;


namespace Groove.CSFE.Application.Organizations.ViewModels
{
    public class SupplierViewModel : ViewModelBase<OrganizationModel>
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string ContactEmail { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string WebsiteDomain { get; set; }

        public long LocationId { get; set; }

        public bool IsApplyAffiliates { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
