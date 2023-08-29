using System;

using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class BookingPolicyModelConfiguration : IEntityTypeConfiguration<BookingPolicyModel>
    {
        public void Configure(EntityTypeBuilder<BookingPolicyModel> builder)
        {
            builder.Property(e => e.Name).IsRequired();
            builder.Property(e => e.Action).IsRequired();
        }
    }
}
