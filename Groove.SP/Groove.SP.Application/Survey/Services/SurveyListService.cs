using Groove.SP.Application.Common;
using Groove.SP.Application.Survey.Services.Interfaces;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Survey.Services
{
    public class SurveyListService : ISurveyListService
    {
        private readonly IDataQuery _dataQuery;
		private readonly AppConfig _appConfig;

		public SurveyListService(IDataQuery dataQuery,
			IOptions<AppConfig> appConfig)
        {
            _dataQuery = dataQuery;
			_appConfig = appConfig.Value;
		}

        public async Task<DataSourceResult> GetListAsync(DataSourceRequest request, IdentityInfo currentUser)
        {
            IQueryable<SurveyQueryModel> query;

            string sql = @"
                           SELECT 
							   [Id]
							  ,S.[Name]
							  ,CASE 
									WHEN UserRole = 4  AND ParticipantType = 10 THEN 'Agent'
									WHEN UserRole = 10 AND ParticipantType = 10 THEN 'Cruise Agent'
									WHEN UserRole = 11 AND ParticipantType = 10 THEN 'Cruise Principal'
									WHEN UserRole = 2  AND ParticipantType = 10 THEN 'CSR'
									WHEN UserRole = 8  AND ParticipantType = 10 THEN 'Principal'
									WHEN UserRole = 9  AND ParticipantType = 10 THEN 'Shipper'
									WHEN UserRole = 1  AND ParticipantType = 10 THEN 'System Admin'
									WHEN UserRole = 12 AND ParticipantType = 10 THEN 'Warehouse'
									WHEN ParticipantType = 20 THEN OrgName
									WHEN ParticipantType = 30 THEN SpecifiedUser
							   END Participants
							  ,[CreatedBy]
							  ,CONVERT(DATE,[CreatedDate]) AS CreatedDate
							  ,CONVERT(DATE,[PublishedDate]) AS PublishedDate
							  ,Status
							  ,CASE 
									WHEN [Status] = 10 THEN 'label.draft'
									WHEN [Status] = 20 THEN 'label.published'
									WHEN [Status] = 30 THEN 'label.closed'
							   END StatusName
						FROM Surveys S
						OUTER APPLY 
						(
							SELECT STUFF(
								(SELECT ', ' + Name 
							FROM Organizations O
							WHERE O.Id IN (SELECT value FROM [dbo].[fn_SplitStringToTable](SpecifiedOrganization,';'))
							FOR XMl PATH('')),1,2, '' 
							) AS OrgName
						) O
                           ";

            query = _dataQuery.GetQueryable<SurveyQueryModel>(sql);

            var surveys = await query.ToDataSourceResultAsync(request);

            return surveys;
        }
    }
}
