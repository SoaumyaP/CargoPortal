using Groove.SP.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Groove.SP.Application.Activity.DomainEventHandlers
{
    public class ContainerActivityChangedDomainEventHandler : INotificationHandler<ActivityChangedDomainEvent>
    {
        public ContainerActivityChangedDomainEventHandler() { }

        public async Task Handle(ActivityChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.CurrentContainerId.HasValue)
            {
                return;
            }

            // TODO: handling logic after container activity has been changed here.

            return;
        }
    }
}