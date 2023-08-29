using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Notification.Interfaces;
using Groove.SP.Application.Notification.ViewModel;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Notification.Services
{
    public class NotificationService : ServiceBase<NotificationModel, NotificationViewModel>, INotificationService
    {
        private readonly IDataQuery _dataQuery;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly INotification _notification;
        private readonly IConfiguration _configuration;

        public NotificationService(IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery,
            IUserNotificationRepository userNotificationRepository,
            IUserProfileRepository userProfileRepository,
            INotification notification,
            IConfiguration configuration)
            : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
            _userNotificationRepository = userNotificationRepository;
            _userProfileRepository = userProfileRepository;
            _notification = notification;
            _configuration = configuration;
        }

        public async Task<ListPagingViewModel<NotificationListItemViewModel>> GetByUserNameAsync(string userName, int skip = 0, int take = 5)
        {
            var range = GetConfiguredDateRange();
            var fromDate = range["fromDate"];
            var toDate = range["toDate"];
            var storedProcedureName = "spu_GetNotificationList";
            List<SqlParameter> filterParameter = new()
            {
                new SqlParameter
                {
                    ParameterName = "@username",
                    Value = userName,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@fromDate",
                    Value = fromDate,
                    DbType = DbType.DateTime2,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@toDate",
                    Value = toDate,
                    DbType = DbType.DateTime2,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@skip",
                    Value = skip,
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@take",
                    Value = take,
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Input
                }
            };

            Func<DbDataReader, ListPagingViewModel<NotificationListItemViewModel>> mapping = (reader) =>
            {
                var mappedData = new ListPagingViewModel<NotificationListItemViewModel>
                {
                    Skip = skip,
                    Take = take
                };
                while (reader.Read())
                {
                    var tmpId = reader[0];
                    var tmpMessageKey = reader[1];
                    var tmpRead = reader[2];
                    var tmpReadUrl = reader[3];
                    var tmpCreatedDate = reader[4];
                    var tmpRowCount = reader[5];

                    var newRow = new NotificationListItemViewModel
                    {
                        Id = (long)tmpId,
                        MessageKey = tmpMessageKey?.ToString() ?? "",
                        IsRead = (bool)tmpRead,
                        ReadUrl = tmpReadUrl?.ToString() ?? "",
                        CreatedDate = (DateTime)tmpCreatedDate
                    };
                    mappedData.TotalItem = (long)(tmpRowCount != DBNull.Value ? tmpRowCount : 0);
                    mappedData.Items.Add(newRow);
                }

                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<int> GetUnreadTotalAsync(string userName)
        {
            var range = GetConfiguredDateRange();
            var fromDate = range["fromDate"];
            var toDate = range["toDate"];
            var query = Repository.QueryAsNoTracking(x => x.UserNotifications.Any(x => x.Username == userName && x.IsRead == false)
                                                    && (fromDate == null || x.CreatedDate >= fromDate)
                                                    && (toDate == null || x.CreatedDate <= toDate));

            return await query.CountAsync();
        }

        public async Task ReadAsync(long id, string userName)
        {
            var userNotification = await _userNotificationRepository.GetAsync(x => x.NotificationId == id && x.Username == userName);
            if (userNotification == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            // mark as read
            userNotification.IsRead = true;
            userNotification.Audit(userName);

            await UnitOfWork.SaveChangesAsync();

            // Send push notification silent
            try
            {
                var pushNotif = new PushNotification
                {
                    NotificationId = userNotification.NotificationId,
                    Type = PushNotificationType.Read
                };
                await _notification.SendAsync(pushNotif, userName);
            }
            catch { }
        }

        public async Task ReadAllAsync(string userName)
        {
            var range = GetConfiguredDateRange();
            var fromDate = range["fromDate"];
            var toDate = range["toDate"];
            var userNotifications = await _userNotificationRepository.Query(x => x.Username == userName
                                                                            && x.IsRead == false
                                                                            && (fromDate == null || x.Notification.CreatedDate >= fromDate)
                                                                            && (toDate == null || x.Notification.CreatedDate <= toDate)
                                                                      ).ToListAsync();

            foreach (var item in userNotifications)
            {
                // mark as read
                item.IsRead = true;
                item.Audit(userName);
            }

            await UnitOfWork.SaveChangesAsync();

            // Send push notification silent
            try
            {
                await _notification.SendAsync(new PushNotification { Type = PushNotificationType.ReadAll }, userName);
            }
            catch { }
        }

        public async Task PushNotificationSilentAsync(long orgId, NotificationViewModel notification)
        {
            try
            {
                var notificationModel = Mapper.Map<NotificationModel>(notification);
                var notifUsers = await _userProfileRepository.QueryAsNoTracking(x => x.OrganizationId == orgId).ToListAsync();

                notificationModel.UserNotifications = new List<UserNotificationModel>();
                foreach (var user in notifUsers)
                {
                    var userNotif = new UserNotificationModel
                    {
                        Notification = notificationModel,
                        IsRead = false,
                        Username = user.Username
                    };
                    userNotif.Audit();
                    notificationModel.UserNotifications.Add(userNotif);
                }

                notificationModel.Audit();
                await Repository.AddAsync(notificationModel);
                await UnitOfWork.SaveChangesAsync();

                var pushNotif = new PushNotification
                {
                    NotificationId = notificationModel.Id,
                    MessageKey = notificationModel.MessageKey
                };
                await _notification.SendToGroupAsync(pushNotif, $"ORG{orgId}");
            }
            catch
            {
            }
        }

        public async Task PushNotificationSilentAsync(List<string> userIds, List<string> emails, NotificationViewModel notification)
        {
            try
            {
                var notificationModel = Mapper.Map<NotificationModel>(notification);

                var notifUsers = await _userProfileRepository.QueryAsNoTracking(x => userIds.Select(c => long.Parse(c)).ToList().Contains(x.Id)).ToListAsync();

                notificationModel.UserNotifications = new List<UserNotificationModel>();
                foreach (var user in notifUsers)
                {
                    var userNotif = new UserNotificationModel
                    {
                        Notification = notificationModel,
                        IsRead = false,
                        Username = user.Username
                    };
                    userNotif.Audit();
                    notificationModel.UserNotifications.Add(userNotif);
                }

                notificationModel.Audit();
                await Repository.AddAsync(notificationModel);
                await UnitOfWork.SaveChangesAsync();

                var pushNotif = new PushNotification
                {
                    NotificationId = notificationModel.Id,
                    MessageKey = notificationModel.MessageKey
                };
                await _notification.SendAsync(pushNotif, emails);
            }
            catch
            {
            }
        }

        public async Task PushNotificationSilentAsync(List<string> userNames, NotificationViewModel notification)
        {
            try
            {
                var notificationModel = Mapper.Map<NotificationModel>(notification);

                notificationModel.UserNotifications = new List<UserNotificationModel>();
                foreach (var userName in userNames)
                {
                    var userNotif = new UserNotificationModel
                    {
                        Notification = notificationModel,
                        IsRead = false,
                        Username = userName
                    };
                    userNotif.Audit();
                    notificationModel.UserNotifications.Add(userNotif);
                }

                notificationModel.Audit();
                await Repository.AddAsync(notificationModel);
                await UnitOfWork.SaveChangesAsync();

                var pushNotif = new PushNotification
                {
                    NotificationId = notificationModel.Id,
                    MessageKey = notificationModel.MessageKey
                };
                await _notification.SendAsync(pushNotif, userNames);
            }
            catch
            {
            }
        }

        private Dictionary<string, DateTime?> GetConfiguredDateRange()
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (int.TryParse(_configuration.GetSection("Notification:ExpiresInDays").Value, out int days))
            {
                fromDate = DateTime.UtcNow.AddDays(-days);
                toDate = DateTime.UtcNow;
            }

            return new Dictionary<string, DateTime?>
            {
                { "fromDate", fromDate },
                { "toDate", toDate }
            };
        }
    }
}