using Microsoft.EntityFrameworkCore;
using Groove.SP.Core.Entities;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Invoices.ViewModels;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.WarehouseFulfillment.ViewModels;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Groove.SP.Persistence.Contexts
{
    public class SpContext : DbContext
    {
        public SpContext(DbContextOptions<SpContext> options)
            : base(options)
        {
        }

        #region Tables
        public virtual DbSet<ShipmentModel> Shipments { get; set; }
        public virtual DbSet<ShipmentContactModel> ShipmentContacts { get; set; }
        public virtual DbSet<CargoDetailModel> CargoDetails { get; set; }
        public virtual DbSet<ShipmentLoadModel> ShipmentLoads { get; set; }
        public virtual DbSet<ShipmentLoadDetailModel> ShipmentLoadDetails { get; set; }
        public virtual DbSet<ShipmentItemModel> ShipmentItems { get; set; }

        public virtual DbSet<ConsignmentModel> Consignments { get; set; }
        public virtual DbSet<ConsignmentItineraryModel> ConsignmentItineraries { get; set; }
        public virtual DbSet<ItineraryModel> Itineraries { get; set; }

        public virtual DbSet<ContainerModel> Containers { get; set; }
        public virtual DbSet<ConsolidationModel> Consolidations { get; set; }
        public virtual DbSet<ContainerItineraryModel> ContainerItineraries { get; set; }

        public virtual DbSet<UserProfileModel> UserProfiles { get; set; }
        public virtual DbSet<UserRoleModel> UserRoles { get; set; }
        public virtual DbSet<UserAuditLogModel> UserAuditLogs { get; set; }
        public virtual DbSet<NotificationModel> Notifications { get; set; }
        public virtual DbSet<UserNotificationModel> UserNotifications { get; set; }

        public virtual DbSet<RoleModel> Roles { get; set; }
        public virtual DbSet<PermissionModel> Permissions { get; set; }
        public virtual DbSet<IntegrationLogModel> IntegrationLogs { get; set; }
        public virtual DbSet<RolePermissionModel> RolePermissions { get; set; }

        public virtual DbSet<BillOfLadingModel> BillOfLadings { get; set; }
        public virtual DbSet<BillOfLadingContactModel> BillOfLadingContacts { get; set; }
        public virtual DbSet<BillOfLadingConsignmentModel> BillOfLadingConsignments { get; set; }
        public virtual DbSet<BillOfLadingShipmentLoadModel> BillOfLadingShipmentLoads { get; set; }
        public virtual DbSet<BillOfLadingItineraryModel> BillOfLadingItineraries { get; set; }

        public virtual DbSet<ShipmentBillOfLadingModel> ShipmentBillOfLadings { get; set; }

        public virtual DbSet<MasterBillOfLadingModel> MasterBills { get; set; }
        public virtual DbSet<MasterBillOfLadingContactModel> MasterBillContacts { get; set; }
        public virtual DbSet<MasterBillOfLadingItineraryModel> MasterBillItineraries { get; set; }
        public virtual DbSet<MasterDialogModel> MasterDialogs { get; set; }

        public virtual DbSet<TranslationModel> Translations { get; set; }

        public virtual DbSet<InvoiceModel> Invoices { get; set; }

        public virtual DbSet<AttachmentModel> Attachments { get; set; }
        public virtual DbSet<AttachmentTypePermissionModel> AttachmentTypePermissions { get; set; }
        public virtual DbSet<AttachmentTypeClassificationModel> AttachmentTypeClassifications { get; set; }

        public virtual DbSet<ShareDocumentModel> ShareDocuments { get; set; }

        public virtual DbSet<ActivityModel> Activities { get; set; }

        public virtual DbSet<GlobalIdModel> GlobalIds { get; set; }
        public virtual DbSet<GlobalIdActivityModel> GlobalIdActivities { get; set; }
        public virtual DbSet<GlobalIdApprovalModel> GlobalIdApprovals { get; set; }
        public virtual DbSet<GlobalIdAttachmentModel> GlobalIdAttachments { get; set; }
        public virtual DbSet<GlobalIdMasterDialogModel> GlobalIdMasterDialogs { get; set; }

        public virtual DbSet<POLineItemModel> POLineItems { get; set; }

        public virtual DbSet<PurchaseOrderModel> PurchaseOrders { get; set; }

        public virtual DbSet<PurchaseOrderContactModel> PurchaseOrderContacts { get; set; }
        public virtual DbSet<PurchaseOrderAdhocChangeModel> PurchaseOrderAdhocChanges { get; set; }


        public virtual DbSet<ImportDataProgressModel> ImportDataProgresses { get; set; }

        public virtual DbSet<BuyerComplianceModel> BuyerCompliances { get; set; }

        public virtual DbSet<BuyerApprovalModel> BuyerApprovals { get; set; }

        public virtual DbSet<BookingValidationLogModel> BookingValidationLogs { get; set; }

        public virtual DbSet<BookingPolicyModel> BookingPolicies { get; set; }

        public virtual DbSet<AgentAssignmentModel> AgentAssignments { get; set; }

        public virtual DbSet<PurchaseOrderVerificationSettingModel> PurchaseOrderVerificationSettings { get; set; }

        public virtual DbSet<ProductVerificationSettingModel> ProductVerificationSettings { get; set; }

        public virtual DbSet<BookingTimelessModel> BookingTimelesses { get; set; }

        public virtual DbSet<CargoLoadabilityModel> CargoLoadabilitys { get; set; }

        public virtual DbSet<ShippingComplianceModel> ShippingCompliances { get; set; }

        public virtual DbSet<ComplianceSelectionModel> ComplianceSelections { get; set; }

        public virtual DbSet<EmailSettingModel> EmailSettings { get; set; }

        public virtual DbSet<POFulfillmentModel> POFulfillments { get; set; }

        public virtual DbSet<POFulfillmentContactModel> POFulfillmentContacts { get; set; }

        public virtual DbSet<POFulfillmentOrderModel> POFulfillmentOrders { get; set; }

        public virtual DbSet<POFulfillmentShortshipOrderModel> POFulfillmentShortshipOrders { get; set; }

        public virtual DbSet<POFulfillmentAllocatedOrderModel> POFulfillmentAllocatedOrders { get; set; }

        public virtual DbSet<POFulfillmentLoadModel> POFulfillmentLoads { get; set; }

        public virtual DbSet<POFulfillmentCargoDetailModel> POFulfillmentCargoDetails { get; set; }

        public virtual DbSet<POFulfillmentLoadDetailModel> POFulfillmentLoadDetails { get; set; }

        public virtual DbSet<POFulfillmentBookingRequestModel> POFulfillmentBookingRequests { get; set; }

        public virtual DbSet<POFulfillmentItineraryModel> POFulfillmentItineraries { get; set; }

        public virtual DbSet<POFulfillmentCargoReceiveModel> POFulfillmentCargoReceives { get; set; }

        public virtual DbSet<POFulfillmentCargoReceiveItemModel> POFulfillmentCargoReceiveItems { get; set; }

        public virtual DbSet<NoteModel> Notes { get; set; }

        public virtual DbSet<FreightSchedulerModel> FreightSchedulers { get; set; }
        public virtual DbSet<FreightSchedulerChangeLogModel> FreightSchedulerChangeLogs { get; set; }
        public virtual DbSet<CurrencyExchangeRateModel> CurrencyExchangeRates { get; set; }
        public virtual DbSet<OrganizationPreferenceModel> OrganizationPreferences { get; set; }
        public virtual DbSet<OrgContactPreferenceModel> OrgContactPreferences { get; set; }
        public virtual DbSet<ContractTypeModel> ContractTypes { get; set; }
        public virtual DbSet<FtpServerModel> FtpServers { get; set; }
        public virtual DbSet<ArticleMasterModel> ArticleMaster { get; set; }

        public virtual DbSet<RoutingOrderModel> RoutingOrders { get; set; }
        public virtual DbSet<RoutingOrderContactModel> RoutingOrderContacts { get; set; }
        public virtual DbSet<ROLineItemModel> ROLineItems { get; set; }
        public virtual DbSet<RoutingOrderContainerModel> RoutingOrderContainers { get; set; }
        public virtual DbSet<RoutingOrderInvoiceModel> RoutingOrderInvoices { get; set; }

        public virtual DbSet<ViewSettingModel> ViewSettings { get; set; }
        public virtual DbSet<ViewRoleSettingModel> ViewRoleSettings { get; set; }


        #region Cruise business

        public virtual DbSet<CruiseOrderModel> CruiseOrders { get; set; }
        public virtual DbSet<CruiseOrderContactModel> CruiseOrderContacts { get; set; }
        public virtual DbSet<CruiseOrderItemModel> CruiseOrderItems { get; set; }
        public virtual DbSet<CruiseOrderWarehouseInfoModel> CruiseOrderWarehouseInfos { get; set; }

        #endregion

        #region Reports
        public virtual DbSet<ReportModel> Reports { get; set; }

        public virtual DbSet<ReportPermissionModel> ReportPermissions { get; set; }

        public virtual DbSet<SchedulingModel> Schedulings { get; set; }


        #endregion Reports

        #region Survey
        public virtual DbSet<SurveyModel> Surveys { get; set; }
        public virtual DbSet<SurveyQuestionModel> SurveyQuestions { get; set; }
        public virtual DbSet<SurveyQuestionOptionModel> SurveyQuestionOptions { get; set; }
        public virtual DbSet<SurveyAnswerModel> SurveyAnswers { get; set; }
        public virtual DbSet<SurveyParticipantModel> SurveyParticipants { get; set; }
        #endregion

        #endregion

        #region Views

        public virtual DbSet<UserRequestModel> UserRequestsView { get; set; }
        public DbSet<ShipmentQueryModel> ShipmentQuery { get; set; }
        public DbSet<ShipmentMilestoneSingleQueryModel> ShipmentMilestoneSingleQuery { get; set; }
        public DbSet<PurchaseOrderQueryModel> PurchaseOrderQuery { get; set; }
        public DbSet<PurchaseOrderSingleQueryModel> PurchaseOrderSignleQueryModel { get; set; }
        public DbSet<ShippedPurchaseOrderQueryModel> ShippedPurchaseOrderQuery { get; set; }
        public DbSet<CruiseOrderQueryModel> CruiseOrderQuery { get; set; }
        public DbSet<BuyerComplianceQueryModel> BuyerComplianceQuery { get; set; }
        public DbSet<ConsignmentQueryModel> ConsignmentQuery { get; set; }
        public DbSet<MasterBillOfLadingQueryModel> MasterBillOfLadingQuery { get; set; }
        public DbSet<POFulfillmentQueryModel> POFulfillmentQuery { get; set; }
        public DbSet<BulkFulfillmentQueryModel> BulkFulfillmentQuery { get; set; }
        public DbSet<ReportingShipmentQueryModel> ReportingShipmentQuery { get; set; }
        public DbSet<InvoiceQueryModel> InvoiceQuery { get; set; }
        public DbSet<ReportQueryModel> ReportQuery { get; set; }
        public DbSet<FreightSchedulerQueryModel> FreightSchedulerQuery { get; set; }
        public DbSet<ContainerQueryModel> ContainerQuery { get; set; }
        public DbSet<UserAuditLogQueryModel> UserAuditLogQuery { get; set; }
        public DbSet<MasterDialogQueryModel> MasterDialogQuery { get; set; }
        public DbSet<ConsolidationQueryModel> ConsolidationQuery { get; set; }
        public DbSet<CargoLoadDetailQueryModel> CargoLoadDetailQuery { get; set; }
        public DbSet<HouseBLQueryModel> HouseBLQuery { get; set; }
        public DbSet<BillOfLadingQueryModel> BillOfLadingQuery { get; set; }
        public DbSet<ContractMasterQueryModel> ContractMasterQuery { get; set; }
        public DbSet<SchedulingQueryModel> SchedulingQuery { get; set; }
        public DbSet<CustomerRelationshipQueryModel> CustomerRelationshipQuery { get; set; }
        public DbSet<ShipmentScheduleQueryModel> ShipmentScheduleQuery { get; set; }
        public DbSet<WarehouseFulfillmentLocationQueryModel> WarehouseFulfillmentLocationQuery { get; set; }
        public DbSet<WarehouseFulfillmentLocationSOFormQueryModel> WarehouseFulfillmentLocationSOFormQuery { get; set; }
        public DbSet<WarehouseArticleMasterQueryModel> WarehouseArticleMasterQuery { get; set; }
        public DbSet<UserListQueryModel> UserListQuery { get; set; }
        public DbSet<POEmailNotificationQueryModel> POEmailNotificationQuery { get; set; }
        public DbSet<BuyerApprovalQueryModel> BuyerApprovalQuery { get; set; }
        public DbSet<IntegrationLogQueryModel> IntegrationLogQuery { get; set; }
        public DbSet<ArticleMasterQueryModel> ArticleMasterQuery { get; set; }
        public DbSet<VesselArrivalQueryModel> VesselArrivalQuery { get; set; }
        public DbSet<SurveyQueryModel> SurveyQuery { get; set; }
        public DbSet<POFulfillmentShortshipOrderQueryModel> POFulfillmentShortshipOrderQuery { get; set; }
        public DbSet<RoutingOrderQueryModel> RoutingOrderQuery { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArticleMasterModel>().Ignore(c => c.CreatedDate).Ignore(c => c.UpdatedDate);

            // Some ad-hoc model configurations, espcially result from custom query without key column
            // Most of here is custom query
            modelBuilder.Entity<CustomerRelationshipQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<WarehouseFulfillmentLocationQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<WarehouseFulfillmentLocationSOFormQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<WarehouseArticleMasterQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<UserRequestModel>(c =>
            {
                c.HasNoKey().ToView("vw_UserRequests");
            });
            modelBuilder.Entity<ShipmentQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ShipmentMilestoneSingleQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<PurchaseOrderQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<PurchaseOrderSingleQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ShippedPurchaseOrderQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<CruiseOrderQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<BuyerComplianceQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ConsignmentQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<MasterBillOfLadingQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<POFulfillmentQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<BulkFulfillmentQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ReportingShipmentQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<InvoiceQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ReportQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ContainerQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<UserAuditLogQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<MasterDialogQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ConsolidationQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<CargoLoadDetailQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<HouseBLQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<BillOfLadingQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ContractMasterQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<SchedulingQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<ShipmentScheduleQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<FreightSchedulerQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<UserListQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<POEmailNotificationQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<BuyerApprovalQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });
            modelBuilder.Entity<POProgressCheckQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<IntegrationLogQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<ArticleMasterQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<VesselArrivalQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<SurveyQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<POFulfillmentShortshipOrderQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            modelBuilder.Entity<RoutingOrderQueryModel>(c =>
            {
                c.HasNoKey().ToView(null);
            });

            // Load Auto-mapper profiles
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SpContext).Assembly);
           
        }
    }
}
