using System;

namespace Groove.SP.Core.Entities;

public class ArticleMasterModel : Entity
{
    public long Id { get; set; }
    public string CompanyCode { get; set; }
    public string CompanyType { get; set; }
    public string PONo { get; set; }
    public string ItemNo { get; set; }
    public string ShipmentNo { get; set; }
    public long POSeq { get; set; }
    public string DestCode { get; set; }
    public string OrderDetailKey { get; set; }
    public string CategoryCode { get; set; }
    public decimal? ItemDepth { get; set; }
    public decimal? ItemHeight { get; set; }
    public decimal? ItemWidth { get; set; }
    public string ItemDesc { get; set; }
    public decimal? UnitWeight { get; set; }
    public string CartonType { get; set; }
    public string AssignedSupplier { get; set; }
    public string SupplierType { get; set; }
    public string Barcode { get; set; }
    public string BarcodeType { get; set; }
    public long? Seller { get; set; }
    public int? InnerQuantity { get; set; }
    public decimal? InnerDepth { get; set; }
    public decimal? InnerHeight { get; set; }
    public decimal? InnerWidth { get; set; }
    public decimal? InnerGrossWeight { get; set; }
    public int? OuterQuantity { get; set; }
    public decimal? OuterDepth { get; set; }
    public decimal? OuterHeight { get; set; }
    public decimal? OuterWidth { get; set; }
    public decimal? OuterGrossWeight { get; set; }
    public string DisplaySetFlat { get; set; }
    public string MembersQuantity { get; set; }
    public string MembersItemId { get; set; }
    public decimal? ItemPrice { get; set; }
    public string ProcurementRule { get; set; }
    public string Status { get; set; }
    public string StyleNo { get; set; }
    public string ColourCode { get; set; }
    public string Size { get; set; }
    public string StyleName { get; set; }
    public string ColourName { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}