using Groove.SP.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Groove.SP.Application.Activity.DomainEventHandlers
{
    public class ContainerActivityCreatedDomainEventHandler : INotificationHandler<ActivityCreatedDomainEvent>
    {
        public ContainerActivityCreatedDomainEventHandler() { }

        public async Task Handle(ActivityCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            if (!notification.ContainerId.HasValue)
            {
                return;
            }

            // TODO: handling logic after container activity has been created here.

            return;
        }
    }
}
