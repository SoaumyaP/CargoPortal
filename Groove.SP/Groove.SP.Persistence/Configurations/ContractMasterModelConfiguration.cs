using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ContractMasterModelConfiguration : IEntityTypeConfiguration<ContractMasterModel>
    {
        public void Configure(EntityTypeBuilder<ContractMasterModel> builder)
        {
            builder.Property(e => e.CarrierContractNo).IsRequired().HasColumnType("VARCHAR(50)").HasMaxLength(50);
            builder.Property(e => e.CarrierCode).IsRequired().HasColumnType("NVARCHAR(128)").HasMaxLength(128).HasDefaultValue(string.Empty);
            builder.Property(e => e.RealContractNo).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.AccountName).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ContractType).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ColoaderCode).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ContractHolder).IsRequired().HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ValidFrom).IsRequired().HasColumnType("DATETIME2(7)");
            builder.Property(e => e.ValidTo).IsRequired().HasColumnType("DATETIME2(7)");
            builder.Property(e => e.Status).IsRequired();
            builder.Property(e => e.IsVIP).IsRequired();
            builder.Property(e => e.Remarks).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.ParentContract).HasColumnType("NVARCHAR(512)").HasMaxLength(512);
            builder.Property(e => e.CustomerContractType).HasColumnType("NVARCHAR(512)").HasMaxLength(512);

            builder.HasIndex(x => x.CarrierContractNo).IncludeProperties("Id", "ContractType").IsUnique();

            builder.ToTable("ContractMaster", "dbo");
        }
    }
}
