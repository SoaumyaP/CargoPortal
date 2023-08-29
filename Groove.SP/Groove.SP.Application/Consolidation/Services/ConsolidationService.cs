using Groove.SP.Application.CargoDetail.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consolidation.Services
{
    public class ConsolidationService : ServiceBase<ConsolidationModel, ConsolidationViewModel>, IConsolidationService
    {
        private readonly IConsignmentRepository _consignmentRepository;
        private readonly IConsolidationRepository _consolidationRepository;
        private readonly IRepository<ShipmentLoadModel> _shipmentLoadRepository;
        private readonly IRepository<ShipmentLoadDetailModel> _shipmentLoadDetailRepository;
        private readonly IRepository<ShipmentContactModel> _shipmentContactRepository;
        private readonly IRepository<ContainerModel> _containerRepository;
        private readonly IRepository<BillOfLadingModel> _billOfLadingRepository;
        private readonly IRepository<ItineraryModel> _itineraryRepository;
        private readonly IServiceProvider _services;
        private readonly IDataQuery _dataQuery;

        /// <summary>
        /// It is lazy service injection.
        /// Please do not use it directly, use ContainerService instead.
        /// </summary>
        private IContainerService _containerServiceLazy;
        public IContainerService ContainerService
        {
            get
            {
                if (_containerServiceLazy == null)
                {
                    _containerServiceLazy = _services.GetRequiredService<IContainerService>();
                }
                return _containerServiceLazy;
            }
        }

        public ConsolidationService(IUnitOfWorkProvider unitOfWorkProvider,
            IConsolidationRepository consolidationRepository,
            IConsignmentRepository consignmentRepository,
            IRepository<ShipmentLoadModel> shipmentLoadRepository,
            IRepository<ShipmentLoadDetailModel> shipmentLoadDetailRepository,
            IRepository<ShipmentContactModel> shipmentContactRepository,
            IRepository<ContainerModel> containerRepository,
            IRepository<BillOfLadingModel> billOfLadingRepository,
            IRepository<ItineraryModel> itineraryRepository,
            IServiceProvider services,
            IDataQuery dataQuery)
            : base(unitOfWorkProvider)
        {
            _services = services;
            _consignmentRepository = consignmentRepository;
            _consolidationRepository = consolidationRepository;
            _shipmentLoadRepository = shipmentLoadRepository;
            _shipmentLoadDetailRepository = shipmentLoadDetailRepository;
            _shipmentContactRepository = shipmentContactRepository;
            _containerRepository = containerRepository;
            _billOfLadingRepository = billOfLadingRepository;
            _itineraryRepository = itineraryRepository;
            _dataQuery = dataQuery;
        }

        public async Task<IEnumerable<ConsolidationViewModel>> GetConsolidationsByConsignmentAsync(long consignmentId)
        {
            var result = await Repository.Query(s => s.ShipmentLoads.Any(l => l.ConsignmentId == consignmentId)).ToListAsync();
            return Mapper.Map<IEnumerable<ConsolidationViewModel>>(result);
        }

        public async Task<IEnumerable<ConsolidationViewModel>> GetConsolidationsByShipmentAsync(long shipmentId)
        {
            var result = await Repository.Query(s => s.ShipmentLoads.Any(l => l.ShipmentId == shipmentId)).ToListAsync();
            return Mapper.Map<IEnumerable<ConsolidationViewModel>>(result);
        }

        public async Task<ConsolidationInternalViewModel> GetInternalConsolidationAsync(long consolidationId, bool isInternal, string affiliates = "")
        {
            var result = await Repository.GetAsNoTrackingAsync(c => c.Id == consolidationId, null, x => x.Include(y => y.ShipmentLoads));
            if (result != null)
            {
                // Check data-access-right
                if (!isInternal)
                {
                    var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                    var shipmentIds = result.ShipmentLoads?.Where(sl => sl.ShipmentId != null)?.Select(sl => sl.ShipmentId);
                    var contacts = await _shipmentContactRepository.QueryAsNoTracking(x => shipmentIds.Any(id => id == x.ShipmentId)).ToListAsync();
                    if (!contacts.Select(c => c.OrganizationId).Any(orgId => listOfAffiliates.Contains(orgId)))
                    {
                        return null;
                    }
                }
            }
            
            return Mapper.Map<ConsolidationInternalViewModel>(result);
        }

        private async Task<string> GenerateLoadPlanID(DateTime createdDate)
        {
            var nextSequenceValue = await _consolidationRepository.GetNextLoadPlanIDSequenceValueAsync();
            return $"LOAD{createdDate.ToString("yyMM")}{nextSequenceValue}";
        }

        public async Task<ConsolidationViewModel> CreateConsolidationAsync(InputConsolidationViewModel model)
        {
            var consolidation = Mapper.Map<ConsolidationModel>(model);
            var consignment = await _consignmentRepository.GetAsNoTrackingAsync(x => x.Id == model.ConsignmentId, null,
                x => x.Include(c => c.ShipmentLoads).Include(c => c.Shipment));

            if (consignment == null)
            {
                throw new AppEntityNotFoundException($"Consignment with the id {string.Join(", ", model.ConsignmentId)} not found!");
            }

            UnitOfWork.BeginTransaction();

            consolidation.Stage = ConsolidationStage.New;
            consolidation.ModeOfTransport = consignment.ModeOfTransport;
            consolidation.ConsolidationNo = await GenerateLoadPlanID(DateTime.UtcNow);

            await Repository.AddAsync(consolidation);
            await UnitOfWork.SaveChangesAsync();

            // Update OR Create ShipmentLoad

            var shipmentLoad = consignment.ShipmentLoads.FirstOrDefault(x => x.ConsolidationId == null);
            // Exists and has NO link with Consolidation yet
            if (shipmentLoad != null)
            {
                shipmentLoad.ConsolidationId = consolidation.Id;
                shipmentLoad.EquipmentType = consolidation.EquipmentType;
                shipmentLoad.CarrierBookingNo = consolidation.CarrierSONo;
                shipmentLoad.LoadingPlace = consignment.Shipment.ShipFrom;
                shipmentLoad.Audit(consolidation.CreatedBy);
                _shipmentLoadRepository.Update(shipmentLoad);
                await UnitOfWork.SaveChangesAsync();
            }
            else
            {
                var newShipmentLoad = new ShipmentLoadModel
                {
                    ConsolidationId = consolidation.Id,
                    ConsignmentId = consignment.Id,
                    ShipmentId = consignment.ShipmentId,
                    LoadingPlace = consignment.Shipment.ShipFrom,
                    ModeOfTransport = consignment.Shipment.ModeOfTransport,
                    EquipmentType = consolidation.EquipmentType,
                    CarrierBookingNo = consolidation.CarrierSONo,
                    IsFCL = false
                };
                newShipmentLoad.Audit(consolidation.CreatedBy);
                await _shipmentLoadRepository.AddAsync(newShipmentLoad);
                await UnitOfWork.SaveChangesAsync();
            }

            UnitOfWork.CommitTransaction();

            return Mapper.Map<ConsolidationViewModel>(consolidation);
        }

        public async Task<ConsolidationViewModel> UpdateConsolidationAsync(UpdateConsolidationViewModel model, long consolidationId, string userName)
        {
            var consolidation = await Repository.GetAsync(x => x.Id == model.Id && x.Stage == ConsolidationStage.New);

            if (consolidation == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", model.Id)} not found!");
            }

            Mapper.Map(model, consolidation);

            consolidation.Audit(userName);

            await this.UnitOfWork.SaveChangesAsync();

            var result = Mapper.Map<ConsolidationViewModel>(consolidation);
            return result;
        }

        public async Task<ShipmentLoadViewModel> CreateLinkingConsignmentAsync(long consolidationId, long consignmentId, ConsignmentViewModel viewModel, string currentUser)
        {
            var consignment = await _consignmentRepository.GetAsNoTrackingAsync(c => c.Id == consignmentId, null, x => x.Include(y => y.Shipment));

            if (consignment == null)
            {
                throw new AppEntityNotFoundException($"Consignment with the id #{consignmentId} not found!");
            }

            var consolidation = await Repository.GetAsNoTrackingAsync(c => c.Id == consolidationId);

            if (consolidation == null)
            {
                throw new AppEntityNotFoundException($"Consolidation with the id #{consolidationId} not found!");
            }

            // Update/Create ShipmentLoads: To create the link between Consolidation and Consignment

            var shipmentLoad = await _shipmentLoadRepository.GetAsync(
                sl => sl.ConsignmentId == consignment.Id && sl.IsFCL == false && sl.ConsolidationId == null);
            // Exists and has NO link with Consolidation yet
            if (shipmentLoad != null)
            {
                shipmentLoad.ConsolidationId = consolidation.Id;
                shipmentLoad.EquipmentType = consolidation.EquipmentType;
                shipmentLoad.CarrierBookingNo = consolidation.CarrierSONo;
                shipmentLoad.ContainerId = consolidation.ContainerId;
                shipmentLoad.LoadingPlace = consignment.Shipment.ShipFrom;
                shipmentLoad.Audit(currentUser);
                _shipmentLoadRepository.Update(shipmentLoad);
            }
            else
            {
                var newShipmentLoad = new ShipmentLoadModel
                {
                    ConsolidationId = consolidation.Id,
                    ConsignmentId = consignment.Id,
                    ShipmentId = consignment.ShipmentId,
                    LoadingPlace = consignment.Shipment.ShipFrom,
                    ModeOfTransport = consignment.Shipment.ModeOfTransport,
                    EquipmentType = consolidation.EquipmentType,
                    CarrierBookingNo = consolidation.CarrierSONo,
                    ContainerId = consolidation.ContainerId,
                    IsFCL = false
                };
                newShipmentLoad.Audit(currentUser);
                await _shipmentLoadRepository.AddAsync(newShipmentLoad);
            }

            await UnitOfWork.SaveChangesAsync();
            return Mapper.Map<ShipmentLoadViewModel>(shipmentLoad);
        }

        public async Task ValidateConfirmConsolidationAsync(long consolidationId)
        {
            var consolidation = await Repository.GetAsNoTrackingAsync(c => c.Id == consolidationId && c.Stage == ConsolidationStage.New,
                null, 
                x => x.Include(c => c.ShipmentLoads).ThenInclude(sl => sl.ShipmentLoadDetails));

            if (consolidation == null)
            {
                throw new AppEntityNotFoundException($"Object with id = {consolidationId} not found!");
            }

            // Bussiness validations
            if (consolidation.ShipmentLoads == null || consolidation.ShipmentLoads.Count == 0)
            {
                throw new AppValidationException($"noCargoWasLoaded#Please load the cargo detail into the consolidation plan.");
            }

            if (consolidation.ShipmentLoads.Any(sl => sl.ShipmentLoadDetails == null || sl.ShipmentLoadDetails.Count <= 0))
            {
                throw new AppValidationException($"noCargoWasLoaded#Please load the cargo detail into the consolidation plan.");
            }

            if (consolidation.TotalPackage <= 0)
            {
                throw new AppValidationException($"totalPackageIsEqualToZero#Subtotal quantity of Loaded Package must be greater than 0.");
            }

            if (string.IsNullOrEmpty(consolidation.CarrierSONo) || string.IsNullOrEmpty(consolidation.ContainerNo) || string.IsNullOrEmpty(consolidation.SealNo))
            {
                throw new AppValidationException($"missingContainerInfo#CarrierNo, ContainerNo, SealNo cannot be empty.");
            }

            if (!consolidation.ContainerId.HasValue)
            {
                consolidation.ContainerId = 0;
            }
            var isDuplicatedContainer = await ContainerService.IsDuplicatedContainerAsync(
                consolidation.ContainerNo,
                consolidation.CarrierSONo,
                consolidation.ContainerId.Value
            );

            if (isDuplicatedContainer)
            {
                throw new AppValidationException($"duplicateOnContainerNoAndCarrierSONo#Carrier SO No. and Container No. are duplicated.");
            }
        }

        public async Task<ConsolidationInternalViewModel> ConfirmConsolidationAsync(long consolidationId, bool isInternal, string userName)
        {
            await ValidateConfirmConsolidationAsync(consolidationId);

            UnitOfWork.BeginTransaction();

            Func<IQueryable<ConsolidationModel>, IQueryable<ConsolidationModel>> includeProperties =  
                x => x.Include(c => c.ShipmentLoads)
                .ThenInclude(sl => sl.Consignment)
                .ThenInclude(c => c.ConsignmentItineraries)
                .Include(c => c.ShipmentLoadDetails);

            var consolidation = await Repository.GetAsync(c => c.Id == consolidationId && c.Stage == ConsolidationStage.New, null, includeProperties);

            consolidation.Stage = ConsolidationStage.Confirmed;

            #region Add/Update Container

            var firstConsignment = consolidation.ShipmentLoads.FirstOrDefault().Consignment;
            var newContainer = new ContainerViewModel
            {
                ContainerNo = consolidation.ContainerNo,
                LoadPlanRefNo = consolidation.ConsolidationNo,
                ContainerType = consolidation.EquipmentType,
                ShipFrom = firstConsignment.ShipFrom ?? string.Empty,
                ShipFromETDDate = firstConsignment.ShipFromETDDate,
                ShipTo = firstConsignment.ShipTo ?? string.Empty,
                ShipToETADate = firstConsignment.ShipToETADate,
                SealNo = consolidation.SealNo,
                Movement = firstConsignment.Movement,
                TotalGrossWeight = consolidation.TotalGrossWeight,
                TotalGrossWeightUOM = consolidation.TotalGrossWeightUOM,
                TotalNetWeight = consolidation.TotalNetWeight,
                TotalNetWeightUOM = consolidation.TotalNetWeightUOM,
                TotalPackage = Convert.ToInt32(consolidation.TotalPackage),
                TotalPackageUOM = consolidation.TotalPackageUOM,
                TotalVolume = consolidation.TotalVolume,
                TotalVolumeUOM = consolidation.TotalVolumeUOM,
                CarrierSONo = consolidation.CarrierSONo,
                SealNo2 = consolidation.SealNo2,
                LoadingDate = consolidation.LoadingDate,
                IsFCL = false
            };

            if (!consolidation.ContainerId.HasValue)
            {
                var newContainerModel = Mapper.Map<ContainerModel>(newContainer);
                newContainerModel.Audit(userName);

                await _containerRepository.AddAsync(newContainerModel);
                await UnitOfWork.SaveChangesAsync();
                consolidation.Container = newContainerModel;
            }
            else
            {
                newContainer.Id = consolidation.ContainerId.Value;
                var containerModel = await _containerRepository.GetAsync(c => c.Id == consolidation.ContainerId.Value);

                Mapper.Map(newContainer, containerModel);
                containerModel.Audit(userName);

                _containerRepository.Update(containerModel);
                await UnitOfWork.SaveChangesAsync();
            }
            #endregion


            #region Update container info into ShipmentLoads/ ShipmentLoadDetails
            foreach (var shipmentLoad in consolidation.ShipmentLoads)
            {
                shipmentLoad.ContainerId = consolidation.Container.Id;
                shipmentLoad.EquipmentType = consolidation.Container.ContainerType;
            }

            foreach (var shipmentLoadDetail in consolidation.ShipmentLoadDetails)
            {
                shipmentLoadDetail.ContainerId = consolidation.Container.Id;
            }
            #endregion

            #region Create linkage between Container and Itinerary (dbo.ContainerItineraries)

            var itineraryIds = consolidation.ShipmentLoads.SelectMany(x => x.Consignment.ConsignmentItineraries.Select(y => y.ItineraryId));

            var itineraryListModel = await _itineraryRepository.Query(i => itineraryIds.Any(a => a == i.Id) && i.ModeOfTransport == ModeOfTransport.Sea,
                null,
                x => x.Include(y => y.ContainerItineraries)).ToListAsync();

            foreach (var itineraryModel in itineraryListModel)
            {
                if (!itineraryModel.ContainerItineraries.Any(ci => ci.ContainerId == consolidation.Container.Id))
                {
                    var newContainerItinerary = new ContainerItineraryModel()
                    {
                        ContainerId = consolidation.Container.Id
                    };
                    newContainerItinerary.Audit(userName);
                    itineraryModel.ContainerItineraries.Add(newContainerItinerary);
                }
            }

            #endregion

            consolidation.Audit(userName);
            Repository.Update(consolidation);
            await UnitOfWork.SaveChangesAsync();

            UnitOfWork.CommitTransaction();

            return Mapper.Map<ConsolidationInternalViewModel>(consolidation);
        }

        public async Task<ConsolidationInternalViewModel> UnconfirmConsolidationAsync(long consolidationId, bool isInternal, string userName)
        {
            var consolidation = await Repository.GetAsync(c => c.Id == consolidationId && c.Stage == ConsolidationStage.Confirmed);

            if (consolidation == null)
            {
                throw new AppEntityNotFoundException($"Object with id = {consolidationId} not found!");
            }
            consolidation.Stage = ConsolidationStage.New;
            Repository.Update(consolidation);
            await UnitOfWork.SaveChangesAsync();
            return Mapper.Map<ConsolidationInternalViewModel>(consolidation);
        }

        public async Task<bool> DeleteLinkingConsignmentAsync(long consolidationId, long consignmentId, string currentUser)
        {
            UnitOfWork.BeginTransaction();

            Func<IQueryable<ShipmentLoadModel>, IQueryable<ShipmentLoadModel>> includeProperties = x => x
                .Include(c => c.BillOfLadingShipmentLoads)
                .Include(sl => sl.Shipment)
                .ThenInclude(c => c.ShipmentBillOfLadings);

            var shipmentLoad = await _shipmentLoadRepository.GetAsync(sl => sl.ConsignmentId == consignmentId && sl.ConsolidationId == consolidationId, null, includeProperties);

            if (shipmentLoad == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            // Remove Consolidation out of ShipmentLoads record.
            shipmentLoad.ConsolidationId = null;
            shipmentLoad.ContainerId = null;
            shipmentLoad.Audit(currentUser);
            _shipmentLoadRepository.Update(shipmentLoad);

            //Remove ShipmentLoadDetails of Consolidation and Consignment.
            var shipmentLoadDetails = await _shipmentLoadDetailRepository.Query(sld => sld.ConsignmentId == consignmentId && sld.ConsolidationId == consolidationId).ToListAsync();
            _shipmentLoadDetailRepository.RemoveRange(shipmentLoadDetails.ToArray());
            await UnitOfWork.SaveChangesAsync();

            var linkedBillOfLadingIds = shipmentLoad.Shipment.ShipmentBillOfLadings?.Select(sbl => sbl.BillOfLadingId);

            if (linkedBillOfLadingIds != null)
            {
                var linkedBillOfLadings = _billOfLadingRepository.Query(x => linkedBillOfLadingIds.Any(a => a == x.Id),
                    null,
                    x => x.Include(y => y.ShipmentBillOfLadings)
                    .Include(y => y.BillOfLadingShipmentLoads)
                    .Include(y => y.BillOfLadingConsignments)
                    .Include(y => y.Consignments)
                    .ThenInclude(y => y.ConsignmentItineraries));

                foreach (var billOfLading in linkedBillOfLadings)
                {
                    /* Note: If the shipment of the removed consignment is sharing the same House BL with other shipments, system will remove the linkage between the shipment and House BL.
                     * Remove these records correspondingly based on the removed ShipmentId and its HouseBlId for:
                     * 
                     * dbo.ShipmentBillOfLadings
                     * 
                     * dbo.BillOfLadingConsignments
                     * 
                     * dbo.BillOfLadingShipmentLoads
                     * 
                     * dbo.Consignments/ dbo.ConsignmentItineraries
                     */
                    if (billOfLading.ShipmentBillOfLadings?.Count > 1)
                    {
                        #region If the removing House BL is linking with a Master BL#, also remove MasterBLId out of Consignments/ConsignmentItineraries

                        var currentConsignment = billOfLading.Consignments.Where(x => x.Id == consignmentId).SingleOrDefault();
                        if (currentConsignment != null)
                        {
                            if (currentConsignment.MasterBillId.HasValue)
                            {
                                currentConsignment.MasterBillId = null;
                                currentConsignment.Audit(currentUser);
                            }
                            
                            foreach (var consignmentItinerary in currentConsignment.ConsignmentItineraries)
                            {
                                if (consignmentItinerary.MasterBillId.HasValue)
                                {
                                    consignmentItinerary.MasterBillId = null;
                                    currentConsignment.Audit(currentUser);
                                }
                            }
                        }
                        #endregion

                        // Remove related shipment out of bill of lading
                        billOfLading.ShipmentBillOfLadings = billOfLading.ShipmentBillOfLadings.Where(
                            x => x.ShipmentId != shipmentLoad.ShipmentId).ToList();

                        // Remove related shipment load out of bill of lading
                        billOfLading.BillOfLadingShipmentLoads = billOfLading.BillOfLadingShipmentLoads.Where(
                            x => x.ShipmentLoadId != shipmentLoad.Id).ToList();

                        // Remove related consignment out of bill of lading
                        billOfLading.BillOfLadingConsignments = billOfLading.BillOfLadingConsignments.Where(
                            x => x.ShipmentId != shipmentLoad.ShipmentId).ToList();

                        billOfLading.Consignments = billOfLading.Consignments
                            .Where(x => x.ShipmentId != shipmentLoad.ShipmentId).ToList();
                    }
                    else
                    {
                        // Remove ConsolidationId out of BillOfLadingShipmentLoads

                        var billOfLadingShipmentLoad = billOfLading.BillOfLadingShipmentLoads.FirstOrDefault(x => x.ShipmentLoadId == shipmentLoad.Id);
                        if (billOfLadingShipmentLoad != null)
                        {
                            billOfLadingShipmentLoad.ConsolidationId = null;
                            billOfLadingShipmentLoad.ContainerId = null;
                            billOfLadingShipmentLoad.Audit(currentUser);
                        }
                    }
                }
                _billOfLadingRepository.UpdateRange(
                    linkedBillOfLadings.ToArray());

                await UnitOfWork.SaveChangesAsync();
            }

            await UpdateConsolidationTotalAmountAsync(consolidationId);
            await UnitOfWork.SaveChangesAsync();
            UnitOfWork.CommitTransaction();
            return true;
        }

        public async Task<IEnumerable<ShipmentLoadDetailViewModel>> LoadCargoDetail(long consolidationId, List<CargoDetailLoadViewModel> model)
        {
            UnitOfWork.BeginTransaction();

            var loadedItem = await _shipmentLoadDetailRepository.Query(sl => sl.ConsolidationId == consolidationId).ToListAsync();
            _shipmentLoadDetailRepository.RemoveRange(loadedItem.ToArray());
            await UnitOfWork.SaveChangesAsync();

            var shipmentLoadDetails = new List<ShipmentLoadDetailModel>();
            foreach (var cargoDetail in model)
            {
                var shipmentLoadDetail = new ShipmentLoadDetailModel
                {
                    ConsolidationId = consolidationId,
                    Sequence = cargoDetail.Sequence,
                    ShipmentId = cargoDetail.ShipmentId,
                    ConsignmentId = cargoDetail.ConsignmentId,
                    CargoDetailId = cargoDetail.Id,
                    ShipmentLoadId = cargoDetail.ShipmentLoadId,
                    ContainerId = cargoDetail.ContainerId,
                    Package = cargoDetail.Package.Value,
                    PackageUOM = cargoDetail.PackageUOM,
                    Unit = cargoDetail.Unit.Value,
                    UnitUOM = cargoDetail.UnitUOM,
                    Volume = cargoDetail.Volume.Value,
                    VolumeUOM = cargoDetail.VolumeUOM,
                    GrossWeight = cargoDetail.GrossWeight,
                    GrossWeightUOM = cargoDetail.GrossWeightUOM,
                    NetWeight = cargoDetail.NetWeight,
                    NetWeightUOM = cargoDetail.NetWeightUOM
                };
                shipmentLoadDetails.Add(shipmentLoadDetail);
            }

            await _shipmentLoadDetailRepository.AddRangeAsync(shipmentLoadDetails.ToArray());
            await UnitOfWork.SaveChangesAsync();

            await UpdateConsolidationTotalAmountAsync(consolidationId);
            await UnitOfWork.SaveChangesAsync();

            UnitOfWork.CommitTransaction();

            return Mapper.Map<IEnumerable<ShipmentLoadDetailViewModel>>(shipmentLoadDetails);
        }

        public async Task UpdateConsolidationTotalAmountAsync(long consolidationId)
        {
            var consolidation = await Repository.GetAsync(c => c.Id == consolidationId, null, 
                x => x.Include(y => y.ShipmentLoadDetails));
            if (consolidation == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {consolidationId} not found!");
            }
            consolidation.TotalGrossWeight = consolidation.ShipmentLoadDetails?.Sum(sl => sl.GrossWeight) ?? 0;
            consolidation.TotalNetWeight = consolidation.ShipmentLoadDetails?.Sum(sl => sl.NetWeight) ?? 0;
            consolidation.TotalPackage = consolidation.ShipmentLoadDetails?.Sum(sl => sl.Package) ?? 0;
            consolidation.TotalVolume = consolidation.ShipmentLoadDetails?.Sum(sl => sl.Volume) ?? 0;

            var firstShipmentLoadDetail = consolidation.ShipmentLoadDetails?.OrderBy(sl => sl.Sequence).FirstOrDefault();
            consolidation.TotalGrossWeightUOM = firstShipmentLoadDetail?.GrossWeightUOM ?? null;
            consolidation.TotalNetWeightUOM = firstShipmentLoadDetail?.NetWeightUOM ?? null;
            consolidation.TotalPackageUOM = firstShipmentLoadDetail?.PackageUOM ?? null;
            consolidation.TotalVolumeUOM = firstShipmentLoadDetail?.VolumeUOM ?? null;
            Repository.Update(consolidation);
        }
    }
}
