using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.BuyerApproval.ViewModels;
using Groove.SP.Application.BookingValidationLog.ViewModels;
using Groove.SP.Infrastructure.CSFE.Models;
using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.Activity.ViewModels;
using Event = Groove.SP.Core.Models.Event;
using System.ComponentModel.DataAnnotations;
using MovementType = Groove.SP.Core.Models.MovementType;
using Groove.SP.Application.Common;
using Hangfire;

namespace Groove.SP.Application.POFulfillment.Services
{
    public partial class POFulfillmentService
    {      

        public async Task<POFulfillmentViewModel> ValidateBookingAsync(POFulfillmentModel poFulfillment, string userName, string companyName)
        {

            if (poFulfillment == null || poFulfillment.Stage != POFulfillmentStage.Draft)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }
            var customer = poFulfillment.Contacts.Where(x => x.OrganizationRole == Core.Models.OrganizationRole.Principal).First();
            var supplier = poFulfillment.Contacts.Where(x => x.OrganizationRole == Core.Models.OrganizationRole.Supplier).First();
            var buyerCompliance = await _buyerComplianceService.GetByOrgIdAsync(customer.OrganizationId);

            bool isMatched = false;
            ValidationResultType validationResult = ValidationResultType.BookingAccepted;
            if (buyerCompliance.BookingPolicies.Count > 0)
            {
                var allCountryLocations = (await _csfeApiClient.GetAllCountryLocationsAsync()).ToList();

                var allCarriers = (await _csfeApiClient.GetAllCarriesAsync()).ToList();

                var poLineIds = poFulfillment.Orders.Select(x => x.POLineItemId);
                var poLineItems = await _poLineItemRepository.Query(x => poLineIds.Contains(x.Id)).ToListAsync();
                foreach (var policy in buyerCompliance.BookingPolicies)
                {
                    var policyCheckResult = await CheckPolicyAsync(policy, poFulfillment, allCountryLocations, allCarriers, buyerCompliance, poLineItems);
                    if (policyCheckResult.All(x => x.IsMatched))
                    {
                        isMatched = true;
                        validationResult = await ApplyPolicyAsync(policy.Action, policy.ApproverSetting, policy.ApproverUser, buyerCompliance, userName, companyName, poFulfillment, customer, supplier, policy.Name, policy.Id, policyCheckResult);
                        break;
                    }
                    else
                    {
                        var errorList = policyCheckResult.Where(x => x.IsMismatched).Select(x => x.Details);
                        // remove loop object to convert to json string 
                        var poFulfillmentJson = Mapper.Map<POFulfillmentViewModel>(poFulfillment);
                        foreach (var item in poFulfillmentJson.BuyerApprovals)
                        {
                            item.POFulfillment = null;
                        }
                        var policyJson = Mapper.Map<BookingPolicyViewModel>(policy);

                        var bookingValidation = new BookingValidationLogViewModel()
                        {
                            POFulfillmentId = poFulfillment.Id,
                            POFulfillmentNumber = poFulfillment.Number,
                            Version = poFulfillment.RowVersion.ToString(),
                            Customer = customer.CompanyName,
                            Supplier = supplier.CompanyName,
                            SubmittedBy = userName,
                            SubmissionDate = DateTime.UtcNow,
                            PolicyID = policy.Id,
                            FulfillmentJSON = JsonConvert.SerializeObject(poFulfillmentJson),
                            PolicyJSON = JsonConvert.SerializeObject(policyJson),
                            BuyerComplianceId = buyerCompliance.Id,
                            ActionType = EnumHelper<ValidationResultType>.GetDisplayName(policy.Action),
                            ActionActivity = null,
                            ActionDatetime = poFulfillment.BookingDate.Value,
                            ActivityRemark = string.Join("; ", errorList)
                        };
                        await _bookingValidationLogService.CreateAsync(bookingValidation);
                    }
                }

            }
            // policy
            if (!isMatched)
            {
                validationResult = await ApplyPolicyAsync(buyerCompliance.BookingPolicyAction, buyerCompliance.BookingApproverSetting, buyerCompliance.BookingApproverUser, buyerCompliance, userName, companyName, poFulfillment, customer, supplier, "Default Policy");
            }
            Repository.Update(poFulfillment);
            await UnitOfWork.SaveChangesAsync();
            var poFulfillmentViewModel = Mapper.Map<POFulfillmentViewModel>(poFulfillment);
            poFulfillmentViewModel.BookingRequestResult = validationResult;
            return poFulfillmentViewModel;
        }

