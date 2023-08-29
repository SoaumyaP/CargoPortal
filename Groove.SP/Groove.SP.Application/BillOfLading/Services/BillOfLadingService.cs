using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.BillOfLading.Services.Interfaces;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.BillOfLadingContact.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.QuickTrack;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Groove.SP.Application.BillOfLading.Services
{
    public class BillOfLadingService : ServiceBase<BillOfLadingModel, BillOfLadingViewModel>, IBillOfLadingService
    {
        private readonly IActivityService _activityService;
        private readonly IDataQuery _dataQuery;
        private readonly IItineraryRepository _itineraryRepository;

        public BillOfLadingService(IUnitOfWorkProvider unitOfWorkProvider,
            IActivityService activityService,
            IDataQuery dataQuery,
            IItineraryRepository itineraryRepository
            )
            : base(unitOfWorkProvider)
        {
            _activityService = activityService;
            _dataQuery = dataQuery;
            _itineraryRepository = itineraryRepository;
        }

        protected override Func<IQueryable<BillOfLadingModel>, IQueryable<BillOfLadingModel>> FullIncludeProperties
        {
            get
            {
                return x => x.Include(m => m.Contacts);
            }
        }

        public async Task<BillOfLadingViewModel> GetBOLAsync(string billOfLadingNoOrId, bool isInternal, string affiliates)
        {
            List<long> listOfAffiliates = new List<long>();

            var query = Repository.GetListQueryable(q => q.Include(b => b.Contacts).Include(x => x.BillOfLadingShipmentLoads).ThenInclude(x => x.MasterBillOfLading));

            query = long.TryParse(billOfLadingNoOrId, out long billOfLadingId)
                ? query.Where(b => b.Id == billOfLadingId)
                : query.Where(b => b.BillOfLadingNo == billOfLadingNoOrId);

            if (!isInternal)
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                query = query.Where(b => b.Contacts.Any(c => listOfAffiliates.Contains(c.OrganizationId)));
            }

            var model = await query.FirstOrDefaultAsync();

            var result = Mapper.Map<BillOfLadingViewModel>(model);
            if (result != null)
            {
                // Fulfill Master bill of lading number
                var masterBillOfLading = model.BillOfLadingShipmentLoads?.FirstOrDefault()?.MasterBillOfLading;
                if (masterBillOfLading != null)
                {
                    result.MasterBillOfLadingId = masterBillOfLading.Id;
                    if (masterBillOfLading.ModeOfTransport != null && masterBillOfLading.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.MasterBillOfLadingNo = masterBillOfLading.MasterBillOfLadingNo.Insert(3, "-");
                    }
                    else
                    {
                        result.MasterBillOfLadingNo = masterBillOfLading.MasterBillOfLadingNo;
                    }
                }
            }
            return result;
        }

        public async Task<QuickTrackBillOfLadingViewModel> GetQuickTrackAsync(string billOfLadingNo)
        {
            var model = await Repository.GetAsync(s => s.BillOfLadingNo == billOfLadingNo, null,
                                                    x => x.Include(m => m.ShipmentBillOfLadings)
                                                        .ThenInclude(sb => sb.Shipment));

            if (model == null)
                return null;

            var result = Mapper.Map<QuickTrackBillOfLadingViewModel>(model);

            var shipmentActivities = model.ShipmentBillOfLadings
                ?.Select(async sb => await _activityService.GetActivities(EntityType.Shipment, sb.Shipment.Id))
                .SelectMany(task => task.Result).OrderBy(x => x.ActivityCode).ThenByDescending(x => x.ActivityDate);

            result.Activities = Mapper.Map<ICollection<QuickTrackActivityViewModel>>(shipmentActivities);

            return result;
        }

        public async Task<IEnumerable<BillOfLadingViewModel>> GetBOLsByMasterBOLAsync(long masterBillOfLadingId, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();

            var query = Repository.GetListQueryable(q => q.Include(b => b.Contacts)).Where(b => b.BillOfLadingShipmentLoads.Any(bsl => bsl.MasterBillOfLadingId == masterBillOfLadingId));

            if (!isInternal)
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                query = query.Where(b => b.Contacts.Any(y => listOfAffiliates.Contains(y.OrganizationId)));
            }

            var models = await query.ToListAsync();

            return Mapper.Map<IEnumerable<BillOfLadingViewModel>>(models);
        }

        public async Task<bool> AssignMasterBLToHouseBLAsync(long houseBOLId, long masterBOLId, string userName)
        {
            //   [dbo].[spu_AssignMasterBOLToHouseBOL]
            //   @houseBLId BIGINT,
            //   @masterBLId BIGINT,	
            //   @updatedBy NVARCHAR(512)

            var sql = @"spu_AssignMasterBOLToHouseBOL 
                        @p0,
	                    @p1,
	                   	@p2";
            var parameters = new object[]
            {
                houseBOLId,
                masterBOLId,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            // Update related Shipment.CarrierContractNo

            // [dbo].[spu_UpdateShipmentCarrierContractNo]
            // @masterBOLId BIGINT = NULL,
            // @shipmentId BIGINT = NULL,
            // @updatedBy NVARCHAR(512)	

            sql = @"spu_UpdateShipmentCarrierContractNo 
                        @p0,
	                    @p1,
	                   	@p2";
            parameters = new object[]
            {
                masterBOLId,
                null,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            return true;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long roleId, long organizationId, string affiliates = "")
        {
            IQueryable<BillOfLadingQueryModel> query;
            string sql;
            if (isInternal)
            {
                sql =
                   @"
                       SELECT	BOL.id,
                             BOL.BillOfLadingNo AS HouseBLNo,
	                         CASE WHEN A.Id IS NULL THEN 0 ELSE A.Id END AttachmentId,
	                         CASE WHEN A.FileName IS NULL THEN '' ELSE A.FileName END AttachmentFileName,
                             CASE WHEN ShipmentNos.[Value] IS NULL THEN '' ELSE STUFF(ShipmentNos.[Value] , 1, 1, '') END ShipmentNo,
                             STUFF(ShipmentInfos.[Value] , 1, 1, '') ShipmentInfo,
                             CASE WHEN MB.ModeOfTransport = 'Air' THEN STUFF(MB.MasterBLNo, 4, 0, '-')
								ELSE MB.MasterBLNo
							    END AS MasterBLNo,
                             CASE WHEN MB.MasterBLId IS NULL THEN 0 ELSE MB.MasterBLId END AS MasterBLId,
                             IIF(BOL.IssueDate <= '1900-01-01', NULL, BOL.IssueDate) AS IssueDate,
                             BOL.ShipFrom,
                             BOL.ShipTo,
                             CASE WHEN BC.CompanyName IS NULL THEN '' ELSE BC.CompanyName END AS Customer
                         FROM BillOfLadings BOL (NOLOCK)
                         OUTER APPLY 
                         (
                             SELECT TOP(1) BC.CompanyName
                             FROM BillOfLadingContacts BC (NOLOCK)
                             WHERE 
                                 BC.BillOfLadingId = BOL.Id
                                 AND BC.OrganizationRole = 'Principal'
                         )BC
                         OUTER APPLY 
                         (
                             SELECT TOP(1) MB.Id AS MasterBLId, MB.MasterBillOfLadingNo AS MasterBLNo,MB.ModeOfTransport, MB.CarrierContractNo
                             FROM MasterBills MB WITH (NOLOCK)
                             INNER JOIN BillOfLadingShipmentLoads BOLS WITH (NOLOCK) ON BOLS.MasterBillOfLadingId = MB.Id
                             WHERE BOLS.BillOfLadingId = BOL.Id
                         )MB
                         OUTER APPLY
                         (
	                        SELECT TOP 1 
	                            A.Id,
	                            A.FileName,
	                            A.AttachmentType,
	                            A.UploadedDate
	                        FROM Attachments A (NOLOCK)
	                        WHERE
                                EXISTS (SELECT 1 FROM GlobalIdAttachments GA WHERE GA.AttachemntId = A.Id AND GlobalId = CONCAT('BOL_',BOL.Id))  
                                AND ({0} = 0 
		                            OR EXISTS (
			                            SELECT CheckContractHolder 
			                            FROM AttachmentTypePermissions 
			                           
			                            WHERE 
				                            RoleId = {0} 
				                            AND AttachmentType = 'House BL' 
				                            AND ({0} <> 8 OR CheckContractHolder = 0)
		                        )) 
	                        ORDER BY A.UploadedDate DESC
                         )A
                         OUTER APPLY
                         (
                             SELECT
                                 (	SELECT ';' + S.ShipmentNo
                                     FROM Shipments S (NOLOCK)
                                     WHERE EXISTS (
                                            SELECT 1
                                            FROM ShipmentBillOfLadings SBOL (NOLOCK)
                                            WHERE SBOL.BillOfLadingId = BOL.Id AND S.Id = SBOL.ShipmentId
                                            )
                                     FOR Xml Path('')
                                 ) AS [Value]
                         ) ShipmentNos
                         OUTER APPLY
                         (
                             SELECT
                                 (	SELECT ';' + CONCAT(S.ShipmentNo ,'~', S.Id)
                                     FROM Shipments S (NOLOCK)
                                     WHERE EXISTS (
                                            SELECT 1
                                            FROM ShipmentBillOfLadings SBOL (NOLOCK)
                                            WHERE SBOL.BillOfLadingId = BOL.Id AND S.Id = SBOL.ShipmentId
                                            )
                                     FOR Xml Path('')
                                 ) AS [Value]

                         ) ShipmentInfos";
                query = _dataQuery.GetQueryable<BillOfLadingQueryModel>(sql, roleId, organizationId);
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
                    SELECT	BOL.id,
		                    BOL.BillOfLadingNo AS HouseBLNo,
                            CASE WHEN A.Id IS NULL THEN 0 ELSE A.Id END AttachmentId,
	                        CASE WHEN A.FileName IS NULL THEN '' ELSE A.FileName END AttachmentFileName,
		                CASE WHEN ShipmentNos.[Value] IS NULL THEN '' ELSE STUFF(ShipmentNos.[Value] , 1, 1, '') END ShipmentNo,
		                STUFF(ShipmentInfos.[Value] , 1, 1, '') ShipmentInfo,
		                CASE WHEN MB.ModeOfTransport = 'Air' THEN STUFF(MB.MasterBLNo, 4, 0, '-')
								ELSE MB.MasterBLNo
						END AS MasterBLNo,
		                CASE WHEN MB.MasterBLId IS NULL THEN 0 ELSE MB.MasterBLId END AS MasterBLId,
		                IIF(BOL.IssueDate <= '1900-01-01', NULL, BOL.IssueDate) AS IssueDate,
		                BOL.ShipFrom,
		                BOL.ShipTo,
		                CASE WHEN BC.CompanyName IS NULL THEN '' ELSE BC.CompanyName END AS Customer
	                FROM BillOfLadings BOL (NOLOCK)
	                OUTER APPLY 
	                (
		                SELECT TOP(1) BC.CompanyName
		                FROM BillOfLadingContacts BC (NOLOCK)
		                WHERE 
			                BC.BillOfLadingId = BOL.Id
			                AND BC.OrganizationRole = 'Principal'
	                )BC
	                OUTER APPLY 
	                (
		                SELECT TOP(1) MB.Id AS MasterBLId, MB.MasterBillOfLadingNo AS MasterBLNo, MB.ModeOfTransport, MB.CarrierContractNo
		                FROM MasterBills MB WITH (NOLOCK)
		                INNER JOIN BillOfLadingShipmentLoads BOLS WITH (NOLOCK) ON BOLS.MasterBillOfLadingId = MB.Id
		                WHERE BOLS.BillOfLadingId = BOL.Id
	                )MB
                    OUTER APPLY
                         (
	                        SELECT TOP 1 
	                        A.Id,
	                        A.FileName,
	                        A.AttachmentType,
	                        A.UploadedDate
	                        FROM Attachments A (NOLOCK)
	                        WHERE
                                EXISTS (SELECT 1 FROM GlobalIdAttachments GA WHERE GA.AttachemntId = A.Id AND GlobalId = CONCAT('BOL_',BOL.Id))  
                                AND ({0} = 0 
		                            OR EXISTS (
			                            SELECT CheckContractHolder 
			                            FROM AttachmentTypePermissions 
			                          
			                            WHERE 
				                            RoleId = {0} 
				                            AND AttachmentType = 'House BL' 
				                            AND ({0} <> 8 OR CheckContractHolder = 0)
		                        )) 
	                        ORDER BY A.UploadedDate DESC
                         )A
	                OUTER APPLY
	                (
		                SELECT
			                (	SELECT ';' + S.ShipmentNo
				                FROM Shipments S (NOLOCK)
				                WHERE EXISTS (
						                SELECT 1
						                FROM ShipmentBillOfLadings SBOL (NOLOCK)
						                WHERE SBOL.BillOfLadingId = BOL.Id AND S.Id = SBOL.ShipmentId
						                )
				                FOR Xml Path('')
			                ) AS [Value]
	                ) ShipmentNos
	                OUTER APPLY
	                (
		                SELECT
			                (	SELECT ';' + CONCAT(S.ShipmentNo ,'~', S.Id)
				                FROM Shipments S (NOLOCK)
				                WHERE EXISTS (
						                SELECT TOP(1) 1
						                FROM ShipmentBillOfLadings SBOL (NOLOCK)
						                WHERE SBOL.BillOfLadingId = BOL.Id AND S.Id = SBOL.ShipmentId
						                )
				                FOR Xml Path('')
			                ) AS [Value]
		
	                ) ShipmentInfos

	                WHERE EXISTS 
				                (
					                SELECT 1
					                FROM BillOfLadingContacts BOC (NOLOCK)
					                WHERE 
						                BOL.Id = BOC.BillOfLadingId
						                AND BOC.OrganizationId IN (SELECT value FROM [dbo].[fn_SplitStringToTable]({2},','))
				                ) ";
                query = _dataQuery.GetQueryable<BillOfLadingQueryModel>(sql,roleId,organizationId,organizationIds);
            }

            var data = await query.ToDataSourceResultAsync(request);
            return data;
        }

        public override async Task<BillOfLadingViewModel> UpdateAsync(BillOfLadingViewModel viewModel, params object[] keys)
        {
            viewModel.ValidateAndThrow(true);

            BillOfLadingModel model = await this.Repository.FindAsync(keys);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", keys)} not found!");
            }

            var itineraries = await _itineraryRepository.Query(s => s.BillOfLadingItineraries.Any(x => x.BillOfLadingId == model.Id) && s.ModeOfTransport == ModeOfTransport.Sea && s.ScheduleId != null,
                                                    n => n.OrderBy(a => a.Sequence), x => x.Include(y => y.FreightScheduler)).ToListAsync();

            // The API should skip the updates of the dates if the linked Freight Scheduler already LOCKED
            if (itineraries.Count() > 0 && !itineraries.First().FreightScheduler.IsAllowExternalUpdate)
            {
                viewModel.ShipFromETDDate = model.ShipFromETDDate;
            }
            if (itineraries.Count() > 0 && !itineraries.Last().FreightScheduler.IsAllowExternalUpdate)
            {
                viewModel.ShipToETADate = model.ShipToETADate;
            }

            Mapper.Map(viewModel, model);

            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<BillOfLadingViewModel>(model);
            return viewModel;
        }

        public async Task<IEnumerable<HouseBLQueryModel>> SearchHouseBLAsync(string houseBLNo, string modeOfTransport, long? executionAgentId, bool isInternal, string affiliates)
        {
            IQueryable<HouseBLQueryModel> query;
            string sql = "";
            if (isInternal)
            {
                sql = @"SELECT Id,
		                    ExecutionAgentId,
		                    BillOfLadingNo,
                            JobNumber ,
                            IssueDate ,
                            ModeOfTransport ,
                            BillOfLadingType ,
                            ShipFromETDDate ,
                            ShipToETADate ,
                            ShipFrom ,
                            ShipTo ,
                            Movement ,
                            Incoterm,
                            TotalGrossWeight,	
                            TotalNetWeight,
                            TotalPackage,	
                            TotalVolume,
		                    OA.CompanyName AS OriginAgent,
		                    DA.CompanyName AS DestinationAgent,
		                    PRIN.CompanyName AS Customer

	                    FROM BillOfLadings BOL WITH (NOLOCK)

	                    OUTER APPLY
	                    (
		                    SELECT BOLC.CompanyName
		                    FROM BillOfLadingContacts BOLC WITH (NOLOCK)
		                    WHERE BOLC.OrganizationRole = 'Origin Agent' AND BOLC.BillOfLadingId = BOL.Id
	                    ) OA

	                    OUTER APPLY
	                    (
		                    SELECT BOLC.CompanyName
		                    FROM BillOfLadingContacts BOLC WITH (NOLOCK)
		                    WHERE BOLC.OrganizationRole = 'Destination Agent' AND BOLC.BillOfLadingId = BOL.Id
	                    ) DA

	                    OUTER APPLY
	                    (
		                    SELECT BOLC.CompanyName
		                    FROM BillOfLadingContacts BOLC WITH (NOLOCK)
		                    WHERE BOLC.OrganizationRole = 'Principal' AND BOLC.BillOfLadingId = BOL.Id
	                    ) PRIN
	                    WHERE 
		                    BOL.ModeOfTransport = {0}
                            AND BOL.ExecutionAgentId = {1}
                            AND BOL.BillOfLadingNo LIKE CONCAT({2},'%')";
                query = _dataQuery.GetQueryable<HouseBLQueryModel>(sql, modeOfTransport, executionAgentId, houseBLNo);
            }
            else
            {
                string organizationIds = string.Empty;
                if (!string.IsNullOrEmpty(affiliates))
                {
                    var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                    organizationIds = string.Join(",", listOfAffiliates);
                }
                sql = @"SELECT Id,
		                    ExecutionAgentId,
		                    BillOfLadingNo,
                            JobNumber ,
                            IssueDate ,
                            ModeOfTransport ,
                            BillOfLadingType ,
                            ShipFromETDDate ,
                            ShipToETADate ,
                            ShipFrom ,
                            ShipTo ,
                            Movement ,
                            Incoterm,
                            TotalGrossWeight,	
                            TotalNetWeight,
                            TotalPackage,	
                            TotalVolume,
		                    OA.CompanyName AS OriginAgent,
		                    DA.CompanyName AS DestinationAgent,
		                    PRIN.CompanyName AS Customer

	                    FROM BillOfLadings BOL WITH (NOLOCK)

	                    OUTER APPLY
	                    (
		                    SELECT BOLC.CompanyName
		                    FROM BillOfLadingContacts BOLC WITH (NOLOCK)
		                    WHERE BOLC.OrganizationRole = 'Origin Agent' AND BOLC.BillOfLadingId = BOL.Id
	                    ) OA

	                    OUTER APPLY
	                    (
		                    SELECT BOLC.CompanyName
		                    FROM BillOfLadingContacts BOLC WITH (NOLOCK)
		                    WHERE BOLC.OrganizationRole = 'Destination Agent' AND BOLC.BillOfLadingId = BOL.Id
	                    ) DA

	                    OUTER APPLY
	                    (
		                    SELECT BOLC.CompanyName
		                    FROM BillOfLadingContacts BOLC WITH (NOLOCK)
		                    WHERE BOLC.OrganizationRole = 'Principal' AND BOLC.BillOfLadingId = BOL.Id
	                    ) PRIN
	                    WHERE 
		                    BOL.ModeOfTransport = {0}
                            AND BOL.ExecutionAgentId = {1}
                            AND BOL.BillOfLadingNo LIKE CONCAT({2},'%')
		                    AND EXISTS 
			                    (
				                    SELECT 1
				                    FROM BillOfLadingContacts BOLC WITH (NOLOCK)
				                    WHERE 
					                    BOLC.BillOfLadingId = BOL.Id
					                    AND BOLC.OrganizationId IN (SELECT value FROM [dbo].[fn_SplitStringToTable]({3},','))
			                    )";
                query = _dataQuery.GetQueryable<HouseBLQueryModel>(sql, modeOfTransport, executionAgentId, houseBLNo, organizationIds);
            }

            var data = await query.ToListAsync();
            return data;
        }

        public async Task<IEnumerable<ShipmentViewModel>> SearchShipmentAsync(long houseBLId, string shipmentNo, string modeOfTransport, long executionAgentId, bool isInternal, string affiliates)
        {
            var storedProcedureName = "spu_GetShipmentSelection_HouseBL";
            List<SqlParameter> filterParameter;
            filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@billOfLadingId",
                        Value = houseBLId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@shipmentNo",
                        Value = shipmentNo,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                     new SqlParameter
                    {
                        ParameterName = "@modeOfTransport",
                        Value = modeOfTransport,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@executionAgentId",
                        Value = executionAgentId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            if (!isInternal)
            {
                affiliates = affiliates.Replace("[", "").Replace("]", "");
                filterParameter.Add(
                    new SqlParameter
                    {
                        ParameterName = "@affiliates",
                        Value = affiliates,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    });
                storedProcedureName = "spu_GetShipmentSelection_HouseBL_External";
            }

            Func<DbDataReader, IEnumerable<ShipmentViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<ShipmentViewModel>();
                while (reader.Read())
                {
                    var id = reader[0];
                    var shipmentNo1 = reader[1];
                    var totalPackage = reader[2];
                    var totalPackageUOM = reader[3];
                    var totalVolume = reader[4];
                    var totalVolumeUOM = reader[5];
                    var cargoReadyDate = reader[6];
                    var executionAgentName = reader[7];
                    var latestMilestone = reader[8];
                    var isConfirmContainer = reader[9];
                    var isConfirmConsolidation = reader[10];
                    var shipperName = reader[11];
                    var consigneeName = reader[12];

                    var newRow = new ShipmentViewModel();
                    newRow.Id = (long)id;
                    newRow.ShipmentNo = (string)shipmentNo1;
                    newRow.TotalPackage = (decimal)totalPackage;
                    newRow.TotalPackageUOM = totalPackageUOM != DBNull.Value ? (string)totalPackageUOM : null;
                    newRow.TotalVolume = (decimal)totalVolume;
                    newRow.TotalVolumeUOM = totalVolumeUOM != DBNull.Value ? (string)totalVolumeUOM : null;
                    newRow.CargoReadyDate = (DateTime)cargoReadyDate;
                    newRow.ExecutionAgentName = executionAgentName != DBNull.Value ? (string)executionAgentName : null;
                    newRow.LastestActivity = latestMilestone != DBNull.Value ? (string)latestMilestone : null;
                    newRow.ShipperName = shipperName != DBNull.Value ? (string)shipperName : null;
                    newRow.ConsigneeName = consigneeName != DBNull.Value ? (string)consigneeName : null;
                    newRow.IsConfirmContainer = (int)isConfirmContainer;
                    newRow.IsConfirmConsolidation = (int)isConfirmConsolidation;
                    mappedData.Add(newRow);
                }

                return mappedData;
            };

            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public Task<IEnumerable<BillOfLadingViewModel>> GetBillOfLadingListBySearchingNumberAsync(string searchTerm, bool isInternal, string affiliates)
        {
            var filterOnAffiliates = $@"
                AND EXISTS (SELECT 1 
                            FROM  BillOfLadingContacts HBC WITH (NOLOCK) 
                            WHERE HBC.BillOfLadingId = HB.Id 
                                AND HBC.OrganizationId IN (SELECT value 
                                                           FROM [dbo].[fn_SplitStringToTable](@affiliates, ','))
                )
            ";

            var sql = $@"
                SET NOCOUNT ON;
                IF OBJECT_ID('tempdb..#BillOfLadingTblResult') IS NOT NULL
                BEGIN
	                DROP TABLE #BillOfLadingTblResult
                END
                
                SELECT HB.Id, HB.BillOfLadingNo, HB.JobNumber, HB.IssueDate, HB.ModeOfTransport, HB.BillOfLadingType, HB.ShipFromETDDate, HB.ShipToETADate, HB.ShipFrom, HB.ShipTo, HB.Movement, HB.Incoterm
                INTO #BillOfLadingTblResult
                FROM BillOfLadings HB WITH (NOLOCK)
                OUTER APPLY 
                (
	                SELECT TOP(1) MasterBillOfLadingId FROM BillOfLadingShipmentLoads WITH (NOLOCK) WHERE HB.Id = BillOfLadingId AND MasterBillOfLadingId IS NOT NULL
                ) MBL
                WHERE HB.BillOfLadingNo LIKE CONCAT(@searchTerm, '%')
	                AND MBL.MasterBillOfLadingId IS NULL
                    AND EXISTS (SELECT 1 FROM ShipmentBillOfLadings WHERE BillOfLadingId = HB.Id)

                {(isInternal ? "" : filterOnAffiliates)}

                ORDER BY HB.BillOfLadingNo ASC

                --Return data in 2 data-sets

                SELECT * FROM #BillOfLadingTblResult

                SELECT HBC.Id, HBC.OrganizationId, HBC.OrganizationRole, HBC.CompanyName, HBC.ContactName, HBC.ContactNumber, HBC.ContactEmail, HBC.[Address], HBC.BillOfLadingId
                FROM BillOfLadingContacts HBC WITH (NOLOCK)
                WHERE HBC.BillOfLadingId IN (SELECT Id FROM #BillOfLadingTblResult)

                IF OBJECT_ID('tempdb..#BillOfLadingTblResult') IS NOT NULL
                BEGIN
	                DROP TABLE #BillOfLadingTblResult
                END
            ";

            var filterParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@searchTerm",
                    Value = searchTerm,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
            };

            if (!isInternal)
            {
                filterParameters.Add(
                                    new SqlParameter
                                    {
                                        ParameterName = "@affiliates",
                                        Value = affiliates?.Replace("[", "").Replace("]", ""),
                                        DbType = DbType.String,
                                        Direction = ParameterDirection.Input
                                    });
            }

            IEnumerable<BillOfLadingViewModel> mappingCallback(DbDataReader reader)
            {
                var mappedData = new List<BillOfLadingViewModel>();

                // Map data for bill of lading information
                while (reader.Read())
                {
                    var id = reader["Id"];
                    var billOfLadingNo = reader["BillOfLadingNo"];
                    var jobNumber = reader["JobNumber"];
                    var issueDate = reader["IssueDate"];
                    var modeOfTransport = reader["ModeOfTransport"];
                    var billOfLadingType = reader["BillOfLadingType"];
                    var shipFromETDDate = reader["ShipFromETDDate"];
                    var shipToETADate = reader["ShipToETADate"];
                    var shipFrom = reader["ShipFrom"];
                    var shipTo = reader["ShipTo"];
                    var movement = reader["Movement"];
                    var incoterm = reader["Incoterm"];


                    var newRow = new BillOfLadingViewModel
                    {
                        Id = (long)id,
                        BillOfLadingNo = (string)billOfLadingNo,
                        JobNumber = jobNumber != DBNull.Value ? jobNumber.ToString() : null,
                        IssueDate = (DateTime)issueDate,
                        ModeOfTransport = modeOfTransport != DBNull.Value ? modeOfTransport.ToString() : null,
                        BillOfLadingType = billOfLadingType != DBNull.Value ? billOfLadingType.ToString() : null,
                        ShipFromETDDate = (DateTime)shipFromETDDate,
                        ShipToETADate = (DateTime)shipToETADate,
                        ShipFrom = shipFrom != DBNull.Value ? shipFrom.ToString() : null,
                        ShipTo = shipTo != DBNull.Value ? shipTo.ToString() : null,
                        Movement = movement != DBNull.Value ? movement.ToString() : null,
                        Incoterm = incoterm != DBNull.Value ? incoterm.ToString() : null,
                        Contacts = new List<BillOfLadingContactViewModel>()

                    };
                    mappedData.Add(newRow);
                }
                reader.NextResult();

                // Map data for contacts
                while (reader.Read())
                {
                    var newRow = new BillOfLadingContactViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];

                    tmpValue = reader[1];
                    newRow.OrganizationId = (long)tmpValue;
                    tmpValue = reader[2];
                    newRow.OrganizationRole = tmpValue.ToString();
                    tmpValue = reader[3];
                    newRow.CompanyName = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[4];
                    newRow.ContactName = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[5];
                    newRow.ContactNumber = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[6];
                    newRow.ContactEmail = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[7];
                    newRow.Address = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[8];
                    newRow.BillOfLadingId = (long)tmpValue;

                    // Add record to contacts
                    mappedData.First(x => x.Id == newRow.BillOfLadingId).Contacts.Add(newRow);
                }


                return mappedData;
            }
            var result = _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
            return Task.FromResult(result);
        }

        public void UnlinkShipment(long houseBLId, long shipmentId, int isTheLastLinkedShipment, string userName)
        {
            var sql = @"spu_UnlinkShipmentFromHouseBL 
                        @p0,
	                    @p1,
                        @p2,
                        @p3";
            var parameters = new object[]
            {
                houseBLId,
                shipmentId,
                isTheLastLinkedShipment,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
        }

        public async Task<bool> CheckHouseBLAlreadyExistsAsync(string houseBLNo)
        {
            var houseBL = await Repository.GetAsNoTrackingAsync(c => c.BillOfLadingNo == houseBLNo);
            return houseBL != null ? true : false;
        }
    }
}
