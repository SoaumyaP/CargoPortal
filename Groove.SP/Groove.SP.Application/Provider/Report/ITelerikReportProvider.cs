// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IReportProvider.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.SP.Application.Provider.Report
{
    using Groove.SP.Application.Reports.ViewModels;
    using Groove.SP.Application.Scheduling.ViewModels;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public interface ITelerikReportProvider
    {
        /// <summary>
        /// Exports report
        /// </summary>
        Task<Stream> ExportAsync(ReportRequest request);

        /// <summary>
        /// To get token from Telerik reporting server
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<TelerikAccessTokenModel> GetAccessToken(string username, string password);

        Task<TelerikTaskModel> CreateSchedulingAsync(TelerikTaskModel taskData);

        Task<IEnumerable<TelerikTaskModel>> GetSchedulingListAsync();

        Task<IEnumerable<TelerikSubscriberModel>> GetSubscriberListAsync(string telerikTaskId);

        Task SetSubscribersAsync(string telerikTaskId, params string[] emails);

        Task RemoveSubscriberAsync(string telerikTaskId, string email);

        Task<IEnumerable<TelerikActivityModel>> GetActivityListAsync(string telerikTaskId);

        Task RemoveActivityAsync(string telerikDocumentId, string telerikTaskId);

        Task UpdateSchedulingAsync(TelerikTaskModel taskData);

        Task DeleteSchedulingAsync(TelerikTaskModel taskData);

        Task ExecuteTaskAsync(string telerikTaskId);

        Task<TelerikDocumentModel> GetDocumentByIdAsync(string telerikDocumentId);
        
        /// <summary>
        /// Create Telerik user
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        public Task<string> CreateUserAsync(TelerikUserModel userData);
    }
}