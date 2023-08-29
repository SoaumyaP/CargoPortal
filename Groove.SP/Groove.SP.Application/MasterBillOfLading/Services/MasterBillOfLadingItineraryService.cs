using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.MasterBillOfLading.Services
{
    public class MasterBillOfLadingItineraryService : ServiceBase<MasterBillOfLadingItineraryModel, MasterBillOfLadingItineraryViewModel>, IMasterBillOfLadingItineraryService
    {
        public MasterBillOfLadingItineraryService(IUnitOfWorkProvider unitOfWorkProvider)
           : base(unitOfWorkProvider)
        {
        }
    }
}
