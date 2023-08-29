using Groove.CSFE.Application.Common;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using System;

namespace Groove.CSFE.Application.Currencies.ViewModels
{
    public class CurrencyViewModel : ViewModelBase<CurrencyModel>
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal? ExchangeRate { get; set; }

        public byte? CurrencyPrecision { get; set; }

        public CurrencyStatus Status { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
