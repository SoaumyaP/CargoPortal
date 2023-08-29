using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.CSFE.Models
{
    public class WarehouseLocation
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string ContactEmail{ get; set; }

        public long OrganizationId { get; set; }

        public Location Location { get; set; }

        public string CombineAddressLine()
        {
            var addressLine = "";
            if (!string.IsNullOrEmpty(AddressLine1))
            {
                addressLine = addressLine + AddressLine1;
            }

            if (!string.IsNullOrEmpty(AddressLine2))
            {
                addressLine = addressLine.Length > 0 ? addressLine + $", {AddressLine2}" : AddressLine2;
            }

            if (!string.IsNullOrEmpty(AddressLine3))
            {
                addressLine = addressLine.Length > 0 ? addressLine + $", {AddressLine3}" : AddressLine3;
            }

            if (!string.IsNullOrEmpty(AddressLine4))
            {
                addressLine = addressLine.Length > 0 ? addressLine + $", {AddressLine4}" : AddressLine4;
            }
            return addressLine;
        }
    }
}
