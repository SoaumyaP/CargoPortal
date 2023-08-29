using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.MasterBillOfLading.Services;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Application.Invoices.Services;
using Groove.SP.Application.Invoices.Services.Interfaces;
using Groove.SP.Application.Permissions.Services;
using Groove.SP.Application.Permissions.Services.Interfaces;
using Groove.SP.Application.Provider.EmailSender;
using Groove.SP.Application.Shipments.Services;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Application.Users.Services;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Groove.SP.Application.AppDocument.Services.Interfaces;
using Groove.SP.Application.AppDocument.Services;
using Groove.SP.Application.Itinerary.Services;
using Groove.SP.Application.BillOfLading.Services;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.BillOfLading.Services.Interfaces;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Container.Services;
using Groove.SP.Application.Attachment.Services;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLadingContact.Services;
using Groove.SP.Application.MasterBillOfLadingContact.Services.Interfaces;
using Groove.SP.Application.BillOfLadingContact.Services.Interfaces;
using Groove.SP.Application.BillOfLadingContact.Services;
using Groove.SP.Application.CargoDetail.Services.Interfaces;
using Groove.SP.Application.CargoDetail.Services;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.Services;
using Groove.SP.Application.ShipmentContact.Services.Interfaces;
using Groove.SP.Application.ShipmentContact.Services;
using Groove.SP.Application.Consignment.Services.Interfaces;
using Groove.SP.Application.Consignment.Services;
using Groove.SP.Application.Consolidation.Services;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using Groove.SP.Application.ShipmentLoads.Services.Interfaces;
using Groove.SP.Application.ShipmentLoads.Services;
using Groove.SP.Application.ShipmentLoadDetails.Services.Interfaces;
using Groove.SP.Application.ShipmentLoadDetails.Services;
using Groove.SP.Application.ShipmentBillOfLading.Services.Interfaces;
using Groove.SP.Application.ShipmentBillOfLading.Services;
using Groove.SP.Application.BillOfLadingShipmentLoad.Services.Interfaces;
using Groove.SP.Application.BillOfLadingShipmentLoad.Services;
using Groove.SP.Application.BillOfLadingConsignment.Services.Interfaces;
using Groove.SP.Application.BillOfLadingConsignment.Services;
using Groove.SP.Application.IntegrationLog.Services;
using Groove.SP.Application.IntegrationLog.Services.Interfaces;
using Groove.SP.Infrastructure.EmailSender;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.Services;
using Groove.SP.Application.ImportData.Services;
using Groove.SP.Application.ImportData.Services.Interfaces;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Application.BuyerComplianceService.Services;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.Services;
using Groove.SP.Application.BuyerApproval.Services;
using Groove.SP.Application.BuyerApproval.Services.Interfaces;
using Groove.SP.Application.BookingValidationLog.Services.Interfaces;
using Groove.SP.Application.BookingValidationLog.Services;
using Groove.SP.Application.GlobalIdActivity.Services;
using Groove.SP.Application.GlobalIdActivity.Services.Interfaces;
using Groove.SP.Application.BuyerCompliance.Services.Interfaces;
using Groove.SP.Application.BuyerCompliance.Services;
using Groove.SP.Application.Reports.Services.Interfaces;
using Groove.SP.Application.Reports.Services;
using Groove.SP.Infrastructure.RazorLight;
using Groove.SP.Application.CruiseOrders.Services.Interfaces;
using Groove.SP.Application.CruiseOrders.Services;
using Groove.SP.Application.Note.Services.Interfaces;
using Groove.SP.Application.Note.Services;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Services.Interfaces;
using Groove.SP.Application.Cruise.CruiseOrderWarehouseInfos.Services;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Application.ApplicationBackgroundJob;
using FluentValidation;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.PurchaseOrders.Validations;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.POFulfillment.Validations;
using Groove.SP.Application.CruiseOrders.ViewModels;
using System.IO;
using System;
using Groove.SP.Infrastructure.DinkToPdf;
using DinkToPdf.Contracts;
using Groove.SP.Application.Provider.Report;
using DinkToPdf;
using Groove.SP.Infrastructure.Report;
using System.Collections.Generic;
using Groove.SP.Application.CruiseOrders.Validations;
using Groove.SP.Application.FreightScheduler.Services.Interfaces;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Application.Itinerary.Validations;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Application.MasterBillOfLading.Validations;
using Groove.SP.Application.FreightScheduler.ViewModels;
using Groove.SP.Application.FreightScheduler.Validations;
using Groove.SP.Application.OrganizationPreference.Services.Interfaces;
using Groove.SP.Application.MasterDialog.Services.Interfaces;
using Groove.SP.Application.MasterDialog.Services;
using Groove.SP.Application.GlobalIdMasterDialog.Services.Interfaces;
using Groove.SP.Application.GlobalIdMasterDialog.Services;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.ShipmentLoadDetails.Validations;
using Groove.SP.Application.PurchaseOrderContact.Services;
using Groove.SP.Application.PurchaseOrderContact.Services.Interfaces;
using Groove.SP.Application.ContractMaster.ViewModels;
using Groove.SP.Application.ContractMaster.Validations;
using Groove.SP.Application.ContractType.Services.Interfaces;
using Groove.SP.Application.ContractType.Services;
using Groove.SP.Application.Scheduling.Services;
using Groove.SP.Application.Scheduling.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Activity.Validations;
using Groove.SP.Application.BulkFulfillment.Services.Interfaces;
using Groove.SP.Application.BulkFulfillment.Services;
using Groove.SP.Application.WarehouseFulfillment.Services.Interfaces;
using Groove.SP.Application.WarehouseFulfillment.Services;
using Groove.SP.Application.OrgContactPreference.Services.Interfaces;
using Groove.SP.Application.OrgContactPreference.Services;
using Groove.SP.Application.POFulfillmentCargoReceive.Services.Interfaces;
using Groove.SP.Application.POFulfillmentCargoReceive.Services;
using Microsoft.AspNetCore.Authorization;
using Groove.SP.API.Filters.Authorization.Handlers;
using Groove.SP.Application.Provider.Sftp;
using Groove.SP.Infrastructure.Sftp;
using Groove.SP.Application.ArticleMaster.Services.Interfaces;
using Groove.SP.Application.ArticleMaster.Services;
using Groove.SP.Infrastructure.QRCoder;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Groove.SP.Infrastructure.SignalR.HubConfigs;
using Groove.SP.Application.Notification.Services;
using Groove.SP.Application.MobileApplication.Services.Interfaces;
using Groove.SP.Application.MobileApplication.Services;
using Groove.SP.Application.Survey.Services.Interfaces;
using Groove.SP.Application.Survey.Services;
using Groove.SP.Application.SurveyQuestion.Services.Interfaces;
using Groove.SP.Application.SurveyQuestion.Services;
using Groove.SP.Application.POFulfillmentShortshipOrder.Services;
using Groove.SP.Application.POFulfillmentShortshipOrder.Services.Interfaces;
using Groove.SP.Application.Invoices.Validations;
using Groove.SP.Application.Invoices.ViewModels;
using Groove.SP.Application.RoutingOrder.Services.Interfaces;
using Groove.SP.Application.RoutingOrder.Services;
using Groove.SP.Application.ViewSetting.Services.Interfaces;
using Groove.SP.Application.ViewSetting.Services;
using Groove.SP.Application.QuickTrack.Services;
using Groove.SP.Application.QuickTrack.Services.Interfaces;

