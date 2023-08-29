using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Provider.Report;
using Groove.SP.Application.Reports.Services.Interfaces;
using Groove.SP.Application.Reports.ViewModels;
using Groove.SP.Application.Scheduling.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Infrastructure.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using MicrosoftSqlParameter = Microsoft.Data.SqlClient.SqlParameter;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Reports.Services
{
    public partial class ReportService : ServiceBase<ReportModel, ReportViewModel>, IReportService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        private readonly IExportManager _exportManager;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly ITelerikReportProvider _telerikReportProvider;

        public ReportService(
            IOptions<AppConfig> appConfig,
            IDataQuery dataQuery,
            IUnitOfWorkProvider unitOfWorkProvider,
            IExportManager exportManager,
            ICSFEApiClient csfeApiClient,
            ITelerikReportProvider reportProvider
            ) : base(unitOfWorkProvider)
        {
            _appConfig = appConfig.Value;
            _exportManager = exportManager;
            _csfeApiClient = csfeApiClient;
            _dataQuery = dataQuery;
            _telerikReportProvider = reportProvider;
        }

        public async Task<bool> IsAuthorized(long reportId, long selectedCustomerId, IdentityInfo currentUser)
        {
            // Currently, internal users can see all reports's data
            if (currentUser.IsInternal)
            {
                return true;
            }

            var report = await Repository.GetAsync(r => r.Id == reportId, null, i => i.Include(r => r.Permissions));

            if (report == null)
            {
                return false;
            }

            // Check on report permission
            var assignedOrganizationIds = report.Permissions
                    .Select(x => x.OrganizationIds)
                    .Distinct()
                    .SelectMany(x => x.Split(','));                

            var hasPermission = report.Permissions.Any(p => (p.RoleId == currentUser.UserRoleId) &&
            (string.IsNullOrEmpty(p.OrganizationIds) || assignedOrganizationIds.Contains(currentUser.OrganizationId.ToString())));

            // Check on selected principal as viewing report data
            var isValidReportPrincipal = false;

            if (currentUser.UserRoleId == (int)Role.Agent)
            {
                var sql = $@"
                        -- Default is invalid
                        SET @result = 0;

                        SELECT @result = 1
                        FROM AgentAssignments AA
                        INNER JOIN BuyerCompliances BC ON AA.BuyerComplianceId = BC.Id
                        WHERE BC.[Status] = 1 AND BC.Stage = 1 AND AA.AgentOrganizationId = {currentUser.OrganizationId} AND BC.OrganizationId = {selectedCustomerId}
                ";

                isValidReportPrincipal = _dataQuery.GetValueFromVariable(sql, null).Equals("1");
            }
            else
            {
                // Call CSFE for other roles
                isValidReportPrincipal = await _csfeApiClient.CheckValidReportPrincipalAsync(selectedCustomerId, currentUser.UserRoleId, currentUser.OrganizationId);
            }
            return hasPermission && isValidReportPrincipal;
        }
        public async Task<byte[]> ExportDataAsync(long reportId, string jsonFilter)
        {
            var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@jsonFilterSet",
                        Value = jsonFilter,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, DataTable> mappingCallback = (reader) =>
            {
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                return dataTable;
            };

            var report = await Repository.FindAsync(reportId);

            if (report == null)
            {
                throw new AppEntityNotFoundException($"Object with the key {reportId} not found!");
            }

            if (string.IsNullOrWhiteSpace(report.StoredProcedureName))
            {
                throw new AppException($"Stored procedure with the report {reportId} not found!");
            }

            // should be here to prevent exception as long report running
            report.LastRunTime = DateTime.UtcNow;
            await UnitOfWork.SaveChangesAsync();

            // 5 minutes
            // extend timeout to 5 minutes to run reports
            var timeoutInSeconds = 300;

            var data = await _dataQuery.GetDataByStoredProcedureAsync(report.StoredProcedureName, mappingCallback, filterParameter.ToArray(), timeoutInSeconds, AppDbConnectionName.SecondaryCsPortalDb);

            if (data.Rows.Count == 0)
            {
                return null;
            }           

            var xlsx = _exportManager.ExportDataTableToXlsx(data);

            // Customize excel template for each report.

            switch (report.Id)
            {
                case 1:
                    // booked status report                  
                    break;
                case 2:
                    // Not booked status report
                    break;
                case 3:
                    // Master summary report
                    var sheet = xlsx.Workbook.Worksheets["sheet1"];

                    // Grouped: Reference
                    using (var range = sheet.Cells["A1:B1"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(193, 199, 208));
                    }
                    // Grouped: PO
                    using (var range = sheet.Cells["C1:AA1"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(234, 230, 255));
                    }
                    // Grouped: Booking
                    using (var range = sheet.Cells["AB1:AL1"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 235, 230));
                    }
                    // Grouped: Shipment
                    using (var range = sheet.Cells["AM1:CC1"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(227, 252, 239));
                    }
                    // Grouped: Dialog
                    using (var range = sheet.Cells["CD1"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(234, 230, 255));
                    }
                    // Grouped: Status
                    using (var range = sheet.Cells["CE1"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(234, 230, 255));
                    }

                    var jsonResult = JsonConvert.DeserializeObject<dynamic>(jsonFilter);

                    if ("PO level".Equals((string)jsonResult.ReportType, StringComparison.OrdinalIgnoreCase))
                    {
                        // BookingRef#; SO#
                        sheet.DeleteColumn(1, 2);
                        // Item#; Promo Code (ProductRemark); Site Code
                        sheet.DeleteColumn(2, 3);
                        // Currency; Item Price
                        sheet.DeleteColumn(6, 2);
                        // Item Description
                        sheet.DeleteColumn(10);
                        // Inner Quantity; Outer Quantity
                        sheet.DeleteColumn(18, 2);
                    }
                    break;
                default:
                    break;
            }

            return xlsx.GetAsByteArray();
        }
        public int GrantReportPermission(ReportGrantPermissionViewModel data)
        {
            var sql = @"spu_GrantReportPermission @ReportId, @OrganizationIds, @GrantInternal, @GrantPrincipal, @GrantShipper, @GrantAgent, @GrantWarehouse, @CreatedBy, @CreatedDate";
            // Add filter parameter
            var parameters = new List<MicrosoftSqlParameter>
            {
                new MicrosoftSqlParameter
                {
                    ParameterName = "@ReportId",
                    Value = data.ReportId,
                    DbType = DbType.Int64,
                    Direction = ParameterDirection.Input
                },
                new MicrosoftSqlParameter
                {
                    ParameterName = "@OrganizationIds",
                    Value = string.IsNullOrEmpty(data.OrganizationIds) ? (object)DBNull.Value : data.OrganizationIds ,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new MicrosoftSqlParameter
                {
                    ParameterName = "@GrantInternal",
                    Value = data.GrantInternal,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new MicrosoftSqlParameter
                {
                    ParameterName = "@GrantPrincipal",
                    Value = data.GrantPrincipal,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new MicrosoftSqlParameter
                {
                    ParameterName = "@GrantShipper",
                    Value = data.GrantShipper,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new MicrosoftSqlParameter
                {
                    ParameterName = "@GrantAgent",
                    Value = data.GrantAgent,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new MicrosoftSqlParameter
                {
                    ParameterName = "@GrantWarehouse",
                    Value = data.GrantWarehouse,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new MicrosoftSqlParameter
                {
                    ParameterName = "@CreatedBy",
                    Value = data.CreatedBy,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new MicrosoftSqlParameter
                {
                    ParameterName = "@CreatedDate",
                    Value = data.CreatedDate,
                    DbType = DbType.DateTime,
                    Direction = ParameterDirection.Input
                }
            };

            var result = _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
            return result;
        }
        public async Task<ReportGrantPermissionViewModel> GetReportPermissionAsync(long reportId)
        {
            if (reportId == 0)
            {
                return null;
            }

            var storedProcedureName = "spu_GetReportPermission";

            var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@ReportId",
                        Value = reportId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            Func<DbDataReader, ReportGrantPermissionViewModel> mappingCallback = (reader) =>
            {

                while (reader.Read())
                {
                    var row = new ReportGrantPermissionViewModel
                    {
                        ReportId = Convert.ToInt64(reader[0].ToString()),
                        OrganizationIds = reader[1] as string,
                        GrantInternal = Convert.ToBoolean(reader[2].ToString()),
                        GrantPrincipal = Convert.ToBoolean(reader[3].ToString()),
                        GrantShipper = Convert.ToBoolean(reader[4].ToString()),
                        GrantAgent = Convert.ToBoolean(reader[5].ToString()),
                        GrantWarehouse = Convert.ToBoolean(reader[6].ToString()),
                        CreatedDate = Convert.ToDateTime(reader[7]),
                        CreatedBy = reader[8] as string
                    };
                    return row;
                }
                return new ReportGrantPermissionViewModel(reportId);
            };

            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mappingCallback, filterParameter.ToArray());
            return result;
        }

        public async Task RegisterTelerikUserAsync(long principalOrganizationId)
        {
            if (principalOrganizationId == 0)
            {
                return;
            }

            try
            {

                var principalOrganization = await _csfeApiClient.GetOrganizationByIdAsync(principalOrganizationId);
                if (principalOrganization == null || string.IsNullOrEmpty(principalOrganization.CustomerPrefix))
                {
                    return;
                }

                var userModel = new TelerikUserModel
                {
                    Username = $"system-{principalOrganization.CustomerPrefix}",
                    Email = $"system-{principalOrganization.CustomerPrefix}-csportal@cargofe.com",
                    FirstName = principalOrganization.CustomerPrefix,
                    LastName = "System User",
                    Password = _appConfig.Report.SystemUserPassword,
                    Enabled = true,
                    UserRoleIds = _appConfig.Report.SystemUserRoleIds.Split(';'),
                    OrganizationId = principalOrganizationId


                };

                var newTelerikUserId = await _telerikReportProvider.CreateUserAsync(userModel);

                var sql = $@"
                    IF NOT EXISTS (SELECT 1 FROM [report].[ReportingUserProfiles] WHERE OrganizationId = @p0 AND ReportUsername = @p1)
                    BEGIN
	                    INSERT INTO [report].[ReportingUserProfiles] ([OrganizationId], [ReportUsername], [SystemUser])
                        VALUES (@p0, @p1, @p2)
                    END
                ";
                var parameters = new object[]
                {
                   principalOrganizationId,
                   userModel.Username,
                   1
                };
                _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());


            }
            catch
            {
                throw;
            }
        }

    }
}