        public async Task<BookingValidationResult> ValidateImportingBookingAsync(POFulfillmentModel poFulfillment, BuyerComplianceModel buyerCompliance)
        {
            var customer = poFulfillment.Contacts.Where(x => x.OrganizationRole == Core.Models.OrganizationRole.Principal).First();
            var supplier = poFulfillment.Contacts.Where(x => x.OrganizationRole == Core.Models.OrganizationRole.Supplier).First();

            if (buyerCompliance.BookingPolicies.Count > 0)
            {
                var allCountryLocations = (await _csfeApiClient.GetAllCountryLocationsAsync()).ToList();
                var allCarriers = (await _csfeApiClient.GetAllCarriesAsync()).ToList();
                var poLineIds = poFulfillment.Orders.Select(x => x.POLineItemId);
                var poLineItems = await _poLineItemRepository.Query(x => poLineIds.Contains(x.Id)).ToListAsync();

                foreach (var policy in buyerCompliance.BookingPolicies)
                {
                    var policyCheckResult = await CheckPolicyAsync(policy, poFulfillment, allCountryLocations, allCarriers, buyerCompliance, poLineItems);
                    if (policyCheckResult.All(x => x.IsMatched))
                    {
                        var matchedPolicy = new BookingValidationResult
                        {
                            ActionType = policy.Action,
                            PolicyCheckResults = policyCheckResult
                        };
                        return matchedPolicy;
                    }
                }

            }

            // policy
            var matchedCompliance = new BookingValidationResult
            {
                ActionType = buyerCompliance.BookingPolicyAction,
                PolicyCheckResults = null
            };

            return matchedCompliance;
        }

        public async Task<BookingValidationResult> TrialValidateBookingAsync(long poffId, IdentityInfo currentUser)
        {
            
            var poFulfillment = await Repository.GetAsync(x => x.Id == poffId,
               null,
               x
               => x.Include(m => m.Contacts)
                   .Include(m => m.Loads)
                   .ThenInclude(i => i.Details)
                   .Include(m => m.Orders)
                   .Include(m => m.CargoDetails)
                   .Include(m => m.BookingRequests)
                   .Include(m => m.BuyerApprovals)
                   .Include(m => m.Itineraries)
                   .Include(m => m.Shipments));

            if (poFulfillment == null || poFulfillment.Stage != POFulfillmentStage.Draft)
            {
                throw new AppEntityNotFoundException($"Object with the id {poffId} not found!");
            }

            // Check purchase order fulfillment data first, before creating booking
            await CheckPOFFDataBeforeCreatingBookingAsync(poFulfillment, currentUser);

            var customer = poFulfillment.Contacts.Where(x => x.OrganizationRole == Core.Models.OrganizationRole.Principal).First();
            var supplier = poFulfillment.Contacts.Where(x => x.OrganizationRole == Core.Models.OrganizationRole.Supplier).First();
            var buyerCompliance = await _buyerComplianceService.GetByOrgIdAsync(customer.OrganizationId);
            
            if (buyerCompliance.BookingPolicies.Count > 0)
            {
                var allCountryLocations = (await _csfeApiClient.GetAllCountryLocationsAsync()).ToList();
                var allCarriers = (await _csfeApiClient.GetAllCarriesAsync()).ToList();
                var poLineIds = poFulfillment.Orders.Select(x => x.POLineItemId);
                var poLineItems = await _poLineItemRepository.Query(x => poLineIds.Contains(x.Id)).ToListAsync();

                foreach (var policy in buyerCompliance.BookingPolicies)
                {
                    var policyCheckResult = await CheckPolicyAsync(policy, poFulfillment, allCountryLocations, allCarriers, buyerCompliance, poLineItems);
                    if (policyCheckResult.All(x => x.IsMatched))
                    {
                        var matchedPolicy = new BookingValidationResult
                        {
                            ActionType = policy.Action,
                            PolicyCheckResults = policyCheckResult
                        };
                        return matchedPolicy;
                    }                    
                }

            }

            // policy
            var matchedCompliance = new BookingValidationResult
            {
                ActionType = buyerCompliance.BookingPolicyAction,
                PolicyCheckResults = null
            };

            return matchedCompliance;

        }

