using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groove.SP.Persistence.Configurations
{
    public class ImportDataProgressModelConfiguration : IEntityTypeConfiguration<ImportDataProgressModel>
    {
        public void Configure(EntityTypeBuilder<ImportDataProgressModel> builder)
        {
        }
    }
}
