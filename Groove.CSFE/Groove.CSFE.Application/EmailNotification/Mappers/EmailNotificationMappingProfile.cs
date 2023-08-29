using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.EmailNotification.ViewModel;
using Groove.CSFE.Application.Mappers;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;

namespace Groove.CSFE.Application.EmailNotification.Mappers
{
    public class EmailNotificationMappingProfile : MappingProfileBase<EmailNotificationModel, EmailNotificationViewModel>
    {
        public EmailNotificationMappingProfile()
        {
            CreateMap<EmailNotificationViewModel, EmailNotificationModel>()
                .ForMember(d => d.RowVersion, opt => opt.Ignore())
                .ForMember(d => d.PortSelectionIds, opt => opt.MapFrom(s => string.Join(",", s.PortSelectionIds)))
                .ReverseMap()
                .ForMember(d => d.PortSelectionIds, opt => opt.MapFrom(s => string.IsNullOrEmpty(s.PortSelectionIds) ? new List<string>() : StringHelper.Split(s.PortSelectionIds, ',')));
        }
    }
}