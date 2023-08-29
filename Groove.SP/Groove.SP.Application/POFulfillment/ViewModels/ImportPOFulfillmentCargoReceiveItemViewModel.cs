using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class ImportPOFulfillmentCargoReceiveItemViewModel : ViewModelBase<POFulfillmentCargoReceiveItemModel>
    {
        public string PONo { get; set; }
        public int POSeq { get; set; }
        public string ProductCode { get; set; }
        public string StyleNo { get; set; }
        public string ColourCode { get; set; }
        public string SizeCode { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public decimal? Volume { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public string DGFlag { get; set; }
        public string Reason { get; set; }
        public int? InnerPacakageQty { get; set; }
        public int? OuterPackageQty { get; set; }
        public int Quantity { get; set; }
        public UnitUOMType? UnitUOM { get; set; }
        public DateTime InDate { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
