using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.BuyerApproval.Services.Interfaces;
using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.POFulfillment.Services.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.ApplicationBackgroundJob.Services;
using Groove.SP.Core.Data;
using System.Data;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;

namespace Groove.SP.Application.BuyerApproval.Services
{
    public class BuyerApprovalService : ServiceBase<BuyerApprovalModel, BuyerApprovalViewModel>, IBuyerApprovalService
    {
        private readonly IBuyerApprovalRepository _buyerApprovalRepository;
        private readonly IActivityService _activityService;
        private readonly AppConfig _appConfig;
        private readonly IUserProfileService _userProfileService;
        private readonly INotificationService _notificationService;
        private readonly IQueuedBackgroundJobs _queuedBackgroundJobs;
        private readonly IDataQuery _dataQuery;


        /// <summary>
        /// It is lazy service injection.
        /// Please do not use it directly, use POFulfillmentService instead.
        /// </summary>
        private IPOFulfillmentService _poFulfillmentServiceLazy;
        public IPOFulfillmentService POFulfillmentService
        {
            get
            {
                if (_poFulfillmentServiceLazy == null)
                {
                    _poFulfillmentServiceLazy = _services.GetRequiredService<IPOFulfillmentService>();
                }
                return _poFulfillmentServiceLazy;
            }
        }

        private readonly IServiceProvider _services;


        public BuyerApprovalService(IUnitOfWorkProvider unitOfWorkProvider,
            IActivityService activityService,
            IOptions<AppConfig> appConfig,
            IUserProfileService userProfileService,
            IServiceProvider services,
            IQueuedBackgroundJobs queuedBackgroundJobs,
            IDataQuery dataQuery,
            INotificationService notificationService)
            : base(unitOfWorkProvider)
        {
            _services = services;
            _buyerApprovalRepository = (IBuyerApprovalRepository)Repository;
            _activityService = activityService;
            _appConfig = appConfig.Value;
            _userProfileService = userProfileService;
            _queuedBackgroundJobs = queuedBackgroundJobs;
            _dataQuery = dataQuery;
            _notificationService = notificationService;
        }

        protected override Func<IQueryable<BuyerApprovalModel>, IQueryable<BuyerApprovalModel>> FullIncludeProperties => x
           => x.Include(m => m.POFulfillment);

        protected override IDictionary<string, string> SortMap => new Dictionary<string, string>() {
            { "exceptionTypeName", "exceptionType" },
            { "stageName", "stage" }
        };

        async Task<string> GenerateBuyerApprovalNumber(DateTime createdDate) =>
            $"APP{createdDate.ToString("yyMM")}{await _buyerApprovalRepository.GetNextBuyerApprovalSequenceValueAsync()}";

        public override async Task<BuyerApprovalViewModel> CreateAsync(BuyerApprovalViewModel viewModel)
        {
            viewModel.ValidateAndThrow();

            var model = Mapper.Map<BuyerApprovalModel>(viewModel);
            model.Reference = await GenerateBuyerApprovalNumber(DateTime.UtcNow);

            UpdateGlobalIdApprovals(model, viewModel);

            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<BuyerApprovalViewModel>(model);
            return viewModel;
        }

        public async Task<BuyerApprovalViewModel> UpdateAsync(BuyerApprovalViewModel viewModel, long id)
        {
            viewModel.ValidateAndThrow(true);

            var model = await this.Repository.GetAsync(x => x.Id == id, null, FullIncludeProperties);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            Mapper.Map(viewModel, model);

            UpdateGlobalIdApprovals(model, viewModel);

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<BuyerApprovalViewModel>(model);
            return viewModel;

        }

