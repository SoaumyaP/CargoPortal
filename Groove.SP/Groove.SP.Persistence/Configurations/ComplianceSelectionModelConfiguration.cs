using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ComplianceSelectionModelConfiguration : IEntityTypeConfiguration<ComplianceSelectionModel>
    {
        public void Configure(EntityTypeBuilder<ComplianceSelectionModel> builder)
        {
        }
    }
}