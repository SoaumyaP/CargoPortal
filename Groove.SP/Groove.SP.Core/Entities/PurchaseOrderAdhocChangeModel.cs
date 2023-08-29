using System.Collections.Generic;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class PurchaseOrderAdhocChangeModel: Entity
    {
        public long Id { get; set; }
        public long POFulfillmentId { get; set; }
        public long PurchaseOrderId { get; set; }

        /// <summary>
        /// Ignore from mapping to database because of big size
        /// Value will be set via spu_ProceedPurchaseOrderAdhocChanges
        /// </summary>
        public string JsonCurrentData { get; set; }

        /// <summary>
        /// Ignore from mapping to database because of big size
        /// Value will be set via spu_ProceedPurchaseOrderAdhocChanges
        /// </summary>
        public string JsonNewData { get; set; }
        public PurchaseOrderAdhocChangePriority Priority { get; set; }
        public string Message { get; set; }

        public virtual POFulfillmentModel POFulfillment { get; set; }

    }
}