using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.RoutingOrder.Services.Interfaces;
using Groove.SP.Application.RoutingOrder.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace Groove.SP.Application.RoutingOrder.Services
{
    public class RoutingOrderService : ServiceBase<RoutingOrderModel, RoutingOrderViewModel>, IRoutingOrderService
    {
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly AppConfig _appConfig;

        protected override Func<IQueryable<RoutingOrderModel>, IQueryable<RoutingOrderModel>> FullIncludeProperties => x
            => x.Include(y => y.Contacts)
            .Include(y => y.LineItems)
            .Include(y => y.Containers)
            .Include(y => y.Invoices);

        protected readonly Func<string, bool> ParseToBoolean = val => Enum.TryParse(typeof(BooleanOption), val, out var res) && Convert.ToBoolean((int)res);
        

        public RoutingOrderService(
            IUnitOfWorkProvider unitOfWorkProvider, 
            ICSFEApiClient csfeApiClient,
            IOptions<AppConfig> appConfig) : base(unitOfWorkProvider)
        {
            _csfeApiClient = csfeApiClient;
            _appConfig = appConfig.Value;
        }

        public async Task<RoutingOrderViewModel> GetByIdAsync(long routingOrderId, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();

            RoutingOrderModel routingOrderModel = null;
            if (isInternal)
            {
                routingOrderModel = await Repository.GetAsNoTrackingAsync(x => x.Id == routingOrderId, includes: FullIncludeProperties);
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                routingOrderModel = await Repository.GetAsNoTrackingAsync(p => p.Id == routingOrderId 
                                                                            && p.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)),
                                                                            includes: FullIncludeProperties);
            }

            return Mapper.Map<RoutingOrderViewModel>(routingOrderModel);
        }

        public async Task<ImportRoutingOrderResultViewModel> ImportXMLAsync(byte[] content, string fileName, string username)
        {
            var importResult = new ImportRoutingOrderResultViewModel();
            try
            {
                using (var ms = new MemoryStream(content))
                {
                    // Create xml reader
                    XmlReader reader = XmlReader.Create(ms);

                    // Deserialize xml reader to model
                    XmlSerializer serializer = new(typeof(ImportRoutingOrderViewModel));
                    var model = (ImportRoutingOrderViewModel)serializer.Deserialize(reader);

                    // Perform model validation
                    model.ValidateAndThrow();

                    // Import data
                    #region import RoutingOrders
                    var newRO = new RoutingOrderModel()
                    {
                        CargoReadyDate = model.CargoReadyDate,
                        EarliestShipDate = model.EarliestShipDate,
                        ExpectedDeliveryDate = model.ExpectedDeliveryDate,
                        ExpectedShipDate = model.ExpectedShipDate ?? DateTime.UtcNow,
                        IsBatteryOrChemical = ParseToBoolean(model.IsBatteryOrChemical),
                        IsContainDangerousGoods = ParseToBoolean(model.IsContainDangerousGoods),
                        IsCIQOrFumigation = ParseToBoolean(model.IsCIQOrFumigation),
                        IsExportLicence = ParseToBoolean(model.IsExportLicence),
                        LastDateForShipment = model.LastDateForShipment ?? DateTime.UtcNow,
                        LatestShipDate = model.LatestShipDate,
                        NumberOfLineItems = model.NumberOfLineItems,
                        Remarks = model.Remarks,
                        Status = model.Status.HasValue ? (RoutingOrderStatus)model.Status : RoutingOrderStatus.Active,
                        RoutingOrderDate = model.RoutingOrderDate,
                        RoutingOrderNumber = model.RoutingOrderNumber,
                        VesselName = model.VesselName,
                        VoyageNo = model.VoyageNo,
                        Stage = RoutingOrderStageType.Released,
                        Contacts = new List<RoutingOrderContactModel>(),
                        LineItems = new List<ROLineItemModel>(),
                        Invoices = new List<RoutingOrderInvoiceModel>(),
                        Containers = new List<RoutingOrderContainerModel>()
                    };

                    if (Enum.TryParse(typeof(IncotermType), model.Incoterm, out var result)) newRO.Incoterm = (IncotermType)result;
                    if (Enum.TryParse(typeof(ModeOfTransportType), model.ModeOfTransport, out result)) newRO.ModeOfTransport = (ModeOfTransportType)result;

                    if (!string.IsNullOrWhiteSpace(model.LogisticsService))
                    {
                        newRO.LogisticsService = EnumExtension.GetEnumValueFromDescription<LogisticsServiceType>(model.LogisticsService);
                    }

                    if (!string.IsNullOrWhiteSpace(model.MovementType))
                    {
                        if (model.MovementType == "CY")
                        {
                            newRO.MovementType = MovementType.CY_CY;
                        }
                        else if (model.MovementType == "CFS")
                        {
                            newRO.MovementType = MovementType.CFS_CY;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(model.Carrier))
                    {
                        var carrier = (await _csfeApiClient.GetAllCarriesAsync())?.FirstOrDefault(
                            x => x.CarrierCode == model.Carrier && x.ModeOfTransport == model.ModeOfTransport);

                        if (carrier is not null) newRO.CarrierId = carrier.Id;
                        else throw new AppEntityNotFoundException($"Carrier code {model.Carrier} is not existing.");
                    }

                    if (!string.IsNullOrWhiteSpace(model.ShipFrom))
                    {
                        var location = await _csfeApiClient.GetLocationByCodeAsync(model.ShipFrom);
                        if (location is not null) newRO.ShipFromId = location.Id;
                        else throw new AppEntityNotFoundException($"Location code {model.ShipFrom} is not existing.");
                    }

                    if (!string.IsNullOrWhiteSpace(model.ShipTo))
                    {
                        var location = await _csfeApiClient.GetLocationByCodeAsync(model.ShipTo);

                        if (location is not null) newRO.ShipToId = location.Id;
                        else throw new AppEntityNotFoundException($"Location code {model.ShipTo} is not existing.");
                    }

                    if (!string.IsNullOrWhiteSpace(model.DeliveryPort))
                    {
                        var location = await _csfeApiClient.GetLocationByCodeAsync(model.DeliveryPort);

                        if (location is not null) newRO.DeliveryPortId = location.Id;
                        else throw new AppEntityNotFoundException($"Location code {model.DeliveryPort} is not existing.");
                    }

                    if (!string.IsNullOrWhiteSpace(model.ReceiptPort))
                    {
                        var location = await _csfeApiClient.GetLocationByCodeAsync(model.ReceiptPort);

                        if (location is not null) newRO.ReceiptPortId = location.Id;
                        else throw new AppEntityNotFoundException($"Location code {model.ReceiptPort} is not existing.");
                    }

                    newRO.Audit(model.CreatedBy);
                    #endregion

                    #region import Contacts
                    if (model.Contacts is not null && model.Contacts.Any())
                    {
                        var conOrganizationCodes = model.Contacts.Select(x => x.OrganizationCode).Where(code => !string.IsNullOrWhiteSpace(code)).ToList();
                        var conOrganizations = await _csfeApiClient.GetOrganizationsByCodesAsync(conOrganizationCodes);

                        foreach (var con in model.Contacts)
                        {
                            if (!string.IsNullOrWhiteSpace(con.ContactEmail)) con.ContactEmail = con.ContactEmail.Replace(';', ',');
                            var newROContact = new RoutingOrderContactModel();
                            Mapper.Map(con, newROContact);

                            var org = conOrganizations.FirstOrDefault(x => x.Code == con.OrganizationCode);
                            if (org is not null)
                            {
                                newROContact.OrganizationId = org.Id;
                                newROContact.CompanyName = org.Name;
                                newROContact.AddressLine1 = org.Address;
                                newROContact.AddressLine2 = org.AddressLine2;
                                newROContact.AddressLine3 = org.AddressLine3;
                                newROContact.AddressLine4 = org.AddressLine4;
                                newROContact.ContactName = org.ContactName;
                                newROContact.ContactNumber = org.ContactNumber;
                                newROContact.ContactEmail = org.ContactEmail;
                            }
                            else
                            {
                                throw new AppEntityNotFoundException($"Organization code '{con.OrganizationCode}' is not existing.");
                            }

                            newROContact.Audit(model.CreatedBy);
                            newRO.Contacts.Add(newROContact);
                        }
                    }
                    #endregion

                    #region import LineItems
                    if (model.LineItems is not null && model.LineItems.Any())
                    {
                        foreach (var item in model.LineItems)
                        {
                            var newROItem = new ROLineItemModel
                            {
                                PONo = item.PONo,
                                ItemNo = item.ItemNo,
                                DescriptionOfGoods = item.DescriptionOfGoods,
                                ChineseDescription = item.ChineseDescription,
                                OrderedUnitQty = item.OrderedUnitQty,
                                BookedPackage = item.BookedPackage,
                                GrossWeight = item.GrossWeight,
                                NetWeight = item.NetWeight,
                                Volume = item.Volume,
                                HsCode = item.HsCode,
                                Commodity = item.Commodity,
                                ShippingMarks = item.ShippingMarks
                            };

                            if (Enum.TryParse(typeof(UnitUOMType), item.UnitUOM, out var res)) newROItem.UnitUOM = (UnitUOMType)res;
                            if (Enum.TryParse(typeof(PackageUOMType), item.PackageUOM, out res)) newROItem.PackageUOM = (PackageUOMType)res;

                            if (!string.IsNullOrWhiteSpace(item.CountryCodeOfOrigin))
                            {
                                var country = await _csfeApiClient.GetCountryByCodeAsync(item.CountryCodeOfOrigin);
                                if (country != null) newROItem.CountryCodeOfOrigin = country.Code;
                                else throw new AppEntityNotFoundException($"Country code {item.CountryCodeOfOrigin} is not existing.");
                            }

                            newROItem.Audit(model.CreatedBy);

                            newRO.LineItems.Add(newROItem);
                        }
                    }
                    #endregion

                    #region import Invoices
                    if (model.Invoices is not null && model.Invoices.Any())
                    {
                        foreach (var inv in model.Invoices)
                        {
                            var newInv = new RoutingOrderInvoiceModel
                            {
                                InvoiceNumber = inv.InvoiceNumber,
                                InvoiceType = inv.InvoiceType
                            };
                            newInv.Audit(model.CreatedBy);
                            newRO.Invoices.Add(newInv);
                        }
                    }
                    #endregion

                    #region import Containers
                    if (model.Containers is not null && model.Containers.Any())
                    {
                        foreach (var ctn in model.Containers)
                        {
                            var newCtn = new RoutingOrderContainerModel
                            {
                                Quantity = ctn.Quantity,
                                Volume = ctn.Volume,
                            };

                            if (!string.IsNullOrWhiteSpace(ctn.ContainerType))
                                newCtn.ContainerType = EnumExtension.GetEnumValueFromMember<EquipmentType>(ctn.ContainerType);

                            newCtn.Audit(model.CreatedBy);

                            newRO.Containers.Add(newCtn);
                        }
                    }
                    #endregion

                    await Repository.AddAsync(newRO);
                    await UnitOfWork.SaveChangesAsync();

                    //importingResult.LogBookingSuccess("Id", $"{warehouseBookingViewModel.Id}");
                    //importingResult.LogBookingSuccess("Number", $"{warehouseBookingViewModel.Number}");
                    //importingResult.LogBookingSuccess("Url", $"{_appConfig.ClientUrl}/warehouse-bookings/view/{warehouseBookingViewModel.Id}");
                    importResult.Log(ImportingRoutingOrderResult.Success, "Id", newRO.Id);
                    importResult.Log(ImportingRoutingOrderResult.Success, "Number", newRO.RoutingOrderNumber);
                    importResult.Log(ImportingRoutingOrderResult.Success, "Url", $"{_appConfig.ClientUrl}/routing-orders/view/{newRO.Id}");

                }
            }
            catch (ValidationException ex)
            {
                importResult.Log(ImportingRoutingOrderResult.ValidationFailed, "errors", ex.Errors);
                return importResult;
            }
            catch (Exception ex)
            {
                importResult.Log(ImportingRoutingOrderResult.ErrorDuringImport, "errors", ex);
            }
            finally
            {
                // Write integration log if error
                await WriteIntegrationLogAsync(importResult, fileName);
            }
            return importResult;
        }

        private async Task WriteIntegrationLogAsync(ImportRoutingOrderResultViewModel importingResult, string fileName)
        {
            // Must create new instance of DB context to save integration log isolatedly
            // to prevent issue on other entities
            using (var uow = UnitOfWorkProvider.CreateUnitOfWorkForBackgroundJob())
            {
                var utcNow = DateTime.UtcNow;
                var logModel = new IntegrationLogModel
                {
                    APIName = "Import Routing Order from file XML",
                    APIMessage = $"RoutingOrderForm: {fileName}",
                    EDIMessageRef = string.Empty,
                    EDIMessageType = string.Empty,
                    PostingDate = utcNow,
                    Profile = fileName,
                    Status = importingResult.Success ? IntegrationStatus.Succeed : IntegrationStatus.Failed,
                    Remark = $"{utcNow:yyyy-MM-dd HH:mm:ss} GMT",
                    Response = JsonConvert.SerializeObject(importingResult, Formatting.Indented)
                };
                logModel.Audit();
                var integrationLogRepository = uow.GetRepository<IntegrationLogModel>();
                await integrationLogRepository.AddAsync(logModel);
                await uow.SaveChangesAsync();
            }
        }
    }
}