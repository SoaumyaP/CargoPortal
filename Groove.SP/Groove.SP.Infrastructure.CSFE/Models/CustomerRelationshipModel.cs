using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.CSFE.Models
{
    public class CustomerRelationshipModel 
    {
        public long? SupplierId { get; set; }

        public long? CustomerId { get; set; }
    }
}
