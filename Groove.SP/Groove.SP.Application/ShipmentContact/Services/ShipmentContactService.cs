using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.ShipmentContact.Mappers;
using Groove.SP.Application.ShipmentContact.Services.Interfaces;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.ShipmentContact.Services
{
    public class ShipmentContactService : ServiceBase<ShipmentContactModel, ShipmentContactViewModel>, IShipmentContactService
    {
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        public ShipmentContactService(IUnitOfWorkProvider unitOfWorkProvider, IPOFulfillmentRepository poFulfillmentRepository)
            : base(unitOfWorkProvider)
        {
            _poFulfillmentRepository = poFulfillmentRepository;
        }

        public async Task<IEnumerable<ShipmentContactViewModel>> GetShipmentContactsByShipmentAsync(long shipmentId)
        {
            var result = await Repository.Query(s => s.ShipmentId == shipmentId).ToListAsync();
            return Mapper.Map<IEnumerable<ShipmentContactViewModel>>(result);
        }

        public override async Task<ShipmentContactViewModel> CreateAsync(ShipmentContactViewModel viewModel)
        {
            viewModel.ValidateAndThrow();

            ShipmentContactModel model = Mapper.Map<ShipmentContactModel>(viewModel);

            var poFulfillment = await _poFulfillmentRepository.GetAsync(x => x.Shipments.Any(a => a.Id == viewModel.ShipmentId), includes: x => x.Include(y => y.Contacts));
            if (poFulfillment != null)
            {
                if (poFulfillment.FulfillmentType == FulfillmentType.Bulk
                    && model.OrganizationRole.Equals(OrganizationRole.DestinationAgent, StringComparison.InvariantCultureIgnoreCase))
                {
                    CopyContactToBooking(model, ref poFulfillment);
                }
            }

            var error = await ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();

            OnEntityCreated(model);

            viewModel = Mapper.Map<ShipmentContactViewModel>(model);
            return viewModel;
        }

        public override async Task<ShipmentContactViewModel> UpdateAsync(ShipmentContactViewModel viewModel, params object[] keys)
        {
            viewModel.ValidateAndThrow(true);

            var model = await Repository.FindAsync(keys);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", keys)} not found!");
            }

            Mapper.Map(viewModel, model);

            // if bulk booking, copy destination agent contact.
            var poFulfillment = await _poFulfillmentRepository.GetAsync(x => x.Shipments.Any(a => a.Id == model.ShipmentId), includes: x => x.Include(y => y.Contacts));
            if (poFulfillment != null)
            {
                if (poFulfillment.FulfillmentType == FulfillmentType.Bulk &&
                    model.OrganizationRole.Equals(OrganizationRole.DestinationAgent, StringComparison.InvariantCultureIgnoreCase))
                {
                    CopyContactToBooking(model, ref poFulfillment);
                }
            }

            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            this.OnEntityUpdated(model);

            viewModel = Mapper.Map<ShipmentContactViewModel>(model);
            return viewModel;
        }

        protected void CopyContactToBooking(ShipmentContactModel model, ref POFulfillmentModel poFulfillment)
        {
            var storedContact = poFulfillment.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(model.OrganizationRole, StringComparison.InvariantCultureIgnoreCase));

            if (storedContact != null)
            {
                storedContact.OrganizationId = model.OrganizationId;
                storedContact.CompanyName = model.CompanyName;
                storedContact.ContactName = model.ContactName;
                storedContact.ContactEmail = model.ContactEmail;
                storedContact.ContactNumber = model.ContactNumber;
                storedContact.Address = CompanyAddressLinesResolver.SplitCompanyAddressLines(model.Address, 1, "\\n");
                storedContact.AddressLine2 = CompanyAddressLinesResolver.SplitCompanyAddressLines(model.Address, 2, "\\n");
                storedContact.AddressLine3 = CompanyAddressLinesResolver.SplitCompanyAddressLines(model.Address, 3, "\\n");
                storedContact.AddressLine4 = CompanyAddressLinesResolver.SplitCompanyAddressLines(model.Address, 4, "\\n");
            }
            else
            {
                poFulfillment.Contacts.Add(new()
                {
                    OrganizationId = model.OrganizationId,
                    OrganizationRole = model.OrganizationRole,
                    CompanyName = model.CompanyName,
                    ContactName = model.ContactName,
                    ContactEmail = model.ContactEmail,
                    ContactNumber = model.ContactNumber,
                    Address = CompanyAddressLinesResolver.SplitCompanyAddressLines(model.Address, 1, "\\n"),
                    AddressLine2 = CompanyAddressLinesResolver.SplitCompanyAddressLines(model.Address, 2, "\\n"),
                    AddressLine3 = CompanyAddressLinesResolver.SplitCompanyAddressLines(model.Address, 3, "\\n"),
                    AddressLine4 = CompanyAddressLinesResolver.SplitCompanyAddressLines(model.Address, 4, "\\n"),
                    CreatedBy = model.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = model.CreatedBy,
                    UpdatedDate = DateTime.UtcNow
                });
            }
        }
    }
}