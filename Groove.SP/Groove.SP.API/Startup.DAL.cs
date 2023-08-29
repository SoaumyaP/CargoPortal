using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Entities.Mobile;
using Groove.SP.Persistence.Data;
using Groove.SP.Persistence.Repositories;
using Groove.SP.Persistence.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace Groove.SP.API
{
    public partial class Startup
    {
        private void RegisterDAL(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWorkProvider, UnitOfWorkProvider>();

            // Repository
            services.AddScoped<IRepository<ShipmentModel>, ShipmentRepository>();
            services.AddScoped<IRepository<UserProfileModel>, UserProfileRepository>();
            services.AddScoped<IRepository<UserRequestModel>, UserRequestRepository>();
            services.AddScoped<IRepository<RoleModel>, RoleRepository>();
            services.AddScoped<IRepository<TranslationModel>, TranslationRepository>();
            services.AddScoped<IRepository<PermissionModel>, PermissionRepository>();
            services.AddScoped<IRepository<MasterBillOfLadingModel>, MasterBillOfLadingRepository>();
            services.AddScoped<IRepository<InvoiceModel>, InvoiceRepository>();
            services.AddScoped<IRepository<ItineraryModel>, ItineraryRepository>();
            services.AddScoped<IRepository<BillOfLadingModel>, BillOfLadingRepository>();
            services.AddScoped<IRepository<ContainerModel>, ContainerRepository>();
            services.AddScoped<IRepository<ContainerItineraryModel>, ContainerItineraryRepository>();
            services.AddScoped<IRepository<BillOfLadingContactModel>, BillOfLadingContactRepository>();
            services.AddScoped<IRepository<ShareDocumentModel>, ShareDocumentRepository>();
            services.AddScoped<IRepository<MasterBillOfLadingContactModel>, MasterBillOfLadingContactRepository>();
            services.AddScoped<IRepository<AttachmentModel>, AttachmentRepository>();
            services.AddScoped<IRepository<AttachmentTypePermissionModel>, AttachmentTypePermissionRepository>();
            services.AddScoped<IRepository<AttachmentTypeClassificationModel>, AttachmentTypeClassificationRepository>();
            services.AddScoped<IRepository<CargoDetailModel>, CargoDetailRepository>();
            services.AddScoped<IRepository<ActivityModel>, ActivityRepository>();
            services.AddScoped<IRepository<ShipmentContactModel>, ShipmentContactRepository>();
            services.AddScoped<IRepository<ConsignmentModel>, ConsignmentRepository>();
            services.AddScoped<IRepository<ConsolidationModel>, ConsolidationRepository>();
            services.AddScoped<IRepository<ShipmentLoadModel>, ShipmentLoadRepository>();
            services.AddScoped<IRepository<ShipmentLoadDetailModel>, ShipmentLoadDetailRepository>();
            services.AddScoped<IRepository<ShipmentBillOfLadingModel>, ShipmentBillOfLadingRepository>();
            services.AddScoped<IRepository<BillOfLadingShipmentLoadModel>, BillOfLadingShipmentLoadRepository>();
            services.AddScoped<IRepository<BillOfLadingConsignmentModel>, BillOfLadingConsignmentRepository>();
            services.AddScoped<IRepository<BillOfLadingItineraryModel>, BillOfLadingItineraryRepository>();
            services.AddScoped<IRepository<ContainerItineraryModel>, ContainerItineraryRepository>();
            services.AddScoped<IRepository<ConsignmentItineraryModel>, ConsignmentItineraryRepository>();
            services.AddScoped<IRepository<MasterBillOfLadingItineraryModel>, MasterBillOfLadingItineraryRepository>();
            services.AddScoped<IRepository<IntegrationLogModel>, IntegrationLogRepository>();
            services.AddScoped<IRepository<PurchaseOrderModel>, PurchaseOrderRepository>();
            services.AddScoped<IRepository<POFulfillmentModel>, POFulfillmentRepository>();
            services.AddScoped<IRepository<POFulfillmentContactModel>, POFulfillmentContactRepository>();
            services.AddScoped<IRepository<POFulfillmentBookingRequestModel>, POFulfillmentBookingRequestRepository>();
            services.AddScoped<IRepository<POFulfillmentAllocatedOrderModel>, POFulfillmentAllocatedOrderRepository>();
            services.AddScoped<IRepository<POFulfillmentShortshipOrderModel>, POFulfillmentShortshipOrderRepository>();
            services.AddScoped<IRepository<ImportDataProgressModel>, ImportDataProgressRepository>();
            services.AddScoped<IRepository<BuyerComplianceModel>, BuyerComplianceRepository>();
            services.AddScoped<IRepository<BuyerApprovalModel>, BuyerApprovalRepository>();
            services.AddScoped<IRepository<POLineItemModel>, POLineItemRepository>();
            services.AddScoped<IRepository<BookingValidationLogModel>, BookingValidationLogRepository>();
            services.AddScoped<IRepository<MasterDialogModel>, MasterDialogRepository>();
            services.AddScoped<IRepository<GlobalIdMasterDialogModel>, GlobalIdMasterDialogRepository>();
            services.AddScoped<IRepository<ConsolidationModel>, ConsolidationRepository>();
            services.AddScoped<IRepository<FreightSchedulerModel>, FreightSchedulerRepository>();
            services.AddScoped<IRepository<CruiseOrderModel>, CruiseOrderRepository>();
            services.AddScoped<IRepository<NoteModel>, NoteRepository>();
            services.AddScoped<IRepository<CruiseOrderWarehouseInfoModel>, CruiseOrderWarehouseInfoRepository>();
            services.AddScoped<IRepository<CruiseOrderItemModel>, CruiseOrderItemRepository>();
            services.AddScoped<IRepository<ReportModel>, ReportRepository>();
            services.AddScoped<IRepository<ReportPermissionModel>, ReportPermissionRepository>();
            services.AddScoped<IRepository<GlobalIdActivityModel>, GlobalIdActivityRepository>();
            services.AddScoped<IRepository<ContractMasterModel>, ContractMasterRepository>();
            services.AddScoped<IRepository<ContractTypeModel>, ContractTypeRepository>();
            services.AddScoped<IRepository<SchedulingModel>, SchedulingRepository>();

            services.AddScoped<IPOFulfillmentRepository, POFulfillmentRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IBuyerApprovalRepository, BuyerApprovalRepository>();
            services.AddScoped<IPOFulfillmentOrderRepository, POFulfillmentOrderRepository>();
            services.AddScoped<IPOFulfillmentLoadRepository, POFulfillmentLoadRepository>();
            services.AddScoped<IPOFulfillmentAllocatedOrderRepository, POFulfillmentAllocatedOrderRepository>();
            services.AddScoped<IContainerRepository, ContainerRepository>();
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IFreightSchedulerRepository, FreightSchedulerRepository>();
            services.AddScoped<IUserAuditLogRepository, UserAuditLogRepository>();
            services.AddScoped<IOrganizationPreferenceRepository, OrganizationPreferenceRepository>();
            services.AddScoped<IOrgContactPreferenceRepository, OrgContactPreferenceRepository>();
            services.AddScoped<IMasterDialogRepository, MasterDialogRepository>();
            services.AddScoped<IConsolidationRepository, ConsolidationRepository>();
            services.AddScoped<IConsignmentRepository, ConsignmentRepository>();
            services.AddScoped<IShipmentBillOfLadingRepository, ShipmentBillOfLadingRepository>();
            services.AddScoped<IBillOfLadingConsignmentRepository, BillOfLadingConsignmentRepository>();
            services.AddScoped<IBillOfLadingShipmentLoadRepository, BillOfLadingShipmentLoadRepository>();
            services.AddScoped<IBillOfLadingRepository, BillOfLadingRepository>();
            services.AddScoped<IContractMasterRepository, ContractMasterRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IBuyerComplianceRepository, BuyerComplianceRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRepository<ArticleMasterModel>, ArticleMasterRepository>();
            services.AddScoped<IRepository<SurveyQuestionModel>, SurveyQuestionRepository>();
            services.AddScoped<IRepository<SurveyParticipantModel>, SurveyParticipantRepository>();
            services.AddScoped<IRepository<SurveyAnswerModel>, SurveyAnswerRepository>();

            services.AddScoped<IPOFulfillmentCargoReceiveRepository, POFulfillmentCargoReceiveRepository>();
            services.AddScoped<IPOFulfillmentCargoReceiveItemRepository, POFulfillmentCargoReceiveItemRepository>();
            services.AddScoped<IRepository<SurveyModel>, SurveyRepository>();
            services.AddScoped<IRepository<UserRoleModel>, UserRoleRepository>();

            services.AddScoped<IEmailRecipientRepository, EmailRecipientRepository>();
            services.AddScoped<IItineraryRepository, ItineraryRepository>();
            services.AddScoped<IRepository<NotificationModel>, NotificationRepository>();
            services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
            services.AddScoped<IRepository<RoutingOrderModel>, RoutingOrderRepository>();

            services.AddScoped<IFtpServerRepository, FtpServerRepository>();
            services.AddScoped<IImportDataProgressRepository, ImportDataProgressRepository>();

            services.AddScoped<IPOFulfillmentShortshipOrderRepository, POFulfillmentShortshipOrderRepository>();

            services.AddScoped<IRepository<MobileApplicationModel>, MobileApplicationRepository>();
            services.AddScoped<IMobileApplicationRepository, MobileApplicationRepository>();

            services.AddScoped<IViewSettingRepository, ViewSettingRepository>();

            // Temporarily disable caching.
            //services.Decorate<IViewSettingRepository, CachedViewSettingRepositoryDecorator>();

            // To execute statements to database via SP, query,...
            services.AddScoped<IDataQuery, EfDataQuery>();
        }
    }
}
