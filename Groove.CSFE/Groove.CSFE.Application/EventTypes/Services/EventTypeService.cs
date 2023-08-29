using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.EventTypes.Services.Interfaces;
using Groove.CSFE.Application.EventTypes.ViewModels;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.EventTypes.Services
{
    public class EventTypeService : ServiceBase<EventTypeModel, EventTypeViewModel>, IEventTypeService
    {
        public EventTypeService(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {

        }

        public async Task<IEnumerable<DropDownListItem<string>>> GetEventTypesDropdownAsync()
        {
            var eventType = await Repository.QueryAsNoTracking().Select(c => new DropDownListItem<string> { Text = c.Description, Value = c.Code }).ToListAsync();
            return eventType;
        }
    }
}
