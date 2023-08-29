

using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.EmailNotification.ViewModel;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.EmailNotification.Services.Interfaces
{
    public interface IEmailNotificationService: IServiceBase<EmailNotificationModel, EmailNotificationViewModel>
    {
        Task<EmailNotificationViewModel> GetAsync(long organizationId, long customerId, long locationId);

        Task<List<EmailNotificationViewModel>> GetListAsync(long organizationId, long customerId, long shipFromId);
    }
}