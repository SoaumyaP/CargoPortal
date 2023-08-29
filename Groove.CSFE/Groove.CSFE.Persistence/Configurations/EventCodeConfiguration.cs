using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Groove.SP.Persistence.Configurations
{
    public class EventCodeConfiguration : IEntityTypeConfiguration<EventCodeModel>
    {
        public void Configure(EntityTypeBuilder<EventCodeModel> builder)
        {
            builder.HasKey(t => t.ActivityCode);
            builder.Property(e => e.ActivityCode).IsRequired().HasColumnType("NVARCHAR(10)").HasMaxLength(10);
            builder.Property(e => e.ActivityTypeCode).HasColumnType("NVARCHAR(10)").HasMaxLength(10);
            builder.Property(e => e.ActivityDescription).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.SortSequence).IsRequired();
            builder.Property(e => e.Status).HasDefaultValue(EventCodeStatus.Active);


            builder.HasOne(ec => ec.ActivityType )
                .WithMany(et => et.EventCodes)
                .HasForeignKey(ec => ec.ActivityTypeCode);

            var createdDate = new DateTime(2021, 9, 14);
            builder.HasData
            (
                new EventCodeModel() { ActivityCode = "1000", ActivityTypeCode = "PM", ActivityDescription = "PO Issued", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1001", ActivityTypeCode = "PE", ActivityDescription = "PO on hold", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1002", ActivityTypeCode = "PA", ActivityDescription = "PO Acknowledged", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1003", ActivityTypeCode = "PA", ActivityDescription = "PO Amendment", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1004", ActivityTypeCode = "PE", ActivityDescription = "PO Amendment Failure", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1005", ActivityTypeCode = "PE", ActivityDescription = "PO cancelled", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1006", ActivityTypeCode = "PA", ActivityDescription = "PO Cancellation Failure", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1010", ActivityTypeCode = "PM", ActivityDescription = "PO Closed", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1011", ActivityTypeCode = "PA", ActivityDescription = "PO Reopen", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1013", ActivityTypeCode = "PA", ActivityDescription = "Progress Check", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventCodeModel() { ActivityCode = "1050", ActivityTypeCode = "FM", ActivityDescription = "Booking Planning", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1051", ActivityTypeCode = "FA", ActivityDescription = "Booked", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1052", ActivityTypeCode = "FA", ActivityDescription = "Booking Itinerary Ready", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1053", ActivityTypeCode = "FA", ActivityDescription = "Booking Accepted", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1054", ActivityTypeCode = "FA", ActivityDescription = "Booking Denied", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1055", ActivityTypeCode = "FE", ActivityDescription = "Booking Approval Request", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1056", ActivityTypeCode = "FA", ActivityDescription = "Booking Amended", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1057", ActivityTypeCode = "FE", ActivityDescription = "Booking Cancelled", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1058", ActivityTypeCode = "FA", ActivityDescription = "Booking Approved", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1059", ActivityTypeCode = "FA", ActivityDescription = "Booking Rejected", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1060", ActivityTypeCode = "FA", ActivityDescription = "Booking Approval Overdue", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1061", ActivityTypeCode = "FM", ActivityDescription = "PO Booking confirmed", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1062", ActivityTypeCode = "FA", ActivityDescription = "Plan to ship", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1063", ActivityTypeCode = "FM", ActivityDescription = "Cargo Received", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1069", ActivityTypeCode = "FE", ActivityDescription = "Shipment Amendment", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1070", ActivityTypeCode = "FE", ActivityDescription = "Shipment Cancellation", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1071", ActivityTypeCode = "FM", ActivityDescription = "Booking Closed", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1072", ActivityTypeCode = "FE", ActivityDescription = "Reopen Booking", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "1073", ActivityTypeCode = "FM", ActivityDescription = "Booking Completed", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventCodeModel() { ActivityCode = "1074", ActivityTypeCode = "SA", ActivityDescription = "SO Released Date", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2000", ActivityTypeCode = "SA", ActivityDescription = "Booking received", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2001", ActivityTypeCode = "SE", ActivityDescription = "Booking on hold", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2005", ActivityTypeCode = "SM", ActivityDescription = "Booking confirmed", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2006", ActivityTypeCode = "SA", ActivityDescription = "Container(s) released to shipper", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2009", ActivityTypeCode = "SA", ActivityDescription = "Partial handover in origin CFS", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2011", ActivityTypeCode = "SE", ActivityDescription = "Non-conformance cargo found", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2012", ActivityTypeCode = "SA", ActivityDescription = "Pulled for inspection", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2014", ActivityTypeCode = "SM", ActivityDescription = "Cargo handover at origin", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2015", ActivityTypeCode = "SA", ActivityDescription = "Shipment booked with carrier", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2016", ActivityTypeCode = "SA", ActivityDescription = "Shipment rejected by carrier", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2017", ActivityTypeCode = "SA", ActivityDescription = "Shipment re-book with alternative carrier/schedule", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2021", ActivityTypeCode = "SE", ActivityDescription = "Shipment pending for shipper document", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2026", ActivityTypeCode = "SE", ActivityDescription = "Shipment export customs hold", LocationRequired = true, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2027", ActivityTypeCode = "SA", ActivityDescription = "Shipment export cleared", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2028", ActivityTypeCode = "SE", ActivityDescription = "Shipment late departure alert", LocationRequired = true, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2034", ActivityTypeCode = "SA", ActivityDescription = "Bill of Lading released to Shipper", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2040", ActivityTypeCode = "SA", ActivityDescription = "Shipment import customs hold", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2041", ActivityTypeCode = "SA", ActivityDescription = "Shipment import customs cleared", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2043", ActivityTypeCode = "SA", ActivityDescription = "BOL surrendered received at destination", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2053", ActivityTypeCode = "SA", ActivityDescription = "Appointment made with consignee", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2054", ActivityTypeCode = "SM", ActivityDescription = "Shipment handover to consignee", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "2055", ActivityTypeCode = "SA", ActivityDescription = "Payment received", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate, SortSequence = 641 },

                new EventCodeModel() { ActivityCode = "3000", ActivityTypeCode = "CA", ActivityDescription = "Container - Empty Pickup", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3001", ActivityTypeCode = "CM", ActivityDescription = "Container - Gate In", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3002", ActivityTypeCode = "CM", ActivityDescription = "Container - Vessel Load", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3005", ActivityTypeCode = "CM", ActivityDescription = "Container - Vessel Unload", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3006", ActivityTypeCode = "CM", ActivityDescription = "Container - Gate Out", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3007", ActivityTypeCode = "CA", ActivityDescription = "Container - Empty Return", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3016", ActivityTypeCode = "CA", ActivityDescription = "Customs Hold (PA)", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3017", ActivityTypeCode = "CA", ActivityDescription = "Customs Released (CT)", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3018", ActivityTypeCode = "CA", ActivityDescription = "Carrier Hold (X9)", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3019", ActivityTypeCode = "CA", ActivityDescription = "Carrier Release (CR)", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3031", ActivityTypeCode = "CA", ActivityDescription = "Free Demurrage Ends", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3032", ActivityTypeCode = "CA", ActivityDescription = "Free Detention Ends", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3033", ActivityTypeCode = "CA", ActivityDescription = "Haulage to Offdock CY", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3034", ActivityTypeCode = "CA", ActivityDescription = "Receipt of Containers at Offdock CY", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3050", ActivityTypeCode = "CA", ActivityDescription = "CFS Closing Date", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3051", ActivityTypeCode = "CA", ActivityDescription = "CY Closing Date", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3052", ActivityTypeCode = "CA", ActivityDescription = "Container roll over", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate, SortSequence = 541 },
                new EventCodeModel() { ActivityCode = "3098", ActivityTypeCode = "CE", ActivityDescription = "Demurrage Alert", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "3099", ActivityTypeCode = "CE", ActivityDescription = "Detention Alert", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventCodeModel() { ActivityCode = "4000", ActivityTypeCode  = "ES", ActivityDescription = "Shipment Completed", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "4001", ActivityTypeCode  = "EE", ActivityDescription = "Shipment Abandoned", LocationRequired = false, RemarkRequired = true, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventCodeModel() { ActivityCode = "5000", ActivityTypeCode = "CSM", ActivityDescription = "PICK- Pick up arranged for loose cargo", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5001", ActivityTypeCode = "CSM", ActivityDescription = "ON HAND- Goods received in hub", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5002", ActivityTypeCode = "CSM", ActivityDescription = "ATS- Authorized To Ship", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5003", ActivityTypeCode = "CSM", ActivityDescription = "VC- Booked For Vendor", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5004", ActivityTypeCode = "CSM", ActivityDescription = "OG- Delivered Origin", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5005", ActivityTypeCode = "CSM", ActivityDescription = "COB- Confirm On Board", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5006", ActivityTypeCode = "CSM", ActivityDescription = "SHI- Shipped", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5007", ActivityTypeCode = "CSM", ActivityDescription = "TRN- In Transit", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5008", ActivityTypeCode = "CSM", ActivityDescription = "PT- Transhipment", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5009", ActivityTypeCode = "CSM", ActivityDescription = "AR- Arrived At Destination", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5010", ActivityTypeCode = "CSM", ActivityDescription = "DL- Delivered To Shipyard", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5011", ActivityTypeCode = "CSM", ActivityDescription = "SSD- Container To Ship", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5012", ActivityTypeCode = "CSM", ActivityDescription = "RE- Container Empty", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "5013", ActivityTypeCode = "CSM", ActivityDescription = "IG- Empty Container Returned", LocationRequired = false, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },

                new EventCodeModel() { ActivityCode = "7001", ActivityTypeCode = "VA", ActivityDescription = "Vessel Departure", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "7002", ActivityTypeCode = "VA", ActivityDescription = "Vessel Arrival", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "7003", ActivityTypeCode = "VA", ActivityDescription = "Flight Departure", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new EventCodeModel() { ActivityCode = "7004", ActivityTypeCode = "VA", ActivityDescription = "Flight Arrival", LocationRequired = true, RemarkRequired = false, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate }
            );
        }
    }
}
