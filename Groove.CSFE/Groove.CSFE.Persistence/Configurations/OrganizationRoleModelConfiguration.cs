using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Groove.CSFE.Persistence.Configurations
{
    public class OrganizationRoleModelConfiguration : IEntityTypeConfiguration<OrganizationRoleModel>
    {
        public void Configure(EntityTypeBuilder<OrganizationRoleModel> builder)
        {
            builder.Property(e => e.Name).HasColumnType("nvarchar(256)").IsRequired();
            var createdDate = new DateTime(2019, 01, 01);
            builder.HasData
            (
                new OrganizationRoleModel() { Id = 1, Name = "Shipper", OrganizationTypes = OrganizationType.Agent | OrganizationType.General | OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 2, Name = "Consignee", OrganizationTypes = OrganizationType.Agent | OrganizationType.General | OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 3, Name = "Notify Party", OrganizationTypes = OrganizationType.Agent | OrganizationType.General | OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 4, Name = "Also Notify", OrganizationTypes = OrganizationType.Agent | OrganizationType.General | OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 5, Name = "Import Broker", OrganizationTypes = OrganizationType.Agent | OrganizationType.General | OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 6, Name = "Export Broker", OrganizationTypes = OrganizationType.Agent | OrganizationType.General | OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 7, Name = "Origin Agent", OrganizationTypes = OrganizationType.Agent, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 8, Name = "Destination Agent", OrganizationTypes = OrganizationType.Agent, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 9, Name = "Principal", OrganizationTypes = OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 10, Name = "Supplier", OrganizationTypes = OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 11, Name = "Delegation", OrganizationTypes = OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 12, Name = "Billing Party", OrganizationTypes = OrganizationType.Agent | OrganizationType.General | OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate },
                new OrganizationRoleModel() { Id = 13, Name = "Pickup", OrganizationTypes = OrganizationType.Agent | OrganizationType.General | OrganizationType.Principal, CreatedBy = AppConstants.SYSTEM_USERNAME, CreatedDate = createdDate }
            );
        }
    }
}
