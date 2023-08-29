using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.ArticleMaster.ViewModels;

[JsonConverter(typeof(MyConverter))]
public class ArticleMasterViewModel : ViewModelBase<ArticleMasterModel>, IHasFieldStatus
{
    public long Id { get; set; }
    public string CompanyCode { get; set; }
    public string CompanyName { get; set; }
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
    public string StatusName { 
        get
        {
            var statusName = "";
            if (Status == "0")
            {
                statusName = "Inactive";
            }
            else if (Status == "1")
            {
                statusName = "Active";
            }
            return statusName;
        }
        set { }
    }
    public string StyleNo { get; set; }
    public string ColourCode { get; set; }
    public string Size { get; set; }
    public string StyleName { get; set; }
    public string ColourName { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
    public bool IsPropertyDirty(string name)
    {
        return FieldStatus != null &&
               FieldStatus.ContainsKey(name) &&
               FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
    }

    public override void ValidateAndThrow(bool isUpdating = false)
    {

    }
}