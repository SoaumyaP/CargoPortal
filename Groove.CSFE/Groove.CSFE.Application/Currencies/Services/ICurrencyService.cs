using System.Collections.Generic;

using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Currencies.ViewModels;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Currencies.Services
{
    public interface ICurrencyService : IServiceBase<CurrencyModel, CurrencyViewModel>
    {
        IEnumerable<CurrencyViewModel> GetAllCurrencies();
    }
}
