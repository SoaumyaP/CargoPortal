using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Provider.Report;
using Groove.SP.Application.Provider.Sftp;
using Groove.SP.Application.Scheduling.Services.Interfaces;
using Groove.SP.Application.Scheduling.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.Excel;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Scheduling.Services
{
    public class SchedulingService : ServiceBase<SchedulingModel, SchedulingViewModel>, ISchedulingService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        private readonly ITelerikReportProvider _telerikReportProvider;
        private readonly ISftpProvider _sftpProvider;
        private readonly IReportRepository _reportRepository;
        private readonly IFtpServerRepository _ftpServerRepository;

        public SchedulingService(
            IOptions<AppConfig> appConfig,
            IDataQuery dataQuery,
            IUnitOfWorkProvider unitOfWorkProvider,
            ITelerikReportProvider telerikReportProvider,
            ISftpProvider sftpProvider,
            IReportRepository reportRepository,
            IFtpServerRepository ftpServerRepository
            ) : base(unitOfWorkProvider)
        {
            _appConfig = appConfig.Value;
            _dataQuery = dataQuery;
            _telerikReportProvider = telerikReportProvider;
            _reportRepository = reportRepository;
            _sftpProvider = sftpProvider;
            _ftpServerRepository = ftpServerRepository;
        }

        public async Task<SchedulingViewModel> CreateSchedulingAsync(SchedulingViewModel data, IdentityInfo curentUser)
        {
            // Send request to Telerik to create new scheduled task
            var telerikTaskdata = Mapper.Map<TelerikTaskModel>(data);
            var newTelerikTask = await _telerikReportProvider.CreateSchedulingAsync(telerikTaskdata);


            // Set subscribers of the created task
            var subcribers = string.Join("; ", data.Subscribers?.Select(x => x.Email));
            await _telerikReportProvider.SetSubscribersAsync(newTelerikTask.Id, subcribers);

            // To save data to CS Portal
            var schedulingData = Mapper.Map<SchedulingViewModel>(newTelerikTask);

            // Fulfill audit information
            schedulingData.CSPortalReportId = data.CSPortalReportId;
            schedulingData.CreatedOrganizationId = curentUser.OrganizationId;

            schedulingData.Audit(curentUser.Username);
            var result = await CreateAsync(schedulingData);

            return result;

        }

        public async Task UpdateSchedulingAsync(SchedulingViewModel data, bool isInternal, long organizationId, string userName)
        {
            // Get scheduling information
            IQueryable<SchedulingModel> query;

            if (isInternal)
            {
                query = from scheduling in Repository.GetListQueryable()
                        join report in _reportRepository.GetListQueryable() on scheduling.CSPortalReportId equals report.Id
                        where scheduling.Id == data.Id
                        select scheduling;
            }
            else
            {
                query = from scheduling in Repository.GetListQueryable()
                        join report in _reportRepository.GetListQueryable() on scheduling.CSPortalReportId equals report.Id
                        where scheduling.Id == data.Id && scheduling.CreatedOrganizationId == organizationId
                        select scheduling;

            }
            var storedScheduling = query.FirstOrDefault();
            if (storedScheduling == null)
            {
                throw new ApplicationException($"Object with id {data.Id} not found");
            }

            Mapper.Map(data, storedScheduling);

            // Send request to Telerik to update scheduled task
            var telerikTaskdata = Mapper.Map<TelerikTaskModel>(data);
            await _telerikReportProvider.UpdateSchedulingAsync(telerikTaskdata);

            // Update scheduling is CS Portal
            storedScheduling.Audit(userName);
            Repository.Update(storedScheduling);
            UnitOfWork.SaveChanges();
        }


        public async Task<SchedulingViewModel> GetSchedulingAsync(long schedulingId, bool isInternal, long organizationId)
        {
            // Get scheduling information
            IQueryable<SchedulingModel> query;

            if (isInternal)
            {
                query = from scheduling in Repository.GetListQueryable()
                        join report in _reportRepository.GetListQueryable() on scheduling.CSPortalReportId equals report.Id
                        where scheduling.Id == schedulingId
                        select scheduling;
            }
            else
            {
                query = from scheduling in Repository.GetListQueryable()
                        join report in _reportRepository.GetListQueryable() on scheduling.CSPortalReportId equals report.Id
                        where scheduling.Id == schedulingId && scheduling.CreatedOrganizationId == organizationId
                        select scheduling;

            }
            var data = query.FirstOrDefault();
            if (data == null)
            {
                return null;
            }
            var result = Mapper.Map<SchedulingViewModel>(data);

            if (result != null)
            {
                // Get report information of selected report id
                var sql =
                  $@"
                    SELECT Id, ReportName, ReportUrl, ReportDescription, ReportGroup, null AS [LastRunTime], TelerikCategoryId, TelerikCategoryName, TelerikReportId
		            FROM Reports WITH(NOLOCK)
                    WHERE Id = {result.CSPortalReportId}";
                var reportInformation = _dataQuery.GetQueryable<ReportQueryModel>(sql).FirstOrDefault();
                result.CSPortalReport = reportInformation;

                // Get telerik task information
                var telerikTasks = await _telerikReportProvider.GetSchedulingListAsync();
                var telerikTask = telerikTasks.FirstOrDefault(x => x.Id == result.TelerikSchedulingId);
                if (telerikTask == null)
                {

                    throw new ApplicationException($"Object with id {schedulingId} not found");
                }
                Mapper.Map<TelerikTaskModel, SchedulingViewModel>(telerikTask, result);

                // Get subscribers of current task
                var subscribers = await _telerikReportProvider.GetSubscriberListAsync(telerikTask.Id);
                result.Subscribers = subscribers;

                // Get activities of the created task
                var activities = await _telerikReportProvider.GetActivityListAsync(telerikTask.Id);
                result.Activities = activities;

                return result;
            }
            return result;
        }

        public async Task UpdateSchedulingStatusAsync(long schedulingId, SchedulingStatus newStatus, bool isInternal, long organizationId, string userName)
        {
            // Get scheduling information
            IQueryable<SchedulingModel> query;

            if (isInternal)
            {
                query = from scheduling in Repository.GetListQueryable()
                        join report in _reportRepository.GetListQueryable() on scheduling.CSPortalReportId equals report.Id
                        where scheduling.Id == schedulingId
                        select scheduling;
            }
            else
            {
                query = from scheduling in Repository.GetListQueryable()
                        join report in _reportRepository.GetListQueryable() on scheduling.CSPortalReportId equals report.Id
                        where scheduling.Id == schedulingId && scheduling.CreatedOrganizationId == organizationId
                        select scheduling;

            }
            var data = query.FirstOrDefault();
            if (data == null)
            {
                throw new ApplicationException($"Object with id {schedulingId} not found");
            }

            // Update Telerik task
            var telerikTasks = await _telerikReportProvider.GetSchedulingListAsync();
            var telerikTask = telerikTasks?.FirstOrDefault(x => x.Id == data.TelerikSchedulingId);
            telerikTask.Enabled = newStatus == SchedulingStatus.Active;
            await _telerikReportProvider.UpdateSchedulingAsync(telerikTask);

            // Update scheduling is CS Portal
            data.Status = newStatus;
            data.Audit(userName);
            Repository.Update(data);
            UnitOfWork.SaveChanges();

        }

        public async Task DeleteSchedulingAsync(long schedulingId, bool isInternal, long organizationId)
        {
            // Get scheduling information
            IQueryable<SchedulingModel> query;

            if (isInternal)
            {
                query = from scheduling in Repository.GetListQueryable()
                        join report in _reportRepository.GetListQueryable() on scheduling.CSPortalReportId equals report.Id
                        where scheduling.Id == schedulingId
                        select scheduling;
            }
            else
            {
                query = from scheduling in Repository.GetListQueryable()
                        join report in _reportRepository.GetListQueryable() on scheduling.CSPortalReportId equals report.Id
                        where scheduling.Id == schedulingId && scheduling.CreatedOrganizationId == organizationId
                        select scheduling;

            }
            var data = query.FirstOrDefault();
            if (data == null)
            {
                throw new ApplicationException($"Object with id {schedulingId} not found");
            }

            // Update Telerik task
            var telerikTasks = await _telerikReportProvider.GetSchedulingListAsync();
            var telerikTask = telerikTasks?.FirstOrDefault(x => x.Id == data.TelerikSchedulingId);
            await _telerikReportProvider.DeleteSchedulingAsync(telerikTask);

            // Remove scheduling is CS Portal
            Repository.Remove(data);
            UnitOfWork.SaveChanges();
        }

        public async Task UploadFtpAsync(string telerikSchedulingId, string telerikDocumentId)
        {
            if (telerikSchedulingId is null || telerikDocumentId is null)
            {
                return;
            }

            var scheduling = await Repository.GetAsNoTrackingAsync(x => x.TelerikSchedulingId == telerikSchedulingId);

            if (scheduling is null)
            {
                throw new AppEntityNotFoundException($"Object with the id {telerikSchedulingId} not found!");
            }

            if (!scheduling.FtpServerId.HasValue)
            {
                return;
            }

            BackgroundJob.Enqueue<SchedulingService>(x => x.ProceedUploadFtpAsync(scheduling.Id, telerikDocumentId, scheduling.FtpServerId.Value));
        }

        /// <summary>
        /// To proceed upload document to ftp sever config
        /// </summary>
        /// <param name="schedulingId"></param>
        /// <param name="telerikDocumentId"></param>
        /// <param name="ftpServer"></param>
        /// <returns></returns>
        [JobDisplayName("Upload telerik document to sftp server - schedulingId: {0}, telerikDocumentId: {1}")]
        public async Task ProceedUploadFtpAsync(long schedulingId, string telerikDocumentId, long ftpServerId)
        {
            TelerikDocumentModel document = await _telerikReportProvider.GetDocumentByIdAsync(telerikDocumentId);

            if (document is null)
            {
                throw new AppEntityNotFoundException($"Document with the id {telerikDocumentId} not found!");
            }

            FtpServerModel ftpServer = await _ftpServerRepository.GetAsNoTrackingAsync(x => x.Id == ftpServerId);

            if (ftpServer is null)
            {
                throw new AppEntityNotFoundException($"Ftp server config with the id {ftpServerId} not found!");
            }

            try
            {
                var stream = new MemoryStream(document.Content);

                if (ftpServer.FileProtocol == FileProtocolType.SFTP)
                {
                    // convert excel to csv
                    if (document.Name.EndsWith(FileExtensions.EXCEL_WORKSHEET) || document.Name.EndsWith(FileExtensions.EXCEL_97_2003))
                    {
                        document.Content = stream.ConvertToCsv(document.Name);
                        document.Name = $"{Path.GetFileNameWithoutExtension(document.Name)}{FileExtensions.CSV}";
                    }

                    var profile = new SftpProfile
                    {
                        HostName = ftpServer.HostName,
                        Port = ftpServer.Port,
                        Username = ftpServer.Username,
                        BlobKeyId = ftpServer.PrivateKey
                    };

                    await _sftpProvider.UploadFileAsync(document.Content, document.Name, ftpServer.FolderName, profile);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
