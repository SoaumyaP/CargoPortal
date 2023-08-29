using Groove.SP.Application.Utilities;
using Groove.SP.Core.Events;
using Groove.SP.Core.Models;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Groove.SP.Application.Activity.DomainEventHandlers
{
    public class ContainerActivityDeletedDomainEventHandler : INotificationHandler<ActivityDeletedDomainEvent>
    {
        public ContainerActivityDeletedDomainEventHandler() { }

        public async Task Handle(ActivityDeletedDomainEvent notification, CancellationToken cancellationToken)
        {
            var containerGlobalActivity = notification.Activity.GlobalIdActivities
                .FirstOrDefault(ga => ga.GlobalId.StartsWith(EntityType.Container));

            var containerId = containerGlobalActivity == null ? null : CommonHelper.GetEntityId(containerGlobalActivity.GlobalId, EntityType.Container);

            if (!containerId.HasValue)
            {
                return;
            }

            // TODO: handling logic after container activity has been deleted here.

            return;

        }
    }
}
