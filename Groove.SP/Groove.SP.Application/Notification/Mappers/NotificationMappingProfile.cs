using Groove.SP.Application.Mappers;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Core.Entities;
using System.Net;

namespace Groove.SP.Application.Notification.Mappers
{
    public class NotificationMappingProfile : MappingProfileBase<NotificationModel, NotificationViewModel>
    {
        public NotificationMappingProfile()
        {
            CreateMap<NotificationViewModel, NotificationModel>().ReverseMap();
        }
    }
}
