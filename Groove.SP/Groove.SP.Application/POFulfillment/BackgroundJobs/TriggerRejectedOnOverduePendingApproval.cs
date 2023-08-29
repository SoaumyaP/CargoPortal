using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Groove.SP.Core.Models;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using System.ComponentModel;

namespace Groove.SP.Application.POFulfillment
{
    public class TriggerRejectedOnOverduePendingApproval
    {
        private readonly IBuyerApprovalRepository _buyerApprovalRepository;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IActivityService _activityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IPOFulfillmentService _pOFulfillmentService;

        public TriggerRejectedOnOverduePendingApproval(IBuyerApprovalRepository buyerApprovalRepository,
            IPOFulfillmentRepository poFulfillmentRepository,
            IActivityService activityService,
            IPOFulfillmentService pOFulfillmentService,
            IUnitOfWorkProvider unitOfWorkProvider)
        {
            _buyerApprovalRepository = buyerApprovalRepository;
            _poFulfillmentRepository = poFulfillmentRepository;
            _activityService = activityService;
            _pOFulfillmentService = pOFulfillmentService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWorkForBackgroundJob();
        }

        [DisplayName("Trigger Rejected On Overdue #{0}")]
        public async Task ExecuteAsync(long poFulfillmentId, long pendingApprovalId)
        {
            var pendingApproval = await _buyerApprovalRepository.GetAsync(b => b.Id == pendingApprovalId);

            if (pendingApproval == null)
            {
                throw new AppEntityNotFoundException($"Pending Approval with the id {string.Join(", ", pendingApprovalId)} not found!");
            }

            //Important: if the pending approval is not pending anymore, finish job, do nothing.
            if (pendingApproval.Stage != BuyerApprovalStage.Pending)
            {
                return;
            }

            pendingApproval.Stage = BuyerApprovalStage.Rejected;
            pendingApproval.ExceptionDetail = "Overdue";

            var poff = await _poFulfillmentRepository.GetAsync(x => x.Id == pendingApproval.POFulfillmentId,
                null, i => i.Include(x => x.Itineraries)
                            .Include(x => x.BookingRequests)
                            .Include(x => x.Orders));

            if (poff == null)
            {
                throw new AppEntityNotFoundException($"Booking with the id {string.Join(", ", pendingApproval.POFulfillmentId)} not found!");
            }

            poff.Stage = POFulfillmentStage.Draft;
            poff.IsRejected = true;
            poff.IsForwarderBookingItineraryReady = false;
            poff.BookingDate = null;

            // Release balance quantity from PO line items
            _pOFulfillmentService.ReleaseQuantityOnPOLineItems(poff.Id);

            // Update purchase order stage
            await _pOFulfillmentService.UpdatePurchaseOrderStageByPOFFAsync(poff);

            // Event EVENT_1060
            var event1060 = new ActivityViewModel()
            {
                ActivityCode = Event.EVENT_1060,
                POFulfillmentId = poff.Id,
                ActivityDate = DateTime.UtcNow,
                CreatedBy = AppConstant.SYSTEM_USERNAME
            };
            await _activityService.TriggerAnEvent(event1060);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
