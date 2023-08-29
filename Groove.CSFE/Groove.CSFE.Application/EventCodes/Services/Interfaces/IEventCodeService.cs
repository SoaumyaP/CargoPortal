using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.EventCodes.ViewModels;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.EventCodes.Services.Interfaces
{
    public interface IEventCodeService : IServiceBase<EventCodeModel, EventCodeViewModel>
    {
        Task<EventCodeViewModel> GetByCodeAsync(string code);

        Task<IEnumerable<EventCodeViewModel>> GetByCodesAsync(IEnumerable<string> codes);

        Task<IEnumerable<EventCodeViewModel>> GetByTypeAsync(string types);

        Task<IEnumerable<EventCodeViewModel>> GetByLevelAsync(int level);

        Task<IEnumerable<DropDownListItem<string>>> GetDropdownListAsync();

        Task<bool> CheckEventCodeAlreadyExistAsync(string code);

        Task<CreateEventCodeViewModel> CreateAsync(CreateEventCodeViewModel viewModel, string userName);
        Task<CreateEventCodeViewModel> UpdateAsync(CreateEventCodeViewModel viewModel, string userName);

        Task UpdateSequenceAsync(List<UpdateEventSequenceViewModel> model, string userName);

        Task UpdateStatusAsync(string activityCode, UpdateEventStatusViewModel vm, string username);
    }
}