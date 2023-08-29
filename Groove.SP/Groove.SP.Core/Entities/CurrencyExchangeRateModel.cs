using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Core.Entities
{
    public class CurrencyExchangeRateModel : Entity
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime StartDate { get; set; }
    }
}