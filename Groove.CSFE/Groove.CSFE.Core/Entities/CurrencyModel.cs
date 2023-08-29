using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.CSFE.Core.Entities
{
    public class CurrencyModel : Entity
    {
        public long Id { get; set; }
        
        public string Code { get; set; }
        
        public string Name { get; set; }

        public string Symbol { get; set; }

        public decimal? ExchangeRate { get; set; }

        public byte? CurrencyPrecision { get; set; }

        public CurrencyStatus Status { get; set; }
    }
}
