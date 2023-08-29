using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using EBooking = Groove.SP.Infrastructure.EBookingManagementAPI;
using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Exceptions;
using System.Net;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Infrastructure.CSFE.Models;
using Groove.SP.Infrastructure.EBookingManagementAPI;
using System.Dynamic;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.POFulfillment.Services
{
    public class EdiSonBookingService : IEdiSonBookingService
    {
        private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";
        private readonly EBookingManagementAPIConfig _eBookingManagementAPIConfig;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IRepository<PurchaseOrderModel> _poRepository;
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
            }
        };

        public EdiSonBookingService(IOptions<EBookingManagementAPIConfig> eBookingManagementAPIConfigConfig,
            ICSFEApiClient csfeApiClient,
            IRepository<PurchaseOrderModel> poRepository,
            IHttpClientFactory httpClientfactory,
            IRepository<IntegrationLogModel> integrationLogRepository,
            IUnitOfWorkProvider unitOfWorkProvider)
        {
            _eBookingManagementAPIConfig = eBookingManagementAPIConfigConfig.Value;
            _csfeApiClient = csfeApiClient;
            _poRepository = poRepository;
            _integrationLogRepository = integrationLogRepository;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWork();
            _eBookingServiceClient = new EBooking.EBookingServiceClient(_eBookingManagementAPIConfig.APIEndpoint, httpClientfactory);
        }


        public async Task<POFulfillmentBookingRequestModel> CreateBookingRequestAsync(string userName, POFulfillmentModel poff, bool sendToEdison = true)
        {
            var bookingRequest = new POFulfillmentBookingRequestModel
            {
                BookingReferenceNumber = poff.Number + $"{poff.BookingRequests.Count + 1:00}",
                POFulfillmentId = poff.Id,
                BookedDate = poff.BookingDate.Value,
                Status = POFulfillmentBookingRequestStatus.Active
            };

            var booking = new EBooking.Booking
            {
                ShipmentNumber = bookingRequest.BookingReferenceNumber,
                SubmissionOn = bookingRequest.BookedDate.ToString(DATE_FORMAT),
                SendID = _eBookingManagementAPIConfig.Id,
                FileCreatedOn = bookingRequest.BookedDate,
                IsShipperPickUp = poff.IsShipperPickup ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                IsNotifyPartyAsConsignee = poff.IsNotifyPartyAsConsignee ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                IsContainDangerousGoods = poff.IsContainDangerousGoods ? EBooking.BooleanOption.Y : EBooking.BooleanOption.N,
                ShipmentOn = poff.CargoReadyDate?.ToString(DATE_FORMAT),
                Incoterm = (EBooking.IncotermType)poff.Incoterm,
                Term = (EBooking.Term)poff.LogisticsService,
                Movement = (EBooking.MovementType)poff.MovementType,
                Remarks = poff.Remarks,
                Containers = new List<EBooking.Container>(),
                Parties = new List<EBooking.Party>(),
                Products = new List<EBooking.Product>(),
                Status = EBooking.Status.N
            };

            booking.LoadingPort = (await _csfeApiClient.GetLocationByIdAsync(poff.ShipFrom)).Name;
            booking.DischargePort = (await _csfeApiClient.GetLocationByIdAsync(poff.ShipTo)).Name;
            booking.ReceiptPort = (await _csfeApiClient.GetLocationByIdAsync(poff.ReceiptPortId.Value)).Name;
            booking.DeliveryPort = (await _csfeApiClient.GetLocationByIdAsync(poff.DeliveryPortId.Value)).Name;
            booking.OceanFreightCharge = IncotermOceanFreightChargeMapping[booking.Incoterm.Value];
            booking.PayableAt = booking.OceanFreightCharge == EBooking.OceanFreightCharge.C ?
                EBooking.PayableAt.Destination : EBooking.PayableAt.Origin;

            foreach (var loadGroup in poff.Loads.GroupBy(l => l.EquipmentType))
            {
                booking.Containers.Add(new EBooking.Container
                {
                    Type = (EBooking.EquipmentType)loadGroup.Key,
                    Quantity = loadGroup.Count().ToString()
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
                    booking.Parties.Add(new EBooking.Party
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

            var poNumbers = poff.Orders.Select(o => o.CustomerPONumber);
            var poLineItemList = await _poRepository.Query(p => poNumbers.Contains(p.PONumber),
                    null,
                    i => i.Include(p => p.LineItems)).SelectMany(p => p.LineItems).ToListAsync();

            var customerOrders = poff.Orders.OrderBy(o => o.Id).ToList();
            foreach (var customerOrder in customerOrders)
            {
                int sequence = customerOrders.IndexOf(customerOrder) + 1;
                var poLineItem = poLineItemList.FirstOrDefault(i => i.Id == customerOrder.POLineItemId);

                var newBookingProduct = new EBooking.Product
                {
                    Line = sequence.ToString(),
                    OrderNumber = customerOrder.CustomerPONumber,
                    OrderSequence = poLineItem.LineOrder?.ToString(),
                    ProductNumber = poLineItem.ProductCode,
                    MarksNos = customerOrder.ShippingMarks,
                    GoodsDesc = poLineItem.DescriptionOfGoods,
                    UOM = (EBooking.UOM)poLineItem.UnitUOM,
                    Piece = customerOrder.FulfillmentUnitQty.ToString(),
                    QuantityUnit = customerOrder.BookedPackage.HasValue ? (EBooking.QuantityUnit)customerOrder.PackageUOM : (EBooking.QuantityUnit?)null,
                    Quantity = customerOrder.BookedPackage?.ToString(),
                    GrossWeightUnit = EBooking.WeightUnit.KG,
                    GrossWeight = customerOrder.GrossWeight.HasValue ? customerOrder.GrossWeight.ToString() : "0.0",
                    NetWeightUnit = customerOrder.NetWeight.HasValue ? EBooking.WeightUnit.KG : (EBooking.WeightUnit?)null,
                    NetWeight = customerOrder.NetWeight?.ToString(),
                    CBM = customerOrder.Volume?.ToString("f3"),
                    Currency = poLineItem.CurrencyCode,
                    UnitPrice = poLineItem.UnitPrice.ToString(),
                    HSCode = poLineItem.HSCode  
                };

                if (newBookingProduct.QuantityUnit == EBooking.QuantityUnit.PC)
                {
                    newBookingProduct.QuantityUnit = EBooking.QuantityUnit.PC;
                }

                booking.Products.Add(newBookingProduct);
            }

            bookingRequest.RequestContent = JsonConvert.SerializeObject(booking);
            bookingRequest.Audit(userName);

            if (sendToEdison)
            {
                var login = new EBooking.UserAccount
                {
                    Id = _eBookingManagementAPIConfig.Id,
                    Password = _eBookingManagementAPIConfig.Password
                };
                var authenticationResult = await _eBookingServiceClient.Authentication.PostSilentAsync(login);

                // Call API to obtain token
                if (authenticationResult.Status != (int)HttpStatusCode.OK)
                {
                    var obfuscatedAccount = new EBooking.UserAccount
                    {
                        Id = login.Id,
                        Password = "Not shown as security, ask the team"
                    };
                    var bookingAuthentication = _eBookingServiceClient.Authentication;
                    var additionalData = GenerateEdisonBookingIntegrationLog(
                        false,
                        "POST",
                        bookingAuthentication.RequestUrl,
                        JsonConvert.SerializeObject(obfuscatedAccount, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                        booking.ShipmentNumber,
                        authenticationResult.Content);

                    // Try to get result from response by dynamic object
                    // As authenticationResult.Result will not content value as model binding failed
                    ExpandoObject authenticationFailed = JsonConvert.DeserializeObject<ExpandoObject>(authenticationResult.Content, new ExpandoObjectConverter());
                    if (authenticationFailed.Any(x => "result".Equals(x.Key, StringComparison.OrdinalIgnoreCase)))
                    {
                        var authenticationFailedResult = authenticationFailed.FirstOrDefault(x => "result".Equals(x.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString();
                        throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, authenticationFailedResult);
                    }
                    else
                    {
                        throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, authenticationResult.Content);
                    }

                }

                // Token is valid, then call another request to process booking creation
                _eBookingServiceClient.CurrSessionID = authenticationResult.Result.CurrSessionID;
                var bookingCall = _eBookingServiceClient.Bookings;
                var bookingResult = await _eBookingServiceClient.Bookings.PostSilentAsync(booking);

                var status = bookingResult.Status;
                var result = bookingResult.Result ?? string.Empty;

                // It is duplicated error, try to re-send with Status = C
                if (status == (int)HttpStatusCode.UnprocessableEntity && result.StartsWith("Skip process Shipping Order", StringComparison.OrdinalIgnoreCase))
                {
                    booking.Status = Status.C;
                    bookingResult = await _eBookingServiceClient.Bookings.PostSilentAsync(booking);
                }

                status = bookingResult.Status;
                result = bookingResult.Result ?? string.Empty;

                if (status != (int)HttpStatusCode.OK)
                {
                    var additionalData = GenerateEdisonBookingIntegrationLog(
                        false,
                        "POST",
                        bookingCall.RequestUrl,
                        JsonConvert.SerializeObject(booking, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                        booking.ShipmentNumber,
                        bookingResult.Content);

                    throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, result);
                }
                else
                {
                    var integrationLog = GenerateEdisonBookingIntegrationLog(
                        true,
                        "POST",
                        bookingCall.RequestUrl,
                        JsonConvert.SerializeObject(booking, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                        booking.ShipmentNumber,
                        bookingResult.Content);

                    await _integrationLogRepository.AddAsync(integrationLog);
                    await _unitOfWork.SaveChangesAsync();

                }
            }
            return bookingRequest;
        }

        public async Task CancelBookingRequest(POFulfillmentBookingRequestModel bookingRequest)
        {
            var booking = new EBooking.Booking
            {
                ShipmentNumber = bookingRequest.BookingReferenceNumber,
                SubmissionOn = bookingRequest.BookedDate.ToString(DATE_FORMAT),
                SendID = _eBookingManagementAPIConfig.Id,
                FileCreatedOn = bookingRequest.BookedDate,
                Status = EBooking.Status.D
            };

            var login = new EBooking.UserAccount
            {
                Id = _eBookingManagementAPIConfig.Id,
                Password = _eBookingManagementAPIConfig.Password
            };

            var authenticationResult = await _eBookingServiceClient.Authentication.PostSilentAsync(login);

            // Call API to obtain token
            if (authenticationResult.Status != (int)HttpStatusCode.OK)
            {
                var obfuscatedAccount = new EBooking.UserAccount
                {
                    Id = login.Id,
                    Password = "Not shown as security, ask the team"
                };
                var bookingAuthentication = _eBookingServiceClient.Authentication;
                var additionalData = GenerateEdisonBookingIntegrationLog(
                    false,
                    "POST",
                    bookingAuthentication.RequestUrl,
                    JsonConvert.SerializeObject(obfuscatedAccount, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    booking.ShipmentNumber,
                    authenticationResult.Content);

                // Try to get result from response by dynamic object
                // As authenticationResult.Result will not content value as model binding failed
                ExpandoObject authenticationFailed = JsonConvert.DeserializeObject<ExpandoObject>(authenticationResult.Content, new ExpandoObjectConverter());
                if (authenticationFailed.Any(x => "result".Equals(x.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    var authenticationFailedResult = authenticationFailed.FirstOrDefault(x => "result".Equals(x.Key, StringComparison.OrdinalIgnoreCase)).Value.ToString();
                    throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, authenticationFailedResult);
                }
                else
                {
                    throw new EdisonBookingException("Send Booking unsuccessfully", additionalData, authenticationResult.Content);
                }
            }

            // Token is valid, then call another request to process booking cancellation
            _eBookingServiceClient.CurrSessionID = authenticationResult.Result.CurrSessionID;
            var bookingCall = _eBookingServiceClient.Bookings;
            var bookingResult = await _eBookingServiceClient.Bookings.PostSilentAsync(booking);

            if (bookingResult.Status != (int)HttpStatusCode.OK)
            {
                var additionalData = GenerateEdisonBookingIntegrationLog(
                    false,
                    "POST",
                    bookingCall.RequestUrl,
                    JsonConvert.SerializeObject(booking, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    booking.ShipmentNumber,
                    bookingResult.Content);
                throw new EdisonBookingException("Cancel Booking unsuccessfully", additionalData, bookingResult.Result);
            }
            else
            {
                var integrationLog = GenerateEdisonBookingIntegrationLog(
                    true,
                   "POST",
                    bookingCall.RequestUrl,
                    JsonConvert.SerializeObject(booking, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                    booking.ShipmentNumber,
                    bookingResult.Content);

                await _integrationLogRepository.AddAsync(integrationLog);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private IntegrationLogModel GenerateEdisonBookingIntegrationLog(bool isSuccessful, string requestMethod, string requestUrl, string requestBodyContent, string remark, string responseBodyContent)
        {
            var apiName = "[" + requestMethod.ToUpper() + "] " + requestUrl;
            const string profile = "Booking submission";

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
    }
}
