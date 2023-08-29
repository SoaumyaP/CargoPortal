using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Xml.Serialization;

namespace Groove.SP.Application.RoutingOrderInvoice.ViewModels
{
    public class ImportRoutingOrderInvoiceViewModel : ViewModelBase<RoutingOrderInvoiceModel>
    {
        [XmlElement("InvoiceType")]
        public string InvoiceType { get; set; }

        [XmlElement("InvoiceNumber")]
        public string InvoiceNumber { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
