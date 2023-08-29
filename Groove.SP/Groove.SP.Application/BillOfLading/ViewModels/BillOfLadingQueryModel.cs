using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Groove.SP.Application.BillOfLading.ViewModels
{
    public class BillOfLadingQueryModel
    {
        public long Id { set; get; }
        public string HouseBLNo { set; get; }
        public long AttachmentId { set; get; }
        public string AttachmentFileName { set; get; }
        public string ShipmentNo { set; get; }
        public string ShipmentInfo { set; get; }
        public string MasterBLNo { set; get; }
        public long MasterBLId { set; get; }
        public DateTime? IssueDate { set; get; }
        public string ShipFrom { set; get; }
        public string ShipTo { set; get; }
        public string Customer { set; get; }

        [NotMapped]
        public IEnumerable<Tuple<long, string>> ShipmentInfos
        {
            get
            {
                var listShipmentInfos = new List<Tuple<long, string>>();
                if (!string.IsNullOrWhiteSpace(ShipmentInfo))
                {
                    var shipmentNos = ShipmentInfo.Split(";");
                    foreach (var item in shipmentNos)
                    {
                        var shipmentInfos = item.Split("~");
                        listShipmentInfos.Add(new Tuple<long, string>(long.Parse(shipmentInfos[1]), shipmentInfos[0]));
                    }
                }
                return listShipmentInfos;
            }
        }
    }
}
