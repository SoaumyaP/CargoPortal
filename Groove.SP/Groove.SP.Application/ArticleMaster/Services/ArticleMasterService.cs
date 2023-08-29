using Groove.SP.Application.ArticleMaster.Services.Interfaces;
using Groove.SP.Application.ArticleMaster.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.ArticleMaster.Services;

public class ArticleMasterService : ServiceBase<ArticleMasterModel, ArticleMasterViewModel>, IArticleMasterService
{
	private readonly IDataQuery _dataQuery;
    public ArticleMasterService(IUnitOfWorkProvider unitOfWorkProvider, IDataQuery dataQuery)
        : base(unitOfWorkProvider)
    {
		_dataQuery = dataQuery;
    }

    public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long organizationId, string affiliates = "")
    {
		IQueryable<ArticleMasterQueryModel> query;
		string sql;
		if (isInternal)
		{
			sql =
			   @"
				SELECT
					am.Id,
					org.Name AS CompanyName,
					am.ItemNo,
					am.ItemDesc,
					am.[Status],
					CASE
						WHEN am.[Status] = '1' THEN 'Active'
						WHEN am.[Status] = '0' THEN 'Inactive'
						ELSE ''
					END AS StatusName
			    FROM ArticleMaster am LEFT JOIN 
					 Organizations org ON am.CompanyCode = org.Code
               ";
			query = _dataQuery.GetQueryable<ArticleMasterQueryModel>(sql);
		}
		else
		{
			string organizationIds = string.Empty;
			if (!string.IsNullOrEmpty(affiliates))
			{
				var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
				organizationIds = string.Join(",", listOfAffiliates);
			}

			sql =
			   @"
				SELECT
					am.Id,
					org.Name AS CompanyName,
					am.ItemNo,
					am.ItemDesc,
					am.[Status],
					CASE
						WHEN am.[Status] = '1' THEN 'Active'
						WHEN am.[Status] = '0' THEN 'Inactive'
						ELSE ''
					END AS StatusName
				FROM ArticleMaster am INNER JOIN
					 Organizations org ON am.CompanyCode = org.Code
				WHERE org.Id IN (SELECT value FROM [dbo].[fn_SplitStringToTable]({0},','))
               ";
			query = _dataQuery.GetQueryable<ArticleMasterQueryModel>(sql, organizationIds);
		}

		var data = await query.ToDataSourceResultAsync(request);
		return data;
	}

	public async Task<ArticleMasterViewModel> GetByIdAsync(long id)
    {
		var model = await Repository.QueryAsNoTracking(x => x.Id == id).FirstOrDefaultAsync();

		return Mapper.Map<ArticleMasterViewModel>(model);
    }
}