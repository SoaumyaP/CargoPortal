using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Xml.Serialization;

namespace Groove.SP.Application.RoutingOrderContact.ViewModels
{
    public class ImportRoutingOrderContactViewModel : ViewModelBase<RoutingOrderContactModel>
    {
        [XmlElement("OrganizationCode")]
        public string OrganizationCode { get; set; }

        [XmlElement("OrganizationRole")]
        public string OrganizationRole { get; set; }

        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }

        [XmlElement("AddressLine1")]
        public string AddressLine1 { get; set; }

        [XmlElement("AddressLine2")] 
        public string AddressLine2 { get; set; }

        [XmlElement("AddressLine3")]
        public string AddressLine3 { get; set; }

        [XmlElement("AddressLine4")]
        public string AddressLine4 { get; set; }

        [XmlElement("ContactName")]
        public string ContactName { get; set; }

        [XmlElement("ContactNumber")]
        public string ContactNumber { get; set; }

        [XmlElement("ContactEmail")]
        public string ContactEmail { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
