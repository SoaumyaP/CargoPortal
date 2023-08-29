using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BuyerComplianceModelConfiguration : IEntityTypeConfiguration<BuyerComplianceModel>
    {
        public void Configure(EntityTypeBuilder<BuyerComplianceModel> builder)
        {

            builder.Property(b => b.AllowToBookIn).HasDefaultValue(POType.Allocated);

            builder.Property(b => b.ServiceType).HasDefaultValue(BuyerComplianceServiceType.Freight);

            builder.HasOne(b => b.PurchaseOrderVerificationSetting)
                .WithOne(p => p.BuyerCompliance)
                .HasForeignKey<PurchaseOrderVerificationSettingModel>(p => p.BuyerComplianceId);

            builder.HasOne(b => b.ProductVerificationSetting)
                .WithOne(p => p.BuyerCompliance)
                .HasForeignKey<ProductVerificationSettingModel>(p => p.BuyerComplianceId);

            builder.HasOne(b => b.BookingTimeless)
                .WithOne(bt => bt.BuyerCompliance)
                .HasForeignKey<BookingTimelessModel>(bt => bt.BuyerComplianceId);

            builder.HasOne(b => b.ShippingCompliance)
                .WithOne(s => s.BuyerCompliance)
                .HasForeignKey<ShippingComplianceModel>(s => s.BuyerComplianceId);

            builder.HasOne(b => b.ComplianceSelection)
                .WithOne(c => c.BuyerCompliance)
                .HasForeignKey<ComplianceSelectionModel>(c => c.BuyerComplianceId);

            builder.Property(e => e.ProgressNotifyDay).HasDefaultValue(0);
            builder.Property(e => e.AgentAssignmentMethod).HasDefaultValue(AgentAssignmentMethodType.BySystem);
            builder.Property(e => e.IsEmailNotificationToSupplier).HasDefaultValue(false);
            builder.Property(e => e.EmailNotificationTime).HasMaxLength(30);
            builder.Property(e => e.IsAllowMissingPO).HasDefaultValue(true);
        }
    }
}