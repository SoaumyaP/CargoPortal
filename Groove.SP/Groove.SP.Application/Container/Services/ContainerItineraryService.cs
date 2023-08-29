using Groove.SP.Application.Common;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.Container.Services
{
    public class ContainerItineraryService : ServiceBase<ContainerItineraryModel, ContainerItineraryViewModel>, IContainerItineraryService
    {
        public ContainerItineraryService(IUnitOfWorkProvider unitOfWorkProvider)
           : base(unitOfWorkProvider)
        {
        }
    }
}
