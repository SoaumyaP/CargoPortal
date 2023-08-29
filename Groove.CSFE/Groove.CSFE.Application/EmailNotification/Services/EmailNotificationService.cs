using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.EmailNotification.Services.Interfaces;
using Groove.CSFE.Application.EmailNotification.ViewModel;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Locations.Services.Interfaces;
using Groove.CSFE.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.EmailNotification.Services
{
    public class EmailNotificationService : ServiceBase<EmailNotificationModel, EmailNotificationViewModel>, IEmailNotificationService
    {
        private readonly ILocationService _location;

        public EmailNotificationService(
            IUnitOfWorkProvider unitOfWorkProvider,
            ILocationService location)
            : base(unitOfWorkProvider)
        {
            _location = location;
        }

        public async Task<EmailNotificationViewModel> GetAsync(long organizationId, long customerId, long shipFromId)
        {
            try
            {
                var emailNotificationSetups = await Repository.QueryAsNoTracking(c => c.OrganizationId == organizationId && c.CustomerId == customerId).OrderBy(c => c.Id).ToListAsync();

                var port = emailNotificationSetups.FirstOrDefault(s => !string.IsNullOrEmpty(s.PortSelectionIds) && s.PortSelectionIds.Split(",").Where(a=>!string.IsNullOrEmpty(a)).FirstOrDefault(c => c.Split("-")[1] == shipFromId.ToString()) != null);

                // Check port
                if (port != null)
                {
                    return Mapper.Map<EmailNotificationViewModel>(port);
                }
                // If there is no any port matched then check country
                else
                {
                    var location = await _location.GetAsync(shipFromId);
                    var country = emailNotificationSetups.FirstOrDefault(c => c.CountryId == location.CountryId && string.IsNullOrEmpty(c.PortSelectionIds));
                    if (country != null)
                    {
                        return Mapper.Map<EmailNotificationViewModel>(country);
                    }
                }

                var result = emailNotificationSetups.FirstOrDefault(c => c.CountryId == null && string.IsNullOrEmpty(c.PortSelectionIds));
                if (result != null)
                {
                    return Mapper.Map<EmailNotificationViewModel>(result);
                }
                return null;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<EmailNotificationViewModel>> GetListAsync(long organizationId, long customerId, long shipFromId)
        {
            try
            {
                var emailNotifications = await Repository.QueryAsNoTracking(c => c.OrganizationId == organizationId && c.CustomerId == customerId).OrderBy(c => c.Id).ToListAsync();

                var ports = emailNotifications.Where(
                    s => !string.IsNullOrEmpty(s.PortSelectionIds) &&
                    s.PortSelectionIds.Split(",").Where(a => !string.IsNullOrEmpty(a)).FirstOrDefault(c => c.Split("-")[1] == shipFromId.ToString()) != null).ToList();

                // Check port
                if (ports != null && ports.Any())
                {
                    return Mapper.Map<List<EmailNotificationViewModel>>(ports);
                }
                // If there is no any port matched then check country
                else
                {
                    var location = await _location.GetAsync(shipFromId);
                    var country = emailNotifications.Where(c => c.CountryId == location.CountryId && string.IsNullOrEmpty(c.PortSelectionIds)).ToList();
                    if (country != null)
                    {
                        return Mapper.Map<List<EmailNotificationViewModel>>(country);
                    }
                }

                var result = emailNotifications.Where(c => c.CountryId == null && string.IsNullOrEmpty(c.PortSelectionIds)).ToList();
                if (result != null)
                {
                    return Mapper.Map<List<EmailNotificationViewModel>>(result);
                }
                return null;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}
