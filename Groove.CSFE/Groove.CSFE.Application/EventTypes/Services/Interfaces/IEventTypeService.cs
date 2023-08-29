using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.EventTypes.ViewModels;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.EventTypes.Services.Interfaces
{
    public interface IEventTypeService : IServiceBase<EventTypeModel, EventTypeViewModel>
    {
        Task<IEnumerable<DropDownListItem<string>>> GetEventTypesDropdownAsync();
    }
}

