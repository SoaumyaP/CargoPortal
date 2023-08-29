using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Groove.SP.Core.Entities
{
    public class CustomerRelationshipQueryModel
    {
        public long SupplierId { get; set; }

        public long CustomerId { get; set; }

        public string CustomerRefId { get; set; }
    }
}
