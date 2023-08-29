using Groove.SP.Application.POFulfillment.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.Services
{
    public class SOFormExcelGenerator : ISOFormGeneratorStrategy
    {
        public async Task<byte[]> GenerateAsync(ShippingOrderFormViewModel shippingOrderForm)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory().ToString(), "EmailTemplates\\Excel\\ShippingOrderForm.xlsx");

            using (var stream = new FileStream(path: templatePath, mode: FileMode.Open, access: FileAccess.ReadWrite))
            {
                using ExcelPackage package = new(stream);

                var font = new Font("Arial", 10);
                OfficeOpenXml.ExcelWorksheet worksheet = package.Workbook.Worksheets.First();
                OfficeOpenXml.ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Last();

                #region Form Type
                var cell = worksheet.Cells["G1"];
                cell.Value = shippingOrderForm.FormType == ShippingFormType.Booking ? "BOOKING FORM" : "SHIPPING ORDER FORM";
                #endregion

                #region Shipper contact
                cell = worksheet.Cells["A1"];
                cell.IsRichText = true;

                var cellTitle = "Shipper";
                ExcelRichText richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                var cellValue = "";
                if (shippingOrderForm.Shipper != null)
                {
                    cellValue += $"\n{shippingOrderForm.Shipper.CompanyName}";
                }
                if (shippingOrderForm.ShipperOrganization != null)
                {
                    if (!string.IsNullOrEmpty(shippingOrderForm.ShipperOrganization.Address))
                    {
                        cellValue += $"\n{shippingOrderForm.ShipperOrganization.Address}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.ShipperOrganization.AddressLine2))
                    {
                        cellValue += $"\n{shippingOrderForm.ShipperOrganization.AddressLine2}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.ShipperOrganization.AddressLine3))
                    {
                        cellValue += $"\n{shippingOrderForm.ShipperOrganization.AddressLine3}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.ShipperOrganization.AddressLine4))
                    {
                        cellValue += $"\n{shippingOrderForm.ShipperOrganization.AddressLine4}";
                    }
                    if (shippingOrderForm.ShipperOrganization.Location != null)
                    {
                        cellValue += $"\n{shippingOrderForm.ShipperOrganization.Location.LocationDescription}, {shippingOrderForm.ShipperOrganization.Location.Country.Name}";
                    }
                }
                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Supplier contact
                cell = worksheet.Cells["A2"];
                cell.IsRichText = true;

                cellTitle = "Supplier";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                cellValue = "";
                if (shippingOrderForm.Supplier != null)
                {
                    cellValue += $"\n{shippingOrderForm.Supplier.CompanyName}";
                }
                if (shippingOrderForm.SupplierOrganization != null)
                {
                    if (!string.IsNullOrEmpty(shippingOrderForm.SupplierOrganization.Address))
                    {
                        cellValue += $"\n{shippingOrderForm.SupplierOrganization.Address}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.SupplierOrganization.AddressLine2))
                    {
                        cellValue += $"\n{shippingOrderForm.SupplierOrganization.AddressLine2}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.SupplierOrganization.AddressLine3))
                    {
                        cellValue += $"\n{shippingOrderForm.SupplierOrganization.AddressLine3}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.SupplierOrganization.AddressLine4))
                    {
                        cellValue += $"\n{shippingOrderForm.SupplierOrganization.AddressLine4}";
                    }
                    if (shippingOrderForm.SupplierOrganization.Location != null)
                    {
                        cellValue += $"\n{shippingOrderForm.SupplierOrganization.Location.LocationDescription}, {shippingOrderForm.SupplierOrganization.Location.Country.Name}";
                    }
                }
                if (!string.IsNullOrEmpty(shippingOrderForm.Owner?.EmailAddress))
                {
                    cellValue += $"\n{shippingOrderForm.Owner.EmailAddress}";
                }
                if (!string.IsNullOrEmpty(shippingOrderForm.Owner?.ContactNumber))
                {
                    cellValue += $"\n{shippingOrderForm.Owner.ContactNumber}";
                }
                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion Supplier contact

                #region Consignee contact
                cell = worksheet.Cells["A3"];
                cell.IsRichText = true;

                cellTitle = "Consignee";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                cellValue = "";
                if (shippingOrderForm.Consignee != null)
                {
                    cellValue += $"\n{shippingOrderForm.Consignee.CompanyName}";
                }
                if (shippingOrderForm.ConsigneeOrganization != null)
                {
                    if (!string.IsNullOrEmpty(shippingOrderForm.ConsigneeOrganization.Address))
                    {
                        cellValue += $"\n{shippingOrderForm.ConsigneeOrganization.Address}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.ConsigneeOrganization.AddressLine2))
                    {
                        cellValue += $"\n{shippingOrderForm.ConsigneeOrganization.AddressLine2}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.ConsigneeOrganization.AddressLine3))
                    {
                        cellValue += $"\n{shippingOrderForm.ConsigneeOrganization.AddressLine3}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.ConsigneeOrganization.AddressLine4))
                    {
                        cellValue += $"\n{shippingOrderForm.ConsigneeOrganization.AddressLine4}";
                    }
                    if (shippingOrderForm.ConsigneeOrganization.Location != null)
                    {
                        cellValue += $"\n{shippingOrderForm.ConsigneeOrganization.Location.LocationDescription}, {shippingOrderForm.ConsigneeOrganization.Location.Country.Name}";
                    }
                }
                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Notify Party
                cell = worksheet.Cells["A4"];
                cell.IsRichText = true;

                cellTitle = "Notify Party";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                cellValue = "";
                if (shippingOrderForm.IsNotifyPartyAsConsignee)
                {
                    cellValue += $"\nSame As Consignee";
                }
                else
                {
                    if (shippingOrderForm.Notify != null)
                    {
                        cellValue += $"\n{shippingOrderForm.Notify.CompanyName}";
                    }
                    if (shippingOrderForm.NotifyOrganization != null)
                    {
                        if (!string.IsNullOrEmpty(shippingOrderForm.NotifyOrganization.Address))
                        {
                            cellValue += $"\n{shippingOrderForm.NotifyOrganization.Address}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.NotifyOrganization.AddressLine2))
                        {
                            cellValue += $"\n{shippingOrderForm.NotifyOrganization.AddressLine2}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.NotifyOrganization.AddressLine3))
                        {
                            cellValue += $"\n{shippingOrderForm.NotifyOrganization.AddressLine3}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.NotifyOrganization.AddressLine4))
                        {
                            cellValue += $"\n{shippingOrderForm.NotifyOrganization.AddressLine4}";
                        }
                        if (shippingOrderForm.NotifyOrganization.Location != null)
                        {
                            cellValue += $"\n{shippingOrderForm.NotifyOrganization.Location.LocationDescription}, {shippingOrderForm.NotifyOrganization.Location.Country.Name}";
                        }
                    }
                }

                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Billing Party
                cell = worksheet.Cells["A5"];
                cell.IsRichText = true;

                cellTitle = "Billing Party";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                cellValue = "";
                if (shippingOrderForm.BillingAddress != null)
                {
                    if (!string.IsNullOrEmpty(shippingOrderForm.BillingAddress.CompanyName))
                    {
                        cellValue += $"\n{shippingOrderForm.BillingAddress.CompanyName}";
                    }

                    if (shippingOrderForm.BillingPartyOrganization != null)
                    {
                        if (!string.IsNullOrEmpty(shippingOrderForm.BillingPartyOrganization.Address))
                        {
                            cellValue += $"\n{shippingOrderForm.BillingPartyOrganization.Address}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.BillingPartyOrganization.AddressLine2))
                        {
                            cellValue += $"\n{shippingOrderForm.BillingPartyOrganization.AddressLine2}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.BillingPartyOrganization.AddressLine3))
                        {
                            cellValue += $"\n{shippingOrderForm.BillingPartyOrganization.AddressLine3}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.BillingPartyOrganization.AddressLine4))
                        {
                            cellValue += $"\n{shippingOrderForm.BillingPartyOrganization.AddressLine4}";
                        }
                    }

                    if (!string.IsNullOrEmpty(shippingOrderForm.BillingAddress.ContactDetails))
                    {
                        cellValue += $"\n{shippingOrderForm.BillingAddress.ContactDetails}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.BillingAddress.EmailAddress))
                    {
                        cellValue += $"\n{shippingOrderForm.BillingAddress.EmailAddress}";
                    }
                }

                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Pickup Address
                cell = worksheet.Cells["A6"];
                cell.IsRichText = true;

                cellTitle = "Pickup Address";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                cellValue = "";
                if (shippingOrderForm.PickupAddress != null)
                {
                    if (!string.IsNullOrEmpty(shippingOrderForm.PickupAddress.CompanyName))
                    {
                        cellValue += $"\n{shippingOrderForm.PickupAddress.CompanyName}";
                    }

                    if (shippingOrderForm.PickupOrganization != null)
                    {
                        if (!string.IsNullOrEmpty(shippingOrderForm.PickupOrganization.Address))
                        {
                            cellValue += $"\n{shippingOrderForm.PickupOrganization.Address}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.PickupOrganization.AddressLine2))
                        {
                            cellValue += $"\n{shippingOrderForm.PickupOrganization.AddressLine2}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.PickupOrganization.AddressLine3))
                        {
                            cellValue += $"\n{shippingOrderForm.PickupOrganization.AddressLine3}";
                        }
                        if (!string.IsNullOrEmpty(shippingOrderForm.PickupOrganization.AddressLine4))
                        {
                            cellValue += $"\n{shippingOrderForm.PickupOrganization.AddressLine4}";
                        }
                    }

                    if (!string.IsNullOrEmpty(shippingOrderForm.PickupAddress.ContactDetails))
                    {
                        cellValue += $"\n{shippingOrderForm.PickupAddress.ContactDetails}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.PickupAddress.EmailAddress))
                    {
                        cellValue += $"\n{shippingOrderForm.PickupAddress.EmailAddress}";
                    }
                }

                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Vessel/Voyage or Flight No.
                cell = worksheet.Cells["A7"];
                cell.IsRichText = true;

                var isAirTransport = shippingOrderForm.ModeOfTransport != null && shippingOrderForm.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.CurrentCultureIgnoreCase);

                cellTitle = isAirTransport ? "Flight No." : "Vessel/Voyage";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                cellValue = "";
                if (shippingOrderForm.FulfillmentType == FulfillmentType.PO)
                {
                    if (!string.IsNullOrWhiteSpace(shippingOrderForm.FirstItinerrary?.VesselFlight ?? ""))
                    {
                        cellValue += $"\n{shippingOrderForm.FirstItinerrary?.VesselFlight ?? ""}";
                    }
                }
                if (shippingOrderForm.FulfillmentType == FulfillmentType.Bulk)
                {
                    if (shippingOrderForm.SOFormStage == POFulfillmentStage.ForwarderBookingRequest)
                    {
                        if (!string.IsNullOrEmpty(shippingOrderForm.VesselName) || !string.IsNullOrEmpty(shippingOrderForm.VoyageNo))
                        {
                            cellValue += $"\n{shippingOrderForm.VesselName ?? ""}/{shippingOrderForm.VoyageNo ?? ""}";
                        }
                    }

                    if (shippingOrderForm.SOFormStage == POFulfillmentStage.ForwarderBookingConfirmed)
                    {
                        if (!string.IsNullOrWhiteSpace(shippingOrderForm.FirstItinerrary?.VesselFlight ?? ""))
                        {
                            cellValue += $"\n{shippingOrderForm.FirstItinerrary?.VesselFlight ?? ""}";
                        }
                    }
                }
                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Carrier or Airline
                cell = worksheet.Cells["D7"];
                cell.IsRichText = true;

                cellTitle = isAirTransport ? "Airline" : "Carrier";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                cellValue = "";
                if (shippingOrderForm.FulfillmentType == FulfillmentType.PO)
                {
                    if (!string.IsNullOrWhiteSpace(shippingOrderForm.FirstItinerrary?.CarrierName))
                    {
                        cellValue += $"\n{shippingOrderForm.FirstItinerrary?.CarrierName ?? ""}";
                    }
                }

                if (shippingOrderForm.FulfillmentType == FulfillmentType.Bulk)
                {
                    if (shippingOrderForm.SOFormStage == POFulfillmentStage.ForwarderBookingRequest)
                    {
                        if (!string.IsNullOrWhiteSpace(shippingOrderForm.CarrierName))
                        {
                            cellValue += $"\n{shippingOrderForm.CarrierName ?? ""}";
                        }
                    }

                    if (shippingOrderForm.SOFormStage == POFulfillmentStage.ForwarderBookingConfirmed)
                    {
                        if (!string.IsNullOrWhiteSpace(shippingOrderForm.FirstItinerrary?.CarrierName ?? ""))
                        {
                            cellValue += $"\n{shippingOrderForm.FirstItinerrary?.CarrierName ?? ""}";
                        }
                    }
                }
                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Port of loading or Origin
                cell = worksheet.Cells["A8"];
                cell.IsRichText = true;

                cellTitle = isAirTransport ? "Origin" : "Port of loading";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                if (!string.IsNullOrWhiteSpace(shippingOrderForm.PortOfLoading))
                {
                    richText = cell.RichText.Add($"\n{shippingOrderForm.PortOfLoading}");
                    richText.Bold = false;
                }
                #endregion

                #region Place of receipt
                cell = worksheet.Cells["D8"];
                cell.IsRichText = true;

                cellTitle = "Place of receipt";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                if (!string.IsNullOrWhiteSpace(shippingOrderForm.PlaceOfReceipt))
                {
                    richText = cell.RichText.Add($"\n{shippingOrderForm.PlaceOfReceipt}");
                    richText.Bold = false;
                }
                #endregion

                #region Port of discharge or Destination
                cell = worksheet.Cells["A9"];
                cell.IsRichText = true;

                cellTitle = isAirTransport ? "Destination" : "Port of discharge";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                if (!string.IsNullOrWhiteSpace(shippingOrderForm.PortOfDischarge))
                {
                    richText = cell.RichText.Add($"\n{shippingOrderForm.PortOfDischarge}");
                    richText.Bold = false;
                }
                #endregion

                #region Place of delivery
                cell = worksheet.Cells["D9"];
                cell.IsRichText = true;

                cellTitle = "Place of delivery";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                if (!string.IsNullOrWhiteSpace(shippingOrderForm.PlaceOfDelivery))
                {
                    richText = cell.RichText.Add($"\n{shippingOrderForm.PlaceOfDelivery}");
                    richText.Bold = false;
                }
                #endregion

                #region Incoterm
                cell = worksheet.Cells["G7"];
                cell.IsRichText = true;

                cellTitle = "Incoterm";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                if (!string.IsNullOrWhiteSpace(shippingOrderForm.Incoterm))
                {
                    richText = cell.RichText.Add($"{shippingOrderForm.Incoterm}".PadLeft(28, ' '));
                    richText.Bold = false;
                }
                #endregion

                #region Dangerous Goods
                cell = worksheet.Cells["H8"];
                cell.Value = shippingOrderForm.IsDangerousGoods;
                #endregion
                #region CIQ or Fumigation
                cell = worksheet.Cells["O8"];
                cell.Value = shippingOrderForm.IsCIQOrFumigation;
                #endregion
                #region Battery or Chemical
                cell = worksheet.Cells["H9"];
                cell.Value = shippingOrderForm.IsBatteryOrChemical;
                #endregion
                #region Export Licence
                cell = worksheet.Cells["O9"];
                cell.Value = shippingOrderForm.IsExportLicence;
                #endregion

                #region SO No./ Booking No.
                cell = worksheet.Cells["G2"];
                cell.IsRichText = true;

                cellTitle = "SO No.: ";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;
                if (!string.IsNullOrWhiteSpace(shippingOrderForm.SoNumber))
                {
                    richText = cell.RichText.Add(shippingOrderForm.SoNumber);
                    richText.Bold = false;
                }

                cellTitle = $"\n{(shippingOrderForm.FormType.Equals(ShippingFormType.Booking) ? "Booking Date" : "Confirm Date")}: ";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;
                richText = cell.RichText.Add(shippingOrderForm.ConfirmDate.ToString(shippingOrderForm.DateFormat));
                richText.Bold = false;

                cellTitle = $"\nBooking No.: ";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;
                richText = cell.RichText.Add(shippingOrderForm.PoffNumber);
                richText.Bold = false;

                if (!string.IsNullOrEmpty(shippingOrderForm.BookingRequestReferenceNumber))
                {
                    richText = cell.RichText.Add($"\n({shippingOrderForm.BookingRequestReferenceNumber})");
                    richText.Bold = false;
                }
                #endregion

                #region Origin Agent
                cell = worksheet.Cells["G3"];
                cell.IsRichText = true;

                richText = cell.RichText.Add(shippingOrderForm.OriginAgent?.CompanyName);
                richText.Bold = true;

                cellValue = "";
                if (shippingOrderForm.OriginAgentOrganization != null)
                {
                    if (!string.IsNullOrEmpty(shippingOrderForm.OriginAgentOrganization.Address))
                    {
                        cellValue += $"\n{shippingOrderForm.OriginAgentOrganization.Address}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.OriginAgentOrganization.AddressLine2))
                    {
                        cellValue += $"\n{shippingOrderForm.OriginAgentOrganization.AddressLine2}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.OriginAgentOrganization.AddressLine3))
                    {
                        cellValue += $"\n{shippingOrderForm.OriginAgentOrganization.AddressLine3}";
                    }
                    if (!string.IsNullOrEmpty(shippingOrderForm.OriginAgentOrganization.AddressLine4))
                    {
                        cellValue += $"\n{shippingOrderForm.OriginAgentOrganization.AddressLine4}";
                    }
                    if (shippingOrderForm.OriginAgentOrganization.Location != null)
                    {
                        cellValue += $"\n{shippingOrderForm.OriginAgentOrganization.Location.LocationDescription}, {shippingOrderForm.OriginAgentOrganization.Location.Country.Name}";
                    }
                }
                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Products
                int rowCursor = 11;
                for (int i = 0; i < shippingOrderForm.Orders.Count; i++)
                {
                    rowCursor++;
                    worksheet.Row(rowCursor).Height = 58;
                    worksheet.Row(rowCursor).Style.Font.SetFromFont(font);

                    var order = shippingOrderForm.Orders[i];
                    var product = shippingOrderForm.Products?.FirstOrDefault(x => x.PurchaseOrderId.Equals(order.PurchaseOrderId) && x.Id.Equals(order.POLineItemId));
                    
                    cell = worksheet.Cells[$"A{rowCursor}:B{rowCursor}"]; //Shipping Marks
                    cell.Merge = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Value = order.ShippingMarks;

                    cell = worksheet.Cells[$"C{rowCursor}:D{rowCursor}"]; //Customer PO
                    cell.Merge = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Value = order.CustomerPONumber;

                    cell = worksheet.Cells[$"E{rowCursor}:F{rowCursor}"];  //Product
                    cell.Merge = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    if (shippingOrderForm.FulfillmentType == FulfillmentType.Bulk)
                    {
                        cell.Value = order.ProductCode;
                    }
                    else
                    {
                        cell.IsRichText = true;
                        richText = cell.RichText.Add(order.ProductCode);

                        var innerQty = order.InnerQuantity.HasValue ? order.InnerQuantity.Value.ToString("n0") : "--";
                        var outerQty = order.OuterQuantity.HasValue ? order.OuterQuantity.Value.ToString("n0") : "--";
                        richText = cell.RichText.Add($"\n({innerQty}/{outerQty})");
                    }
                    
                    cell = worksheet.Cells[$"G{rowCursor}"]; // HS Code
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    cell.Value = order.HsCode;

                    cell = worksheet.Cells[$"H{rowCursor}:I{rowCursor}"]; // No. of Cartons
                    cell.Merge = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    if (shippingOrderForm.FulfillmentType == FulfillmentType.Bulk)
                    {
                        cell.Value = order.BookedPackage;
                    }
                    else
                    {
                        if (order.DifferentSizedCarton)
                        {
                            cell.Value = $"* {(order.NoOfCartons.HasValue && order.NoOfCartons > 0 ? order.NoOfCartons.Value.ToString("#,##0") : "")}";
                            cell.Style.Font.Color.SetColor(Color.Red);
                        }
                        else
                        {
                            cell.Value = order.NoOfCartons.HasValue && order.NoOfCartons > 0 ? order.NoOfCartons.Value.ToString("#,##0") : "";
                        }
                    }

                    cell = worksheet.Cells[$"J{rowCursor}:K{rowCursor}"]; // No. of Pieces
                    cell.Merge = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    cell.Value = order.NoOfPieces.ToString("#,##0");
                    
                    cell = worksheet.Cells[$"L{rowCursor}:N{rowCursor}"]; // Package Description 商品说明
                    cell.Merge = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    cell.IsRichText = true;
                    cellValue = shippingOrderForm.FulfillmentType == FulfillmentType.Bulk ? order?.ProductName : product?.DescriptionOfGoods;
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        richText = cell.RichText.Add(cellValue);
                    }
                    if (!string.IsNullOrEmpty(order.ChineseDescription))
                    {
                        richText = cell.RichText.Add($"\n{order.ChineseDescription}");
                    }

                    cell = worksheet.Cells[$"O{rowCursor}"]; // Volume
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    cell.Value = order.Volume.HasValue && order.Volume > 0 ? order.Volume.Value.ToString("#,##0.##0") : "";

                    cell = worksheet.Cells[$"P{rowCursor}:Q{rowCursor}"]; // Weight
                    cell.Merge = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    cell.Value = order.Weight.HasValue && order.Weight > 0 ? order.Weight.Value.ToString("#,##0.#0") : "";

                    // row styling
                    cell = worksheet.Cells[$"A{rowCursor}:Q{rowCursor}"];
                    cell.Style.WrapText = true;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // move to the next row
                rowCursor++;

                // add blank row
                worksheet.Row(rowCursor).Height = 9;
                cell = worksheet.Cells[$"A{rowCursor}:Q{rowCursor}"];
                cell.Merge = true;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                #endregion

                // move to the next row
                rowCursor++;

                #region SubTotal
                worksheet.Row(rowCursor).Height = 21;

                cell = worksheet.Cells[$"A{rowCursor}:G{rowCursor}"]; // Total
                cell.Merge = true;
                cell.Value = "Total";
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // No. of Cartons
                cell = worksheet.Cells[$"H{rowCursor}:I{rowCursor}"];
                cell.Merge = true;
                var subOrderTotal = shippingOrderForm.Orders.Where(x => x.NoOfCartons.HasValue && x.NoOfCartons > 0);
                cell.Value = subOrderTotal != null && subOrderTotal.Any() ? subOrderTotal.Sum(x => x.NoOfCartons).Value.ToString("#,##0") : "";
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // No. of Pieces
                cell = worksheet.Cells[$"J{rowCursor}:K{rowCursor}"];
                cell.Merge = true;
                cell.Value = shippingOrderForm.Orders.Sum(x => x.NoOfPieces).ToString("#,##0");
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Package Description
                cell = worksheet.Cells[$"L{rowCursor}:N{rowCursor}"];
                cell.Merge = true;

                // Volume
                cell = worksheet.Cells[$"O{rowCursor}"];
                subOrderTotal = shippingOrderForm.Orders.Where(x => x.Volume.HasValue && x.Volume > 0);
                cell.Value = subOrderTotal != null && subOrderTotal.Any() ? subOrderTotal.Sum(x => x.Volume).Value.ToString("#,##0.##0") : "";
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Weight
                cell = worksheet.Cells[$"P{rowCursor}:Q{rowCursor}"];
                cell.Merge = true;
                subOrderTotal = shippingOrderForm.Orders.Where(x => x.Weight.HasValue && x.Weight > 0);
                cell.Value = subOrderTotal != null && subOrderTotal.Any() ? subOrderTotal.Sum(x => x.Weight).Value.ToString("#,##0.#0") : "";
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // row styling
                cell = worksheet.Cells[$"A{rowCursor}:Q{rowCursor}"];
                cell.Style.Font.SetFromFont(font);
                cell.Style.WrapText = true;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                #endregion

                #region Expected Ship Date
                cell = worksheet2.Cells[$"A2"];
                cell.IsRichText = true;

                cellTitle = "Expected Ship Date: ";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                richText = cell.RichText.Add(shippingOrderForm.ExpectedShipDate?.ToString(shippingOrderForm.DateFormat) ?? "");
                richText.Bold = false;
                #endregion

                #region Cargo Ready Date
                cell = worksheet2.Cells[$"F2"];
                cell.IsRichText = true;

                cellTitle = "Cargo Ready Date: ";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                cellValue = (shippingOrderForm.FulfillmentType == FulfillmentType.Bulk ? shippingOrderForm.CargoReadyDate?.ToString(shippingOrderForm.DateFormat) : shippingOrderForm.PoffShipmentDate?.ToString(shippingOrderForm.DateFormat)) ?? "";
                richText = cell.RichText.Add(cellValue);
                richText.Bold = false;
                #endregion

                #region Container
                cell = worksheet2.Cells[$"N2"];
                if (!isAirTransport)
                {
                    cell.IsRichText = true;

                    cellTitle = "Container: ";
                    richText = cell.RichText.Add(cellTitle);
                    richText.Bold = true;

                    richText = cell.RichText.Add(shippingOrderForm.BookingContainers);
                    richText.Bold = false;
                }
                else
                {
                    cell = worksheet2.Cells[$"F2:Q2"];
                    cell.Merge = true;
                }
                #endregion

                #region Remark
                cell = worksheet2.Cells[$"A3"];
                cell.IsRichText = true;

                cellTitle = "Remarks: ";
                richText = cell.RichText.Add(cellTitle);
                richText.Bold = true;

                richText = cell.RichText.Add(shippingOrderForm.Remarks);
                richText.Bold = false;
                #endregion

                /*
                 * Copy sheet2 into the end of sheet1.
                 * Then delete the sheet2 out of the workbook
                 */

                // move to the next row
                rowCursor++;

                // copy
                worksheet2.Cells["A1:Q13"].Copy(worksheet.Cells[$"A{rowCursor}:Q{rowCursor}"]);

                var row = worksheet.Row(rowCursor + 1); // Expected Ship Date, Cargo Ready Date, Container
                row.Height = 21;
                row = worksheet.Row(rowCursor + 2); // Remark
                row.Height = 21;

                #region Different Sized Carton
                if (shippingOrderForm.Orders != null && shippingOrderForm.Orders.Any(x => x.DifferentSizedCarton))
                {
                    // The same sized carton
                }
                else
                {
                    /*
                     * Note: Please do not use worksheet.DeleteRow it will make the form control hidden.
                     */
                    cell = worksheet.Cells[$"A{rowCursor + 11}:Q{rowCursor + 11}"];
                    cell.Value = "";
                    cell.Merge = false;
                    cell.Style.Border.Top.Style = ExcelBorderStyle.None;
                    cell.Style.Border.Right.Style = ExcelBorderStyle.None;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.None;
                    cell.Style.Border.Left.Style = ExcelBorderStyle.None;

                    cell = worksheet.Cells[$"A{rowCursor + 12}:Q{rowCursor + 12}"];
                    cell.Value = "";
                    cell.Merge = false;
                    cell.Style.Border.Top.Style = ExcelBorderStyle.None;
                    cell.Style.Border.Right.Style = ExcelBorderStyle.None;
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.None;
                    cell.Style.Border.Left.Style = ExcelBorderStyle.None;
                }
                #endregion

                // Delete sheet2
                package.Workbook.Worksheets.Delete(worksheet2);

                var byteArray = package.GetAsByteArray();

                // Close the package
                package.Dispose();

                return byteArray;
            }
        }
    }
}