        private void UpdateGlobalIdApprovals(BuyerApprovalModel model, BuyerApprovalViewModel viewModel)
        {
            var newGlobalIdApprovals = new List<GlobalIdApprovalModel>();

            if (viewModel.POFulfillmentId.HasValue)
            {
                var globalId = CommonHelper.GenerateGlobalId(viewModel.POFulfillmentId.Value, EntityType.POFullfillment);

                newGlobalIdApprovals.Add(new GlobalIdApprovalModel
                {
                    GlobalId = globalId,
                    ApprovalId = model.Id,
                    CreatedDate = DateTime.UtcNow,
                    RowVersion = model.GlobalIdApprovals?.SingleOrDefault(ga => ga.GlobalId == globalId)?.RowVersion // Avoid concurrency exception
                });
            }

            model.GlobalIdApprovals = newGlobalIdApprovals;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string userName, string affiliates, long? organizationId = 0, string userRole = "", string statisticKey = "")
        {

            DataSourceResult result = null;
            IQueryable<BuyerApprovalQueryModel> query;
            string sql = @"SELECT	BA.Id,
		                            BA.Reference, 
		                            BA.Customer, 
		                            BA.[Owner], 
		                            BA.Stage,
		                            CASE	WHEN BA.Stage = 10 THEN 'label.pending'
				                            WHEN BA.Stage = 20 THEN 'label.approved'
				                            WHEN BA.Stage = 30 THEN 'label.rejected'
				                            WHEN BA.Stage = 40 THEN 'label.cancelled'
				                            WHEN BA.Stage = 50 THEN 'label.overdue'
		                            ELSE '' END AS [stageName],
		                            POFF.Id AS [POFulfillmentId],
		                            POff.Number AS [POFulfillmentNumber],
		                            POFF.FulfillmentType AS [POFulfillmentType],
		                            (SELECT TOP (1) POFFC.CompanyName FROM POfulfillmentContacts POFFC WITH (NOLOCK) WHERE POFF.Id = POFFC.POFulfillmentId AND POFFC.OrganizationRole = 'Supplier') AS [POFulfillmentSupplier]
                            FROM BuyerApprovals BA WITH (NOLOCK)
                            INNER JOIN POFulfillments POFF WITH (NOLOCK) ON BA.POFulfillmentId = POFF.Id";


            if (isInternal)
            {
                if (statisticKey.Equals("pendingBooking", StringComparison.OrdinalIgnoreCase))
                {
                    switch (userRole)
                    {
                        case "CSR":
                            sql += @$" WHERE POFF.Stage = {(int)POFulfillmentStage.ForwarderBookingRequest} AND POFF.Status = {(int)POFulfillmentStatus.Active} AND BA.Stage = {(int)BuyerApprovalStage.Pending}";
                            break;
                        default:
                            break;
                    }
                }
                query = _dataQuery.GetQueryable<BuyerApprovalQueryModel>(sql);
                result = await query.ToDataSourceResultAsync(request);
            }
            else
            {

                string organizationIds = string.Empty;
                if (!string.IsNullOrEmpty(affiliates))
                {
                    var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                    organizationIds = string.Join(",", listOfAffiliates);
                }

                if (statisticKey == "pendingBooking")
                {
                    switch (userRole)
                    {
                        case "Principal":
                            sql += @$"
                                      WHERE POFF.Stage = {(int)POFulfillmentStage.ForwarderBookingRequest} AND POFF.Status = {(int)POFulfillmentStatus.Active} AND BA.Stage = {(int)BuyerApprovalStage.Pending}
                                            AND  ((BA.ApproverSetting = 10 AND BA.ApproverOrgId IN (SELECT value FROM [dbo].[fn_SplitStringToTable]('{organizationIds}',','))) 
                                            OR (BA.ApproverSetting = 20 AND '{userName}' IN (SELECT TRIM(value) FROM [dbo].[fn_SplitStringToTable](BA.ApproverUser, ',')))) 
                                    ";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    sql += @"
                                WHERE (BA.ApproverSetting = 10 AND BA.ApproverOrgId IN (SELECT value FROM [dbo].[fn_SplitStringToTable]({0}, ',')))
		                            OR (BA.ApproverSetting = 20 AND {1} IN (SELECT TRIM(value) FROM [dbo].[fn_SplitStringToTable](BA.ApproverUser, ',')))
                        ";
                }

                query = _dataQuery.GetQueryable<BuyerApprovalQueryModel>(sql, organizationIds, userName);
                result = await query.ToDataSourceResultAsync(request);
            }

            return result;
        }

        public async Task<BuyerApprovalViewModel> GetAsync(long id, bool isInternal, string userName, string affiliates)
        {
            var listOfAffiliates = new List<long>();

            var model = await Repository.GetListQueryable(x
                => x.Include(y => y.POFulfillment)
                .ThenInclude(y => y.Contacts)
                .Include(y => y.POFulfillment)
                .ThenInclude(y => y.Orders)).FirstOrDefaultAsync(p => p.Id == id);
            if (!isInternal)
            {
                bool isAccessible = true;

                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                if (model.ApproverSetting == ApproverSettingType.AnyoneInOrganization)
                {
                    isAccessible = listOfAffiliates.Contains(model.ApproverOrgId.Value);
                }
                if (model.ApproverSetting == ApproverSettingType.SpecifiedApprover)
                {
                    isAccessible = model.ApproverUser.Split(Seperator.COMMA, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Contains(userName);
                }

                if (!isAccessible)
                {
                    return null;
                }
            }
            var result = Mapper.Map<BuyerApprovalViewModel>(model);
            return result;
        }

        public async Task<BuyerApprovalViewModel> ApproveAsync(long id, BuyerApprovalViewModel viewModel, string userName)
        {
            UnitOfWork.BeginTransaction();

            var model = await this.Repository.GetAsync(x => x.Id == id, null,
                x => x.Include(y => y.POFulfillment).ThenInclude(y => y.Orders)
                );

            if (model == null && model.Stage != BuyerApprovalStage.Pending && model.POFulfillment.Stage != POFulfillmentStage.Draft)
            {
                throw new AppEntityNotFoundException($"Object with the id {id} not found!");
            }

            model.Stage = BuyerApprovalStage.Approved;
            model.Status = BuyerApprovalStatus.Active;
            model.Owner = userName;
            model.POFulfillment.Stage = POFulfillmentStage.ForwarderBookingRequest;
            model.ExceptionDetail = viewModel.ExceptionDetail;
            model.ResponseOn = DateTime.UtcNow;

            var event1058 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1058,
                POFulfillmentId = model.POFulfillment.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName,
                Remark = model.ExceptionDetail
            };
            await _activityService.TriggerAnEvent(event1058);

            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();

            // then proceed more actions on purchase order fulfillment
            if (viewModel.POFulfillmentId.HasValue)
            {
                await POFulfillmentService.ProceedBookingForPurchaseOrderFulfillment(viewModel.POFulfillmentId.Value, userName, ActionCalledFrom.ApprovedByUser);
            }

            UnitOfWork.CommitTransaction();

            viewModel = Mapper.Map<BuyerApprovalViewModel>(model);
            return viewModel;
        }

        public async Task<BuyerApprovalViewModel> RejectAsync(long id, BuyerApprovalViewModel viewModel, string userName)
        {
            Func<IQueryable<BuyerApprovalModel>, IQueryable<BuyerApprovalModel>> includeProperties = x
                => x.Include(m => m.POFulfillment)
                   .ThenInclude(m => m.BookingRequests)
                   .Include(m => m.POFulfillment)
                   .ThenInclude(m => m.Shipments)
                   .Include(m => m.POFulfillment)
                   .ThenInclude(m => m.Contacts)
                   .Include(m => m.POFulfillment)
                   .ThenInclude(m => m.Loads);

            var model = await this.Repository.GetAsync(x => x.Id == id, null, includeProperties);

            if (model == null && model.Stage != BuyerApprovalStage.Pending)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            model.Stage = BuyerApprovalStage.Rejected;
            model.Status = BuyerApprovalStatus.Active;
            model.Owner = userName;
            model.ExceptionDetail = viewModel.ExceptionDetail;
            model.POFulfillment.Stage = POFulfillmentStage.Draft;
            model.POFulfillment.IsRejected = true;
            model.POFulfillment.BookingDate = null;
            model.ResponseOn = DateTime.UtcNow;

            var bookingRequest = model.POFulfillment.BookingRequests.SingleOrDefault(x => x.Status == POFulfillmentBookingRequestStatus.Active);
            var shipment = model.POFulfillment.Shipments.FirstOrDefault(x => x.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase));
            var supplier = model.POFulfillment.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Supplier);
            var shipper = model.POFulfillment.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Shipper);
            var consignee = model.POFulfillment.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Consignee);

            //collect all equipment types of the booking
            var equipmentTypes = model.POFulfillment.Loads?.Select(x => EnumHelper<EquipmentType>.GetDisplayDescription(x.EquipmentType)).ToList();

            var event1059 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1059,
                POFulfillmentId = model.POFulfillment.Id,
                Remark = model.ExceptionDetail,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = userName
            };
            await _activityService.TriggerAnEvent(event1059);

            this.Repository.Update(model);

            if (viewModel.POFulfillmentId.HasValue)
            {
                _poFulfillmentServiceLazy = _services.GetRequiredService<IPOFulfillmentService>();
                // Release purchase order line items quantity (booked and balance)
                _poFulfillmentServiceLazy.ReleaseQuantityOnPOLineItems(viewModel.POFulfillmentId.Value);

                // Update stage of related purchase orders
                await _poFulfillmentServiceLazy.UpdatePurchaseOrderStageByPOFFAsync(viewModel.POFulfillmentId.Value);
            }

            await this.UnitOfWork.SaveChangesAsync();
            var ownerInfo = await _userProfileService.GetAsync(model.POFulfillment.CreatedBy);
            var mailCCList = new List<string> { supplier.ContactEmail };
            var emailModel = new POFulfillmentEmailViewModel()
            {
                Name = ownerInfo.Name,
                BookingRefNumber = model.POFulfillment.Number,
                Shipper = shipper?.CompanyName,
                Consignee = consignee?.CompanyName,
                ShipFrom = model.POFulfillment.ShipFromName,
                ShipTo = model.POFulfillment.ShipToName,
                CargoReadyDate = model.POFulfillment.CargoReadyDate,
                EquipmentTypes = equipmentTypes,
                DetailPage = $"{_appConfig.ClientUrl}/po-fulfillments/view/{model.POFulfillment.Id}",
                SupportEmail = _appConfig.SupportEmail
            };
            _queuedBackgroundJobs.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync($"Booking has been rejected #{model.POFulfillment.Id}", "POFulfillment_Rejected", emailModel,
                ownerInfo.Email, mailCCList, $"Shipment Portal: Booking has been rejected ({model.POFulfillment.Number} - {model.POFulfillment.ShipFromName})"));

            // Send push notification
            await _notificationService.PushNotificationSilentAsync(ownerInfo.OrganizationId ?? 0, new NotificationViewModel
            {
                MessageKey = $"~notification.msg.bookingNo~ <span class=\"k-link\">{model.POFulfillment.Number}</span> ~notification.msg.hasBeenRejected~.",
                ReadUrl = $"/po-fulfillments/view/{model.POFulfillment.Id}"
            });

            viewModel = Mapper.Map<BuyerApprovalViewModel>(model);
            return viewModel;
        }
    }
}