        private async Task<ValidationResultType> ApplyPolicyAsync(ValidationResultType validationResultType, ApproverSettingType bookingApproverSetting, string bookingApproverUser, BuyerComplianceModel buyerCompliance,
                string userName, string companyName, POFulfillmentModel poFulfillment, POFulfillmentContactModel customer, POFulfillmentContactModel supplier, string policyName, long? policyId = null, List<BookingPolicyCheckResult> bookingPolicyCheckResults = null)
        {
            switch (validationResultType)
            {
                case ValidationResultType.PendingForApproval:
                    poFulfillment.IsRejected = false;
                    poFulfillment.Stage = POFulfillmentStage.ForwarderBookingRequest;

                    // Adjust subtract purchase order line items linking to purchase order fulfillment
                    AdjustQuantityOnPOLineItems(poFulfillment.Id, AdjustBalanceOnPOLineItemsType.Deduct);

                    var failureReasons = bookingPolicyCheckResults?.Where(x => x.Criteria == CheckCriteria.BookingAccuracy && x.IsMatched).Select(x => x.Details).ToList();
                    var reason = "";
                    if (failureReasons != null && failureReasons.Any())
                    {
                        reason = string.Join($",{Environment.NewLine}", failureReasons);
                    }
                    var buyerApprovalViewModel = new BuyerApprovalViewModel()
                    {
                        POFulfillmentId = poFulfillment.Id,
                        Reference = "",
                        Owner = "",
                        CreatedDate = DateTime.UtcNow,
                        ExceptionType = ExceptionType.POFulfillmentException,
                        Customer = customer.CompanyName,
                        ApproverSetting = bookingApproverSetting,
                        RequestByOrganization = companyName,
                        Requestor = poFulfillment.CreatedBy,
                        ExceptionActivity = "Booking Approval Request",
                        ActivityDate = DateTime.UtcNow,
                        AlertNotificationFrequencyType = buyerCompliance.ApprovalAlertFrequency,
                        Severity = "",
                        ExceptionDetail = "",
                        Stage = BuyerApprovalStage.Pending,
                        Status = BuyerApprovalStatus.Active,
                        Transaction = CommonHelper.GenerateGlobalId(poFulfillment.Id, EntityType.POFullfillment),
                        Reason = reason
                    };

                    var dueOnDate = DateTime.UtcNow.AddHours((int)buyerCompliance.ApprovalDuration);

                    if (DateTime.Compare(dueOnDate, DateTime.UtcNow) > 0)
                    {
                        buyerApprovalViewModel.DueOnDate = dueOnDate;
                    }

                    if (buyerApprovalViewModel.ApproverSetting == ApproverSettingType.AnyoneInOrganization)
                    {
                        buyerApprovalViewModel.ApproverOrgId = buyerCompliance.OrganizationId;
                    }
                    else
                    {
                        buyerApprovalViewModel.ApproverUser = bookingApproverUser;
                    }

                    var pendingApproval = await BuyerApprovalService.CreateAsync(buyerApprovalViewModel);

                    var event1051 = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1051,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = userName
                    };
                    await _activityService.TriggerAnEvent(event1051);

                    var event1055PFA = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1055,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = AppConstant.SYSTEM_USERNAME
                    };
                    await _activityService.TriggerAnEvent(event1055PFA);

                    var purchaseOrderListId = poFulfillment.Orders.Select(x => x.PurchaseOrderId).ToList();
                    var purchaseOrderList = await _purchaseOrderRepository.Query(x => purchaseOrderListId.Any(y => x.Id == y)).ToListAsync();
                    foreach (var singlePO in purchaseOrderList)
                    {
                        if (singlePO.Stage == POStageType.Released)
                        {
                            singlePO.Stage = POStageType.ForwarderBookingRequest;
                        }

                    }
                    _purchaseOrderRepository.UpdateRange(purchaseOrderList.ToArray());

                    #region Update stage for related allocated/blanket POs

                    switch (poFulfillment.FulfilledFromPOType)
                    {
                        case POType.Bulk:
                            break;
                        case POType.Blanket:
                            // If it is blanket, must update stage for all allocated POs
                            var allocatedPOs = await _purchaseOrderRepository.Query(x =>
                                                x.POType == POType.Allocated
                                                && x.BlanketPOId != null
                                                && x.Stage == POStageType.Released
                                                && purchaseOrderListId.Any(y => y == x.BlanketPOId.Value)).ToListAsync();
                            if (allocatedPOs != null && allocatedPOs.Any())
                            {
                                foreach (var allocatedPO in allocatedPOs)
                                {
                                    allocatedPO.Stage = POStageType.ForwarderBookingRequest;
                                }
                            }
                            _purchaseOrderRepository.UpdateRange(allocatedPOs.ToArray());
                            break;
                        case POType.Allocated:
                            // If it is allocated purchase order fulfillment
                            // Update stage on related blanket POs
                            purchaseOrderListId = poFulfillment.Orders.Select(x => x.PurchaseOrderId).Distinct().ToList();
                            var blanketPOIds = _purchaseOrderRepository.QueryAsNoTracking(x =>
                                                   x.POType == POType.Allocated
                                                   && x.BlanketPOId != null
                                                   && purchaseOrderListId.Any(y => y == x.Id)).Select(x => x.BlanketPOId).Distinct().ToList();

                            var blanketPOs = await _purchaseOrderRepository.Query(x =>
                                                    x.POType == POType.Blanket
                                                    && x.BlanketPOId == null
                                                    && x.Stage == POStageType.Released
                                                    && blanketPOIds.Any(y => y == x.Id)).ToListAsync();
                            if (blanketPOs != null && blanketPOs.Any())
                            {
                                foreach (var blanketPO in blanketPOs)
                                {
                                    blanketPO.Stage = POStageType.ForwarderBookingRequest;
                                }
                                _purchaseOrderRepository.UpdateRange(blanketPOs.ToArray());
                            }
                            break;
                        default:
                            break;
                    }

                    #endregion

                    _queuedBackgroundJobs.Enqueue<ApprovalNotificationEmailToApprover>(j => j.ExecuteAsync(pendingApproval.Id, (int)buyerCompliance.ApprovalAlertFrequency));
                    if (buyerCompliance.ApprovalDuration != ApprovalDurationType.NoExpiration)
                    {
                        _queuedBackgroundJobs.Schedule<TriggerRejectedOnOverduePendingApproval>(j => j.ExecuteAsync(pendingApproval.POFulfillmentId.Value, pendingApproval.Id), TimeSpan.FromHours((int)buyerCompliance.ApprovalDuration));
                    }
                    break;

