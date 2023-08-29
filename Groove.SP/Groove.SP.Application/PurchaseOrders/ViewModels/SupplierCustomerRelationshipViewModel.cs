using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class SupplierCustomerRelationshipViewModel
    {
        public long SupplierId { get; set; }

        public long CustomerId { get; set; }

        public static IEnumerable<SupplierCustomerRelationshipViewModel> Parse(string dataString)
        {
            if(string.IsNullOrEmpty(dataString))
            {
                return null;
            }

            var result = new List<SupplierCustomerRelationshipViewModel>();
            var items = dataString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
            {
                var values = item.Split(',', StringSplitOptions.RemoveEmptyEntries);

                var newItem = new SupplierCustomerRelationshipViewModel
                {
                    SupplierId = long.Parse(values[0]),
                    CustomerId = long.Parse(values[1])
                };
                result.Add(newItem);
            }
            return result;
        }
    }
}
