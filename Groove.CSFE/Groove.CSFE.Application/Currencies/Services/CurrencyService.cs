using System.Collections.Generic;

using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Currencies.ViewModels;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Currencies.Services
{
    public class CurrencyService : ServiceBase<CurrencyModel, CurrencyViewModel>, ICurrencyService
    {
        public CurrencyService(IUnitOfWorkProvider unitOfWorkProvider)
           : base(unitOfWorkProvider)
        { }

        public IEnumerable<CurrencyViewModel> GetAllCurrencies()
        {
            var model = this.Repository.GetListQueryable(null);

            return Mapper.Map<IEnumerable<CurrencyViewModel>>(model);
        }
    }
}
