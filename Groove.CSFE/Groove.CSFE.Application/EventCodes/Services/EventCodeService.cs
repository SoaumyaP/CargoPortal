using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.EventCodes.Services.Interfaces;
using Groove.CSFE.Application.EventCodes.ViewModels;
using Groove.CSFE.Application.Exceptions;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Groove.CSFE.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Groove.CSFE.Application.EventCodes.Services
{
    public class EventCodeService : ServiceBase<EventCodeModel, EventCodeViewModel>, IEventCodeService
    {
        private readonly IRepository<EventTypeModel> _eventTypeRepository;
        public EventCodeService(IUnitOfWorkProvider unitOfWorkProvider, IRepository<EventTypeModel> eventTypeRepository)
            : base(unitOfWorkProvider)
        {
            _eventTypeRepository = eventTypeRepository;
        }

        public override async Task<IEnumerable<EventCodeViewModel>> GetAllAsync()
        {
            var models = await this.Repository.QueryAsNoTracking(null, null, x => x.Include(ec => ec.ActivityType)).ToListAsync();
            return Mapper.Map<IEnumerable<EventCodeViewModel>>(models);
        }

        public async Task<EventCodeViewModel> GetByCodeAsync(string code)
        {
            var model = await Repository.GetAsNoTrackingAsync(s => s.ActivityCode == code,
                null,
                x => x.Include(ec => ec.ActivityType));

            return Mapper.Map<EventCodeModel, EventCodeViewModel>(model);
        }

        public async Task<IEnumerable<EventCodeViewModel>> GetByCodesAsync(IEnumerable<string> codes)
        {
            var events = await Repository.QueryAsNoTracking(s => codes.Contains(s.ActivityCode),
                null,
                x => x.Include(ec => ec.ActivityType)).ToListAsync();

            return Mapper.Map<IEnumerable<EventCodeViewModel>>(events);
        }

        public async Task<IEnumerable<EventCodeViewModel>> GetByTypeAsync(string types)
        {
            var listOfTypes = new List<string>();

            if (!string.IsNullOrEmpty(types))
            {
                listOfTypes = JsonConvert.DeserializeObject<List<string>>(types);
            }

            var models = await this.Repository.QueryAsNoTracking(s => listOfTypes.Contains(s.ActivityType.Code), null, x => x.Include(ec => ec.ActivityType)).ToListAsync();
            return Mapper.Map<IEnumerable<EventCodeViewModel>>(models);
        }

        public async Task<IEnumerable<EventCodeViewModel>> GetByLevelAsync(int level)
        {
            var models = await Repository.QueryAsNoTracking(s => s.ActivityType.EventLevel >= level, null, x => x.Include(ec => ec.ActivityType)).OrderBy(x => x.ActivityCode).ToListAsync();
            return Mapper.Map<IEnumerable<EventCodeViewModel>>(models);
        }

        public async Task<IEnumerable<DropDownListItem<string>>> GetDropdownListAsync()
        {
            var eventCodes = await Repository.
                    QueryAsNoTracking(orderBy: c => c.OrderBy(c => c.SortSequence))
                    .Select(c => new DropDownListItem<string> { Text = $"{c.ActivityCode} - {c.ActivityDescription}", Value = c.ActivityCode })
                    .ToListAsync();
            return eventCodes;
        }

        public async Task<bool> CheckEventCodeAlreadyExistAsync(string code)
        {
            var result = await Repository.AnyAsync(c => c.ActivityCode == code);
            return result;
        }

        public async Task<CreateEventCodeViewModel> UpdateAsync(CreateEventCodeViewModel viewModel, string userName)
        {
            var model = await Repository.GetAsync(c => c.ActivityCode == viewModel.ActivityCode);
            Mapper.Map(viewModel, model);
            model.Audit(userName);

            var eventCodes = await Repository.Query(orderBy: c => c.OrderBy(c => c.SortSequence)).ToListAsync();

            //Need to multiply 2 times SortSequence of events to swap index of 2 events
            //Then decrease or increase 1 value of sortSequence of event base on before/after selected event

            //e.g
            //1001 -> 1        //1001 -> 2 
            //1002 -> 2        //1002 -> 4
            //1003 -> 3   *2   //1003 -> 6
            //1004 -> 4        //1004 -> 8
            //1005 -> 5        //1005 -> 10

            //swap index of 1003 to before index of 1001
            //1003 -> 6
            //1001 -> 2 
            //1002 -> 4
            //1004 -> 8
            //1005 -> 10

            //decrease SortSequence of event 1003 by 1 value
            //1003 -> 1 (SortSequence of event 1001 is 2 so 2 - 1 = 1)

            //Arrange list of events by SortSequence again
            //1003 -> 1
            //1001 -> 2 
            //1002 -> 4
            //1004 -> 8
            //1005 -> 10

            //Loop and set value for SortSequence again
            //1003 -> 1
            //1001 -> 2 
            //1002 -> 3
            //1004 -> 4
            //1005 -> 5

            for (int i = 0; i < eventCodes.Count; i++)
            {
                eventCodes[i].SortSequence = eventCodes[i].SortSequence * 2;
            }

            if (viewModel.EventOrderType == EventOrderType.Before)
            {
                var beforeEvent = eventCodes.FirstOrDefault(c => c.ActivityCode == viewModel.BeforeEvent);
                model.SortSequence = beforeEvent.SortSequence - 1;
            }
            else
            {
                var afterEvent = eventCodes.FirstOrDefault(c => c.ActivityCode == viewModel.AfterEvent);
                model.SortSequence = afterEvent.SortSequence + 1;
            }
            eventCodes = eventCodes.OrderBy(c => c.SortSequence).ToList();
            for (int i = 0; i < eventCodes.Count; i++)
            {
                eventCodes[i].SortSequence = i + 1;
            }

            await UnitOfWork.SaveChangesAsync();
            return viewModel;
        }

        public async Task<CreateEventCodeViewModel> CreateAsync(CreateEventCodeViewModel viewModel, string userName)
        {
            var model = Mapper.Map<EventCodeModel>(viewModel);
            model.Audit(userName);
            var eventCodes = await Repository.Query().ToListAsync();

            switch (viewModel.EventOrderType)
            {
                case EventOrderType.Before:
                    var beforeEvent = eventCodes.FirstOrDefault(c => c.ActivityCode == viewModel.BeforeEvent);
                    if (beforeEvent?.SortSequence == 1)
                    {
                        model.SortSequence = 1;
                        foreach (var item in eventCodes)
                        {
                            item.SortSequence++;
                        }
                    }
                    else
                    {
                        model.SortSequence = beforeEvent.SortSequence;
                        for (int i = 0; i < eventCodes.Count; i++)
                        {
                            if (eventCodes[i].SortSequence >= model.SortSequence)
                            {
                                eventCodes[i].SortSequence++;
                            }
                        }
                    }
                    break;

                case EventOrderType.After:
                    var afterEvent = eventCodes.FirstOrDefault(c => c.ActivityCode == viewModel.AfterEvent);
                    var maxSequence = eventCodes.Max(c => c.SortSequence);
                    if (afterEvent?.SortSequence == maxSequence)
                    {
                        model.SortSequence = maxSequence + 1;
                    }
                    else
                    {
                        model.SortSequence = afterEvent.SortSequence + 1;
                        for (int i = 0; i < eventCodes.Count; i++)
                        {
                            if (eventCodes[i].SortSequence >= model.SortSequence)
                            {
                                eventCodes[i].SortSequence++;
                            }
                        }
                    }

                    break;
                default:
                    break;
            }

            UnitOfWork.BeginTransaction();

            await Repository.AddAsync(model);
            await UnitOfWork.SaveChangesAsync();

            UnitOfWork.CommitTransaction();
            return Mapper.Map<CreateEventCodeViewModel>(model);
        }

        public async Task UpdateSequenceAsync(List<UpdateEventSequenceViewModel> model, string userName)
        {
            var actCodes = model.Select(x => x.ActivityCode).ToList();
            var evCodes = await Repository.Query(x => actCodes.Contains(x.ActivityCode)).ToListAsync();

            foreach (var ev in model)
            {
                var evCode = evCodes.First(x => x.ActivityCode == ev.ActivityCode);
                Mapper.Map(ev, evCode);
                evCode.Audit(userName);
            }

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(string activityCode, UpdateEventStatusViewModel vm, string username)
        {
            var ev = await Repository.FindAsync(activityCode);

            if (ev == null) throw new AppEntityNotFoundException($"Object with the code {activityCode} not found!");

            if (Enum.IsDefined(typeof(EventCodeStatus), vm.Status))
            {
                ev.Status = vm.Status;
                ev.Audit(username);
                await UnitOfWork.SaveChangesAsync();
            }
        }
    }
}
