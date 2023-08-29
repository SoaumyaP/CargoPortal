using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Groove.SP.Persistence.Configurations
{
    public class ActivityModelConfiguration : IEntityTypeConfiguration<ActivityModel>
    {
        public void Configure(EntityTypeBuilder<ActivityModel> builder)
        {
            builder.Property(e => e.ActivityCode).IsRequired().HasColumnType("NVARCHAR(10)").HasMaxLength(10);
            builder.Property(e => e.ActivityType).IsRequired().HasColumnType("NVARCHAR(10)").HasMaxLength(10);
            builder.Property(e => e.ActivityDescription).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ActivityDate).IsRequired();
            builder.Property(e => e.Location).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.Remark).HasColumnType("NVARCHAR(MAX)");
            builder.Property(e => e.Resolution).HasColumnType("NVARCHAR(MAX)");

            var createdDate = new DateTime(2019, 01, 01); 
            builder.HasData
            (
                new ActivityModel() { Id = 1, ActivityCode = "1007", ActivityType = "PA", ActivityDescription = "Forwarder Booking Request", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 2, ActivityCode = "1008", ActivityType = "PM", ActivityDescription = "Forwarder Booking Confirmed", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 3, ActivityCode = "1009", ActivityType = "PM", ActivityDescription = "Shipment Dispatch", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                             
                new ActivityModel() { Id = 4, ActivityCode = "1051", ActivityType = "FA", ActivityDescription = "Forwarder Booking Request", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 5, ActivityCode = "1052", ActivityType = "FA", ActivityDescription = "Forwarder Booking Itinerary Ready", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 6, ActivityCode = "1053", ActivityType = "FA", ActivityDescription = "Forwarder Booking Accepted", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 7, ActivityCode = "1054", ActivityType = "FA", ActivityDescription = "Forwarder Booking Denied", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 8, ActivityCode = "1055", ActivityType = "FE", ActivityDescription = "Forwarder Booking Approval Request", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 9, ActivityCode = "1056", ActivityType = "FA", ActivityDescription = "Forwarder Booking Amendment", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 10, ActivityCode = "1057", ActivityType = "FE", ActivityDescription = "Forwarder Booking Cancellation", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 11, ActivityCode = "1058", ActivityType = "FA", ActivityDescription = "Booking Approval Accepted", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 12, ActivityCode = "1059", ActivityType = "FA", ActivityDescription = "Booking Approval Denied", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 13, ActivityCode = "1060", ActivityType = "FA", ActivityDescription = "Booking Approval Overdue", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                                    
                new ActivityModel() { Id = 14, ActivityCode = "1061", ActivityType = "FM", ActivityDescription = "Forwarder Booking Confirmation", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 15, ActivityCode = "1062", ActivityType = "FA", ActivityDescription = "Plan to ship", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 16, ActivityCode = "1063", ActivityType = "FA", ActivityDescription = "Packing List Ready", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 17, ActivityCode = "1065", ActivityType = "FA", ActivityDescription = "PO Invoice Ready", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 18, ActivityCode = "1067", ActivityType = "FA", ActivityDescription = "Ready to ship", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate },
                new ActivityModel() { Id = 19, ActivityCode = "1068", ActivityType = "FM", ActivityDescription = "Goods Dispatch", CreatedBy = AppConstant.SYSTEM_USERNAME, ActivityDate = createdDate, CreatedDate = createdDate }
            );
        }
    }
}
