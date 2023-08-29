using Groove.SP.Application.Common;
using Groove.SP.Application.ContractType.Services.Interfaces;
using Groove.SP.Application.ContractType.ViewModels;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.ContractType.Services
{
    public class ContractTypeService : ServiceBase<ContractTypeModel, ContractTypeViewModel>, IContractTypeService
    {
        public ContractTypeService(IUnitOfWorkProvider uow) : base(uow)
        {
        }
    }
}