                case ValidationResultType.BookingRejected:
                    poFulfillment.BookingDate = null;
                    poFulfillment.IsRejected = true;

                    var event1054 = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1054,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = AppConstant.SYSTEM_USERNAME,
                        Remark = buyerCompliance.Name + " - " + policyName
                    };
                    await _activityService.TriggerAnEvent(event1054);

                    break;
                case ValidationResultType.BookingAccepted:

                    poFulfillment.IsRejected = false;
                    poFulfillment.Stage = POFulfillmentStage.ForwarderBookingRequest;

                    var bookingAccuracyCheckResult = bookingPolicyCheckResults?.FirstOrDefault(x => x.Criteria == CheckCriteria.BookingAccuracy);

                    if (bookingAccuracyCheckResult != null)
                    {
                        var policy = buyerCompliance.BookingPolicies.FirstOrDefault(x => x.Id == policyId);

                        // for further tracking
                        var policyLog = JsonConvert.SerializeObject(new
                        {
                            PolicyName = policy?.Name,
                            FulfillmentAccuracies = policy?.FulfillmentAccuracies,
                            ShortShipTolerancePercentage = buyerCompliance?.ShortShipTolerancePercentage,
                            PolicyCheckResult = bookingAccuracyCheckResult.Details
                        });
                        BindPOFulfillmentShortshipOrders(poFulfillment, bookingAccuracyCheckResult.ShortshipOrderIds, userName, policyLog);
                    }

                    var event1051BA = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1051,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = userName
                    };
                    await _activityService.TriggerAnEvent(event1051BA);

                    var event1053 = new ActivityViewModel()
                    {
                        ActivityCode = Event.EVENT_1053,
                        POFulfillmentId = poFulfillment.Id,
                        ActivityDate = DateTime.UtcNow,
                        CreatedBy = AppConstant.SYSTEM_USERNAME
                    };
                    await _activityService.TriggerAnEvent(event1053);

                    // Adjust subtract purchase order line items linking to purchase order fulfillment
                    AdjustQuantityOnPOLineItems(poFulfillment.Id, AdjustBalanceOnPOLineItemsType.Deduct);

                    await ProceedBookingForPurchaseOrderFulfillment(poFulfillment.Id, userName, ActionCalledFrom.AcceptedByBookingValidation);

                    break;
            }
            return validationResultType;
        }

        private async Task<List<BookingPolicyCheckResult>> CheckPolicyAsync(BookingPolicyModel policy, POFulfillmentModel poFulfillment, List<CountryLocations> allCountryLocations,
            List<Carrier> allCarriers, BuyerComplianceModel buyerCompliance, List<POLineItemModel> poLineItems)
        {
            var errorList = new List<string>();
            var validationResult = new List<BookingPolicyCheckResult>();
            if (policy.IncotermSelections > 0)
            {
                if ((poFulfillment.Incoterm & policy.IncotermSelections) != poFulfillment.Incoterm)
                {
                    var incotermFailed = BookingPolicyCheckResult.FailedResult(CheckCriteria.Incoterm, poFulfillment.Incoterm.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    validationResult.Add(incotermFailed);
                }
                else
                {
                    var incotermPassed = BookingPolicyCheckResult.PassedResult(CheckCriteria.Incoterm, poFulfillment.Incoterm.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    validationResult.Add(incotermPassed);
                }
            }
            if (policy.LogisticsServiceSelections > 0)
            {
                if((poFulfillment.LogisticsService & policy.LogisticsServiceSelections) != poFulfillment.LogisticsService)
                {
                    var logisticServiceTypeFailed = BookingPolicyCheckResult.FailedResult(CheckCriteria.LogisticServiceType, poFulfillment.LogisticsService.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    validationResult.Add(logisticServiceTypeFailed);
                }
                else
                {
                    var logisticServiceTypePassed = BookingPolicyCheckResult.PassedResult(CheckCriteria.LogisticServiceType, poFulfillment.LogisticsService.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    validationResult.Add(logisticServiceTypePassed);
                }
               
            }
            if (policy.ModeOfTransports > 0 )
            {
                if((poFulfillment.ModeOfTransport & policy.ModeOfTransports) != poFulfillment.ModeOfTransport)
                {
                    var modeOfTransportFailed = BookingPolicyCheckResult.FailedResult(CheckCriteria.ModeOfTransport, poFulfillment.ModeOfTransport.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    validationResult.Add(modeOfTransportFailed);
                }
                else
                {
                    var modeOfTransportPassed = BookingPolicyCheckResult.PassedResult(CheckCriteria.ModeOfTransport, poFulfillment.ModeOfTransport.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    validationResult.Add(modeOfTransportPassed);
                }
               
            }
            if (policy.MovementTypeSelections > 0)
            {
                if((poFulfillment.MovementType & policy.MovementTypeSelections) != poFulfillment.MovementType)
                {
                    var movementTypeFailed = BookingPolicyCheckResult.FailedResult(CheckCriteria.MovementType, poFulfillment.MovementType.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    validationResult.Add(movementTypeFailed);
                }
                else
                {
                    var movementTypePassed = BookingPolicyCheckResult.PassedResult(CheckCriteria.MovementType, poFulfillment.MovementType.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    validationResult.Add(movementTypePassed);
                }
                
            }
            if (!string.IsNullOrEmpty(policy.ShipFromLocationSelections))
            {
                var poFF_country = allCountryLocations.Where(x => x.LocationIds.Contains(poFulfillment.ShipFrom)).FirstOrDefault();
                if (poFF_country != null)
                {
                    var poFF_countryId = poFF_country.CountryId.ToString();
                    var policy_ShipFrom_CountryLevel = policy.ShipFromLocationSelections.Trim(',').Split(',');
                    bool isExistsLocationInCountry = false;
                    foreach (var item in policy_ShipFrom_CountryLevel)
                    {
                        if (item == poFF_countryId || item.StartsWith(poFF_countryId + "-"))
                        {
                            isExistsLocationInCountry = true;
                            break;
                        }
                    }
                    if (isExistsLocationInCountry)
                    {
                        var shipFromPassed = BookingPolicyCheckResult.PassedResult(CheckCriteria.ShipFrom, poFulfillment.ShipFromName);
                        validationResult.Add(shipFromPassed);                      
                    }
                    else
                    {
                        var shipFromFailed = BookingPolicyCheckResult.FailedResult(CheckCriteria.ShipFrom, poFulfillment.ShipFromName);
                        validationResult.Add(shipFromFailed);
                    }
                }
            }
            if (!string.IsNullOrEmpty(policy.ShipToLocationSelections))
            {
                var poFF_country = allCountryLocations.Where(x => x.LocationIds.Contains(poFulfillment.ShipTo)).FirstOrDefault();
                if (poFF_country != null)
                {
                    var poFF_countryId = poFF_country.CountryId.ToString();
                    var policy_ShipTo_CountryLevel = policy.ShipToLocationSelections.Trim(',').Split(',');
                    bool isExistsLocationInCountry = false;
                    foreach (var item in policy_ShipTo_CountryLevel)
                    {
                        if (item == poFF_countryId || item.StartsWith(poFF_countryId + "-"))
                        {
                            isExistsLocationInCountry = true;
                            break;
                        }
                    }
                    if (isExistsLocationInCountry)
                    {
                        var shipToPassed = BookingPolicyCheckResult.PassedResult(CheckCriteria.ShipTo, poFulfillment.ShipToName);
                        validationResult.Add(shipToPassed);
                    }
                    else
                    {
                        var shipToFailed = BookingPolicyCheckResult.FailedResult(CheckCriteria.ShipTo, poFulfillment.ShipToName);
                        validationResult.Add(shipToFailed);
                    }
                }
            }
            var carrierId = poFulfillment.PreferredCarrier.ToString();
            if (!string.IsNullOrEmpty(policy.CarrierSelections))
            {
                var bookingCarrier = allCarriers.FirstOrDefault(x => x.Id == poFulfillment.PreferredCarrier);

                if(!policy.CarrierSelections.Contains(carrierId))
                {
                    var carrierFailed = BookingPolicyCheckResult.FailedResult(CheckCriteria.Carrier, bookingCarrier?.Name ?? "");
                    validationResult.Add(carrierFailed);
                }
                else
                {
                    var carrierPassed = BookingPolicyCheckResult.PassedResult(CheckCriteria.Carrier, bookingCarrier?.Name ?? "");
                    validationResult.Add(carrierPassed);
                }
               
            }

            // Fulfillment Accuracy
            if (policy.FulfillmentAccuracies > 0)
            {
                var bookingAccuracy = CheckBookingAccuracy(policy, poFulfillment, poLineItems, buyerCompliance);
                validationResult.Add(bookingAccuracy);

            }

            // Loadability Type
            // If Movement Type = CFS-XXX, ignore Loadability Type, check the 10 settings.
            if (policy.CargoLoadabilities > 0 && ((poFulfillment.MovementType & MovementType.CY_CY) == MovementType.CY_CY ||
                (poFulfillment.MovementType & MovementType.CY_CFS) == MovementType.CY_CFS))
            {
                var bookingLoadability = CheckLoadabilityType(policy, poFulfillment, buyerCompliance);
                validationResult.Add(bookingLoadability);
            }

            // Booking Timeless
            if (policy.BookingTimeless > 0)
            {
                var bookingTimeless = await CheckBookingTimelessAsync(policy, poFulfillment, buyerCompliance);
                validationResult.Add(bookingTimeless);

            }

            // Itinerary
            //if(policy.ItineraryIsEmpty != null && policy.ItineraryIsEmpty == ItineraryIsEmptyType.Yes)
            //{
            //    errorList.Add("Itinerary");
            //}            

            return validationResult;
        }

        private async Task<BookingPolicyCheckResult> CheckBookingTimelessAsync(BookingPolicyModel policy, POFulfillmentModel poFulfillment, BuyerComplianceModel buyerCompliance)
        {
            // In case trial validate booking, booking date is still null;
            DateTime bookingDate = poFulfillment.BookingDate ?? DateTime.UtcNow;

            int deltaDays;
            if (buyerCompliance.BookingTimeless.DateForComparison == DateForComparison.ExpectedShipDate)
            {
                deltaDays = Convert.ToInt32((poFulfillment.ExpectedShipDate.Value - bookingDate).TotalDays);
            }
            else
            {
                var firstPOId = poFulfillment.Orders?.Min(x => x.PurchaseOrderId);
                if (!firstPOId.HasValue)
                {
                    throw new ApplicationException("The ex-work date is not available for booking validation.");
                }
                var firstPO = await _purchaseOrderRepository.GetAsNoTrackingAsync(x => x.Id == firstPOId);

                if (!firstPO.CargoReadyDate.HasValue)
                {
                    throw new ApplicationException("The ex-work date is not available for booking validation.");
                }
                deltaDays = Convert.ToInt32((firstPO.CargoReadyDate.Value - bookingDate).TotalDays);
            }
            if (poFulfillment.ModeOfTransport == ModeOfTransportType.Air)
            {
                if ((policy.BookingTimeless & BookingTimelessType.EarlyBooking) == BookingTimelessType.EarlyBooking && deltaDays > buyerCompliance.BookingTimeless.AirEarlyBookingTimeless.Value)
                {
                    return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.EarlyBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                }
                if ((policy.BookingTimeless & BookingTimelessType.OntimeBooking) == BookingTimelessType.OntimeBooking &&
                    (deltaDays >= buyerCompliance.BookingTimeless.AirLateBookingTimeless.Value && deltaDays <= buyerCompliance.BookingTimeless.AirEarlyBookingTimeless.Value))
                {
                    return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.OntimeBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));

                }
                if ((policy.BookingTimeless & BookingTimelessType.LateBooking) == BookingTimelessType.LateBooking && deltaDays < buyerCompliance.BookingTimeless.AirLateBookingTimeless.Value)
                {
                    return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.LateBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));

                }
            }
            else
            {
                if (poFulfillment.MovementType == MovementType.CY_CY || poFulfillment.MovementType == MovementType.CY_CFS)
                {
                    if ((policy.BookingTimeless & BookingTimelessType.EarlyBooking) == BookingTimelessType.EarlyBooking && deltaDays > buyerCompliance.BookingTimeless.CyEarlyBookingTimeless.Value)
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.EarlyBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    }
                    if ((policy.BookingTimeless & BookingTimelessType.OntimeBooking) == BookingTimelessType.OntimeBooking &&
                        (deltaDays >= buyerCompliance.BookingTimeless.CyLateBookingTimeless.Value) && deltaDays <= buyerCompliance.BookingTimeless.CyEarlyBookingTimeless.Value)
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.OntimeBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));

                    }
                    if ((policy.BookingTimeless & BookingTimelessType.LateBooking) == BookingTimelessType.LateBooking && deltaDays < buyerCompliance.BookingTimeless.CyLateBookingTimeless.Value)
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.LateBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));

                    }
                }
                if (poFulfillment.MovementType == MovementType.CFS_CY || poFulfillment.MovementType == MovementType.CFS_CFS)
                {
                    if ((policy.BookingTimeless & BookingTimelessType.EarlyBooking) == BookingTimelessType.EarlyBooking && deltaDays > buyerCompliance.BookingTimeless.CfsEarlyBookingTimeless.Value)
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.EarlyBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));
                    }
                    if ((policy.BookingTimeless & BookingTimelessType.OntimeBooking) == BookingTimelessType.OntimeBooking &&
                        (deltaDays >= buyerCompliance.BookingTimeless.CfsLateBookingTimeless.Value && deltaDays <= buyerCompliance.BookingTimeless.CfsEarlyBookingTimeless.Value))
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.OntimeBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));

                    }
                    if ((policy.BookingTimeless & BookingTimelessType.LateBooking) == BookingTimelessType.LateBooking && deltaDays < buyerCompliance.BookingTimeless.CfsLateBookingTimeless.Value)
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.BookingTimeless, BookingTimelessType.LateBooking.GetAttributeValue<DisplayAttribute, string>(x => x.Description));

                    }
                }
            }
            return BookingPolicyCheckResult.FailedResult(CheckCriteria.BookingTimeless, null);
        }

        private BookingPolicyCheckResult CheckLoadabilityType(BookingPolicyModel policy, POFulfillmentModel poFulfillment, BuyerComplianceModel buyerCompliance)
        {
            foreach (var load in poFulfillment.Loads)
            {
                var complianceLoad = buyerCompliance.CargoLoadabilities.Where(x => x.EquipmentType == load.EquipmentType).FirstOrDefault();
                if (complianceLoad != null)
                {
                    var message = $"{load.EquipmentType.GetAttributeValue<DisplayAttribute, string>(x => x.Description)} ({load.PlannedVolume:N3} CBM)";
                    if ((policy.CargoLoadabilities & CargoLoadabilityType.LightLoaded) == CargoLoadabilityType.LightLoaded &&
                        load.PlannedVolume < complianceLoad.CyMinimumCBM)
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.CargoLoadability, message);
                    }
                    if ((policy.CargoLoadabilities & CargoLoadabilityType.NormalLoaded) == CargoLoadabilityType.NormalLoaded &&
                        (load.PlannedVolume >= complianceLoad.CyMinimumCBM && load.PlannedVolume <= complianceLoad.CyMaximumCBM))
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.CargoLoadability, message);

                    }
                    if ((policy.CargoLoadabilities & CargoLoadabilityType.OverLoaded) == CargoLoadabilityType.OverLoaded &&
                        load.PlannedVolume > complianceLoad.CyMaximumCBM)
                    {
                        return BookingPolicyCheckResult.PassedResult(CheckCriteria.CargoLoadability, message);

                    }
                }
            }
            return BookingPolicyCheckResult.FailedResult(CheckCriteria.CargoLoadability, null);
        }

        private BookingPolicyCheckResult CheckBookingAccuracy(BookingPolicyModel policy, POFulfillmentModel poFulfillment, List<POLineItemModel> poLineItems, BuyerComplianceModel buyerCompliance)
        {
            var result = BookingPolicyCheckResult.FailedResult(CheckCriteria.BookingAccuracy, null);
            foreach (var order in poFulfillment.Orders)
            {
                var poLineItem = poLineItems.Where(x => x.Id == order.POLineItemId).FirstOrDefault();
                if (poLineItem != null)
                {
                    var min = poLineItem.BalanceUnitQty - (buyerCompliance.ShortShipTolerancePercentage * poLineItem.BalanceUnitQty);
                    var max = poLineItem.BalanceUnitQty + (buyerCompliance.OvershipTolerancePercentage * poLineItem.BalanceUnitQty);

                    // light 
                    if ((policy.FulfillmentAccuracies & FulfillmentAccuracyType.ShortShipment) == FulfillmentAccuracyType.ShortShipment &&
                        order.FulfillmentUnitQty < min)
                    {
                        var details = $"(PO#{order.CustomerPONumber}, Item#{order.ProductCode}): Booked {order.FulfillmentUnitQty:N0}/{poLineItem.BalanceUnitQty:N0} pcs";
                        result.Result = CheckResult.Matched;
                        result.AddDetails(details);
                        result.AddShortshipOrder(order.Id, order.ProductCode);
                        result.StoreLog = true;
                        continue;
                    }
                    // normal 
                    if ((policy.FulfillmentAccuracies & FulfillmentAccuracyType.NormalShipment) == FulfillmentAccuracyType.NormalShipment &&
                        (order.FulfillmentUnitQty >= min && order.FulfillmentUnitQty <= max))
                    {
                        var details = $"(PO#{order.CustomerPONumber}, Item#{order.ProductCode}): Booked {order.FulfillmentUnitQty:N0}/{poLineItem.BalanceUnitQty:N0} pcs";
                        result.Result = CheckResult.Matched;
                        result.AddDetails(details);
                        result.StoreLog = true;
                        continue;

                    }
                    // over
                    if ((policy.FulfillmentAccuracies & FulfillmentAccuracyType.OverShipment) == FulfillmentAccuracyType.OverShipment &&
                        order.FulfillmentUnitQty > max)
                    {
                        var details = $"(PO#{order.CustomerPONumber}, Item#{order.ProductCode}): Booked {order.FulfillmentUnitQty:N0}/{poLineItem.BalanceUnitQty:N0} pcs";
                        result.Result = CheckResult.Matched;
                        result.AddDetails(details);
                        result.StoreLog = true;
                        continue;

                    }
                }
            }
            return result;
        }

        private void BindPOFulfillmentShortshipOrders(POFulfillmentModel poFulfillment, List<long> shortshipOrderIds, string userName = "", string policyLog = "")
        {
            var poffOrders = poFulfillment.Orders.Where(x => shortshipOrderIds.Contains(x.Id));

            if (!poffOrders.Any())
            {
                return;
            }

            poFulfillment.ShortshipOrders = new List<POFulfillmentShortshipOrderModel>();

            foreach (var poffOrder in poffOrders)
            {
                var shortshipOrderModel = new POFulfillmentShortshipOrderModel
                {
                    POFulfillmentNumber = poFulfillment.Number,
                    PurchaseOrderId = poffOrder.PurchaseOrderId,
                    CustomerPONumber = poffOrder.CustomerPONumber,
                    ProductCode = poffOrder.ProductCode,
                    OrderedUnitQty = poffOrder.OrderedUnitQty,
                    FulfillmentUnitQty = poffOrder.FulfillmentUnitQty,
                    BalanceUnitQty = poffOrder.BalanceUnitQty,
                    BookedPackage = poffOrder.BookedPackage,
                    Volume = poffOrder.Volume,
                    GrossWeight = poffOrder.GrossWeight,
                    PolicyLog = policyLog,
                    ApprovedOn = DateTime.UtcNow,
                };
                shortshipOrderModel.Audit(userName);
                poFulfillment.ShortshipOrders.Add(shortshipOrderModel);
            }
        }

    }

    public class BookingPolicyCheckResult
    {
        private List<string> _listOfDetails;

        private List<long> _listOfShortshipOrderIds;

        [JsonIgnore]
        /// <summary>
        /// It is a type of result, Passed or Failed
        /// </summary>
        public CheckResult Result { get; set; } = CheckResult.Mismatched;

        [JsonIgnore]
        /// <summary>
        /// It is name of criteria of policy: Logistic Service Type, Mode of Transport, Booking Accuracy,...
        /// </summary>
        public CheckCriteria Criteria { get; set; }

        public string CriteriaName
        {
            get
            {
                return $"{Criteria.GetAttributeValue<DisplayAttribute, string>(x => x.Description)}";
            }
        }

        public string CriteriaTranslationName
        {
            get
            {
                return $"{Criteria.GetAttributeValue<DisplayAttribute, string>(x => x.Name)}";
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Should store Details into database?
        /// </summary>
        public bool StoreLog { get; set; }

        [JsonIgnore]
        public bool IsMismatched
        {
            get
            {
                return Result == CheckResult.Mismatched;
            }
        }

        public bool IsMatched
        {
            get
            {
                return Result == CheckResult.Matched;
            }
        }

        public string Details 
        {
            get
            {
                if(_listOfDetails == null || !_listOfDetails.Any())
                {
                    return $"{Criteria.GetAttributeValue<DisplayAttribute, string>(x => x.Description)}";
                }
                else
                {
                    return string.Join($",{Environment.NewLine}", _listOfDetails);
                }
            }
        }

        public List<long> ShortshipOrderIds
        {
            get
            {
                if (_listOfShortshipOrderIds == null)
                {
                    _listOfShortshipOrderIds = new();
                }

                return _listOfShortshipOrderIds;
            }
        }

        public void AddShortshipOrder(long id, string productCode)
        {
            if (_listOfShortshipOrderIds == null)
            {
                _listOfShortshipOrderIds = new();
            }

            this._listOfShortshipOrderIds.Add(id);
        }

        public void AddDetails(params string[] details)
        {
            if (_listOfDetails == null)
            {
                _listOfDetails = new List<string>();
            }
            _listOfDetails.AddRange(details);
        }


        public static BookingPolicyCheckResult PassedResult(CheckCriteria criteria, object details, bool? storeLog = false)
        {
            var result = new BookingPolicyCheckResult
            {
                Result = CheckResult.Matched,
                Criteria = criteria,
                _listOfDetails = new List<string>(),
                StoreLog = storeLog.Value
            };
            if (details != null)
            {
                result.AddDetails($"{details}");
            }
            return result;
        }

        public static BookingPolicyCheckResult FailedResult(CheckCriteria criteria, object details, bool? storeLog = true)
        {
            var result = new BookingPolicyCheckResult
            {
                Result = CheckResult.Mismatched,
                Criteria = criteria,
                _listOfDetails = new List<string>(),
                StoreLog = storeLog.Value
            };
            if (details != null)
            {
                result.AddDetails($"{details}");
            }
            return result;
        }
    }

    public enum CheckResult
    {
        
        [Display(Description = "Mismatched")]
        Mismatched = 0,
        [Display(Description = "Matched")]
        Matched
    }

    public enum CheckCriteria
    {
        [Display(Description = "Logistic Service Type", Name = "label.logisticsServiceType")]
        LogisticServiceType = 0,

        [Display(Description = "Mode of Transport", Name = "label.modeOfTransport")]
        ModeOfTransport,

        [Display(Description = "Movement Type", Name = "label.movementType")]
        MovementType,

        [Display(Description = "Incoterm", Name = "label.incoterm")]
        Incoterm,

        [Display(Description = "Carrier", Name = "label.carrier")]
        Carrier,

        [Display(Description = "Ship From", Name = "label.shipFrom")]
        ShipFrom,

        [Display(Description = "Ship To", Name = "label.shipTo")]
        ShipTo,

        [Display(Description = "Booking Accuracy", Name = "label.fulfillmentAccuracy")]
        BookingAccuracy,

        [Display(Description = "Loadability Type", Name = "label.cargoLoadability")]
        CargoLoadability,

        [Display(Description = "Booking Timeless", Name = "label.bookingPeriod")]
        BookingTimeless,

        [Display(Description = "Itinerary", Name = "label.itinerary")]
        Itinerary
    }

    public class BookingValidationResult
    {
        public List<BookingPolicyCheckResult> PolicyCheckResults { get; set; }
        public ValidationResultType ActionType { get; set; }

        [JsonIgnore]
        public string Message
        {
            get
            {
                if (PolicyCheckResults != null && PolicyCheckResults.Any())
                {
                    return string.Join($".{Environment.NewLine}", PolicyCheckResults.Select(x => x.Details)) + '.';
                }
                return string.Empty;
            }
        }
    }
}
