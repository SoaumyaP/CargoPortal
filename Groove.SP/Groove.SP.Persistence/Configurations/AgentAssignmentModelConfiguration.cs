using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Persistence.Configurations
{
    public class AgentAssignmentModelConfiguration : IEntityTypeConfiguration<AgentAssignmentModel>
    {
        public void Configure(EntityTypeBuilder<AgentAssignmentModel> builder)
        {
            builder.Property(e => e.ModeOfTransport).HasMaxLength(50).IsRequired().HasDefaultValue(ModeOfTransport.Sea);
        }
    }
}
