using AutoMapper.QueryableExtensions;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consolidation.Services.Interfaces;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consolidation.Services
{
    public class ConsolidationListService : ServiceBase<ConsolidationModel, ConsolidationListViewModel>, IConsolidationListService
    {
        private readonly IDataQuery _dataQuery;
        public ConsolidationListService(
            IUnitOfWorkProvider unitOfWorkProvider,
            IDataQuery dataQuery)
            : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0)
        {
            IQueryable<ConsolidationQueryModel> query;
            string sql;

            if (!isInternal)
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
                            csol.Id,
                            csol.ContainerNo,
                            csol.ContainerId,
                            csol.ConsolidationNo,
                            CASE	WHEN csol.EquipmentType = '20DG' THEN '20'' Dangerous Container'
								    WHEN csol.EquipmentType = '20FR' THEN '20'' Flat Rack'
								    WHEN csol.EquipmentType = '20GH' THEN '20'' GOH Container'
								    WHEN csol.EquipmentType = '20GP' THEN '20'' Container'
								    WHEN csol.EquipmentType = '20HC' THEN '20'' High Cube'
								    WHEN csol.EquipmentType = '20HT' THEN '20'' HT Container'
								    WHEN csol.EquipmentType = '20HW' THEN '20'' High Wide'
								    WHEN csol.EquipmentType = '20NOR' THEN '20'' Reefer Dry'
								    WHEN csol.EquipmentType = '20OS' THEN '20'' Both Full Side Door Opening Container'
								    WHEN csol.EquipmentType = '20OT' THEN '20'' Open Top Container'
								    WHEN csol.EquipmentType = '40GP' THEN '40'' Container'
								    WHEN csol.EquipmentType = '40HC' THEN '40'' High Cube'
								    WHEN csol.EquipmentType = '40HG' THEN '40'' HC GOH Container'
								    WHEN csol.EquipmentType = '40HNOR' THEN '40'' HC Reefer Dry Container'
								    WHEN csol.EquipmentType = '40HO' THEN '40'' HC Open Top Container'
								    WHEN csol.EquipmentType = '40HQDG' THEN '40'' HQ DG Container'
								    WHEN csol.EquipmentType = '40HR' THEN '40'' HC Reefer Container'
								    WHEN csol.EquipmentType = '40HW' THEN '40'' High Cube Pallet Wide'
								    WHEN csol.EquipmentType = '40NOR' THEN '40'' Reefer Dry'
								    WHEN csol.EquipmentType = '40OT' THEN '40'' Open Top Container'
								    WHEN csol.EquipmentType = '20RF' THEN '20'' Reefer'
								    WHEN csol.EquipmentType = '20TK' THEN '20'' Tank Container'
								    WHEN csol.EquipmentType = '20VH' THEN '20'' Ventilated Container'
								    WHEN csol.EquipmentType = '40DG' THEN '40'' Dangerous Conatiner'
								    WHEN csol.EquipmentType = '40FQ' THEN '40'' High Cube Flat Rack'
								    WHEN csol.EquipmentType = '40FR' THEN '40'' Flat Rack'
								    WHEN csol.EquipmentType = '40GH' THEN '40'' GOH Container'
								    WHEN csol.EquipmentType = '40PS' THEN '40'' Plus'
								    WHEN csol.EquipmentType = '40RF' THEN '40'' Reefer'
								    WHEN csol.EquipmentType = '40TK' THEN '40'' Tank'
								    WHEN csol.EquipmentType = '45GO' THEN '45'' GOH'
								    WHEN csol.EquipmentType = '45HC' THEN '45'' High Cube'
								    WHEN csol.EquipmentType = '45HG' THEN '45'' HC GOH Container'
								    WHEN csol.EquipmentType = '45HT' THEN '45'' Hard Top Container'
								    WHEN csol.EquipmentType = '45HW' THEN '45'' HC Pallet Wide'
								    WHEN csol.EquipmentType = '45RF' THEN '45'' Reefer Container'
								    WHEN csol.EquipmentType = '48HC' THEN '48'' HC Container'
								    WHEN csol.EquipmentType = 'Air' THEN 'Air'
								    WHEN csol.EquipmentType = 'LCL' THEN 'LCL'
								    WHEN csol.EquipmentType = 'Truck' THEN 'Truck'
								    ELSE '' END AS [EquipmentType],
                            csol.OriginCFS,
                            csol.CFSCutoffDate,
                            CAST(csol.CFSCutoffDate AS DATE) AS CFSCutoffDateOnly,
                            CASE	WHEN csol.Stage = 10 THEN 'New'
								    WHEN csol.Stage = 20 THEN 'Confirmed'
								    ELSE '' END AS [Stage],
                            csol.LoadingDate,
	                        CAST(csol.LoadingDate AS DATE) AS LoadingDateOnly,
	                        t.ShipmentInfo,
	                        STUFF(
			                        (SELECT DISTINCT ';' + t1
                                        FROM (
				                        SELECT PARSENAME( REPLACE(Value,'~','.'), 2) t1 FROM dbo.fn_SplitStringToTable(t.ShipmentInfo, ';')
			                            ) a
                                        FOR Xml Path('')),
                                        1,
                                        1,
                                        ''
	                        ) AS ShipmentNo
                        FROM Consolidations csol WITH (NOLOCK)
                        OUTER APPLY (
	                        SELECT STUFF(
			                        (SELECT DISTINCT ';' + s.ShipmentNo + '~' + CAST(sl.ShipmentId AS VARCHAR(100))
                                        FROM ShipmentLoads sl WITH (NOLOCK)
                                        INNER JOIN Shipments s WITH (NOLOCK) ON sl.ShipmentId = s.Id
                                        WHERE sl.ConsolidationId = csol.id
                                        FOR Xml Path('')),
                                        1,
                                        1,
                                        ''
	                        ) AS ShipmentInfo
                        ) t
                        WHERE EXISTS (
	                        SELECT 1
	                        FROM ShipmentLoads sl WITH (NOLOCK)
                            INNER JOIN ShipmentContacts sc WITH (NOLOCK) ON sl.ShipmentId = sc.ShipmentId 
                                                                                AND sl.ConsolidationId = csol.Id 
                                                                                AND sc.OrganizationId IN (SELECT value FROM [dbo].[fn_SplitStringToTable]({0},','))
                        )
                    ";
                query = _dataQuery.GetQueryable<ConsolidationQueryModel>(sql, organizationIds);
            }
            else
            {
                sql =
                @"
                    SELECT
                        csol.Id,
                        csol.ContainerNo,
                        csol.ContainerId,
                        csol.ConsolidationNo,
                        CASE	WHEN csol.EquipmentType = '20DG' THEN '20'' Dangerous Container'
								WHEN csol.EquipmentType = '20FR' THEN '20'' Flat Rack'
								WHEN csol.EquipmentType = '20GH' THEN '20'' GOH Container'
								WHEN csol.EquipmentType = '20GP' THEN '20'' Container'
								WHEN csol.EquipmentType = '20HC' THEN '20'' High Cube'
								WHEN csol.EquipmentType = '20HT' THEN '20'' HT Container'
								WHEN csol.EquipmentType = '20HW' THEN '20'' High Wide'
								WHEN csol.EquipmentType = '20NOR' THEN '20'' Reefer Dry'
								WHEN csol.EquipmentType = '20OS' THEN '20'' Both Full Side Door Opening Container'
								WHEN csol.EquipmentType = '20OT' THEN '20'' Open Top Container'
								WHEN csol.EquipmentType = '40GP' THEN '40'' Container'
								WHEN csol.EquipmentType = '40HC' THEN '40'' High Cube'
								WHEN csol.EquipmentType = '40HG' THEN '40'' HC GOH Container'
								WHEN csol.EquipmentType = '40HNOR' THEN '40'' HC Reefer Dry Container'
								WHEN csol.EquipmentType = '40HO' THEN '40'' HC Open Top Container'
								WHEN csol.EquipmentType = '40HQDG' THEN '40'' HQ DG Container'
								WHEN csol.EquipmentType = '40HR' THEN '40'' HC Reefer Container'
								WHEN csol.EquipmentType = '40HW' THEN '40'' High Cube Pallet Wide'
								WHEN csol.EquipmentType = '40NOR' THEN '40'' Reefer Dry'
								WHEN csol.EquipmentType = '40OT' THEN '40'' Open Top Container'
								WHEN csol.EquipmentType = '20RF' THEN '20'' Reefer'
								WHEN csol.EquipmentType = '20TK' THEN '20'' Tank Container'
								WHEN csol.EquipmentType = '20VH' THEN '20'' Ventilated Container'
								WHEN csol.EquipmentType = '40DG' THEN '40'' Dangerous Conatiner'
								WHEN csol.EquipmentType = '40FQ' THEN '40'' High Cube Flat Rack'
								WHEN csol.EquipmentType = '40FR' THEN '40'' Flat Rack'
								WHEN csol.EquipmentType = '40GH' THEN '40'' GOH Container'
								WHEN csol.EquipmentType = '40PS' THEN '40'' Plus'
								WHEN csol.EquipmentType = '40RF' THEN '40'' Reefer'
								WHEN csol.EquipmentType = '40TK' THEN '40'' Tank'
								WHEN csol.EquipmentType = '45GO' THEN '45'' GOH'
								WHEN csol.EquipmentType = '45HC' THEN '45'' High Cube'
								WHEN csol.EquipmentType = '45HG' THEN '45'' HC GOH Container'
								WHEN csol.EquipmentType = '45HT' THEN '45'' Hard Top Container'
								WHEN csol.EquipmentType = '45HW' THEN '45'' HC Pallet Wide'
								WHEN csol.EquipmentType = '45RF' THEN '45'' Reefer Container'
								WHEN csol.EquipmentType = '48HC' THEN '48'' HC Container'
								WHEN csol.EquipmentType = 'Air' THEN 'Air'
								WHEN csol.EquipmentType = 'LCL' THEN 'LCL'
								WHEN csol.EquipmentType = 'Truck' THEN 'Truck'
								ELSE '' END AS [EquipmentType],
                        csol.OriginCFS,
                        csol.CFSCutoffDate,
                        CAST(csol.CFSCutoffDate AS DATE) AS CFSCutoffDateOnly,
                        CASE	WHEN csol.Stage = 10 THEN 'New'
								WHEN csol.Stage = 20 THEN 'Confirmed'
								ELSE '' END AS [Stage],
                        csol.LoadingDate,
	                    CAST(csol.LoadingDate AS DATE) AS LoadingDateOnly,
	                    t.ShipmentInfo,
	                    STUFF(
			                    (SELECT DISTINCT ';' + t1
                                    FROM (
				                    SELECT PARSENAME( REPLACE(Value,'~','.'), 2) t1 FROM dbo.fn_SplitStringToTable(t.ShipmentInfo, ';')
			                        ) a
                                    FOR Xml Path('')),
                                    1,
                                    1,
                                    ''
	                    ) AS ShipmentNo
                    FROM Consolidations csol WITH (NOLOCK)
                    OUTER APPLY (
	                    SELECT STUFF(
			                    (SELECT DISTINCT ';' + s.ShipmentNo + '~' + CAST(sl.ShipmentId AS VARCHAR(100))
                                    FROM ShipmentLoads sl WITH (NOLOCK)
                                    INNER JOIN Shipments s WITH (NOLOCK) ON sl.ShipmentId = s.Id
                                    WHERE sl.ConsolidationId = csol.id
                                    FOR Xml Path('')),
                                    1,
                                    1,
                                    ''
	                    ) AS ShipmentInfo
                    ) t
                ";
                query = _dataQuery.GetQueryable<ConsolidationQueryModel>(sql);
            }

            /**Custom filtering data*/
            if (request.Filters != null)
            {
                foreach (var filter in request.Filters)
                {
                    var descriptor = filter as FilterDescriptor;
                    FilterOperator[] appliedOperators = { FilterOperator.IsEqualTo, FilterOperator.IsNotEqualTo, FilterOperator.IsLessThanOrEqualTo, FilterOperator.IsGreaterThan };

                    if (descriptor != null && Regex.IsMatch(descriptor.Member, @"\.*Date$") && appliedOperators.Contains(descriptor.Operator))
                    {
                        descriptor.Member += "Only";
                    }
                    else if (filter is CompositeFilterDescriptor)
                    {
                        ModifyFilters(((CompositeFilterDescriptor)filter).FilterDescriptors);
                    }
                }
            }

            return await query.ProjectTo<ConsolidationListViewModel>(Mapper.ConfigurationProvider)
                .ToDataSourceResultAsync(request);
        }

    }
}
