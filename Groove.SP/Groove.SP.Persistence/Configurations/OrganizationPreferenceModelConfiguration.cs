using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Persistence.Configurations
{
    public class OrganizationPreferenceModelConfiguration : IEntityTypeConfiguration<OrganizationPreferenceModel>
    {
        public void Configure(EntityTypeBuilder<OrganizationPreferenceModel> builder)
        {
            builder.HasIndex(e => new { e.ProductCode, e.OrganizationId }).IsUnique();

            builder.Property(e => e.HSCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128);
            builder.Property(e => e.ProductCode).HasColumnType("NVARCHAR(128)").HasMaxLength(128).IsRequired();
        }
    }
}
