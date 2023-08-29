using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Groove.CSFE.Persistence.Contexts
{
    public class CsfeContext : DbContext
    {
        public CsfeContext(DbContextOptions<CsfeContext> options)
            : base(options)
        {
        }

        #region Tables
        public virtual DbSet<CountryModel> Countries { get; set; }

        public virtual DbSet<LocationModel> Locations { get; set; }

        public virtual DbSet<OrganizationModel> Organizations { get; set; }

        public virtual DbSet<UserOfficeModel> UserOffices { get; set; }

        public virtual DbSet<OrganizationRoleModel> OrganizationRoles { get; set; }

        public virtual DbSet<OrganizationInRoleModel> OrganizationInRoles { get; set; }

        public virtual DbSet<CurrencyModel> Currencies { get; set; }

        public virtual DbSet<EventCodeModel> EventCodes { get; set; }

        public virtual DbSet<EventTypeModel> EventTypes { get; set; }

        public virtual DbSet<PortModel> Ports { get; set; }

        public virtual DbSet<CarrierModel> Carriers { get; set; }

        public virtual DbSet<CustomerRelationshipModel> CustomerRelationship { get; set; }

        public virtual DbSet<AlternativeLocationModel> AlternativeLocations { get; set; }

        public virtual DbSet<VesselModel> Vessels { get; set; }

        public virtual DbSet<WarehouseLocationModel> WarehouseLocations { get; set; }

        public virtual DbSet<WarehouseAssignmentModel> WarehouseAssignments { get; set; }

        public virtual DbSet<EmailNotificationModel> EmailNotifications { get; set; }

        public virtual DbSet<WarehouseModel> Warehouses { get; set; }

        public virtual DbSet<TerminalModel> Terminals { get; set; }

        #endregion

        #region Views 

        public DbSet<OrganizationQueryModel> OrganizationQuery { get; set; }
        public DbSet<CustomerRelationshipQueryModel> SupplierRelationshipQuery { get; set; }
        public DbSet<LocationQueryModel> LocationQuery { get; set; }
        public DbSet<VesselQueryModel> VesselQuery { get; set; }
        public DbSet<CarrierQueryModel> CarrierQuery { get; set; }
        public DbSet<WarehouseLocationQueryModel> WarehouseLocationQuery { get; set; }
        public DbSet<EventCodeQueryModel> EventCodeQuery { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CsfeContext).Assembly);

            // Some ad-hoc model configurations, espcially result from custom query without key column
            // Most of here is custom query
            modelBuilder.Entity<CarrierQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<LocationQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<OrganizationQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<CustomerRelationshipQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<VesselQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<WarehouseLocationQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<EventCodeQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
        }
    }
}