namespace Groove.SP.API
{
    public partial class Startup
    {
        private void RegisterBusinessServices(IServiceCollection services)
        {
            // Service
            services.AddScoped<IShipmentService, ShipmentService>();
            services.AddScoped<IShipmentListService, ShipmentListService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IUserRequestService, UserRequestService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IInvoiceListService, InvoiceListService>();
            services.AddScoped<IItineraryService, ItineraryService>();
            services.AddScoped<IBillOfLadingService, BillOfLadingService>();
            services.AddScoped<IContainerService, ContainerService>();
            services.AddScoped<IContainerItineraryService, ContainerItineraryService>();
            services.AddScoped<IBillOfLadingContactService, BillOfLadingContactService>();
            services.AddScoped<IMasterBillOfLadingContactService, MasterBillOfLadingContactService>();
            services.AddScoped<IMasterBillOfLadingService, MasterBillOfLadingService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<ICargoDetailService, CargoDetailService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IShipmentContactService, ShipmentContactService>();
            services.AddScoped<IConsignmentService, ConsignmentService>();
            services.AddScoped<IConsignmentListService, ConsignmentListService>();
            services.AddScoped<IConsolidationService, ConsolidationService>();
            services.AddScoped<IConsolidationListService, ConsolidationListService>();
            services.AddScoped<IShipmentLoadService, ShipmentLoadService>();
            services.AddScoped<IShipmentLoadDetailService, ShipmentLoadDetailService>();
            services.AddScoped<IShareDocumentService, ShareDocumentService>();
            services.AddScoped<IShipmentBillOfLadingService, ShipmentBillOfLadingService>();
            services.AddScoped<IBillOfLadingShipmentLoadService, BillOfLadingShipmentLoadService>();
            services.AddScoped<IBillOfLadingConsignmentService, BillOfLadingConsignmentService>();
            services.AddScoped<IMasterBillOfLadingItineraryService, MasterBillOfLadingItineraryService>();
            services.AddScoped<IBillOfLadingItineraryService, BillOfLadingItineraryService>();
            services.AddScoped<IContainerItineraryService, ContainerItineraryService>();
            services.AddScoped<IConsignmentItineraryService, ConsignmentItineraryService>();
            services.AddScoped<IIntegrationLogService, IntegrationLogService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IPurchaseOrderListService, PurchaseOrderListService>();
            services.AddScoped<IBuyerComplianceListService, BuyerComplianceListService>();
            services.AddScoped<IPOFulfillmentService, POFulfillmentService>();
            services.AddScoped<IBulkFulfillmentService, BulkFulfillmentService>();
            services.AddScoped<IWarehouseFulfillmentService, WarehouseFulfillmentService>();
            services.AddScoped<IPOFulfillmentListService, POFulfillmentListService>();
            services.AddScoped<IPOFulfillmentShortshipOrderListService, POFulfillmentShortshipOrderListService>();
            services.AddScoped<IPOFulfillmentShortshipOrderService, POFulfillmentShortshipOrderService>();
            services.AddScoped<IImportDataProgressService, ImportDataProgressService>();
            services.AddScoped<IBuyerComplianceService, BuyerComplianceService>();
            services.AddScoped<IBuyerApprovalService, BuyerApprovalService>();
            services.AddScoped<IBookingValidationLogService, BookingValidationLogService>();
            services.AddScoped<IGlobalIdActivityService, GlobalIdActivityService>();
            services.AddScoped<IPOFulfillmentOrderRepository, POFulfillmentOrderRepository>();
            services.AddScoped<IContainerRepository, ContainerRepository>();
            services.AddScoped<IReportListService, ReportListService>();
            services.AddScoped<IEdiSonBookingService, EdiSonBookingService>();
            services.AddScoped<IEdisonBulkFulfillmentService, EdisonBulkFulfillmentService>();
            services.AddScoped<IEdiSonConfirmService, EdiSonConfirmService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IAllocatedPurchaseOrderService, AllocatedPurchaseOrderService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IHtmlStringBuilder, HtmlStringBuilder>();
            services.AddScoped<IQRCodeBuilder, QRCodeBuilder>();
            services.AddScoped<IQueuedBackgroundJobs, QueuedBackgroundJobs>();
            services.AddScoped<IFreightSchedulerService, FreightSchedulerService>();
            services.AddScoped<IOrganizationPreferenceService, OrganizationPreferenceService>();
            services.AddScoped<IOrgContactPreferenceService, OrgContactPreferenceService>();
            services.AddScoped<IPurchaseOrderContactService, PurchaseOrderContactService>();
            services.AddScoped<IMasterDialogService, MasterDialogService>();
            services.AddScoped<IGlobalIdMasterDialogService, GlobalIdMasterDialogService>();
            services.AddScoped<IContractMasterService, ContractMasterService>();
            services.AddScoped<IContractTypeService, ContractTypeService>();
            services.AddScoped<ISchedulingService, SchedulingService>();
            services.AddScoped<ISchedulingListService, SchedulingListService>();
            services.AddScoped<IPOFulfillmentCargoReceiveService, POFulfillmentCargoReceiveService>();
            services.AddScoped<IArticleMasterService, ArticleMasterService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISurveyListService, SurveyListService>();
            services.AddScoped<ISurveyService, SurveyService>();
            services.AddScoped<ISurveyQuestionService, SurveyQuestionService>();
            services.AddScoped<IRoutingOrderService, RoutingOrderService>();
            services.AddScoped<IRoutingOrderListService, RoutingOrderListService>();
            services.AddScoped<ISOFormGeneratorContext, SOFormGeneratorContext>();
            services.AddScoped<ISOFormGeneratorContext, SOFormGeneratorContext>();
            services.AddScoped<IViewSettingService, ViewSettingService>();

            services.AddScoped<IQuickTrackService, QuickTrackService>();

            // Cruise business
            services.AddScoped<ICruiseOrderService, CruiseOrderService>();
            services.AddScoped<ICruiseOrderItemService, CruiseOrderItemService>();
            services.AddScoped<ICruiseOrderListService, CruiseOrderListService>();
            services.AddScoped<ICruiseOrderWarehouseInfoService, CruiseOrderWarehouseInfoService>();


            // CSED/EdiSon Service bus integration
            services.AddScoped<ICSEDShippingDocumentProcessor, CSEDShippingDocumentProcessor>();

            services.AddSingleton<IAuthorizationHandler, WebhookSignatureVerificationHandler>();
            services.AddSingleton<IAuthorizationHandler, MobileAppSecurityVerificationHandler>();

            services.AddScoped<INotification, Notification>();

            // Mobile
            services.AddScoped<IMobileApplicationService, MobileApplicationService>();

        }

