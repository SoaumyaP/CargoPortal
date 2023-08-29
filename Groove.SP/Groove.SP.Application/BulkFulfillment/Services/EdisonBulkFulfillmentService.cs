using Groove.SP.Application.BulkFulfillment.Services.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using EBooking = Groove.SP.Infrastructure.EBookingManagementAPI;
using Groove.SP.Infrastructure.EBookingManagementAPI;
using System.Linq;
using Groove.SP.Infrastructure.CSFE.Models;
using System.Net;
using Groove.SP.Application.Exceptions;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Dynamic;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.BulkFulfillment.Services
{
    /// <summary>
    /// To handle NVO/Bulk Booking sent to ediSON
    /// </summary>
    public class EdisonBulkFulfillmentService : IEdisonBulkFulfillmentService
    {
        private EBooking.Booking EdisonBooking { set; get; }
        private AuthenticationResult AuthenticationResult { set; get; }

        private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";
        private readonly EBookingManagementAPIConfig _eBookingManagementAPIConfig;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IRepository<IntegrationLogModel> _integrationLogRepository;
        private readonly EBooking.EBookingServiceClient _eBookingServiceClient;
        private readonly IUnitOfWork _unitOfWork;

        private readonly Dictionary<EBooking.IncotermType, EBooking.OceanFreightCharge> IncotermOceanFreightChargeMapping =
            new Dictionary<EBooking.IncotermType, EBooking.OceanFreightCharge>()
            {
                { EBooking.IncotermType.CFR, EBooking.OceanFreightCharge.P},
                { EBooking.IncotermType.CIF, EBooking.OceanFreightCharge.P},
                { EBooking.IncotermType.CIP, EBooking.OceanFreightCharge.P},
                { EBooking.IncotermType.CPT, EBooking.OceanFreightCharge.P},
                { EBooking.IncotermType.DAP, EBooking.OceanFreightCharge.P},
                { EBooking.IncotermType.DAT, EBooking.OceanFreightCharge.P},
                { EBooking.IncotermType.DDP, EBooking.OceanFreightCharge.P},
                { EBooking.IncotermType.EXW, EBooking.OceanFreightCharge.C},
                { EBooking.IncotermType.FAS, EBooking.OceanFreightCharge.C},
                { EBooking.IncotermType.FCA, EBooking.OceanFreightCharge.C},
                { EBooking.IncotermType.FOB, EBooking.OceanFreightCharge.C},
                { EBooking.IncotermType.DPU, EBooking.OceanFreightCharge.P},
            };

        private readonly Dictionary<string, PartyType> PortalToEdisonPartyMapping = new Dictionary<string, PartyType>()
        {
            {
                "shipper", PartyType.SHIPPER
            },
            {
                "consignee", PartyType.CONSIGNEE
            },
            {
                "notify party", PartyType.NOTIFY1
            },
            {
                "also notify", PartyType.NOTIFY2
            },
            {
                "origin agent", PartyType.ORIGINAGENT
            }
        };

        public EdisonBulkFulfillmentService(IOptions<EBookingManagementAPIConfig> eBookingManagementAPIConfigConfig,
            ICSFEApiClient csfeApiClient,
            IHttpClientFactory httpClientfactory,
            IRepository<IntegrationLogModel> integrationLogRepository,
            IUnitOfWorkProvider unitOfWorkProvider)
        {
            _eBookingManagementAPIConfig = eBookingManagementAPIConfigConfig.Value;
            _csfeApiClient = csfeApiClient;
            _integrationLogRepository = integrationLogRepository;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWork();
            _eBookingServiceClient = new EBooking.EBookingServiceClient(_eBookingManagementAPIConfig.APIEndpoint, httpClientfactory);
        }

        public async Task<POFulfillmentBookingRequestModel> CreateBookingRequest(string userName, POFulfillmentModel bulkBooking)
        {
            var bookingRequest = new POFulfillmentBookingRequestModel
            {
                BookingReferenceNumber = bulkBooking.Number + $"{bulkBooking.BookingRequests.Count + 1:00}",
                POFulfillmentId = bulkBooking.Id,
                BookedDate = DateTime.UtcNow,
                Status = POFulfillmentBookingRequestStatus.Active
            };

            EdisonBooking = await CreateEbooking(bulkBooking);

            bookingRequest.RequestContent = JsonConvert.SerializeObject(EdisonBooking);
            bookingRequest.Audit(userName);

            return bookingRequest;
        }

        /// <summary>
        /// Send eSI request to ediSON
        /// </summary>
        /// <param name="poff"></param>
        /// <param name="bookingRequest"></param>
        /// <returns></returns>
        public async Task ProcesseSIAsync(POFulfillmentModel poff, POFulfillmentBookingRequestModel bookingRequest)
        {
            var model = new EBooking.eSI.Booking
            {
                ShipmentNumber = bookingRequest.BookingReferenceNumber,
                SubmissionOn = bookingRequest.BookedDate.ToString(DATE_FORMAT),
                SendID = _eBookingManagementAPIConfig.Id,
                FileCreatedOn = DateTime.UtcNow,
                IsShipperPickUp = poff.IsShipperPickup ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                IsNotifyPartyAsConsignee = poff.IsNotifyPartyAsConsignee ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                IsContainDangerousGoods = poff.IsContainDangerousGoods ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                DangerousGoodsRemarks = EBooking.BooleanOption.N.ToString(),
                ShipmentOn = poff.CargoReadyDate?.ToString(DATE_FORMAT),
                Incoterm = (EBooking.IncotermType)poff.Incoterm,
                Term = (EBooking.Term)poff.LogisticsService,
                Movement = (EBooking.MovementType)poff.MovementType,
                Vessel = poff.VesselName,
                Voyage = poff.VoyageNo,
                Remarks = poff.Remarks,
                ContainerInfo = new List<EBooking.eSI.Container>(),
                Parties = new List<EBooking.eSI.Party>(),
                Products = new List<EBooking.eSI.Product>(),
                Status = poff.IsGeneratePlanToShip ? EBooking.EeSIStatus.C : EBooking.EeSIStatus.N
            };
            model.LoadingPort = (await _csfeApiClient.GetLocationByIdAsync(poff.ShipFrom)).Name;
            model.DischargePort = (await _csfeApiClient.GetLocationByIdAsync(poff.ShipTo)).Name;
            model.ReceiptPort = (await _csfeApiClient.GetLocationByIdAsync(poff.ReceiptPortId.Value)).Name;
            model.DeliveryPort = (await _csfeApiClient.GetLocationByIdAsync(poff.DeliveryPortId.Value)).Name;
            model.OceanFreightCharge = IncotermOceanFreightChargeMapping[model.Incoterm.Value];
            model.PayableAt = model.OceanFreightCharge == EBooking.OceanFreightCharge.C ?
                EBooking.PayableAt.Destination : EBooking.PayableAt.Origin;

            foreach (var load in poff.Loads)
            {
                model.ContainerInfo.Add(new EBooking.eSI.Container
                {
                    Type = (EBooking.EquipmentType)load.EquipmentType,
                    ContainerNo = load.ContainerNumber,
                    SealNo = load.SealNumber,
                    SealNo2 = load.SealNumber2,
                    LoadingDate = load.LoadingDate?.ToString(DATE_FORMAT),
                    GateInDate = load.GateInDate?.ToString(DATE_FORMAT)
                });
            }

            foreach (var contact in poff.Contacts)
            {
                if (PortalToEdisonPartyMapping.TryGetValue(contact.OrganizationRole.ToLowerInvariant(), out var partyType))
                {
                    Organization organization = null;
                    if (contact.OrganizationId != 0)
                    {
                        organization = await _csfeApiClient.GetOrganizationByIdAsync(contact.OrganizationId);
                    }
                    model.Parties.Add(new EBooking.eSI.Party
                    {
                        Type = partyType,
                        CompanyCode = organization?.EdisonCompanyCodeId,
                        CompanyName = contact.CompanyName,
                        CompanyPerson = contact.ContactName,
                        CompanyPhone = contact.ContactNumber,
                        CompanyEmail = contact.ContactEmail,
                        CompanyAddress1 = contact.Address,
                        CompanyAddress2 = contact.AddressLine2,
                        CompanyAddress3 = contact.AddressLine3,
                        CompanyAddress4 = contact.AddressLine4,
                        CountryCode = organization?.Location?.Country?.Code,
                        CityCode = organization?.Location?.Name
                    });
                }
            }

            var orders = poff.Orders.OrderBy(o => o.Id).ToList();
            foreach (var order in orders)
            {
                int sequence = orders.IndexOf(order) + 1;
                var newBookingProduct = new EBooking.eSI.Product
                {
                    Line = sequence.ToString(),
                    ProductNumber = order.ProductCode,
                    MarksNos = order.ShippingMarks,
                    GoodsDesc = order.ProductName,
                    QuantityUnit = order.BookedPackage.HasValue ? (EBooking.QuantityUnit)order.PackageUOM : (EBooking.QuantityUnit?)null,
                    Quantity = order.BookedPackage?.ToString(),
                    UOM = (EBooking.UOM)order.UnitUOM,
                    Piece = order.FulfillmentUnitQty.ToString(),
                    GrossWeightUnit = EBooking.WeightUnit.KG,
                    GrossWeight = order.GrossWeight.HasValue ? order.GrossWeight.ToString() : "0.0",
                    NetWeightUnit = EBooking.WeightUnit.KG,
                    NetWeight = order.NetWeight?.ToString(),
                    CBM = order.Volume?.ToString("f3"),
                    HSCode = order.HsCode,
                    PackingList = new List<EBooking.eSI.Packing>()
                };

                if (newBookingProduct.QuantityUnit == EBooking.QuantityUnit.PC)
                {
                    newBookingProduct.QuantityUnit = EBooking.QuantityUnit.PC;
                }

                var loadDetails = poff.Loads.SelectMany(l => l.Details.Where(ld => ld.POFulfillmentOrderId == order.Id).Select(ld => ld));
                foreach (var loadDetail in loadDetails)
                {
                    newBookingProduct.PackingList.Add(new EBooking.eSI.Packing
                    {
                        ContainerNo = loadDetail.PoFulfillmentLoad.ContainerNumber,
                        Quantity = loadDetail.PackageQuantity.ToString(),
                        Piece = loadDetail.UnitQuantity.ToString(),
                        GrossWeight = loadDetail.GrossWeight.ToString("f3"),
                        CBM = loadDetail.Volume.ToString("f3")
                    });
                }

                model.Products.Add(newBookingProduct);
            }

            await LoginToEdisonAsync();

            // Token is valid, then call another request to process eSI
            _eBookingServiceClient.CurrSessionID = AuthenticationResult.Result.CurrSessionID;
            var eSICall = _eBookingServiceClient.ProcesseSI;
            var eSIResult = await _eBookingServiceClient.ProcesseSI.PostSilentAsync(model);

            if (eSIResult.Status != (int)HttpStatusCode.OK)
            {
                var additionalData = GenerateEdisonBookingIntegrationLog(
                    false,
                    "POST",
                    eSICall.RequestUrl,
                    JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    model.ShipmentNumber,
                    eSIResult.Content,
                    "Process eSI");               

                throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, eSIResult.Result);
            }
            else
            {
                var integrationLog = GenerateEdisonBookingIntegrationLog(
                    true,
                    "POST",
                    eSICall.RequestUrl,
                    JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    model.ShipmentNumber,
                    eSIResult.Content,
                    "Process eSI");

                await _integrationLogRepository.AddAsync(integrationLog);
            }
        }

        private async Task<EBooking.Booking> CreateEbooking(POFulfillmentModel bulkBooking)
        {
            var eBooking = new EBooking.Booking
            {
                ShipmentNumber = bulkBooking.Number + $"{bulkBooking.BookingRequests.Count + 1:00}",
                SubmissionOn = DateTime.UtcNow.ToString(DATE_FORMAT),
                SendID = _eBookingManagementAPIConfig.Id,
                FileCreatedOn = DateTime.UtcNow,
                IsShipperPickUp = bulkBooking.IsShipperPickup ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                IsNotifyPartyAsConsignee = bulkBooking.IsNotifyPartyAsConsignee ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                IsContainDangerousGoods = bulkBooking.IsContainDangerousGoods ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                ShipmentOn = bulkBooking.CargoReadyDate?.ToString(DATE_FORMAT),
                Incoterm = (EBooking.IncotermType)bulkBooking.Incoterm,
                Term = (EBooking.Term)bulkBooking.LogisticsService,
                Movement = (EBooking.MovementType)bulkBooking.MovementType,
                Remarks = bulkBooking.Remarks,
                Containers = new List<EBooking.Container>(),
                Parties = new List<EBooking.Party>(),
                Products = new List<EBooking.Product>(),
                Status = EBooking.Status.N,
                VesselVoyage = string.IsNullOrEmpty(bulkBooking.VesselName) && string.IsNullOrEmpty(bulkBooking.VoyageNo)
                                ? null : $"{bulkBooking.VesselName}/{bulkBooking.VoyageNo}"
            };

            eBooking.LoadingPort = (await _csfeApiClient.GetLocationByIdAsync(bulkBooking.ShipFrom)).Name;
            eBooking.DischargePort = (await _csfeApiClient.GetLocationByIdAsync(bulkBooking.ShipTo)).Name;
            eBooking.ReceiptPort = (await _csfeApiClient.GetLocationByIdAsync(bulkBooking.ReceiptPortId.Value)).Name;
            eBooking.DeliveryPort = (await _csfeApiClient.GetLocationByIdAsync(bulkBooking.DeliveryPortId.Value)).Name;
            eBooking.OceanFreightCharge = IncotermOceanFreightChargeMapping[eBooking.Incoterm.Value];
            eBooking.PayableAt = eBooking.OceanFreightCharge == EBooking.OceanFreightCharge.C ? EBooking.PayableAt.Destination : EBooking.PayableAt.Origin;

            eBooking.Containers = CreateContainers(bulkBooking.Loads);
            eBooking.Parties = await CreateParties(bulkBooking.Contacts);
            eBooking.Products = CreateProducts(bulkBooking.Orders);

            return eBooking;
        }

        private List<EBooking.Container> CreateContainers(IEnumerable<POFulfillmentLoadModel> loads)
        {
            var containers = new List<EBooking.Container>();
            foreach (var loadGroup in loads.GroupBy(l => l.EquipmentType))
            {
                containers.Add(new EBooking.Container
                {
                    Type = (EBooking.EquipmentType)loadGroup.Key,
                    Quantity = loadGroup.Count().ToString()
                });
            }
            return containers;
        }

        private async Task<List<EBooking.Party>> CreateParties(IEnumerable<POFulfillmentContactModel> contacts)
        {
            var parties = new List<EBooking.Party>();
            foreach (var contact in contacts)
            {
                if (PortalToEdisonPartyMapping.TryGetValue(contact.OrganizationRole.ToLowerInvariant(), out var partyType))
                {
                    Organization organization = null;
                    if (contact.OrganizationId != 0)
                    {
                        organization = await _csfeApiClient.GetOrganizationByIdAsync(contact.OrganizationId);
                    }
                    parties.Add(new EBooking.Party
                    {
                        Type = partyType,
                        CompanyCode = organization?.EdisonCompanyCodeId,
                        CompanyName = contact.CompanyName,
                        CompanyPerson = contact.ContactName,
                        CompanyPhone = contact.ContactNumber,
                        CompanyEmail = contact.ContactEmail,
                        CompanyAddress1 = contact.Address,
                        CompanyAddress2 = contact.AddressLine2,
                        CompanyAddress3 = contact.AddressLine3,
                        CompanyAddress4 = contact.AddressLine4,
                        CountryCode = organization?.Location?.Country?.Code,
                        CityCode = organization?.Location?.Name
                    });
                }
            }
            return parties;
        }

        private List<EBooking.Product> CreateProducts(IEnumerable<POFulfillmentOrderModel> orders)
        {
            var products = new List<EBooking.Product>();

            for (int i = 0; i < orders.Count(); i++)
            {
                var order = orders.ToArray()[i];
                var newBookingProduct = new EBooking.Product
                {
                    Line = $"{i + 1}",
                    ProductNumber = order.ProductCode ?? string.Empty,
                    MarksNos = order.ShippingMarks,
                    GoodsDesc = order.ProductName,
                    QuantityUnit = order.BookedPackage.HasValue ? (EBooking.QuantityUnit)order.PackageUOM : (EBooking.QuantityUnit?)null,
                    Quantity = order.BookedPackage?.ToString(),
                    UOM = (EBooking.UOM)order.UnitUOM,
                    Piece = order.FulfillmentUnitQty.ToString(),
                    GrossWeightUnit = EBooking.WeightUnit.KG,
                    GrossWeight = order.GrossWeight.HasValue ? order.GrossWeight.ToString() : "0.0",
                    NetWeightUnit = EBooking.WeightUnit.KG,
                    NetWeight = order.NetWeight?.ToString() ?? "0.00",
                    CBM = order.Volume?.ToString("f3"),
                    HSCode = order.HsCode,
                    OrderNumber = order.CustomerPONumber ?? string.Empty
                };

                if (newBookingProduct.QuantityUnit == EBooking.QuantityUnit.PC)
                {
                    newBookingProduct.QuantityUnit = EBooking.QuantityUnit.PC;
                }
                products.Add(newBookingProduct);
            }
            return products;
        }

        private IntegrationLogModel GenerateEdisonBookingIntegrationLog(bool isSuccessful, string requestMethod, string requestUrl, string requestBodyContent, string remark, string responseBodyContent, string profile = "Booking submission")
        {
            var apiName = "[" + requestMethod.ToUpper() + "] " + requestUrl;

            // try to indent Json string if possible
            try
            {
                var tmpObj = JsonConvert.DeserializeObject(responseBodyContent);
                responseBodyContent = JsonConvert.SerializeObject(tmpObj, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            catch { }

            var entityToStore = new IntegrationLogModel
            {
                APIName = apiName,
                APIMessage = requestBodyContent,
                EDIMessageRef = string.Empty,
                EDIMessageType = string.Empty,
                PostingDate = DateTime.UtcNow,
                Profile = profile,
                Status = isSuccessful ? IntegrationStatus.Succeed : IntegrationStatus.Failed,
                Remark = remark,
                Response = responseBodyContent
            };
            return entityToStore;

        }

        public async Task LoginToEdisonAsync()
        {
            var login = new EBooking.UserAccount
            {
                Id = _eBookingManagementAPIConfig.Id,
                Password = _eBookingManagementAPIConfig.Password
            };

            AuthenticationResult = await _eBookingServiceClient.Authentication.PostSilentAsync(login);

            if (AuthenticationResult.Status != (int)HttpStatusCode.OK)
            {
                var obfuscatedAccount = new EBooking.UserAccount
                {
                    Id = _eBookingManagementAPIConfig.Id,
                    Password = "Not shown as security, ask the team"
                };
                var bookingAuthentication = _eBookingServiceClient.Authentication;
                var additionalData = GenerateEdisonBookingIntegrationLog(
                    false,
                    "POST",
                    bookingAuthentication.RequestUrl,
                    JsonConvert.SerializeObject(obfuscatedAccount, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    EdisonBooking.ShipmentNumber,
                    AuthenticationResult.Content);

                // Try to get result from response by dynamic object
                // As authenticationResult.Result will not content value as model binding failed
                ExpandoObject authenticationFailed = JsonConvert.DeserializeObject<ExpandoObject>(AuthenticationResult.Content, new ExpandoObjectConverter());
                if (authenticationFailed.Any(x => "result".Equals(x.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    var authenticationFailedResult = authenticationFailed.FirstOrDefault(x => "result".Equals(x.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString();
                    throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, authenticationFailedResult);
                }
                else
                {
                    throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, AuthenticationResult.Content);
                }

            }
        }

        public async Task SendEBookingToEdisonAsync()
        {
            _eBookingServiceClient.CurrSessionID = AuthenticationResult.Result.CurrSessionID;
            var bookingCall = _eBookingServiceClient.Bookings;
            var bookingResult = await _eBookingServiceClient.Bookings.PostSilentAsync(EdisonBooking);

            var status = bookingResult.Status;
            var result = bookingResult.Result ?? string.Empty;

            // It is duplicated error, try to re-send with Status = C
            if (status == (int)HttpStatusCode.UnprocessableEntity && result.StartsWith("Skip process Shipping Order", StringComparison.OrdinalIgnoreCase))
            {
                EdisonBooking.Status = Status.C;
                bookingResult = await _eBookingServiceClient.Bookings.PostSilentAsync(EdisonBooking);
            }

            status = bookingResult.Status;
            result = bookingResult.Result ?? string.Empty;

            if (status != (int)HttpStatusCode.OK)
            {
                var additionalData = GenerateEdisonBookingIntegrationLog(
                    false,
                    "POST",
                    bookingCall.RequestUrl,
                    JsonConvert.SerializeObject(EdisonBooking, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    EdisonBooking.ShipmentNumber,
                    bookingResult.Content);

                throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, result);
            }
            else
            {
                var integrationLog = GenerateEdisonBookingIntegrationLog(
                    true,
                    "POST",
                    bookingCall.RequestUrl,
                    JsonConvert.SerializeObject(EdisonBooking, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    EdisonBooking.ShipmentNumber,
                    bookingResult.Content);

                await _integrationLogRepository.AddAsync(integrationLog);
            }
        }
    }
}
