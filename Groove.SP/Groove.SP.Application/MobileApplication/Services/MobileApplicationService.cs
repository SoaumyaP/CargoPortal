using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.MobileApplication.Services.Interfaces;
using Groove.SP.Application.MobileApplication.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities.Mobile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.MobileApplication.Services
{
    public class MobileApplicationService : ServiceBase<MobileApplicationModel, MobileApplicationViewModel>, IMobileApplicationService
    {
        private readonly IDataQuery _efdataQuery;
        public MobileApplicationService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery)
            : base(unitOfWorkProvider)
        {
            _efdataQuery = dataQuery;
        }

        public async Task<MobileTodayUpdateViewModel> CheckForTodayUpdatesAsync()
        {
            var sql = @"
                
                SELECT TOP(1) APP.[Version], App.PackageUrl FROM mobile.Applications APP ORDER BY APP.PublishedDate ASC;

                SELECT TOP(1) APP.[Version], App.PackageUrl FROM mobile.Applications APP ORDER BY APP.PublishedDate DESC;

                 ";

            Func<DbDataReader, List<MobileTodayUpdateViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<MobileTodayUpdateViewModel>();
                MobileTodayUpdateViewModel newRow;

                while (reader.Read())
                {
                    newRow = new MobileTodayUpdateViewModel
                    {
                        Version = reader[0].ToString(),
                        PackageUrl = reader[1].ToString()
                    };
                    mappedData.Add(newRow);
                }

                if (!mappedData.Any())
                {
                    mappedData.Add(null);
                }

                reader.NextResult();

                while (reader.Read())
                {
                    newRow = new MobileTodayUpdateViewModel
                    {
                        Version = reader[0].ToString(),
                        PackageUrl = reader[1].ToString()
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            };

            var result = _efdataQuery.GetDataBySql(sql, mapping);

            var currentVersion = result[0];
            var newVersion = result[1];

            return new MobileTodayUpdateViewModel
            {
                Version = newVersion.Version,
                PackageUrl = newVersion.PackageUrl,
                IsNewRelease = newVersion.Version != currentVersion.Version
            };

        }

        public async Task<UpdateCheckerMobileModel> CheckForUpdateAsync(string currentVersionNumber)
        {

            var sql = @"
                
                SELECT TOP(1) APP.[Version], APP.PublishedDate, APP.IsDiscontinued, APP.PackageName, App.PackageUrl FROM mobile.Applications APP WHERE APP.[Version] = @currentVersion

                SELECT TOP(1) APP.[Version], APP.PublishedDate, APP.IsDiscontinued, APP.PackageName, App.PackageUrl FROM mobile.Applications APP ORDER BY APP.PublishedDate DESC;

                 ";

            Func<DbDataReader, List<MobileApplicationViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<MobileApplicationViewModel>();
                MobileApplicationViewModel newRow;

                while (reader.Read())
                {
                    newRow = new MobileApplicationViewModel
                    {
                        Version = reader[0].ToString(),
                        PublishedDate = DateTime.Parse(reader[1].ToString()),
                        IsDiscontinued = bool.Parse(reader[2].ToString()),
                        PackageName = reader[3].ToString(),
                        PackageUrl = reader[4].ToString(),

                    };
                    mappedData.Add(newRow);
                }

                if (!mappedData.Any())
                {
                    mappedData.Add(null);
                }

                reader.NextResult();

                while (reader.Read())
                {
                    newRow = new MobileApplicationViewModel
                    {
                        Version = reader[0].ToString(),
                        PublishedDate = DateTime.Parse(reader[1].ToString()),
                        IsDiscontinued = bool.Parse(reader[2].ToString()),
                        PackageName = reader[3].ToString(),
                        PackageUrl = reader[4].ToString(),

                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            };

            var filterParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@currentVersion",
                    Value = currentVersionNumber,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
            };

            var result = _efdataQuery.GetDataBySql(sql, mapping, filterParameters.ToArray());

            var currentVersion = result[0];
            var newVersion = result[1];

            if (currentVersion == null || currentVersion.IsDiscontinued)
            {
                return new UpdateCheckerMobileModel
                {
                    Result = UpdateCheckerResult.ForceUpdate,
                    Message = $"A new version ({newVersion.Version}) is available. Please upgrade the application to continue.",
                    NewVersion = newVersion?.Version,
                    PackageName = newVersion?.PackageName,
                    PackageUrl = newVersion?.PackageUrl
                };
            }
            else
            {
                if (newVersion.Version == currentVersion.Version)
                {
                    return new UpdateCheckerMobileModel
                    {
                        Result = UpdateCheckerResult.UpToDate,
                        Message = $"A new version ({currentVersion.Version}) is up to date."
                    };
                }
                else
                {
                    return new UpdateCheckerMobileModel
                    {
                        Result = UpdateCheckerResult.NewUpdate,
                        Message = $"A new version ({newVersion.Version}) is available. Do you want to upgrade the application?",
                        NewVersion = newVersion?.Version,
                        PackageName = newVersion?.PackageName,
                        PackageUrl = newVersion?.PackageUrl
                    };
                }
            }

        }
    }
}