        private void RegisterValidators(IServiceCollection services)
        {
            // Validators
            services.AddScoped<IValidator<CreatePOViewModel>, CreatePOViewModelValidator>();
            services.AddScoped<IValidator<UpdatePOViewModel>, UpdatePOViewModelValidator>();
            services.AddScoped<IValidator<EdiSonConfirmPOFFViewModel>, EdiSonConfirmPOFFValidator>();
            services.AddScoped<IValidator<EdiSonUpdateConfirmPOFFViewModel>, EdiSonUpdateConfirmPOFFValidator>();
            services.AddScoped<IValidator<CreateContractMasterViewModel>, CreateContractMasterValidator>();
            services.AddScoped<IValidator<UpdateContractMasterViewModel>, UpdateContractMasterValidator>();

            services.AddScoped<IValidator<CreateCruiseOrderViewModel>, CreateCruiseOrderViewModelValidator>();
            services.AddScoped<IValidator<UpdateCruiseOrderViewModel>, UpdateCruiseOrderViewModelValidator>();
            services.AddScoped<IValidator<IEnumerable<CreateCruiseOrderViewModel>>, CreateBulkCruiseOrderViewModelValidator>();

            services.AddScoped<IValidator<CreateItineraryViewModel>, CreateItineraryViewModelValidator>();
            services.AddScoped<IValidator<UpdateItineraryViewModel>, UpdateItineraryViewModelValidator>();

            services.AddScoped<IValidator<CreateItineraryViewModel>, CreateItineraryViewModelValidator>();
            services.AddScoped<IValidator<UpdateItineraryViewModel>, UpdateItineraryViewModelValidator>();

            services.AddScoped<IValidator<UpdateMasterBillOfLadingViewModel>, UpdateMasterBillViewModelValidator>();
            services.AddScoped<IValidator<CreateMasterBillOfLadingViewModel>, CreateMasterBillViewModelValidator>();

            services.AddScoped<IValidator<UpdateFreightSchedulerApiViewModel>, UpdateFreightSchedulerApiViewModelValidator>();

            services.AddScoped<IValidator<UpdateShipmentLoadDetailViewModel>, UpdateShipmentLoadDetailViewModelValidator>();

            services.AddScoped<IValidator<AgentActivityCreateViewModel>, AgentActivityCreateViewModelValidator>();
            services.AddScoped<IValidator<AgentActivityUpdateViewModel>, AgentActivityUpdateViewModelValidator>();
            services.AddScoped<IValidator<AgentActivityDeleteViewModel>, AgentActivityDeleteViewModelValidator>();

            services.AddScoped<IValidator<ImportPOFulfillmentCargoReceiveViewModel>, ImportPOFulfillmentCargoReceiveViewModelValidator>();
            services.AddScoped<IValidator<InvoiceUpdatePaymentViewModel>, InvoiceUpdatePaymentViewModelValidator>();
        }

        private void RegisterProviders(IServiceCollection services)
        {
            // Service for PdfGenerator
            var assemblyLoadContext = new CustomAssemblyLoadContext();
            var architectureFolder = IntPtr.Size == 8 ? "64" : "32";
            assemblyLoadContext.LoadUnmanagedLibrary(Path.Combine($"{Directory.GetCurrentDirectory()}/libwkhtmltox/{architectureFolder}/",
                "libwkhtmltox.dll"));

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddScoped<IPdfGenerator, PdfGenerator>();

            services.AddScoped<ITelerikReportProvider, TelerikReportProvider>();

            services.AddScoped<ISftpProvider, SftpProvider>();

            services.AddSingleton<IUserIdProvider, UserNameBasedUserIdProvider>();
        }
    }
}
