using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Persistence.Configurations
{
    public class WarehouseAssignmentModelConfiguration : IEntityTypeConfiguration<WarehouseAssignmentModel>
    {
        public void Configure(EntityTypeBuilder<WarehouseAssignmentModel> builder)
        {
            builder.HasKey(e => new { e.OrganizationId, e.WarehouseLocationId });
            builder.Property(e => e.ContactPerson).HasColumnType("nvarchar(256)");
            builder.Property(e => e.ContactPhone).HasColumnType("nvarchar(32)");
            builder.Property(e => e.ContactEmail).HasColumnType("nvarchar(128)");
        }
    }
}