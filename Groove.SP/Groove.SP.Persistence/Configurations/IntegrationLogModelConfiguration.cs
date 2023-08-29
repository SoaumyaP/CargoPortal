using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class IntegrationLogModelConfiguration : IEntityTypeConfiguration<IntegrationLogModel>
    {
        public void Configure(EntityTypeBuilder<IntegrationLogModel> builder)
        {
            builder.HasIndex(e => new { e.PostingDate, e.Id });
        }
    }
}
