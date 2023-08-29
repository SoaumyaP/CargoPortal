using System;

using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ContractTypeModelConfiguration : IEntityTypeConfiguration<ContractTypeModel>
    {
        public void Configure(EntityTypeBuilder<ContractTypeModel> builder)
        {
            builder.Property(e => e.Name).HasColumnType("NVARCHAR(512)");
        }
    }
}
