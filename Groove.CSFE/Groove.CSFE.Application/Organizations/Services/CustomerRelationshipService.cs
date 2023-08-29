using AutoMapper;
using Groove.CSFE.Application.Exceptions;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Organizations.Services.Interfaces;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Organizations.Services
{
    public class CustomerRelationshipService : ICustomerRelationshipService
    {
        private readonly IRepository<CustomerRelationshipModel> _customerRelationshipRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper Mapper;

        public CustomerRelationshipService(IRepository<CustomerRelationshipModel> customerRelationshipRepository,
            IUnitOfWorkProvider unitOfWorkProvider)
        {
            _customerRelationshipRepository = customerRelationshipRepository;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWork();
            this.Mapper = this._unitOfWork.GetMapper();
        }

        public async Task UpdateAsync(long customerId, long supplierId, CustomerRelationshipViewModel viewModel, string userName)
        {
            var model = await _customerRelationshipRepository.GetAsync(x => x.CustomerId == customerId && x.SupplierId == supplierId);           

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", new long[] { customerId, supplierId })} not found!");
            }

            model.Audit(userName);
            Mapper.Map(viewModel, model);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
