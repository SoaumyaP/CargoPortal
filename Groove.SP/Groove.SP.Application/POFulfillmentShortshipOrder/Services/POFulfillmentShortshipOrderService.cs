using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.POFulfillmentShortshipOrder.Services.Interfaces;
using Groove.SP.Application.POFulfillmentShortshipOrder.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillmentShortshipOrder.Services
{
    public class POFulfillmentShortshipOrderService : ServiceBase<POFulfillmentShortshipOrderModel, POFulfillmentShortshipOrderViewModel>, IPOFulfillmentShortshipOrderService
    {
        public POFulfillmentShortshipOrderService(IUnitOfWorkProvider unitOfWorkProvider) : base(unitOfWorkProvider)
        {
        }

        public async Task<int> CountUnreadAsync(bool isInternal, string affiliates, long? orgId)
        {
            var unreadShortshipQuery = Repository.QueryAsNoTracking(c => c.IsRead == false && c.POFulfillment.Stage == POFulfillmentStage.ForwarderBookingRequest);
            List<IGrouping<long, POFulfillmentShortshipOrderModel>> groupedUnreadShortship;
            var result = new List<POFulfillmentShortshipOrderModel>();

            if (isInternal)
            {
                result = await unreadShortshipQuery.ToListAsync();
            }
            else
            {
                var listOfAffiliates = new List<long>();
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }
                unreadShortshipQuery = unreadShortshipQuery.Include(c => c.POFulfillment).ThenInclude(c => c.Contacts);
                result = await unreadShortshipQuery.Where(c => c.POFulfillment.Contacts.Any(s => s.OrganizationId == orgId)).ToListAsync();
            }

            groupedUnreadShortship = result.GroupBy(c => c.POFulfillmentId).ToList();
            return groupedUnreadShortship.Count();
        }

        public async Task ReadOrUnreadAsync(long id, POFulfillmentShortshipOrderViewModel viewModel)
        {
            var shortShipModel = await Repository.GetAsync(c => c.Id == id);
            shortShipModel.IsRead = !viewModel.IsRead;
            await UnitOfWork.SaveChangesAsync();
        }
    }
}
