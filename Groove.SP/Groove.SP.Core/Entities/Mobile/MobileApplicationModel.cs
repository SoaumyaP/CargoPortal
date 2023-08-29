
using System;

namespace Groove.SP.Core.Entities.Mobile
{
    public class MobileApplicationModel: Entity
    {
        public long Id { get; set; }

        public string Version { get; set; }

        public DateTime PublishedDate { get; set; }

        public bool IsDiscontinued { get; set; }

        public string PackageName { get; set; }

        public string PackageUrl { get; set; }

        public string Description { get; set; }

    }
}
