using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.FreightScheduler.Services.Interfaces;
using Groove.SP.Application.GlobalIdActivity.Services.Interfaces;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Itinerary.Services.Interfaces;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.Shipments.Services.Interfaces;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Groove.SP.Application.Itinerary.Services
{
    public class ItineraryService : ServiceBase<ItineraryModel, ItineraryViewModel>, IItineraryService
    {
        private readonly IRepository<ConsignmentModel> _consignmentRepository;
        private readonly IRepository<ConsignmentItineraryModel> _consignmentItineraryRepository;
        private readonly IRepository<ContainerItineraryModel> _containerItineraryRepository;
        private readonly IRepository<ContainerModel> _containerRepository;
        private readonly IRepository<ItineraryModel> _itineraryRepository;
        private readonly IRepository<ShipmentModel> _shipmentRepository;
        private readonly IRepository<ShipmentBillOfLadingModel> _shipmentBillOfLadingRepository;
        private readonly IRepository<MasterBillOfLadingItineraryModel> _masterBillOfLadingItineraryRepository;
        private readonly IRepository<BillOfLadingItineraryModel> _billOfLadingItineraryRepository;
        private readonly IRepository<MasterBillOfLadingModel> _masterBillRepository;
        private readonly IRepository<POFulfillmentModel> _poFulfillmentRepository;
        private readonly IBillOfLadingRepository _billOfLadingRepository;
        private readonly IBillOfLadingShipmentLoadRepository _billOfLadingShipmentLoadRepository;
        private readonly IFreightSchedulerRepository _freightSchedulerRepository;
        private readonly IFreightSchedulerService _freightSchedulerService;
        private readonly IShipmentService _shipmentService;
        private readonly IContainerService _containerService;
        private readonly IUserProfileService _userProfileService;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IActivityService _activityService;
        private readonly IActivityRepository _activityRepository;
        private readonly IServiceProvider _serviceProvider;
        private IPOFulfillmentService _poFulfillmentService;
        private readonly IDataQuery _dataQuery;

        protected override Func<IQueryable<ItineraryModel>, IQueryable<ItineraryModel>> FullIncludeProperties
        {
            get
            {
                return x => x.Include(m => m.BillOfLadingItineraries);
            }
        }

        public ItineraryService(IUnitOfWorkProvider unitOfWorkProvider,
            IRepository<ConsignmentItineraryModel> consignmentItineraryRepository,
            IRepository<ConsignmentModel> consignmentRepository,
            IRepository<ItineraryModel> itineraryRepository,
            IRepository<ContainerItineraryModel> containerItineraryRepository,
            IRepository<MasterBillOfLadingItineraryModel> masterBillOfLadingItineraryRepository,
            IRepository<BillOfLadingItineraryModel> billOfLadingItineraryRepository,
            IRepository<POFulfillmentModel> poFulfillmentRepository,
            IBillOfLadingRepository billOfLadingRepository,
            IFreightSchedulerRepository freightSchedulerRepository,
            IFreightSchedulerService freightSchedulerService,
            IShipmentService shipmentService,
            IContainerService containerService,
            IUserProfileService userProfileService,
            ICSFEApiClient csfeApiClient,
            IRepository<ContainerModel> containerRepository,
            IRepository<ShipmentModel> shipmentRepository,
            IRepository<ShipmentBillOfLadingModel> shipmentBillOfLadingRepository,
            IRepository<MasterBillOfLadingModel> masterBillRepository,
            IBillOfLadingShipmentLoadRepository billOfLadingShipmentLoadRepository,
            IActivityService activityService,
            IServiceProvider serviceProvider,
            IDataQuery dataQuery
            )
            : base(unitOfWorkProvider)
        {
            _consignmentRepository = consignmentRepository;
            _itineraryRepository = itineraryRepository;
            _consignmentItineraryRepository = consignmentItineraryRepository;
            _containerItineraryRepository = containerItineraryRepository;
            _containerRepository = containerRepository;
            _shipmentRepository = shipmentRepository;
            _shipmentBillOfLadingRepository = shipmentBillOfLadingRepository;
            _masterBillOfLadingItineraryRepository = masterBillOfLadingItineraryRepository;
            _billOfLadingItineraryRepository = billOfLadingItineraryRepository;
            _billOfLadingShipmentLoadRepository = billOfLadingShipmentLoadRepository;
            _freightSchedulerRepository = freightSchedulerRepository;
            _poFulfillmentRepository = poFulfillmentRepository;
            _masterBillRepository = masterBillRepository;
            _billOfLadingRepository = billOfLadingRepository;
            _freightSchedulerService = freightSchedulerService;
            _shipmentService = shipmentService;
            _containerService = containerService;
            _userProfileService = userProfileService;
            _csfeApiClient = csfeApiClient;
            _activityService = activityService;
            _serviceProvider = serviceProvider;
            _activityRepository = (IActivityRepository)UnitOfWork.GetRepository<ActivityModel>();
            _dataQuery = dataQuery;
        }

        public async Task<IEnumerable<ItineraryViewModel>> GetItinerariesByBOL(long billOfLadingId)
        {
            var result = await Repository.Query(s => s.BillOfLadingItineraries.Any(x => x.BillOfLadingId == billOfLadingId),
                                                    n => n.OrderBy(a => a.Sequence)).ToListAsync();

            return Mapper.Map<IEnumerable<ItineraryViewModel>>(result);
        }

        public async Task<IEnumerable<ItineraryViewModel>> GetItinerariesByMasterBOL(long masterBillOfLadingId)
        {
            var result = await Repository.Query(s => s.MasterBillOfLadingItineraries.Any(x => x.MasterBillOfLadingId == masterBillOfLadingId),
                                                    n => n.OrderBy(a => a.Sequence)).ToListAsync();

            return Mapper.Map<IEnumerable<ItineraryViewModel>>(result);
        }

        public async Task<IEnumerable<ItineraryViewModel>> GetItinerariesByContainer(long containerId)
        {
            var result = await Repository.Query(s => s.ContainerItineraries.Any(x => x.ContainerId == containerId),
                                                    n => n.OrderBy(a => a.Sequence)).ToListAsync();

            return Mapper.Map<IEnumerable<ItineraryViewModel>>(result);
        }

        public async Task<IEnumerable<ItineraryViewModel>> GetItinerariesByShipmentAsync(long shipmentId)
        {
            var result = await Repository.QueryAsNoTracking(s => s.ConsignmentItineraries.Any(x => x.ShipmentId == shipmentId),
                n => n.OrderBy(a => a.Sequence), x => x.Include(s => s.ConsignmentItineraries)).ToListAsync();
            foreach (var itinerary in result)
            {
                itinerary.ConsignmentItineraries = itinerary.ConsignmentItineraries.Where(x => x.ShipmentId == shipmentId).ToList();
            }
            return Mapper.Map<IEnumerable<ItineraryViewModel>>(result);
        }

        public async Task<IEnumerable<ItineraryViewModel>> GetItinerariesByConsignmentAsync(long consignmentId, string userName, bool isInternal, long userRoleId)
        {
            IEnumerable<ItineraryModel> itineraries = null;

            itineraries = await Repository.Query(i => i.ConsignmentItineraries.Any(ci => ci.ConsignmentId == consignmentId),
               o => o.OrderBy(i => i.Sequence)).ToListAsync();

            return Mapper.Map<IEnumerable<ItineraryViewModel>>(itineraries);
        }

        /// <summary>
        /// To handle to add Itinerary via application UI
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="consignmentId"></param>
        /// <param name="currentUser"></param>
        /// <param name="affiliates"></param>
        /// <param name="validateHAWB"></param>
        /// <returns></returns>
        public async Task<ItineraryViewModel> CreateAsync(ItineraryViewModel viewModel, long consignmentId, IdentityInfo currentUser, string affiliates, bool validateHAWB = true)
        {
            var userName = currentUser.Username;
            var isInternal = currentUser.IsInternal;

            var consignment = await _consignmentRepository.GetAsNoTrackingAsync(c => c.Id == consignmentId, includes: x => x.Include(y => y.Shipment));
            
            if (consignment.Shipment.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase) && validateHAWB)
            {
                var hasHAWB = await _shipmentBillOfLadingRepository.AnyAsync(x => x.ShipmentId == consignment.ShipmentId);

                if (!hasHAWB)
                {
                    throw new ApplicationException("Air Shipment does not link to HAWB# yet!");
                }
            }

            var currentFirstItinerary = await FirstItineraryAsNoTrackingAsync(consignment.ShipmentId);

            UnitOfWork.BeginTransaction();

            ItineraryModel itinerary = new();

            /**
             * Reuse the stored Itinerary. IF not found => create a new one 
             */

            // The Itinerary that has the same schedule and sequence
            var storedItinerary = await Repository.GetAsync(x
                => x.ScheduleId == viewModel.ScheduleId && x.Sequence == viewModel.Sequence
                && x.ModeOfTransport == viewModel.ModeOfTransport, includes: i => i.Include(y => y.FreightScheduler));

            FreightSchedulerModel freightScheduler = null;

            if (storedItinerary != null && IsSeaOrAir(viewModel.ModeOfTransport))
            {
                // Set for further uses
                itinerary = storedItinerary;
                freightScheduler = storedItinerary.FreightScheduler;

                // To correct Vessel Flight
                if (freightScheduler.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                {
                    storedItinerary.VesselFlight = freightScheduler.FlightNumber;
                    storedItinerary.FlightNumber = freightScheduler.FlightNumber;
                    storedItinerary.AirlineCode = freightScheduler.CarrierCode;
                }
                if (freightScheduler.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase))
                {
                    storedItinerary.VesselFlight = freightScheduler.VesselName + "/" + freightScheduler.Voyage;
                    storedItinerary.VesselName = freightScheduler.VesselName;
                    storedItinerary.Voyage = freightScheduler.Voyage;
                    storedItinerary.SCAC = freightScheduler.CarrierCode;
                }
            }
            else
            {
                itinerary = Mapper.Map<ItineraryModel>(viewModel);
                // Fulfill the itinerary from its schedule.
                if (itinerary.ScheduleId.HasValue)
                {
                    var schedule = await _freightSchedulerRepository.GetAsNoTrackingAsync(
                        f => f.Id == itinerary.ScheduleId.Value);

                    if (schedule != null)
                    {
                        itinerary.CarrierName = schedule.CarrierName;
                        itinerary.ETDDate = schedule.ETDDate;
                        itinerary.ETADate = schedule.ETADate;
                        itinerary.VesselName = schedule.VesselName;
                        itinerary.Voyage = schedule.Voyage;
                        itinerary.FlightNumber = schedule.FlightNumber;
                        itinerary.CYClosingDate = schedule.CYClosingDate;

                        // VesselFlight
                        if (schedule.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                        {
                            itinerary.VesselFlight = schedule.FlightNumber;
                            itinerary.AirlineCode = schedule.CarrierCode;
                        }
                        if (schedule.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase))
                        {
                            itinerary.SCAC = schedule.CarrierCode;
                            itinerary.VesselFlight = schedule.VesselName + "/" + schedule.Voyage;
                        }
                        // Set for further uses
                        freightScheduler = schedule;
                    }
                }
                itinerary.Status = StatusType.ACTIVE;
                itinerary.Audit(userName);
                await this.Repository.AddAsync(itinerary);
            }

            await UnitOfWork.SaveChangesAsync();

            #region Consignment Itinerary
            ConsignmentItineraryModel consignmentItinerary = new()
            {
                ConsignmentId = consignment.Id,
                ItineraryId = itinerary.Id,
                Sequence = itinerary.Sequence,
                ShipmentId = consignment.ShipmentId
            };

            consignmentItinerary.Audit(userName);
            await _consignmentItineraryRepository.AddAsync(consignmentItinerary);
            #endregion

            var shipment = await _shipmentService.GetAsync(consignment.ShipmentId.ToString(), isInternal, affiliates);
            var consignments = await _consignmentRepository.Query(c => c.ShipmentId == consignment.ShipmentId, includes: x => x.Include(y => y.ConsignmentItineraries)).ToListAsync();
            var consignmentItineraries = consignments.SelectMany(c => c.ConsignmentItineraries).ToList();
            // all itineraries of the shipment
            var itineraryIds = consignmentItineraries.Select(x => x.ItineraryId).ToList();
            var itineraries = await _itineraryRepository.Query(i => itineraryIds.Contains(i.Id), includes: x => x.Include(y => y.BillOfLadingItineraries)).ToListAsync();
            // all BillOfLadingItineraries of the shipment
            var billOfLadingItineraries = new List<BillOfLadingItineraryModel>();

            #region Bill of Lading
            List<BillOfLadingModel> billOfLadings = new();
            List<long> houseBillIds = new();
            List<string> houseBillNumbers = new();
            if (shipment?.BillOfLadingNos != null && shipment.BillOfLadingNos.Any())
            {
                houseBillIds = shipment.BillOfLadingNos.Select(bl => bl.Item1).ToList();
                houseBillNumbers = shipment.BillOfLadingNos.Select(bl => bl.Item2).ToList();

                billOfLadings = await _billOfLadingRepository.Query(x => houseBillIds.Contains(x.Id), includes: x => x.Include(y => y.Contacts).Include(y => y.BillOfLadingItineraries)).ToListAsync();

                var linkedBillOfLadingItineraries = await _billOfLadingItineraryRepository.QueryAsNoTracking(x
                   => x.ItineraryId == itinerary.Id && houseBillIds.Contains(x.BillOfLadingId)).ToListAsync();

                var toCreateBillOfLadingItineraries = new List<BillOfLadingItineraryModel>();
                foreach (var houseBillId in houseBillIds)
                {
                    var isLinked = linkedBillOfLadingItineraries.Count > 0
                        && linkedBillOfLadingItineraries.Where(x => x.BillOfLadingId == houseBillId && x.ItineraryId == itinerary.Id) != null;

                    if (!isLinked)
                    {
                        BillOfLadingItineraryModel billOfLadingItinerary = new()
                        {
                            BillOfLadingId = houseBillId,
                            ItineraryId = itinerary.Id
                        };
                        billOfLadingItinerary.Audit(userName);
                        toCreateBillOfLadingItineraries.Add(billOfLadingItinerary);
                    }
                }
                await _billOfLadingItineraryRepository.AddRangeAsync(toCreateBillOfLadingItineraries.ToArray());
                // for further uses.
                billOfLadingItineraries.AddRange(toCreateBillOfLadingItineraries);
            }
            if (itineraries != null && itineraries.Any())
            {
                billOfLadingItineraries.AddRange(itineraries.Where(x => x.BillOfLadingItineraries != null)
                    .SelectMany(x => x.BillOfLadingItineraries)
                    .Where(x => houseBillIds.Contains(x.BillOfLadingId))
                    .ToList());
            }
            var billOfLadingShipmentLoads = await _billOfLadingShipmentLoadRepository.Query(x => x.ShipmentLoad.ShipmentId == consignment.ShipmentId).ToListAsync();
            #endregion

            #region Master Bill
            List<long> masterBillIds = new();
            List<string> masterBillNumbers = new();
            if (shipment.MasterBillNos != null && shipment.MasterBillNos.Any())
            {
                masterBillIds = shipment.MasterBillNos?.Select(x => x.Item1).ToList();
                masterBillNumbers = shipment.MasterBillNos?.Select(x => x.Item2).ToList();
            }

            var isFirstItinerary = !itineraries.Any(i => i.Sequence < itinerary.Sequence);
            var mawb = freightScheduler?.MAWB;

            if (itinerary.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase)
                && isFirstItinerary
                && !string.IsNullOrEmpty(mawb))
            {
                MasterBillOfLadingModel masterBill = null;

                var hasLinked = masterBillNumbers.Any(n => n.Equals(mawb, StringComparison.InvariantCultureIgnoreCase));
                if (!hasLinked)
                {
                    // get master bill by mawb#
                    masterBill = await _masterBillRepository.GetAsync(m => m.MasterBillOfLadingNo == mawb, includes: x => x.Include(y => y.Contacts).Include(y => y.MasterBillOfLadingItineraries));
                    // if not found, create new
                    if (masterBill == null)
                    {
                        masterBill = new MasterBillOfLadingModel()
                        {
                            ModeOfTransport = ModeOfTransport.Air,
                            MasterBillOfLadingNo = mawb,
                            IssueDate = freightScheduler.ETDDate,
                            OnBoardDate = freightScheduler.ETDDate,
                            CarrierName = itinerary.CarrierName,
                            SCAC = itinerary.SCAC,
                            AirlineCode = itinerary.AirlineCode,
                            VesselFlight = itinerary.VesselFlight,
                            FlightNo = itinerary.FlightNumber,
                            Contacts = new List<MasterBillOfLadingContactModel>(),
                            MasterBillOfLadingItineraries = new List<MasterBillOfLadingItineraryModel>()
                        };
                        masterBill.Audit(userName);
                    }
                    if (masterBillIds.Any())
                    {
                        // get current master bill of the shipment

                        // curently, each shipment is linked to only one master bill.
                        var linkedMasterBill = await _masterBillRepository.GetAsync(m => m.Id == masterBillIds.First(), includes: x => x.Include(y => y.Contacts).Include(y => y.MasterBillOfLadingItineraries));
                        // check if master bill is shared with another shipment. 
                        var inSharingMasterBill = await _consignmentRepository.AnyAsync(csm => csm.MasterBillId == linkedMasterBill.Id && csm.ShipmentId != consignment.ShipmentId);

                        if (!inSharingMasterBill)
                        {
                            // if mawb# does not exist in the system => use the current master bill and update the number.
                            if (masterBill.Id == 0)
                            {
                                // use the current master bill
                                masterBill = linkedMasterBill;
                                // update the current master bill
                                masterBill.MasterBillOfLadingNo = mawb;
                                masterBill.CarrierName = itinerary.CarrierName;
                                masterBill.AirlineCode = itinerary.AirlineCode;
                                masterBill.VesselFlight = itinerary.VesselFlight;
                                masterBill.FlightNo = itinerary.FlightNumber;
                                masterBill.IssueDate = freightScheduler.ETDDate;
                                masterBill.OnBoardDate = freightScheduler.ETDDate;
                                masterBill.Audit(currentUser.Username);
                            }
                            else
                            {
                                linkedMasterBill.MasterBillOfLadingItineraries.Clear();
                                _masterBillRepository.Remove(linkedMasterBill);
                            }
                        }
                    }
                }
                else
                {
                    masterBill = await _masterBillRepository.GetAsync(m => m.MasterBillOfLadingNo == mawb, includes: x => x.Include(y => y.Contacts).Include(y => y.MasterBillOfLadingItineraries));
                }

                hasLinked = masterBillIds.Any(id => id == masterBill.Id);
                if (!hasLinked)
                {
                    var houseBillContacts = billOfLadings.SelectMany(x => x.Contacts);
                    List<string> masterBillContactRoles = new()
                    {
                        OrganizationRole.OriginAgent,
                        OrganizationRole.DestinationAgent,
                        OrganizationRole.Principal,
                    };
                    foreach (var item in houseBillContacts)
                    {
                        if (masterBillContactRoles.Contains(item.OrganizationRole, StringComparer.InvariantCultureIgnoreCase))
                        {
                            var isExisting = masterBill.Contacts.Any(c => c.OrganizationRole.Equals(item.OrganizationRole, StringComparison.InvariantCultureIgnoreCase) && c.OrganizationId == item.OrganizationId);
                            if (!isExisting)
                            {
                                var newMasterBillContact = new MasterBillOfLadingContactModel()
                                {
                                    MasterBillOfLadingId = masterBill.Id,
                                    CompanyName = item.CompanyName,
                                    ContactName = item.ContactName,
                                    ContactEmail = item.ContactEmail,
                                    ContactNumber = item.ContactNumber,
                                    OrganizationRole = item.OrganizationRole,
                                    OrganizationId = item.OrganizationId,
                                    Address = item.Address,
                                };
                                newMasterBillContact.Audit(userName);
                                masterBill.Contacts.Add(newMasterBillContact);
                            }
                        }
                    }

                    var houseBillItineraries = billOfLadings.SelectMany(x => x.BillOfLadingItineraries);
                    var houseBillItineraryIds = houseBillItineraries.Select(x => x.ItineraryId).ToList();
                    var houseBillItineraryModes = await _itineraryRepository.Query(i => houseBillItineraryIds.Contains(i.Id)).Select(i => new { i.Id, i.ModeOfTransport }).ToListAsync();
                    foreach (var item in houseBillItineraries)
                    {
                        var modeOfTransport = houseBillItineraryModes.First(x => x.Id == item.ItineraryId).ModeOfTransport;
                        if (modeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var isExisting = masterBill.MasterBillOfLadingItineraries.Any(x => x.ItineraryId == item.ItineraryId);
                            if (!isExisting)
                            {
                                var newMasterBillOfLadingItinerary = new MasterBillOfLadingItineraryModel()
                                {
                                    MasterBillOfLadingId = masterBill.Id,
                                    ItineraryId = item.ItineraryId
                                };
                                newMasterBillOfLadingItinerary.Audit(userName);
                                masterBill.MasterBillOfLadingItineraries.Add(newMasterBillOfLadingItinerary);
                            }
                        }
                    }
                }
                // new master bill
                if (masterBill.Id == 0)
                {
                    await _masterBillRepository.AddAsync(masterBill);
                }
                else
                {
                    _masterBillRepository.Update(masterBill);
                }
                await UnitOfWork.SaveChangesAsync();

                // to update MasterBillId
                consignments.ForEach(c => c.MasterBillId = masterBill.Id);
                consignmentItineraries.ForEach(c => c.MasterBillId = masterBill.Id);
                billOfLadingItineraries.ForEach(c => c.MasterBillOfLadingId = masterBill.Id);
                billOfLadingShipmentLoads.ForEach(c => c.MasterBillOfLadingId = masterBill.Id);

                await LinkMasterBillsToItineraryAsync(itinerary, new() { masterBill.Id }, userName);
            }
            else
            {
                await LinkMasterBillsToItineraryAsync(itinerary, masterBillIds, userName);
            }
            #endregion

            #region Container
            var containers = await _containerService.GetContainersByShipmentAsync(consignment.ShipmentId, isInternal, affiliates);
            if (containers != null && containers.Any())
            {
                var linkedContainerItineraries = await _containerItineraryRepository.QueryAsNoTracking(x
                    => x.ItineraryId == itinerary.Id && containers.Select(y => y.Id).Any(y => y == x.ContainerId)).ToListAsync();

                var containerItineraries = new List<ContainerItineraryModel>();
                foreach (var item in containers)
                {
                    var isLinked = linkedContainerItineraries.Count > 0
                        && linkedContainerItineraries.Where(x => x.ContainerId == item.Id && x.ItineraryId == itinerary.Id) != null;

                    if (!isLinked)
                    {
                        ContainerItineraryModel containerItinerary = new()
                        {
                            ContainerId = item.Id,
                            ItineraryId = itinerary.Id
                        };
                        containerItinerary.Audit(userName);
                        containerItineraries.Add(containerItinerary);
                    }
                }
                await _containerItineraryRepository.AddRangeAsync(containerItineraries.ToArray());
            }
            #endregion

            await UnitOfWork.SaveChangesAsync();

            var firstItinerary = await FirstItineraryAsNoTrackingAsync(shipment.Id);

            var hasChangeFirstSchedule = currentFirstItinerary == null || firstItinerary.ScheduleId != currentFirstItinerary.ScheduleId;
            if (hasChangeFirstSchedule &&
                shipment.FulfillmentId != null &&
                shipment.IsFCL)
            {
                await UpdatePOFFCYClosingDateAsync(shipment.Id, firstItinerary.CYClosingDate, userName);
            }

            await UpdateShipmentETDETAByItineraryAsync(consignment.ShipmentId, itinerary);

            await ChangeStageOfBookingAndPOAsync(shipment.Id);

            UnitOfWork.CommitTransaction();

            viewModel = Mapper.Map<ItineraryViewModel>(itinerary);
            viewModel.ConsignmentId = consignmentId;
            return viewModel;
        }

        private async Task LinkMasterBillsToItineraryAsync(ItineraryModel itinerary, List<long> masterBillIds, string userName = "")
        {
            if (masterBillIds == null || !masterBillIds.Any())
            {
                return;
            }

            var linkedMasterBillOfLadingItineraries = await _masterBillOfLadingItineraryRepository.QueryAsNoTracking(x
                    => x.ItineraryId == itinerary.Id && masterBillIds.Any(y => y == x.MasterBillOfLadingId)).ToListAsync();

            var masterBillOfLadingItineraries = new List<MasterBillOfLadingItineraryModel>();
            foreach (var masterBillId in masterBillIds)
            {
                var isLinked = linkedMasterBillOfLadingItineraries.Count > 0
                    && linkedMasterBillOfLadingItineraries.Where(x => x.MasterBillOfLadingId == masterBillId && x.ItineraryId == itinerary.Id) != null;

                if (!isLinked)
                {
                    MasterBillOfLadingItineraryModel masterBillOfLadingItinerary = new()
                    {
                        MasterBillOfLadingId = masterBillId,
                        ItineraryId = itinerary.Id
                    };
                    masterBillOfLadingItinerary.Audit(userName);
                    masterBillOfLadingItineraries.Add(masterBillOfLadingItinerary);
                }
            }
            await _masterBillOfLadingItineraryRepository.AddRangeAsync(masterBillOfLadingItineraries.ToArray());
        }

        public async Task ChangeStageOfBookingAndPOAsync(long? shipmentId,POFulfillmentModel bookingModel = null)
        {
            var shipment = await _shipmentRepository.GetAsNoTrackingAsync(c => c.Id == shipmentId);

            if (shipment == null)
            {
                return;
            }
            var seaOrAir = new string[]
            {
                ModeOfTransport.Sea,
                ModeOfTransport.Air
            };
            _poFulfillmentService = _serviceProvider.GetRequiredService<IPOFulfillmentService>();
            var consignmentItineraries = await _consignmentItineraryRepository
                    .QueryAsNoTracking(c =>
                    c.ShipmentId == shipmentId
                    && seaOrAir.Contains(c.Itinerary.ModeOfTransport)
                    && c.Itinerary.ScheduleId.HasValue
                    , null, c => c
                        .Include(x => x.Itinerary).ThenInclude(c => c.FreightScheduler)
                        .Include(s => s.Shipment).ThenInclude(c => c.POFulfillment))
                    .ToListAsync();

            var itineraryLinkedFSHasATD = consignmentItineraries.Where(c => c.Itinerary.FreightScheduler.ATDDate.HasValue).ToList();
            var itineraryLinkedFSHasATA = consignmentItineraries.Where(c =>
                    c.Itinerary.FreightScheduler.ATADate.HasValue
                    && (c.Shipment.ServiceType.Contains("to-Port", StringComparison.OrdinalIgnoreCase) || c.Shipment.ServiceType.Contains("to-Airport", StringComparison.OrdinalIgnoreCase))
                    && c.Itinerary.FreightScheduler.LocationToName.Equals(shipment.ShipTo ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                    && c.Shipment.Status.Equals(StatusType.ACTIVE, StringComparison.OrdinalIgnoreCase)
                    && c.Shipment.POFulfillment?.FulfilledFromPOType != POType.Blanket
                    && c.Shipment.POFulfillment?.Status == POFulfillmentStatus.Active
            ).ToList();

            var bookingIds = new List<long>();
            POFulfillmentModel booking = null;
            if (consignmentItineraries.Count == 0)
            {
                if (bookingModel?.Id != null)
                {
                    bookingIds.Add(bookingModel?.Id ?? 0);
                    booking = bookingModel;
                }
            }
            else
            {
                booking = consignmentItineraries[0].Shipment?.POFulfillment;
                // As there is no booking linked in cruise PO
                if (booking?.Id != null)
                {
                    bookingIds.Add(booking.Id);
                }
            }

            #region Add/Update Itinerary
            if (itineraryLinkedFSHasATD.Count > 0)
            {
                if (booking?.Stage == POFulfillmentStage.ForwarderBookingConfirmed)
                {
                    await _poFulfillmentService.ChangeStageToShipmentDispatchAsync(bookingIds, AppConstant.SYSTEM_USERNAME);
                }
            }

            if (itineraryLinkedFSHasATA.Count > 0)
            {
                await _poFulfillmentService.ChangeStageToClosedAsync(
                  bookingIds,
                  AppConstant.SYSTEM_USERNAME,
                  itineraryLinkedFSHasATA[0].Itinerary.FreightScheduler.ATADate ?? new DateTime(1, 1, 1),
                  itineraryLinkedFSHasATA[0].Itinerary.FreightScheduler.LocationToName);
            }
            #endregion Add/Update Itinerary

            #region Delete Itinerary
            if (itineraryLinkedFSHasATA.Count == 0)
            {
                if (booking?.Stage == POFulfillmentStage.Closed)
                {
                    await _poFulfillmentService.RevertStageToShipmentDispatchAsync(bookingIds, AppConstant.SYSTEM_USERNAME);
                }
            }

            if (itineraryLinkedFSHasATD.Count == 0)
            {
                await _poFulfillmentService.RevertStageToFBConfirmedAsync(bookingIds, AppConstant.SYSTEM_USERNAME);
            }
            #endregion Delete
        }

        /// <summary>
        /// Link Itinerary to Freight Schedule if Mode of transport = Sea
        /// </summary>
        /// <param name="itinerary"></param>
        /// <param name="overrideSchedule">If true, override ETD/ETA from Itinerary</param>
        /// <returns></returns>
        /// <remarks>
        /// Only work if itinerary contains location description
        /// </remarks>
        private async Task<FreightSchedulerModel> MapScheduleToItinerary(ItineraryModel itinerary)
        {
            if (itinerary.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.OrdinalIgnoreCase))
            {
                // Need to get location code by location description from itinerary
                var locations = await _csfeApiClient.GetAllLocationsAsync();
                var locationFromCode = locations.First(x => x.LocationDescription.Equals(itinerary.LoadingPort)).Name;
                var locationToCode = locations.First(x => x.LocationDescription.Equals(itinerary.DischargePort)).Name;

                var activeVessels = await _csfeApiClient.GetActiveVesselsAsync();
                var isRealVessel = activeVessels.FirstOrDefault(x => x.Name == itinerary.VesselName)?.IsRealVessel;
                // Unreal vessel won't link to Freight Schedule
                if (!isRealVessel.HasValue || !isRealVessel.Value)
                {
                    itinerary.ScheduleId = null;
                    return null;
                }

                var scheduler = await _freightSchedulerRepository.GetAsync(
                    fs => fs.ModeOfTransport == ModeOfTransport.Sea
                          && fs.CarrierCode == itinerary.SCAC
                          && fs.VesselName == itinerary.VesselName
                          && fs.Voyage == itinerary.Voyage
                          && fs.LocationFromName == itinerary.LoadingPort
                          && fs.LocationToName == itinerary.DischargePort);
                if (scheduler == null)
                {
                    var carrier = await _csfeApiClient.GetCarrierByCodeAsync(itinerary.SCAC);
                    FreightSchedulerModel freightScheduler = new FreightSchedulerModel
                    {
                        ModeOfTransport = itinerary.ModeOfTransport,
                        CarrierCode = itinerary.SCAC,
                        CarrierName = carrier.Name,
                        VesselName = itinerary.VesselName,
                        Voyage = itinerary.Voyage,
                        LocationFromCode = locationFromCode,
                        LocationFromName = itinerary.LoadingPort,
                        LocationToCode = locationToCode,
                        LocationToName = itinerary.DischargePort,
                        ETDDate = itinerary.ETDDate,
                        ETADate = itinerary.ETADate,
                        IsAllowExternalUpdate = true
                    };

                    freightScheduler.Audit(itinerary.CreatedBy);
                    return freightScheduler;
                }
                else
                {
                    itinerary.ScheduleId = scheduler.Id;
                    if (scheduler.IsAllowExternalUpdate)
                    {
                        scheduler.ETADate = itinerary.ETADate;
                        scheduler.ETDDate = itinerary.ETDDate;
                        scheduler.Audit();
                    }
                    return scheduler;
                }

            }

            return null;
        }

        /// <summary>
        /// To handle Itinerary Update via application UI
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="consignmentId"></param>
        /// <param name="currentUser"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        public async Task<ItineraryViewModel> UpdateAsync(ItineraryViewModel viewModel, long consignmentId, IdentityInfo currentUser, string affiliates)
        {
            var isInternal = currentUser.IsInternal;
            var userName = currentUser.Username;

            var model = await this.Repository.GetAsync(i => i.Id == viewModel.Id,
                includes: x => x.Include(i => i.ConsignmentItineraries).Include(i => i.FreightScheduler));

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {viewModel.Id} not found!");
            }

            var oldConsignmentItinerary = model.ConsignmentItineraries.FirstOrDefault(ci => ci.ConsignmentId == consignmentId);

            // The Itinerary that has the same schedule and sequence
            var storedItinerary = await Repository.GetAsNoTrackingAsync(x
                => x.ScheduleId != null && x.ScheduleId == viewModel.ScheduleId && x.Sequence == viewModel.Sequence && x.ModeOfTransport == viewModel.ModeOfTransport);

            var result = new ItineraryViewModel();

            if (IsSeaOrAir(model.ModeOfTransport)
                && storedItinerary?.Id != model.Id)
            {
                await RemoveShipmentItineraryReferences(oldConsignmentItinerary.ShipmentId.Value, model.Id, isInternal, affiliates);
                _consignmentItineraryRepository.Remove(oldConsignmentItinerary);
                viewModel.Id = 0;
                viewModel.CreatedBy = null;
                result = await CreateAsync(viewModel, viewModel.ConsignmentId.Value, currentUser, affiliates, false);

                await UnitOfWork.SaveChangesAsync();
            }
            else
            {
                var currentFirstItinerary = await FirstItineraryAsNoTrackingAsync(oldConsignmentItinerary.ShipmentId.Value);

                UnitOfWork.BeginTransaction();

                // Some original data of itinerary
                var originalItinerary = new ItineraryModel
                {
                    ScheduleId = model.ScheduleId,
                    ETADate = model.ETADate,
                    ETDDate = model.ETDDate
                };

                Mapper.Map(viewModel, model);
                model.Audit(userName);

                //It Schedule Id is still same
                if (originalItinerary.ScheduleId == model.ScheduleId)
                {
                    if (model.FreightScheduler != null && !model.FreightScheduler.IsAllowExternalUpdate)
                    {
                        model.ETADate = originalItinerary.ETADate;
                        model.ETDDate = originalItinerary.ETDDate;
                    }
                }
                // It Schedule id is different, changed to another
                else
                {

                }

                if (viewModel.ConsignmentId.Value != consignmentId)
                {
                    var newConsignmentItinerary = new ConsignmentItineraryModel
                    {
                        ConsignmentId = viewModel.ConsignmentId.Value,
                        ItineraryId = oldConsignmentItinerary.ItineraryId,
                        Sequence = oldConsignmentItinerary.Sequence,
                        ShipmentId = oldConsignmentItinerary.ShipmentId
                    };

                    newConsignmentItinerary.Audit(model.CreatedBy);
                    await _consignmentItineraryRepository.AddAsync(newConsignmentItinerary);
                    _consignmentItineraryRepository.Remove(oldConsignmentItinerary);
                }

                if (IsSeaOrAir(model.ModeOfTransport) && model.ScheduleId.HasValue)
                {
                    FreightSchedulerModel schedule = await _freightSchedulerRepository.GetAsNoTrackingAsync(
                        f => f.Id == model.ScheduleId.Value);

                    model.CarrierName = schedule.CarrierName;
                    model.SCAC = schedule.CarrierCode;
                    model.ETDDate = schedule.ETDDate;
                    model.ETADate = schedule.ETADate;
                    
                    // Vessel
                    if (model.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase))
                    {
                        model.VesselName = schedule.VesselName;
                        model.Voyage = schedule.Voyage;
                        model.VesselFlight = schedule.VesselName + "/" + schedule.Voyage;
                    }
                    // Flight
                    if (model.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                    {
                        model.FlightNumber = schedule.FlightNumber;
                        model.VesselFlight = schedule.FlightNumber;
                    }
                }
                await UnitOfWork.SaveChangesAsync();
                await UpdateShipmentETDETAByItineraryAsync(oldConsignmentItinerary.ShipmentId.Value, model);

                var firstItinerary = await FirstItineraryAsNoTrackingAsync(oldConsignmentItinerary.ShipmentId.Value);

                var hasChangeFirstSchedule = currentFirstItinerary == null || firstItinerary.ScheduleId != currentFirstItinerary.ScheduleId;
                if (hasChangeFirstSchedule)
                {
                    await UpdatePOFFCYClosingDateAsync(oldConsignmentItinerary.ShipmentId.Value, firstItinerary.CYClosingDate, userName);
                }

                UnitOfWork.CommitTransaction();
                result = Mapper.Map<ItineraryViewModel>(model);
            }

            result.ConsignmentId = viewModel.ConsignmentId;
            return result;
        }

        private async Task UpdatePOFFCYClosingDateAsync(long shipmentId, DateTime? cyClosingDate, string userName)
        {
            var poFulfillment = await _poFulfillmentRepository.GetAsync(poff => poff.Shipments.Any(x => x.Id == shipmentId && x.IsFCL));

            if (poFulfillment == null)
            {
                return;
            }

            poFulfillment.CYClosingDate = cyClosingDate;
            poFulfillment.Audit(userName);
            await UnitOfWork.SaveChangesAsync();
        }

        private async Task<ItineraryModel> FirstItineraryAsNoTrackingAsync(long shipmentId)
        {
            return await Repository.QueryAsNoTracking(x => x.ConsignmentItineraries.Any(a => a.ShipmentId == shipmentId)).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
        }

        /// <summary>
        /// To remove the link between itinerary and shipment's Bill of Lading/ Master Bill of Lading/ Container
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="itineraryId"></param>
        /// <param name="userName"></param>
        /// <param name="affiliates"></param>
        private async Task RemoveShipmentItineraryReferences(long shipmentId, long itineraryId, bool isInternal, string affiliates)
        {
            var shipment = await _shipmentService.GetAsync(shipmentId.ToString(), isInternal, affiliates);

            if (shipment == null)
            {
                return;
            }
            var masterBillIds = new List<long>();

            if (shipment.MasterBillNos != null && shipment.MasterBillNos.Any())
            {
                masterBillIds = shipment.MasterBillNos.Select(y => y.Item1).ToList();
            }

            #region House Bill
            var billOfLadingItineraries = await _billOfLadingItineraryRepository.QueryAsNoTracking(x
                   => x.ItineraryId == itineraryId && shipment.BillOfLadingNos.Select(y => y.Item1).Any(y => y == x.BillOfLadingId)).ToListAsync();

            _billOfLadingItineraryRepository.RemoveRange(billOfLadingItineraries.ToArray());
            #endregion

            #region Master Bill
            // To check sharing before removing MasterBillItineraries.
            var inSharingMasterBillItineraries = await _consignmentRepository.QueryAsNoTracking(x => x.ShipmentId != shipmentId && x.MasterBillId != null && masterBillIds.Contains(x.MasterBillId.Value))
                .SelectMany(
                    x => x.ConsignmentItineraries.Where(c => c.ItineraryId == itineraryId)
                                                 .Select(y => new { x.MasterBillId, y.ItineraryId })
                ).ToListAsync();
            
            var masterBillOfLadingItineraries = await _masterBillOfLadingItineraryRepository.QueryAsNoTracking(x
                    => x.ItineraryId == itineraryId && masterBillIds.Any(y => y == x.MasterBillOfLadingId)).ToListAsync();

            masterBillOfLadingItineraries = masterBillOfLadingItineraries.Where(x => !inSharingMasterBillItineraries.Any(a => a.MasterBillId == x.MasterBillOfLadingId && a.ItineraryId == x.ItineraryId)).ToList();
            _masterBillOfLadingItineraryRepository.RemoveRange(masterBillOfLadingItineraries.ToArray());
            #endregion

            #region Containers
            var containers = await _containerService.GetContainersByShipmentAsync(shipmentId, isInternal, affiliates);
            if (containers != null && containers.Any())
            {
                var containerItineraries = await _containerItineraryRepository.QueryAsNoTracking(x
                    => x.ItineraryId == itineraryId && containers.Select(y => y.Id).Any(y => y == x.ContainerId)).ToListAsync();
                _containerItineraryRepository.RemoveRange(containerItineraries.ToArray());
            }
            #endregion
        }

        /// <summary>
        /// To update the ETD/ETA Date of shipment and related tables (Consignment/ Container/ BillOfLading)
        /// based on ETD/ETA of linked Itineraries
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <param name="itinerary"></param>
        /// <returns></returns>
        private async Task UpdateShipmentETDETAByItineraryAsync(long shipmentId, ItineraryModel itinerary)
        {
            var seaOrAirMode = new string[] { ModeOfTransport.Sea, ModeOfTransport.Air };
            bool isSeaOrAirItinerary = seaOrAirMode.Contains(itinerary.ModeOfTransport);

            var shipment = await _shipmentRepository.GetAsync(x => x.Id == shipmentId, null,
                x => x.Include(s => s.Consignments)
                .Include(s => s.ConsignmentItineraries)
                .Include(s => s.ShipmentLoads)
                .Include(s => s.ShipmentBillOfLadings)
                .ThenInclude(sbl => sbl.BillOfLading)
                .ThenInclude(bl => bl.BillOfLadingItineraries));

            var itineraryIds = new List<long>();
            if (shipment.ConsignmentItineraries != null && shipment.ConsignmentItineraries.Any())
            {
                itineraryIds = shipment.ConsignmentItineraries.Select(ci => ci.ItineraryId).ToList();
            }

            // All Sea or Air Itineraries
            var seaOrAirItineraries = await _itineraryRepository.QueryAsNoTracking(i => itineraryIds.Any(a => a == i.Id) && seaOrAirMode.Contains(i.ModeOfTransport)).ToListAsync();
            // First Sea or Air Itinerary
            var firstSeaOrAirItinerary = seaOrAirItineraries.OrderBy(i => i.Sequence).FirstOrDefault();
            // Lastest Sea or Air Itinerary
            var latestSeaOrAirItinerary = seaOrAirItineraries.OrderByDescending(i => i.ETADate).FirstOrDefault();

            #region Shipment
            if (firstSeaOrAirItinerary != null)
            {
                shipment.ShipFromETDDate = firstSeaOrAirItinerary.ETDDate;
            }

            if (latestSeaOrAirItinerary != null)
            {
                shipment.ShipToETADate = latestSeaOrAirItinerary.ETADate;
            }
            #endregion Shipment

            #region Consignment
            foreach (var consignment in shipment.Consignments)
            {
                if (firstSeaOrAirItinerary != null)
                {
                    consignment.ShipFromETDDate = firstSeaOrAirItinerary.ETDDate;
                }
                if (latestSeaOrAirItinerary != null)
                {
                    consignment.ShipToETADate = latestSeaOrAirItinerary.ETADate;
                }
            }
            #endregion Consignment

            #region Update ETD of linked Containers
            var linkedContainerIds = shipment.ShipmentLoads?.Where(sl => sl.ContainerId.HasValue).Select(sl => sl.ContainerId);
            var containers = await _containerRepository.Query(c => linkedContainerIds.Any(a => a == c.Id),
                null,
                x => x.Include(y => y.ContainerItineraries)).ToListAsync();

            foreach (var container in containers)
            {
                if (shipment.IsFCL)
                {
                    if (firstSeaOrAirItinerary != null)
                    {
                        container.ShipFromETDDate = firstSeaOrAirItinerary.ETDDate;
                    }
                    if (latestSeaOrAirItinerary != null)
                    {
                        container.ShipToETADate = latestSeaOrAirItinerary.ETADate;
                    }
                }
                else
                {
                    var linkedItineraryIds = container.ContainerItineraries.Select(c => c.ItineraryId);
                    var linkedItineraries = await _itineraryRepository
                        .QueryAsNoTracking(i => linkedItineraryIds.Any(a => a == i.Id))
                        .ToListAsync();

                    var earliestItinerary = linkedItineraries?
                        .OrderBy(i => i.ETDDate)
                        .FirstOrDefault();
                    if (earliestItinerary != null && seaOrAirMode.Contains(earliestItinerary.ModeOfTransport))
                    {
                        container.ShipFromETDDate = earliestItinerary.ETDDate;
                    }

                    var latestItinerary = linkedItineraries?
                        .OrderByDescending(i => i.ETADate)
                        .FirstOrDefault();
                    if (latestItinerary != null && seaOrAirMode.Contains(latestItinerary.ModeOfTransport))
                    {
                        container.ShipToETADate = latestItinerary.ETADate;
                    }
                }
            }
            #endregion

            #region Update ETD of linked House BillOfLadings
            var billOfLadings = shipment.ShipmentBillOfLadings?.Select(sbl => sbl.BillOfLading);
            foreach (var billOfLading in billOfLadings)
            {
                var linkedItineraryIds = billOfLading.BillOfLadingItineraries.Select(x => x.ItineraryId);
                var linkedItineraries = await _itineraryRepository
                    .QueryAsNoTracking(i => linkedItineraryIds.Any(a => a == i.Id))
                    .ToListAsync();

                var earliestItinerary = linkedItineraries?
                        .OrderBy(i => i.ETDDate)
                        .FirstOrDefault();
                if (earliestItinerary != null && seaOrAirMode.Contains(earliestItinerary.ModeOfTransport))
                {
                    billOfLading.ShipFromETDDate = earliestItinerary.ETDDate;
                }

                var latestItinerary = linkedItineraries?
                        .OrderByDescending(i => i.ETADate)
                        .FirstOrDefault();
                if (latestItinerary != null && seaOrAirMode.Contains(latestItinerary.ModeOfTransport))
                {
                    billOfLading.ShipToETADate = latestItinerary.ETADate;
                }
            }
            #endregion

            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        ///  To handle Itinerary Import API which call from ediSON/third-party
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public override async Task<ItineraryViewModel> CreateAsync(ItineraryViewModel viewModel)
        {
            // Itinerary Import/Update API will pass location code
            // Then, need to get location description by location code
            await MapLocationCodeToDescriptionAsync(viewModel);

            // Correct CarrierName from MasterData when user create/update Itinerary from API
            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) || viewModel.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
            {
                await CorrectCarrierNameAsync(viewModel);
            }

            ItineraryModel model = Mapper.Map<ItineraryModel>(viewModel);

            // Only add CYOpenDate/CYClosingDate if ModeOfTransport = sea
            if (model.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                model.CYOpenDate = model.CYClosingDate = null;
            }

            var error = await ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();

            // Allow to override FS ETA/ETD
            var scheduler = await MapScheduleToItinerary(model);

            // Add scheduler
            if (scheduler != null && scheduler.Id == 0)
            {
                scheduler.ATDDate = viewModel.ATDDate;
                scheduler.ATADate = viewModel.ATADate;
                scheduler.CYOpenDate = viewModel.CYOpenDate;
                scheduler.CYClosingDate = viewModel.CYClosingDate;

                await _freightSchedulerRepository.AddAsync(scheduler);
                await this.UnitOfWork.SaveChangesAsync();

                // Set Scheduler for Itinerary
                model.FreightScheduler = scheduler;
                this.Repository.Update(model);

                await this.UnitOfWork.SaveChangesAsync();

                if (viewModel.ATDDate.HasValue)
                {
                    var activity7001 = await CreateActivityAsync(
                             Event.EVENT_7001,
                             viewModel.LoadingPort,
                             viewModel.ATDDate ?? new DateTime(1, 1, 1),
                             scheduler.Id,
                             viewModel.CreatedBy);

                    await _activityService.CreateAsync(activity7001);
                }

                if (viewModel.ATADate.HasValue)
                {
                    var activity7002 = await CreateActivityAsync(
                            Event.EVENT_7002,
                            viewModel.DischargePort,
                            viewModel.ATADate ?? new DateTime(1, 1, 1),
                            scheduler.Id,
                            viewModel.CreatedBy);

                    await _activityService.CreateAsync(activity7002);
                }
            }

            // Update scheduler must be called after itinerary saved
            else
            {
                var oldATDDateScheduler = scheduler?.ATDDate;
                var oldATADateScheduler = scheduler?.ATADate;

                // Only update FreightScheduler and trigger event when FreightScheduler is not locked
                if (scheduler != null && scheduler.IsAllowExternalUpdate)
                {
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.ATDDate)))
                    {
                        scheduler.ATDDate = viewModel.ATDDate;
                    }
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.ATADate)))
                    {
                        scheduler.ATADate = viewModel.ATADate;
                    }
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.CYOpenDate)))
                    {
                        scheduler.CYOpenDate = viewModel.CYOpenDate;
                    }
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.CYClosingDate)))
                    {
                        scheduler.CYClosingDate = viewModel.CYClosingDate;
                    }

                    _freightSchedulerRepository.Update(scheduler);
                    await this.UnitOfWork.SaveChangesAsync();
                    await TriggerEventForFreightScheduler(oldATDDateScheduler, oldATADateScheduler, scheduler.Id, viewModel, model.CreatedBy);
                }
            }

            viewModel = Mapper.Map<ItineraryViewModel>(model);
            return viewModel;
        }

        private async Task TriggerEventForFreightScheduler(DateTime? atdDate, DateTime? ataDate, long schedulerId, ItineraryViewModel itineraryViewModel, string userName)
        {
            //ATD and ATA of Freight Scheduler do not exist in system
            if (!atdDate.HasValue && !ataDate.HasValue)
            {
                if (itineraryViewModel.ATDDate.HasValue)
                {
                    var activity7001 = await CreateActivityAsync(
                             Event.EVENT_7001,
                             itineraryViewModel.LoadingPort,
                             itineraryViewModel.ATDDate ?? new DateTime(1, 1, 1),
                             schedulerId,
                             userName
                             );

                    await _activityService.CreateAsync(activity7001);
                }

                if (itineraryViewModel.ATADate.HasValue)
                {
                    var activity7002 = await CreateActivityAsync(
                            Event.EVENT_7002,
                            itineraryViewModel.DischargePort,
                            itineraryViewModel.ATADate ?? new DateTime(1, 1, 1),
                            schedulerId,
                            userName);

                    await _activityService.CreateAsync(activity7002);
                }
            }
            else
            {
                // WARNING: event 7002 should be hanlde before event 7001
                // Only update activity when atd/ata inputted in JSON 
                if (itineraryViewModel.IsPropertyDirty(nameof(ItineraryViewModel.ATADate)))
                {
                    await UpdateOrDeleteActivityAsync(schedulerId, Event.EVENT_7002, itineraryViewModel, itineraryViewModel.DischargePort, itineraryViewModel.ATADate, userName);
                }

                if (itineraryViewModel.IsPropertyDirty(nameof(ItineraryViewModel.ATDDate)))
                {
                    await UpdateOrDeleteActivityAsync(schedulerId, Event.EVENT_7001, itineraryViewModel, itineraryViewModel.LoadingPort, itineraryViewModel.ATDDate, userName);
                }
            }
        }

        private async Task UpdateOrDeleteActivityAsync(long schedulerId, string eventCode, ItineraryViewModel itineraryViewModel, string location, DateTime? atdOrATADate, string userName)
        {
            var globalId = CommonHelper.GenerateGlobalId(schedulerId, EntityType.FreightScheduler);
            var activities = await _activityRepository.Query(s => s.GlobalIdActivities.Any(g => g.GlobalId == globalId)).ToListAsync();

            var activityInSystem = activities?.Find(c => c.ActivityCode == eventCode);

            if (atdOrATADate.HasValue)
            {
                if (activityInSystem != null)
                {
                    var activityViewModel = await CreateActivityAsync(
                     eventCode,
                     location,
                     atdOrATADate ?? new DateTime(1, 1, 1),
                     schedulerId,
                     AppConstant.SYSTEM_USERNAME);

                    activityViewModel.Id = activityInSystem.Id;
                    if (itineraryViewModel.IsPropertyDirty(nameof(ItineraryViewModel.UpdatedBy)))
                    {
                        activityViewModel.Audit(itineraryViewModel.UpdatedBy);
                    }

                    // to work with Auto mapper
                    activityViewModel.FieldStatus = new Dictionary<string, FieldDeserializationStatus>
                                {
                                    {
                                        nameof(ActivityViewModel.UpdatedBy), FieldDeserializationStatus.HasValue
                                    },
                                    {
                                        nameof(ActivityViewModel.UpdatedDate), FieldDeserializationStatus.HasValue
                                    },
                                    {
                                        nameof(ActivityViewModel.ActivityDate), FieldDeserializationStatus.HasValue
                                    }
                                };
                    await _activityService.UpdateAsync(activityViewModel, activityViewModel.Id);
                }
                else
                {
                    var activity = await CreateActivityAsync(
                            eventCode,
                            location,
                            atdOrATADate ?? new DateTime(1, 1, 1),
                            schedulerId,
                            userName);

                    await _activityService.CreateAsync(activity);
                }
            }
            else if (activityInSystem != null)
            {
                await _activityService.DeleteAsync(activityInSystem.Id);
            }
        }

        private async Task<ActivityViewModel> CreateActivityAsync(string eventCode, string location, DateTime activityDate, long freightSchedulerId, string createdBy, string metaData)
        {
            var eventModel = await _csfeApiClient.GetEventByCodeAsync(eventCode);
            var newActivity = new ActivityViewModel
            {
                FreightSchedulerId = freightSchedulerId,
                ActivityCode = eventCode,
                ActivityType = eventModel.ActivityType,
                ActivityDescription = eventModel.ActivityDescription,
                Location = location,
                ActivityDate = activityDate,
                CreatedBy = createdBy,
                MetaData = metaData
            };
            return newActivity;
        }

        private async Task<ActivityViewModel> CreateActivityAsync(string eventCode, string location, DateTime activityDate, long freightSchedulerId, string createdBy)
        {
            var eventModel = await _csfeApiClient.GetEventByCodeAsync(eventCode);
            var newActivity = new ActivityViewModel
            {
                FreightSchedulerId = freightSchedulerId,
                ActivityCode = eventCode,
                ActivityType = eventModel.ActivityType,
                ActivityDescription = eventModel.ActivityDescription,
                Location = location,
                ActivityDate = activityDate,
                CreatedBy = createdBy
            };
            return newActivity;
        }

        /// <summary>
        /// To handle Itinerary Update API which call from ediSON/third-party
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public override async Task<ItineraryViewModel> UpdateAsync(ItineraryViewModel viewModel, params object[] keys)
        {
            ItineraryModel model = await this.Repository.GetAsync(x => x.Id == viewModel.Id, null, x => x.Include(y => y.FreightScheduler));

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", keys)} not found!");
            }

            // Itinerary Import/Update API will pass location code
            // Then, need to get location description by location code
            await MapLocationCodeToDescriptionAsync(viewModel);

            // Some original data of itinerary
            var originalItinerary = new ItineraryModel
            {
                ScheduleId = model.ScheduleId,
                ETADate = model.ETADate,
                ETDDate = model.ETDDate
            };

            // Correct CarrierName from MasterData when user create/update Itinerary from API
            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) || viewModel.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
            {
                await CorrectCarrierNameAsync(viewModel);
            }

            var oldCYOpenDate = model.CYOpenDate;
            var oldCYClosingDate = model.CYClosingDate;

            Mapper.Map(viewModel, model);

            // Only update CYOpenDate/CYClosingDate if ModeOfTransport = sea
            if (model.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) == false)
            {
                model.CYOpenDate = oldCYOpenDate;
                model.CYClosingDate = oldCYClosingDate;
            }

            FreightSchedulerModel scheduler = null;
            // If mode of transport "Sea", try to link FreightScheduler
            var isSeaMode = model.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.OrdinalIgnoreCase);
            if (isSeaMode)
            {
                // Allow to override if current Itinerary not blocked yet.
                scheduler = await MapScheduleToItinerary(model);

                if (scheduler?.Id > 0 && scheduler.IsAllowExternalUpdate)
                {
                    // Check to allow update ATD
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.ATDDate)))
                    {
                        var canUpdateATD = (await _freightSchedulerService.IsReadyContainerManifestAsync(scheduler.Id)).Item1;
                        if (!canUpdateATD)
                        {
                            throw new ApplicationException($"Container manifest not ready for all shipments");
                        }
                    }
                }
            }

            // It Schedule Id is still same
            // The API should skip the updates of the dates if related Scheduler has been LOCKED.
            if (originalItinerary.ScheduleId == model.ScheduleId)
            {
                //PSP-3219: Itinerary = Air no linked to FS -> Ignore
                if (!(model.FreightScheduler?.IsAllowExternalUpdate ?? true))
                {
                    model.ETADate = originalItinerary.ETADate;
                    model.ETDDate = originalItinerary.ETDDate;
                }
            }
            // It Schedule id is different, changed to another
            else
            {

            }

            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }           

            // Add scheduler
            if (scheduler != null && scheduler.Id == 0)
            {
                scheduler.ATDDate = viewModel.ATDDate;
                scheduler.ATADate = viewModel.ATADate;
                scheduler.CYOpenDate = viewModel.CYOpenDate;
                scheduler.CYClosingDate = viewModel.CYClosingDate;

                await _freightSchedulerRepository.AddAsync(scheduler);
                await UnitOfWork.SaveChangesAsync();

                // Need to update Itinerary again with the new FS Id
                model.ScheduleId = scheduler.Id;
                model.FreightScheduler = scheduler;

                Repository.Update(model);
                await UnitOfWork.SaveChangesAsync();

                if (viewModel.ATDDate.HasValue)
                {
                    var activity7001 = await CreateActivityAsync(
                             Event.EVENT_7001,
                             viewModel.LoadingPort,
                             viewModel.ATDDate ?? new DateTime(1, 1, 1),
                             scheduler.Id,
                             viewModel.CreatedBy,
                             "createdfromitineraryapi");

                    await _activityService.CreateAsync(activity7001);
                }

                if (viewModel.ATADate.HasValue)
                {
                    var activity7002 = await CreateActivityAsync(
                            Event.EVENT_7002,
                            viewModel.DischargePort,
                            viewModel.ATADate ?? new DateTime(1, 1, 1),
                            scheduler.Id,
                            viewModel.CreatedBy,
                            "createdfromitineraryapi");

                    await _activityService.CreateAsync(activity7002);
                }
            }

            else
            {
                // Update scheduler must be called after itinerary saved
                Repository.Update(model);
                await UnitOfWork.SaveChangesAsync();
                

                // Store current data to write log later
                var jsonCurrentData = JsonConvert.SerializeObject(scheduler, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

                var oldATDDateScheduler = scheduler?.ATDDate;
                var oldATADateScheduler = scheduler?.ATADate;

                // Only update/trigger event when FreightScheduler is not locked
                if (scheduler != null && scheduler.IsAllowExternalUpdate)
                {
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.ATDDate)))
                    {
                        scheduler.ATDDate = viewModel.ATDDate;
                    }
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.ATADate)))
                    {
                        scheduler.ATADate = viewModel.ATADate;
                    }
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.CYOpenDate)))
                    {
                        scheduler.CYOpenDate = viewModel.CYOpenDate;
                    }
                    if (viewModel.IsPropertyDirty(nameof(ItineraryViewModel.CYClosingDate)))
                    {
                        scheduler.CYClosingDate = viewModel.CYClosingDate;
                    }

                    _freightSchedulerRepository.Update(scheduler);
                    await this.UnitOfWork.SaveChangesAsync();
                    await TriggerEventForFreightScheduler(oldATDDateScheduler, oldATADateScheduler, scheduler.Id, viewModel, model.CreatedBy);

                    // Store new data to write log later
                    var jsonNewData = JsonConvert.SerializeObject(scheduler, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

                    //  Call spu to write log
                    //  [dbo].[spu_WriteLogFreightScheduler]
                    //  @freightSchedulerId BIGINT,
                    //  @jsonCurrentData NVARCHAR(MAX),
                    //  @jsonNewData NVARCHAR(MAX),
                    //  @updatedBy NVARCHAR(512)	
                    var sql = @"[dbo].[spu_WriteLogFreightScheduler] @p0, @p1, @p2, @p3";
                    var parameters = new object[]
                    {
                        scheduler.Id,
                        $"[{jsonCurrentData}]",
                        $"[{jsonNewData}]",
                        scheduler.UpdatedBy
                    };
                    _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

                    if (scheduler.IsAllowExternalUpdate)
                    {
                        await _freightSchedulerService.BroadcastFreightScheduleUpdatesAsync(new List<long> { scheduler.Id }, Schedulers.UpdatedViaItineraryAPI, false, viewModel.UpdatedBy);
                    }
                }
            }

            // Broadcast CYClosingDate to FS
            await BroadcastCYClosingDateAsync(model.Id);

            viewModel = Mapper.Map<ItineraryViewModel>(model);
            return viewModel;
        }

        /// <summary>
        /// To handle to delete Itinerary which called from application UI
        /// </summary>
        /// <param name="itineraryId"></param>
        /// <param name="consignmentId"></param>
        /// <param name="currentUser"></param>
        /// <param name="affiliates"></param>
        /// <returns></returns>
        public async Task DeleteAsync(long itineraryId, long consignmentId, IdentityInfo currentUser, string affiliates)
        {
            UnitOfWork.BeginTransaction();

            var isInternal = currentUser.IsInternal;
            var model = await _consignmentItineraryRepository.GetAsync(x
                => x.ItineraryId == itineraryId && x.ConsignmentId == consignmentId);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {itineraryId} not found!");
            }

            _consignmentItineraryRepository.Remove(model);
            await RemoveShipmentItineraryReferences(model.ShipmentId.Value, model.ItineraryId, isInternal, affiliates);
            await UnitOfWork.SaveChangesAsync();

            await ChangeStageOfBookingAndPOAsync(model.ShipmentId.Value);

            UnitOfWork.CommitTransaction();
        }

        /// <summary>
        /// From Itinerary API Import/Update, view-model contains location code, then must to map location code to location description
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private async Task MapLocationCodeToDescriptionAsync(ItineraryViewModel viewModel)
        {
            var checkingModeOfTransports = new[] { ModeOfTransport.Sea, ModeOfTransport.Air };
            // Itinerary Import/Update API will pass location code
            // Then, need to get location description by location code if Sea or Air
            if (checkingModeOfTransports.Contains(viewModel.ModeOfTransport.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase)
                & (!string.IsNullOrEmpty(viewModel.LoadingPort) || !string.IsNullOrEmpty(viewModel.DischargePort))
            )
            {
                var locations = await _csfeApiClient.GetAllLocationsAsync();
                viewModel.LoadingPort = string.IsNullOrEmpty(viewModel.LoadingPort)
                    ? viewModel.LoadingPort
                    : locations.First(x => x.Name == viewModel.LoadingPort).LocationDescription;
                viewModel.DischargePort = string.IsNullOrEmpty(viewModel.DischargePort)
                    ? viewModel.DischargePort
                    : locations.First(x => x.Name == viewModel.DischargePort).LocationDescription;
            }
        }

        private async Task CorrectCarrierNameAsync(ItineraryViewModel viewModel)
        {
            var carriers = await _csfeApiClient.GetAllCarriesAsync();
            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase))
            {
                var carrierNameFromMasterData = carriers.SingleOrDefault(c =>
                    c.CarrierCode == viewModel.SCAC &&
                    c.ModeOfTransport.Equals(viewModel.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));

                viewModel.CarrierName = carrierNameFromMasterData?.Name;
            }

            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
            {
                var carrierNameFromMasterData = carriers.SingleOrDefault(c =>
                    c.CarrierCode == viewModel.AirlineCode &&
                    c.ModeOfTransport.Equals(viewModel.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));

                viewModel.CarrierName = carrierNameFromMasterData?.Name;
            }
        }

        private bool IsSeaOrAir(string modeOfTransport)
        {
            if (modeOfTransport == null)
            {
                return false;
            }

            return modeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase)
                || modeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);
        }

        public async Task<bool> BroadcastCYClosingDateAsync(long itinearyId)
        {
            var sql = @"

                        DECLARE @FsId TABLE (Id BIGINT);

                        UPDATE FreightSchedulers

                        SET CYClosingDate = T.CYClosingDate

                        OUTPUT inserted.Id
                        INTO @FsId

                        FROM FreightSchedulers FS
                        CROSS APPLY (
	                        SELECT TOP(1) ITI.CYClosingDate
	                        FROM Itineraries ITI
	                        WHERE ITI.Id = @p0
	                        AND ITI.ScheduleId = FS.Id
                        )T
                        WHERE FS.IsAllowExternalUpdate = 1


                        UPDATE Itineraries
                        SET CYClosingDate = T.CYClosingDate
                        FROM Itineraries ITI
                        CROSS APPLY (
	                        SELECT TOP(1) FS.CYClosingDate
	                        FROM FreightSchedulers FS
	                        WHERE FS.Id IN (SELECT Id FROM @FsId)
	                        AND ITI.ScheduleId = FS.Id
                        ) T


                        UPDATE POFulfillments
                        SET CYClosingDate =  T.CYClosingDate
                        FROM POFulfillments POFF
                        CROSS APPLY (
	                        SELECT TOP(1) FS.Id, FS.CYClosingDate
	                        FROM FreightSchedulers FS
	                        INNER JOIN Itineraries ITI ON FS.Id = ITI.ScheduleId
	                        INNER JOIN ConsignmentItineraries CI ON ITI.Id = CI.ItineraryId
	                        INNER JOIN Shipments SHI ON SHI.Id = CI.ShipmentId AND SHI.[Status] = 'Active'
	                        WHERE SHI.POFulfillmentId = POFF.Id
                            ORDER BY ITI.Sequence ASC

                        ) T
                        WHERE T.Id IN (SELECT Id FROM @FsId)
                ";
            var parameters = new object[]
            {
               itinearyId
            };

            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            return true;
        }

        public async Task<bool> UpdateCYClosingDateToPOFulfillmentAsync(long consignmentId)
        {
            var sql = @"
                        DECLARE @Itineraries TABLE (
	                        Id BIGINT,
                            CYClosingDate DATETIME2(7)
                        );

                        INSERT INTO @Itineraries
                        SELECT TOP(1) I.Id, I.CYClosingDate
                        FROM Itineraries I

                        WHERE EXISTS (
								SELECT 1 
								FROM ConsignmentItineraries CSI
								WHERE CSI.ItineraryId = I.Id AND CSI.ConsignmentId = @p0
                                )
						ORDER BY I.[Sequence], I.[Id]

                        UPDATE POFF
                        SET POFF.CYClosingDate =  ITI.CYClosingDate
                        FROM POFulfillments POFF
						INNER JOIN Shipments SHI ON SHI.POFulfillmentId = POFF.Id AND SHI.[Status] = 'Active'
						INNER JOIN ConsignmentItineraries CI ON SHI.Id = CI.ShipmentId AND CI.ConsignmentId = @p0
						INNER JOIN @Itineraries ITI ON ITI.Id = CI.ItineraryId
						WHERE POFF.[Status] = 10
                    ";

            var parameters = new object[]
            {
                consignmentId
            };
            var result = _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            return true;
        }
    }
}
