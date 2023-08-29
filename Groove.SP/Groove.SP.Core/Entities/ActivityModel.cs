using Groove.SP.Core.Events;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ActivityModel : Entity
    {
        #region ctors
        public ActivityModel()
        {
            GlobalIdActivities = new List<GlobalIdActivityModel>();
        }

        public ActivityModel(
            string code,
            string type,
            string description,
            string location, 
            DateTime activityDate,
            string createdBy,
            string remark = null,
            string resolution = null,
            DateTime? resolutionDate = null,
            bool? resolved = null,
            long? shipmentId = null, 
            long? containerId = null, 
            long? consignmentId = null, 
            long? purchaseOrderId = null,
            long? poFulfillmentId = null, 
            long? cruiseOrderId = null, 
            long? freightSchedulerId = null,
            string metaData = "") : this()
        {
            ActivityCode = code;
            ActivityType = type;
            ActivityDescription = description;
            ActivityDate = activityDate;
            Location = location;
            Remark = remark;
            CreatedDate = DateTime.UtcNow;
            CreatedBy = createdBy;
            Resolution = resolution;
            ResolutionDate = resolutionDate;
            Resolved = resolved;

            if (shipmentId.HasValue)
            {
                AddGlobalActivity(remark, location, activityDate, shipmentId.Value, EntityType.Shipment);
            }

            if (containerId.HasValue)
            {
                AddGlobalActivity(remark, location, activityDate, containerId.Value, EntityType.Container);
            }

            if (consignmentId.HasValue)
            {
                AddGlobalActivity(remark, location, activityDate, consignmentId.Value, EntityType.Consignment);
            }

            if (purchaseOrderId.HasValue)
            {
                AddGlobalActivity(remark, location, activityDate, purchaseOrderId.Value, EntityType.CustomerPO);
            }

            if (poFulfillmentId.HasValue)
            {
                AddGlobalActivity(remark, location, activityDate, poFulfillmentId.Value, EntityType.POFullfillment);
            }

            if (cruiseOrderId.HasValue)
            {
                AddGlobalActivity(remark, location, activityDate, cruiseOrderId.Value, EntityType.CruiseOrder);
            }

            if (freightSchedulerId.HasValue)
            {
                AddGlobalActivity(remark, location, activityDate, freightSchedulerId.Value, EntityType.FreightScheduler);
            }

            AddDomainEvent(new ActivityCreatedDomainEvent(this, containerId, shipmentId, freightSchedulerId, metaData));
        }

        #endregion

        #region Properties

        public long Id { get; set; }

        public string ActivityCode { get; set; }

        public string ActivityType { get; set; }

        public string ActivityDescription { get; set; }

        public DateTime ActivityDate { get; set; }

        public string Location { get; set; }

        public string Remark { get; set; }

        public bool? Resolved { get; set; }

        public string Resolution { get; set; }

        public DateTime? ResolutionDate { get; set; }

        public virtual ICollection<GlobalIdActivityModel> GlobalIdActivities { get; set; }

        #endregion

        #region Methods
        private void AddGlobalActivity(string remark, string location, DateTime activityDate, long entityId, string entityType)
        {
            var globalId = entityType + "_" + entityId;
            var globalActivity = new GlobalIdActivityModel
            {
                GlobalId = globalId,
                ActivityId = Id,
                CreatedDate = CreatedDate,
                CreatedBy = CreatedBy,
                UpdatedDate = UpdatedDate,
                UpdatedBy = UpdatedBy,
                Remark = remark,
                Location = location,
                ActivityDate = activityDate
            };
            GlobalIdActivities.Add(globalActivity);
        }
        #endregion
    }
}
