using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Groove.CSFE.Persistence.Configurations
{
    public class EventTypeConfiguration : IEntityTypeConfiguration<EventTypeModel>
    {
        public void Configure(EntityTypeBuilder<EventTypeModel> builder)
        {
            builder.HasKey(t => t.Code);
            builder.Property(e => e.Code).HasColumnType("NVARCHAR(10)").HasMaxLength(10);
            builder.Property(e => e.Description).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.LevelDescription).HasColumnType("NVARCHAR(100)").HasMaxLength(100).IsRequired();

            var createdDate = new DateTime(2019, 01, 01);
            builder.HasData(
                new EventTypeModel { Code = "PM", Description = "Purchase Order Milestone Event", EventLevel = 1, LevelDescription = EventTypeLevelDescription.PurchaseOrder, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "PE", Description = "Purchase Order Exception Event", EventLevel = 1, LevelDescription = EventTypeLevelDescription.PurchaseOrder, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "PA", Description = "Purchase Order Activity Event", EventLevel = 1, LevelDescription = EventTypeLevelDescription.PurchaseOrder, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventTypeModel { Code = "FM", Description = "Booking Milestone Event", EventLevel = 2, LevelDescription = EventTypeLevelDescription.POFulfillment, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "FA", Description = "Booking Activity Event", EventLevel = 2, LevelDescription = EventTypeLevelDescription.POFulfillment, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "FE", Description = "Booking Exception Event", EventLevel = 2, LevelDescription = EventTypeLevelDescription.POFulfillment, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventTypeModel { Code = "OM", Description = "Shipping Order Milestone Event", EventLevel = 3, LevelDescription = EventTypeLevelDescription.ShippingOrder, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "OE", Description = "Shipping Order Exception Event", EventLevel = 3, LevelDescription = EventTypeLevelDescription.ShippingOrder, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "OA", Description = "Shipping Order Activity Event", EventLevel = 3, LevelDescription = EventTypeLevelDescription.ShippingOrder, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventTypeModel { Code = "SE", Description = "Shipment Exception Event", EventLevel = 4, LevelDescription = EventTypeLevelDescription.Shipment, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "SM", Description = "Shipment Milestone Event", EventLevel = 4, LevelDescription = EventTypeLevelDescription.Shipment, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "SA", Description = "Shipment Activity Event", EventLevel = 4, LevelDescription = EventTypeLevelDescription.Shipment, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventTypeModel { Code = "CA", Description = "Container Activity Event", EventLevel = 5, LevelDescription = EventTypeLevelDescription.Container, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "CM", Description = "Container Milestone Event", EventLevel = 5, LevelDescription = EventTypeLevelDescription.Container, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "CE", Description = "Container Exception Event", EventLevel = 5, LevelDescription = EventTypeLevelDescription.Container, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventTypeModel { Code = "MM", Description = "Master Milestone Event", EventLevel = 6, LevelDescription = EventTypeLevelDescription.MasterBL, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "ME", Description = "Master Exception Event", EventLevel = 6, LevelDescription = EventTypeLevelDescription.MasterBL, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "MA", Description = "Master Activity Event", EventLevel = 6, LevelDescription = EventTypeLevelDescription.MasterBL, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventTypeModel { Code = "VM", Description = "Vessel Milestone Event", EventLevel = 7, LevelDescription = EventTypeLevelDescription.Vessel, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "VE", Description = "Vessel Exception Event", EventLevel = 7, LevelDescription = EventTypeLevelDescription.Vessel, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "VA", Description = "Vessel Activity Event", EventLevel = 7, LevelDescription = EventTypeLevelDescription.Vessel, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventTypeModel { Code = "EP", Description = "End of Purchase Order", EventLevel = 1, LevelDescription = EventTypeLevelDescription.End, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "ES", Description = "End of Shipment", EventLevel = 4, LevelDescription = EventTypeLevelDescription.End, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventTypeModel { Code = "EE", Description = "End with Exception", EventLevel = 4, LevelDescription = EventTypeLevelDescription.End, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventTypeModel { Code = "CSM", Description = "Cruise Shipment Milestone", EventLevel = 4, LevelDescription = EventTypeLevelDescription.CruiseOrder, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate }
            );
        }
    }
}