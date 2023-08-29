using System.Linq;
using System;
using FluentValidation;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Core.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Groove.SP.Core.Entities;
using Groove.SP.Infrastructure.CSFE.Models;
using System.Collections.Generic;

namespace Groove.SP.Application.PurchaseOrders.Validations
{
    public class ExcelPOContactViewModelValidator : AbstractValidator<ExcelPOContactViewModel>
    {
        private readonly IDataQuery _dataQuery;
        private IEnumerable<Organization> _organizations;
        private long? _customerId;

        public ExcelPOContactViewModelValidator(ICSFEApiClient csfeApiClient, IDataQuery dataQuery, long? customerId)
        {
            _dataQuery = dataQuery;
            var roles = csfeApiClient.GetAllOrganizationRolesAsync().Result;
            var organizations = csfeApiClient.GetActiveOrganizationsAsync().Result;
            _organizations = organizations;
            _customerId = customerId;

            RuleFor(a => a.PONumber).NotEmpty();
            RuleFor(a => a.OrganizationRole).NotEmpty();
            RuleFor(a => a.OrganizationCode).NotEmpty();

            RuleFor(a => a.OrganizationRole)
                .Must(x => roles.Any(r => r.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("'OrganizationRole':;importLog.inputtedDataNotExisting");

            RuleFor(a => a.OrganizationCode)
               .Must(x => organizations.Any(r => r.Code.Equals(x, StringComparison.OrdinalIgnoreCase)))
               .When(c => !c.OrganizationRole.Equals("Shipper", StringComparison.OrdinalIgnoreCase) && !c.OrganizationRole.Equals("Supplier", StringComparison.OrdinalIgnoreCase))
               .WithMessage("'OrganizationCode':;importLog.inputtedDataNotExisting");

            RuleFor(a => a.OrganizationCode)
               .MustAsync(async (organizationCode, cancellation) => await ValidateShipperSupplierCode(organizationCode))
               .When(c => string.IsNullOrEmpty(c.OrganizationCode) == false)
               .When(c => c.OrganizationRole.Equals("Shipper", StringComparison.OrdinalIgnoreCase) || c.OrganizationRole.Equals("Supplier", StringComparison.OrdinalIgnoreCase))
               .WithMessage("importLog.supplierAndShipperNotFound");
        }

        private async Task<bool> ValidateShipperSupplierCode(string orgCodeOrCustomerRefId)
        {
            var org = _organizations.FirstOrDefault(c => c.Code == orgCodeOrCustomerRefId);

            IQueryable<CustomerRelationshipQueryModel> query;
            string sql;
            sql = @"
                    SELECT SupplierId, CustomerId, CustomerRefId
                    FROM CustomerRelationship
                    WHERE  CustomerId = {0} AND (SupplierId = {1} OR CustomerRefId = {2})
                ";

            query = _dataQuery.GetQueryable<CustomerRelationshipQueryModel>(sql, _customerId, org?.Id, orgCodeOrCustomerRefId);
            var result = await query.CountAsync();
            return result > 0;
        }
    }
}
