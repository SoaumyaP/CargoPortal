// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportProvider.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Groove.SP.Infrastructure.Report
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Groove.SP.Application.Exceptions;
    using Groove.SP.Application.Provider.Report;
    using Groove.SP.Application.Reports.ViewModels;
    using Groove.SP.Application.Scheduling.Validations;
    using Groove.SP.Application.Scheduling.ViewModels;
    using Groove.SP.Core.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    public class TelerikReportProvider : ITelerikReportProvider
    {
        private readonly ILogger<TelerikReportProvider> _logger;
        private readonly AppConfig _appConfig;
        private readonly HttpClient _httpClient;
        private readonly string _reportServerApi;
        private dynamic _telerikAccessToken;

        /// <summary>
        /// Caching report ID for better performance
        /// </summary>
        private readonly Dictionary<string, string> _reportIdCache = new Dictionary<string, string>();

        private readonly Dictionary<string, string> _environmentCategoryMap = new Dictionary<string, string>()
        {
            { "QC-SP" , "QC-SP" },
            { "Staging" , "Staging" },
            { "Production" , "Production" }
        };

        /// <summary>
        /// Caching access token for better performance
        /// </summary>
        private string _accessToken;
        private DateTime _accessTokenExpiredTime;

        public TelerikReportProvider(IOptions<AppConfig> appConfig, ILogger<TelerikReportProvider> logger)
        {
            _logger = logger;
            _appConfig = appConfig.Value;
            _httpClient = new HttpClient();
            _reportServerApi = Path.Combine(_appConfig.Report.ReportServerUrl, "api/reportserver/");
        }

        public async Task<Stream> ExportAsync(ReportRequest request)
        {
            var reportIdInCache = false;
            try
            {
                // get report server access token if expired
                if (DateTime.UtcNow >= _accessTokenExpiredTime)
                {
                    var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                    _accessToken = loginResult.access_token;
                    _accessTokenExpiredTime = DateTime.UtcNow.AddSeconds((double)loginResult.expires_in - 30); // lets renew the token 30s earlier for safety
                }

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                // get the report ID, report ID is cached for better performance each time a new report is requested
                reportIdInCache = _reportIdCache.TryGetValue(request.ReportName, out var reportId);
                if (string.IsNullOrEmpty(reportId))
                {
                    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    environmentName = environmentName == "Development" ? "QC" : environmentName;
                    environmentName = environmentName + "-SP";
                    _environmentCategoryMap.TryGetValue(environmentName, out var categoryName);
                    categoryName = categoryName ?? environmentName;

                    var reportName = request.ReportName;
                    var categoryId = await GetCategoryIdAsync(categoryName);
                    reportId = await GetReportIdAsync(reportName, categoryId);

                    // remove key if existed before add
                    // to prevent key already exist in dictionary error
                    // for case same report is requested while awaiting reportId of previous request
                    _reportIdCache.Remove(request.ReportName);
                    _reportIdCache.Add(request.ReportName, reportId);
                }

                // export and download the report
                var documentId = await GenerateReportAsync(reportId, request.ReportFormat, request.ReportParameters);
                var response = await _httpClient.GetAsync(Path.Combine(_reportServerApi, $"Documents/{documentId}"));
                var result = await response.Content.ReadAsStreamAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Telerik Report Server - {AppException.GetTrueExceptionMessage(ex)}"); // try to find out the report server problem
                if (reportIdInCache)
                {
                    _logger.LogInformation($"Telerik Report Server - Report cache is outdate. Invalidate cache for {request.ReportName}.");
                    _reportIdCache.Remove(request.ReportName);
                }

                throw;
            }
        }       

        public static string GetExtension(ReportFormat format)
        {
            string extension;
            switch (format)
            {
                case ReportFormat.Pdf:
                    extension = "pdf";
                    break;
                default:
                    extension = "pdf";
                    break;
            }

            return extension;
        }

        public async Task<TelerikAccessTokenModel> GetAccessToken(string username, string password)
        {
            var httpClient = new HttpClient();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            var response = await httpClient.PostAsync($"{Path.Combine(_appConfig.Report.ReportServerUrl, "Token")}", formContent);
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TelerikAccessTokenModel>(jsonResult);

            return result;
        }

        public async Task<TelerikTaskModel> CreateSchedulingAsync(TelerikTaskModel taskData)
        {

            var dataValidator = new TelerikTaskModelValidator();
            var dataValidatorResult = dataValidator.Validate(taskData);
            if (!dataValidatorResult.IsValid)
            {
                throw new ApplicationException($"Bad data prior to create new Telerik task: {dataValidatorResult.Errors.Select(x => string.Join(", ", x.ErrorMessage))}");
            }

            if (!await ValidateTelerikResourcesAsync(taskData))
            {
                throw new ApplicationException($"Telerik report resources are not found");
            }

            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                //var value = new TelerikTaskModel
                //{
                //    Category = "CSPortal-Local",
                //    CategoryId = "29acf893658",
                //    DocumentFormat = "XLS",
                //    DocumentFormatDescr = "Excel 97-2003",
                //    Enabled = false,
                //    Id = "",
                //    MailTemplateBody = "<p>Hello {FirstName},</p><p><em>{ReportName}</em> report has been generated based on the following scheduled task:&nbsp;<strong>{TaskName}</strong>.</p><p>The report is available as an attachment to this e-mail.</p>",
                //    MailTemplateSubject = "Telerik Report Server - Task Scheduler notification",
                //    Name = Guid.NewGuid().ToString(),
                //    NextOccurrence = null,
                //    Parameters = "{\"filteringDate\":\"1\",\"periodDays\":10,\"etdFrom\":null,\"etdTo\":null,\"etaFrom\":null,\"etaTo\":null,\"cargoReadyDateFrom\":null,\"cargoReadyDateTo\":null,\"shipFromCountry\":[null],\"shipFromLocation\":[null],\"shipToCountry\":[null],\"shipToLocation\":[null],\"poNoFrom\":null,\"poNoTo\":null,\"bookingNo\":null,\"shipmentNo\":null,\"selectedCustomerId\":461,\"supplier\":462,\"houseBLNumber\":null,\"masterBLNumber\":null,\"poStage\":null,\"movementType\":null,\"containerNumber\":null,\"containerSize\":[null],\"contractType\":null,\"selectedColumns\":[\"BookingRef#\",\"SO#\",\"PO#\",\"Supplier Code\",\"Supplier Name\",\"Ship Mode\",\"Incoterm\",\"ShipFrom (POR)\",\"ShipTo (PODel)\",\"PO QTY\",\"PO Container\",\"Ex work Date (1st Archive)\",\"Ex work Date (Latest)\",\"Exp Delivery Date\",\"CBM\",\"KGS\",\"Item Quantity\",\"Booking Date (1st Archive)\",\"Booking Date (Latest)\",\"Cargo Ready Date\",\"Booked QTY (Ctns)\",\"Booked QTY (Pieces)\",\"Carrier\",\"Contract Type\",\"Booked Load Type\",\"Booked Container Size\",\"Booked Vessel\",\"Booked Voyage\",\"Actual Loading Type\",\"Actual Ship mode\",\"MBL\",\"HBL\",\"Actual Shipped Container Number\",\"Actual Shipped Container Size\",\"ShipToETADate\",\"Gate-In Date\",\"CY/CFS Closing Date\",\"1st Leg POL\",\"1st Leg POD\",\"1st Leg Vessel\",\"1st Leg Voyage\",\"1st Leg ETD\",\"1st Leg ATD\",\"1st Vessel ETA\",\"2nd Leg POL\",\"2nd Leg POD\",\"2nd Leg Vessel\",\"2nd Leg Voyage\",\"2nd Leg ETD\",\"2nd Leg ATD\",\"2nd Vessel ETA\",\"SO Release Date\",\"Handover at Origin (CargoReceive InDate)\",\"Shipped Qty\",\"Shipped Carton\",\"Shipped cbm\",\"Shipped weight\",\"Dialog\",\"PO Stage\"]}",
                //    RecurrenceRule = "FREQ=MONTHLY;INTERVAL=2;COUNT=100;BYDAY=2SU",
                //    Report = "Master Summary Report (PO Level)",
                //    ReportId = "dff698c5c94",
                //    StartDate = "2021-09-09T15:02:13.909Z"
                //};


                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Scheduling/Create"), new StringContent(JsonConvert.SerializeObject(taskData), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.NotAcceptable)
                {
                    throw new ApplicationException(response.ReasonPhrase);
                }

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TelerikSchedulingResultModel>(jsonResult);

                return result.Data.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateSchedulingAsync(TelerikTaskModel taskData)
        {
            var dataValidator = new TelerikTaskModelValidator();
            var dataValidatorResult = dataValidator.Validate(taskData);
            if (!dataValidatorResult.IsValid)
            {
                throw new ApplicationException($"Bad data prior to create new Telerik task: {dataValidatorResult.Errors.Select(x => string.Join(", ", x.ErrorMessage))}");
            }

            if (!await ValidateTelerikResourcesAsync(taskData))
            {
                throw new ApplicationException($"Telerik report resources are not found");
            }

            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");


                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Scheduling/Update"), new StringContent(JsonConvert.SerializeObject(taskData), Encoding.UTF8, "application/json"));

                // Not check, telerik is old version 2018, sometimes incorrect 500 response
                // response.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteSchedulingAsync(TelerikTaskModel taskData)
        {
            var dataValidator = new TelerikTaskModelValidator();
            var dataValidatorResult = dataValidator.Validate(taskData);
            if (!dataValidatorResult.IsValid)
            {
                throw new ApplicationException($"Bad data prior to create new Telerik task: {dataValidatorResult.Errors.Select(x => string.Join(", ", x.ErrorMessage))}");
            }

            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");


                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Scheduling/Destroy"), new StringContent(JsonConvert.SerializeObject(taskData), Encoding.UTF8, "application/json"));

                // Not check, telerik is old version 2018, sometimes incorrect 500 response
                // response.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<TelerikTaskModel>> GetSchedulingListAsync()
        {
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Scheduling/Read"), null);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TelerikSchedulingResultModel>(jsonResult);

                    return result.Data;
                }
                else
                {
                    Thread.Sleep(5000);
                    return await GetSchedulingListAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<IEnumerable<TelerikReportModel>> GetReportListAsync()
        {
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Report/Reports_Read"), null);

                // Not check, telerik is old version 2018, sometimes incorrect 500 response
                // response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TelerikReportResultModel>(jsonResult);

                return result.Data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<TelerikSubscriberModel>> GetSubscriberListAsync(string telerikTaskId)
        {
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.GetAsync(Path.Combine(_appConfig.Report.ReportServerUrl, $"Scheduling/Read_Subscribers?scheduledTaskId={telerikTaskId}"));

                // Not check, telerik is old version 2018, sometimes incorrect 500 response
                // response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<TelerikSubscriberModel>>(jsonResult);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task SetSubscribersAsync(string telerikTaskId, params string[] emails)
        {
            if (string.IsNullOrEmpty(telerikTaskId))
            {
                throw new ApplicationException($"Bad data, {telerikTaskId} is empty");
            }
            try
            {
                var scheduledTask = await GetScheduledTaskDetailsAsync(telerikTaskId);
                // RecurrenceRule != Once/Never
                if (!string.IsNullOrEmpty(scheduledTask.RecurrenceRule))
                {
                    scheduledTask.SubscriberIds = null;
                    scheduledTask.ExternalEmails = emails.ToList();
                    await UpdateScheduledTaskDetailsAsync(scheduledTask);
                }
                else
                {
                    // RecurrenceRule = Once/Never

                    // Replace with dummy recurrence rule to call Telerik API
                    // As legacy version Telerik 2018, there is error if RecurrenceRule = Once/Never
                    var recurrenceRule = scheduledTask.RecurrenceRule;
                    scheduledTask.SubscriberIds = null;
                    scheduledTask.ExternalEmails = emails.ToList();
                    scheduledTask.RecurrenceRule = "FREQ=DAILY;UNTIL=19700101T000000Z";
                    await UpdateScheduledTaskDetailsAsync(scheduledTask);

                    // Re-correct recurrence rule
                    var storedTasks = await GetSchedulingListAsync();
                    var storedTask = storedTasks.FirstOrDefault(x => x.Id == telerikTaskId);
                    storedTask.RecurrenceRule = recurrenceRule;
                    await UpdateSchedulingAsync(storedTask);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task RemoveSubscriberAsync(string telerikTaskId, string email)
        {
            if (string.IsNullOrEmpty(telerikTaskId))
            {
                throw new ApplicationException($"Bad data, telerik task id {telerikTaskId} is empty");
            }
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var postData = new
                {
                    TaskId = telerikTaskId,
                    UserId = email
                };

                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Scheduling/RemoveSubscriber"), new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));

                // Not check, telerik is old version 2018, sometimes incorrect 500 response
                // response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<TelerikActivityModel>> GetActivityListAsync(string telerikTaskId)
        {
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.GetAsync(Path.Combine(_appConfig.Report.ReportServerUrl, $"Scheduling/Read_Documents?id={telerikTaskId}"));

                // Not check, telerik is old version 2018, sometimes incorrect 500 response
                // response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IEnumerable<TelerikActivityModel>>(jsonResult);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task RemoveActivityAsync(string telerikDocumentId, string telerikTaskId)
        {
            if (string.IsNullOrEmpty(telerikTaskId) || string.IsNullOrEmpty(telerikTaskId))
            {
                throw new ApplicationException($"Bad data, telerik activity id {telerikDocumentId} or telerik task id {telerikTaskId} is empty");
            }
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var postData = new
                {
                    DocumentId = telerikDocumentId,
                    TaskId = telerikTaskId
                };

                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Scheduling/DeleteDocument"), new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));

                // Not check, telerik is old version 2018, sometimes incorrect 500 response
                // response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task ExecuteTaskAsync(string telerikTaskId)
        {
            if (string.IsNullOrEmpty(telerikTaskId))
            {
                throw new ApplicationException($"Bad data, telerik task id {telerikTaskId} is empty");
            }
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var postData = new
                {
                    Id = telerikTaskId
                };

                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Scheduling/ExecuteTask"), new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));

                // Not check, telerik is old version 2018, sometimes incorrect 500 response
                // response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<TelerikDocumentModel> GetDocumentByIdAsync(string telerikDocumentId)
        {
            if (string.IsNullOrEmpty(telerikDocumentId))
            {
                throw new ApplicationException($"Bad data, telerik document id {telerikDocumentId} is empty");
            }
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;
                
                _httpClient.DefaultRequestHeaders.Clear();

                // attach the access token to the requests
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                HttpResponseMessage response = await _httpClient.GetAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Scheduling/DownloadDocument/", telerikDocumentId));
                if (response.IsSuccessStatusCode)
                {
                    HttpContent content = response.Content;

                    var contentStream = await content.ReadAsStreamAsync(); // get the actual content stream
                    var contentBytes = await content.ReadAsByteArrayAsync();

                    var currentDateTimeFormat = DateTime.UtcNow.ToString("yyyyMMddTHHmmss");
                    var fileName = $"Doc_{telerikDocumentId}_{currentDateTimeFormat}.xlsx"; // default file name

                    if (response.Content.Headers.TryGetValues("content-disposition", out var values))
                    {
                        var docName = values.First(x => x.Contains("filename"))?.Split(";")
                            ?.Single(x => x.Contains("filename"))
                            ?.Replace("filename=", "")
                            ?.Replace("\"", "");
                        
                        if (!string.IsNullOrWhiteSpace(docName))
                        {
                            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(docName.Trim());
                            var extension = Path.GetExtension(docName.Trim());

                            fileName = $"{fileNameWithoutExtension}_{currentDateTimeFormat}{extension}";
                        }
                    }

                    return new TelerikDocumentModel
                    {
                        Id = telerikDocumentId,
                        Name = fileName,
                        Content = contentBytes
                    };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return null;
        }

        public async Task<string> CreateUserAsync(TelerikUserModel userData)
        {
            var dataValidator = new TelerikUserModelValidator();
            var dataValidatorResult = dataValidator.Validate(userData);
            if (!dataValidatorResult.IsValid)
            {
                throw new ApplicationException($"Bad data prior to create new Telerik user: {dataValidatorResult.Errors.Select(x => string.Join(", ", x.ErrorMessage))}");
            }

            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                // Telerik needs
                //{
                //    "Username": "system-psp3050-03081132",
                //    "Password": "Pass@123456",
                //    "Email": "psp3050+03081132@gmail.com",
                //    "FirstName": "psp3050-03081132",
                //    "LastName": "System User",
                //    "Enabled": true
                //}

                var response = await _httpClient.PostAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Api/ReportServer/Users/Local"), new StringContent(JsonConvert.SerializeObject(userData), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.NotAcceptable)
                {
                    throw new ApplicationException(response.ReasonPhrase);
                }
                

                var userId = await response.Content.ReadAsStringAsync();

                if(userData.UserRoleIds == null || !userData.UserRoleIds.Any())
                {
                    return userId;
                }

                // Telerik needs
                //{
                //    "UserRoleIds": [
                //        "2ca6b6eb3b9"
                //    ]
                //}

                response = await _httpClient.PutAsync(Path.Combine(_appConfig.Report.ReportServerUrl, $"Api/ReportServer/Users/{userId}/Roles"), new StringContent(JsonConvert.SerializeObject(userData), Encoding.UTF8, "application/json"));

                if (response.StatusCode == System.Net.HttpStatusCode.NotAcceptable)
                {
                    throw new ApplicationException(response.ReasonPhrase);
                }

                return userId;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<dynamic> LoginAsync(string username, string password)
        {
            if (_telerikAccessToken == null)
            {
                var httpClient = new HttpClient();

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("grant_type", "password")
                });

                var response = await httpClient.PostAsync($"{Path.Combine(_appConfig.Report.ReportServerUrl, "Token")}", formContent);
                response.EnsureSuccessStatusCode();

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(jsonResult);

                _telerikAccessToken = result;
            }
            return _telerikAccessToken;
        }

        private async Task<string> GetCategoryIdAsync(string categoryName)
        {
            var response = await _httpClient.GetAsync(Path.Combine(_reportServerApi, "Categories"));
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<dynamic>>(jsonResult);
            var category = result.Single(x => string.Equals((string)x.Name, categoryName, StringComparison.OrdinalIgnoreCase));

            return category.Id;
        }

        private async Task<string> GetReportIdAsync(string reportName, string categoryId)
        {
            var response = await _httpClient.GetAsync(Path.Combine(_reportServerApi, $"Categories/{categoryId}/Reports"));
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<dynamic>>(jsonResult);
            var report = result.Single(x => string.Equals((string)x.Name, reportName, StringComparison.OrdinalIgnoreCase));

            return report.Id;
        }

        private async Task<string> GenerateReportAsync(string reportId, ReportFormat format, dynamic reportParams)
        {
            var parameters = new { ReportId = reportId, Format = format.ToString().ToUpper(), ParameterValues = reportParams };
            var jsonParams = JsonConvert.SerializeObject(parameters);

            var response = await _httpClient.PostAsync(Path.Combine(_reportServerApi, "Documents"), new StringContent(jsonParams, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(jsonResult);

            return result.DocumentId;
        }

        /// <summary>
        /// To validate Telerik resource available: category id & name, report id & name
        /// </summary>
        /// <param name="taskData"></param>
        /// <returns></returns>
        private async Task<bool> ValidateTelerikResourcesAsync(TelerikTaskModel taskData)
        {
            var availableTelerikReports = await GetReportListAsync();

            if (availableTelerikReports == null || !availableTelerikReports.Any())
            {
                throw new ApplicationException("Telerik resource not found");
            }

            var isExisting = availableTelerikReports.Any(x => x.CategoryName == taskData.Category
                && x.CategoryId == taskData.CategoryId
                && x.Name == taskData.Report
                && x.Id == taskData.ReportId
            );

            return isExisting;
        }

        private async Task<TelerikAPIScheduledTaskModel> GetScheduledTaskDetailsAsync (string telerikTaskId)
        {
            if (string.IsNullOrEmpty(telerikTaskId))
            {
                throw new ApplicationException($"Bad data, telerik task id {telerikTaskId} is empty");
            }
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var postData = new
                {
                    Id = telerikTaskId
                };

                var response = await _httpClient.GetAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Api/ReportServer/ScheduledTasks/", telerikTaskId));

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<TelerikAPIScheduledTaskModel>(jsonResult);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return null;
        }

        private async Task UpdateScheduledTaskDetailsAsync(TelerikAPIScheduledTaskModel model)
        {
            try
            {
                var loginResult = await LoginAsync(_appConfig.Report.ReportUsername, _appConfig.Report.ReportPassword);
                _accessToken = loginResult.access_token;

                // attach the access token to all report server requests
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

                var response = await _httpClient.PutAsync(Path.Combine(_appConfig.Report.ReportServerUrl, "Api/ReportServer/ScheduledTasks/"), new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}