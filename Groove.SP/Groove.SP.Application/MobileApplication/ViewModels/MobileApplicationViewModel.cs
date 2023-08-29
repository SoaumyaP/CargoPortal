using Groove.SP.Application.Common;
using Groove.SP.Core.Entities.Mobile;
using System;

namespace Groove.SP.Application.MobileApplication.ViewModels
{
    public class MobileApplicationViewModel : ViewModelBase<MobileApplicationModel>
    {
        public string Version { get; set; }

        public DateTime PublishedDate { get; set; }

        public bool IsDiscontinued { get; set; }

        public string PackageName { get; set; }

        public string PackageUrl { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
