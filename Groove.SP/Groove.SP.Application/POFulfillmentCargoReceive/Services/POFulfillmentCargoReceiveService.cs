using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.POFulfillmentCargoReceive.Services.Interfaces;
using Groove.SP.Application.POFulfillmentCargoReceive.ViewModels;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillmentCargoReceive.Services
{
    public class POFulfillmentCargoReceiveService : ServiceBase<POFulfillmentCargoReceiveModel, POFulfillmentCargoReceiveViewModel>, IPOFulfillmentCargoReceiveService
    {
        private readonly IPOFulfillmentCargoReceiveRepository _poFulfillmentCargoReceiveRepository;
        protected override Func<IQueryable<POFulfillmentCargoReceiveModel>, IQueryable<POFulfillmentCargoReceiveModel>> FullIncludeProperties
        {
            get
            {
                return x => x.Include(y => y.CargoReceiveItems);
            }
        }
        public POFulfillmentCargoReceiveService(IUnitOfWorkProvider unitOfWorkProvider,
            IPOFulfillmentCargoReceiveRepository poFulfillmentCargoReceiveRepository) : base(unitOfWorkProvider)
        {
            _poFulfillmentCargoReceiveRepository = poFulfillmentCargoReceiveRepository;
        }
        public async Task<POFulfillmentCargoReceiveViewModel> FirstByPOFulfillmentIdAsync(long poffId)
        {
            var model = await _poFulfillmentCargoReceiveRepository.QueryAsNoTracking(x => x.POFulfillmentId == poffId, null, FullIncludeProperties).FirstOrDefaultAsync();

            return model != null ? Mapper.Map<POFulfillmentCargoReceiveViewModel>(model) : null;
        }
    }
}
