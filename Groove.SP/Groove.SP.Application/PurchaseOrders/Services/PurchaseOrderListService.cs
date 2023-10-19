using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Groove.SP.Application.PurchaseOrders.Services
{
    public class PurchaseOrderListService : ServiceBase<PurchaseOrderModel, PurchaseOrderListViewModel>, IPurchaseOrderListService
    {
        private readonly IDataQuery _dataQuery;

        public PurchaseOrderListService(IUnitOfWorkProvider unitOfWorkProvider, IDataQuery dataQuery) : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
        }

        protected override IDictionary<string, string> SortMap => new Dictionary<string, string>() {
            { "statusName", "status" },
            { "stageName", "stage" }
        };

        public async Task<Core.Data.DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates, string supplierCustomerRelationships, long? delegatedOrganizationId = 0, long? id = null, string statisticKey = "", string statisticFilter = "", string statisticValue = "", string itemNo = "", bool isExport = false)
        {
            var listOfAffiliates = new List<long>();

            if (!string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            IQueryable<PurchaseOrderQueryModel> query;
            string sql;

            if (isInternal)
            {
                if (!string.IsNullOrEmpty(statisticKey))
                {
                    var dates = CommonHelper.GetDateRange(statisticFilter);
                    sql = "";

                    string categorizedPOViewSelectorSql = $@"
                        SELECT 
                               *
                        FROM vw_PurchaseOrderList_CategorizedPO_InternalUsers v
                        OUTER APPLY 
					    (
						    SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
										    WHEN Stage = 20 THEN 'label.released'
										    WHEN Stage = 30 THEN 'label.booked'
										    WHEN Stage = 40 THEN 'label.bookingConfirmed'
										    WHEN Stage = 45 THEN 'label.cargoReceived'
										    WHEN Stage = 50 THEN 'label.shipmentDispatch'
										    WHEN Stage = 60 THEN 'label.closed'
										    WHEN Stage = 70 THEN 'label.completed'
										    ELSE '' END AS [StageName]

					    ) STG
					    OUTER APPLY 
					    (
						    SELECT CASE		WHEN Status = 1 THEN 'label.active'
										    WHEN Status = 0 THEN 'label.cancel'
										    ELSE '' END AS [StatusName]

					    ) STA
                    ";

                    switch (statisticKey)
                    {
                        case "booked":
                        case "unbooked":
                            sql = $@"   SELECT * 
                                FROM vw_PurchaseOrderList_{statisticKey}_InternalUsers
                                OUTER APPLY 
							    (
								    SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
												    WHEN Stage = 20 THEN 'label.released'
												    WHEN Stage = 30 THEN 'label.booked'
												    WHEN Stage = 40 THEN 'label.bookingConfirmed'
												    WHEN Stage = 45 THEN 'label.cargoReceived'
												    WHEN Stage = 50 THEN 'label.shipmentDispatch'
												    WHEN Stage = 60 THEN 'label.closed'
												    WHEN Stage = 70 THEN 'label.completed'
												    ELSE '' END AS [StageName]

							    ) STG
							    OUTER APPLY 
							    (
								    SELECT CASE		WHEN Status = 1 THEN 'label.active'
												    WHEN Status = 0 THEN 'label.cancel'
												    ELSE '' END AS [StatusName]

							    ) STA
                                WHERE CargoReadyDate >= '{dates["FromDate"]}'  AND CargoReadyDate <= '{dates["ToDate"]}'";
                            break;
                        case "inOriginDC":
                        case "inTransit":
                        case "customsCleared":
                        case "pendingDCDelivery":
                        case "managedToDate":
                        case "dcDeliveryConfirmed":
                            sql = $@"
                                SELECT * 
                                FROM vw_PurchaseOrderList_{statisticKey}_InternalUsers
                                OUTER APPLY 
							    (
								    SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
												    WHEN Stage = 20 THEN 'label.released'
												    WHEN Stage = 30 THEN 'label.booked'
												    WHEN Stage = 40 THEN 'label.bookingConfirmed'
												    WHEN Stage = 45 THEN 'label.cargoReceived'
												    WHEN Stage = 50 THEN 'label.shipmentDispatch'
												    WHEN Stage = 60 THEN 'label.closed'
												    WHEN Stage = 70 THEN 'label.completed'
												    ELSE '' END AS [StageName]

							    ) STG
							    OUTER APPLY 
							    (
								    SELECT CASE		WHEN Status = 1 THEN 'label.active'
												    WHEN Status = 0 THEN 'label.cancel'
												    ELSE '' END AS [StatusName]

							    ) STA
                                WHERE ActivityDate >= '{dates["FromDate"]}' AND ActivityDate <= '{dates["ToDate"]}'";
                            break;
                        case "poIssuedInThisWeek":
                        case "poIssuedInLastWeek":
                            sql = $@"
                                SELECT * 
                                FROM vw_PurchaseOrderList_{statisticKey}_InternalUsers
                                OUTER APPLY 
							    (
								    SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
												    WHEN Stage = 20 THEN 'label.released'
												    WHEN Stage = 30 THEN 'label.booked'
												    WHEN Stage = 40 THEN 'label.bookingConfirmed'
												    WHEN Stage = 45 THEN 'label.cargoReceived'
												    WHEN Stage = 50 THEN 'label.shipmentDispatch'
												    WHEN Stage = 60 THEN 'label.closed'
												    WHEN Stage = 70 THEN 'label.completed'
												    ELSE '' END AS [StageName]

							    ) STG
							    OUTER APPLY 
							    (
								    SELECT CASE		WHEN Status = 1 THEN 'label.active'
												    WHEN Status = 0 THEN 'label.cancel'
												    ELSE '' END AS [StatusName]

							    ) STA
                                WHERE POIssueDate >= '{dates["FromDate"]}' AND POIssueDate <= '{dates["ToDate"]}'";
                            break;
                        case "categorizedSupplier":
                            sql = categorizedPOViewSelectorSql + $@"WHERE Supplier = '{statisticValue}'";
                            break;
                        case "categorizedConsignee":
                            sql = categorizedPOViewSelectorSql + $@"WHERE Consignee = '{statisticValue}'";
                            break;
                        case "categorizedDestination":
                            sql = categorizedPOViewSelectorSql + $@"WHERE ShipTo = '{statisticValue}'";
                            break;
                        case "categorizedStage":
                            sql = categorizedPOViewSelectorSql + $@"WHERE Stage = '{statisticValue}'";
                            break;
                        case "categorizedStatus":
                            sql = categorizedPOViewSelectorSql + $@"WHERE Status = '{statisticValue}'";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    sql =
                        @"
                            SELECT 
		                        po.Id, 
                                IIF(BCOM.IsAllowShowAdditionalInforPOListing = 1, CONCAT(po.PONumber,'~',ProductCode,'~',GridValue,'~',LineOrder), po.PONumber) as PONumber,
                                po.CustomerReferences,
		                        po.POIssueDate, 
		                        po.[Status], 
		                        po.Stage,
		                        po.CargoReadyDate,
		                        po.CreatedDate, 
		                        po.CreatedBy,
		                        SUP.Supplier,
		                        po.POType,
		                        BCOM.IsProgressCargoReadyDate AS IsProgressCargoReadyDates,
		                        BCOM.ProgressNotifyDay,
                                po.ProductionStarted,
                                po.ModeOfTransport,
                                po.ShipFrom,
								po.ShipTo,
								po.Incoterm,
								po.ExpectedDeliveryDate,
								po.ExpectedShipDate,
                                po.ContainerType,
								po.PORemark,
		                        STG.StageName,
		                        STA.StatusName,
                                0  as [check]
	                        FROM PurchaseOrders po WITH (NOLOCK)
	                        OUTER APPLY
	                        (
		                        SELECT TOP 1 pc.CompanyName AS Supplier
		                        FROM PurchaseOrderContacts pc WITH (NOLOCK) WHERE po.Id = pc.PurchaseOrderId AND pc.OrganizationRole = 'Supplier'
	                        ) SUP

	                        OUTER APPLY
	                        (
		                         SELECT 
		                                BC.IsProgressCargoReadyDate,
		                                BC.ProgressNotifyDay,
                                        BC.IsAllowShowAdditionalInforPOListing
	                                FROM BuyerCompliances BC (NOLOCK)
	                                INNER JOIN PurchaseOrderContacts POC (NOLOCK) ON POC.OrganizationId = BC.OrganizationId 
		                                AND POC.OrganizationRole = 'Principal'
                                        AND BC.Stage = 1
				                        AND POC.PurchaseOrderId = PO.Id
	                        ) BCOM
                            OUTER APPLY
                            (
							        SELECT TOP 1 ProductCode, GridValue, LineOrder
							        FROM POLineItems POL
							        WHERE POL.PurchaseOrderId = PO.Id AND BCOM.IsAllowShowAdditionalInforPOListing = 1							
    	                    ) POL
	                        OUTER APPLY 
		                    (
			                    SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
							                    WHEN Stage = 20 THEN 'label.released'
							                    WHEN Stage = 30 THEN 'label.booked'
							                    WHEN Stage = 40 THEN 'label.bookingConfirmed'
							                    WHEN Stage = 45 THEN 'label.cargoReceived'
							                    WHEN Stage = 50 THEN 'label.shipmentDispatch'
							                    WHEN Stage = 60 THEN 'label.closed'
							                    WHEN Stage = 70 THEN 'label.completed'
							                    ELSE '' END AS [StageName]

		                    ) STG
		                    OUTER APPLY 
		                    (
			                    SELECT CASE		WHEN Status = 1 THEN 'label.active'
							                    WHEN Status = 0 THEN 'label.cancel'
							                    ELSE '' END AS [StatusName]

		                    ) STA";
                }

                if (!string.IsNullOrEmpty(itemNo))
                {
                    sql += @$"
                            WHERE EXISTS (SELECT 1 FROM POLineItems POI WHERE  POI.PurchaseOrderId = PO.Id AND POI.ProductCode like '%{itemNo.Split("~")[1]}%')
                           ";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(statisticKey))
                {
                    var dates = CommonHelper.GetDateRange(statisticFilter);
                    sql = "";

                    string categorizedPOViewSelectorSql = $@"   
                            SELECT DISTINCT 
                            Id,
                            PONumber,
                            CustomerReferences,
			                CreatedBy,
			                POIssueDate,
			                [Status],
			                Stage,
			                POType,
			                CargoReadyDate,
			                CreatedDate, 
                            Supplier,
                            IsProgressCargoReadyDates,
                            ProgressNotifyDay,
                            ProductionStarted,
                            ModeOfTransport,
                            ShipFrom,
                            ShipTo,
                            Incoterm,
                            ExpectedDeliveryDate,
                            ExpectedShipDate,
                            ContainerType,
                            PORemark,
                            STG.StageName,
                            STA.StatusName
                            FROM vw_PurchaseOrderList_CategorizePO_ExternalUsers

                            OUTER APPLY 
							(
								SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
												WHEN Stage = 20 THEN 'label.released'
												WHEN Stage = 30 THEN 'label.booked'
												WHEN Stage = 40 THEN 'label.bookingConfirmed'
												WHEN Stage = 45 THEN 'label.cargoReceived'
												WHEN Stage = 50 THEN 'label.shipmentDispatch'
												WHEN Stage = 60 THEN 'label.closed'
												WHEN Stage = 70 THEN 'label.completed'
												ELSE '' END AS [StageName]

							) STG
							OUTER APPLY 
							(
								SELECT CASE		WHEN Status = 1 THEN 'label.active'
												WHEN Status = 0 THEN 'label.cancel'
												ELSE '' END AS [StatusName]

							) STA

                            WHERE [OrganizationId] IN ({affiliates.Replace("[", "").Replace("]", "")})
                    ";

                    // shipper
                    if (!string.IsNullOrWhiteSpace(supplierCustomerRelationships))
                    {
                        categorizedPOViewSelectorSql += @$"  AND ([OrganizationRole] = 'Delegation'
                                                            OR(
                                                                [OrganizationRole] = 'Supplier'
                                                                AND CAST([SupplierID] AS NVARCHAR(20)) + ',' + CAST([CustomerId] AS NVARCHAR(20)) IN(SELECT tmp.[Value]
                                                                FROM dbo.fn_SplitStringToTable('{supplierCustomerRelationships}', ';') tmp )
	                                                        ))";
                    }

                    switch (statisticKey)
                    {
                        case "booked":
                        case "unbooked":
                            sql = $@"   
                            SELECT DISTINCT 
                            Id,
                            PONumber,
                            CustomerReferences,
			                CreatedBy,
			                POIssueDate,
			                [Status],
			                Stage,
			                POType,
			                CargoReadyDate,
			                CreatedDate, 
                            Supplier,
                            IsProgressCargoReadyDates,
                            ProgressNotifyDay,
                            ProductionStarted,
                            ModeOfTransport,
                            ShipFrom,
                            ShipTo,
                            Incoterm,
                            ExpectedDeliveryDate,
                            ExpectedShipDate,
                            ContainerType,
                            PORemark,
                            STG.StageName,
                            STA.StatusName
                            FROM vw_PurchaseOrderList_{statisticKey}_ExternalUsers  

                            OUTER APPLY 
							(
								SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
												WHEN Stage = 20 THEN 'label.released'
												WHEN Stage = 30 THEN 'label.booked'
												WHEN Stage = 40 THEN 'label.bookingConfirmed'
												WHEN Stage = 45 THEN 'label.cargoReceived'
												WHEN Stage = 50 THEN 'label.shipmentDispatch'
												WHEN Stage = 60 THEN 'label.closed'
												WHEN Stage = 70 THEN 'label.completed'
												ELSE '' END AS [StageName]

							) STG
							OUTER APPLY 
							(
								SELECT CASE		WHEN Status = 1 THEN 'label.active'
												WHEN Status = 0 THEN 'label.cancel'
												ELSE '' END AS [StatusName]

							) STA

                            WHERE [OrganizationId] IN ({affiliates.Replace("[", "").Replace("]", "")})
                                AND CargoReadyDate >= '{dates["FromDate"]}'  AND CargoReadyDate <= '{dates["ToDate"]}'";
                            break;
                        case "inOriginDC":
                        case "inTransit":
                        case "customsCleared":
                        case "pendingDCDelivery":
                        case "managedToDate":
                        case "dcDeliveryConfirmed":
                            sql = $@"
                                 SELECT DISTINCT 
                            Id,
                            PONumber,
                            CustomerReferences,
			                CreatedBy,
			                POIssueDate,
			                [Status],
			                Stage,
			                POType,
			                CargoReadyDate,
			                CreatedDate, 
                            Supplier,
                            IsProgressCargoReadyDates,
                            ProgressNotifyDay,
                            ProductionStarted,
                            ModeOfTransport,
                            ShipFrom,
                            ShipTo,
                            Incoterm,
                            ExpectedDeliveryDate,
                            ExpectedShipDate,
                            ContainerType,
                            PORemark,
                            STG.StageName,
                            STA.StatusName
                            FROM vw_PurchaseOrderList_{statisticKey}_ExternalUsers  

                            OUTER APPLY 
							(
								SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
												WHEN Stage = 20 THEN 'label.released'
												WHEN Stage = 30 THEN 'label.booked'
												WHEN Stage = 40 THEN 'label.bookingConfirmed'
												WHEN Stage = 45 THEN 'label.cargoReceived'
												WHEN Stage = 50 THEN 'label.shipmentDispatch'
												WHEN Stage = 60 THEN 'label.closed'
												WHEN Stage = 70 THEN 'label.completed'
												ELSE '' END AS [StageName]

							) STG
							OUTER APPLY 
							(
								SELECT CASE		WHEN Status = 1 THEN 'label.active'
												WHEN Status = 0 THEN 'label.cancel'
												ELSE '' END AS [StatusName]

							) STA

                            WHERE [OrganizationId] IN ({affiliates.Replace("[", "").Replace("]", "")})
                                AND ActivityDate >= '{dates["FromDate"]}' AND ActivityDate <= '{dates["ToDate"]}'";
                            break;
                        case "poIssuedInThisWeek":
                        case "poIssuedInLastWeek":
                            sql = $@"
                                SELECT DISTINCT 
                            Id,
                            PONumber,
                            CustomerReferences,
			                CreatedBy,
			                POIssueDate,
			                [Status],
			                Stage,
			                POType,
			                CargoReadyDate,
			                CreatedDate, 
                            Supplier,
                            IsProgressCargoReadyDates,
                            ProgressNotifyDay,
                            ProductionStarted,
                            ModeOfTransport,
                            ShipFrom,
                            ShipTo,
                            Incoterm,
                            ExpectedDeliveryDate,
                            ExpectedShipDate,
                            ContainerType,
                            PORemark,
                            STG.StageName,
                            STA.StatusName
                            FROM vw_PurchaseOrderList_{statisticKey}_ExternalUsers  

                            OUTER APPLY 
							(
								SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
												WHEN Stage = 20 THEN 'label.released'
												WHEN Stage = 30 THEN 'label.booked'
												WHEN Stage = 40 THEN 'label.bookingConfirmed'
												WHEN Stage = 45 THEN 'label.cargoReceived'
												WHEN Stage = 50 THEN 'label.shipmentDispatch'
												WHEN Stage = 60 THEN 'label.closed'
												WHEN Stage = 70 THEN 'label.completed'
												ELSE '' END AS [StageName]

							) STG
							OUTER APPLY 
							(
								SELECT CASE		WHEN Status = 1 THEN 'label.active'
												WHEN Status = 0 THEN 'label.cancel'
												ELSE '' END AS [StatusName]

							) STA

                            WHERE [OrganizationId] IN ({affiliates.Replace("[", "").Replace("]", "")})
                               AND  POIssueDate >= '{dates["FromDate"]}' AND POIssueDate <= '{dates["ToDate"]}'";
                            break;
                        case "categorizedSupplier":
                            sql = categorizedPOViewSelectorSql + $@"AND Supplier = '{statisticValue}'";
                            break;
                        case "categorizedConsignee":
                            sql = categorizedPOViewSelectorSql + $@"AND Consignee = '{statisticValue}'";
                            break;
                        case "categorizedDestination":
                            sql = categorizedPOViewSelectorSql + $@"AND ShipTo = '{statisticValue}'";
                            break;
                        case "categorizedStage":
                            sql = categorizedPOViewSelectorSql + $@"AND Stage = '{statisticValue}'";
                            break;
                        case "categorizedStatus":
                            sql = categorizedPOViewSelectorSql + $@"AND Status = '{statisticValue}'";
                            break;
                        default:
                            break;
                    }
                }

                // user role = agent/ principal
                else if (string.IsNullOrEmpty(supplierCustomerRelationships))
                {
                    sql =
                        $@"
                            SELECT 
	                            po.Id, 
                                    IIF(BCOM.IsAllowShowAdditionalInforPOListing = 1, CONCAT(po.PONumber,'~',ProductCode,'~',GridValue,'~',LineOrder), po.PONumber) as PONumber,
                                    po.CustomerReferences, 
                                    po.POIssueDate, po.Status, po.Stage, po.CargoReadyDate, 
                                    po.CreatedDate, po.CreatedBy, po.POType, 
                                    po.ProductionStarted,
                                    po.ModeOfTransport,
                                    po.ShipFrom,
                                    po.ShipTo,
                                    po.Incoterm,
                                    po.ExpectedDeliveryDate,
                                    po.ExpectedShipDate,
                                    po.ContainerType,
                                    po.PORemark, 
	                            SUP.*,
	                            BCOM.IsProgressCargoReadyDate AS IsProgressCargoReadyDates,
	                            BCOM.ProgressNotifyDay,
	                            STG.StageName,
	                            STA.StatusName,
                                0  as [check]
                            FROM
                            (
	                            SELECT 
                                    po.Id, 
                                    po.PONumber,
                                    po.CustomerReferences, 
                                    po.POIssueDate, po.Status, po.Stage, po.CargoReadyDate, 
                                    po.CreatedDate, po.CreatedBy, po.POType, 
                                    po.ProductionStarted,
                                    po.ModeOfTransport,
                                    po.ShipFrom,
                                    po.ShipTo,
                                    po.Incoterm,
                                    po.ExpectedDeliveryDate,
                                    po.ExpectedShipDate,
                                    po.ContainerType,
                                    po.PORemark
	                            FROM PurchaseOrders po WITH (NOLOCK)
	                            WHERE po.Id IN (
		                            SELECT pc.PurchaseOrderId FROM PurchaseOrderContacts pc
		                            WHERE po.Id = pc.PurchaseOrderId AND pc.OrganizationId IN ({string.Join(",", listOfAffiliates)}))
                            ) PO
                            OUTER APPLY
                            (
	                            SELECT TOP(1) sc.CompanyName AS Supplier
	                            FROM PurchaseOrderContacts sc WITH (NOLOCK)
	                            WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
                            ) SUP
                            OUTER APPLY
                            (
		                            SELECT 
		                                BC.IsProgressCargoReadyDate,
		                                BC.ProgressNotifyDay,
                                        BC.IsAllowShowAdditionalInforPOListing
	                                FROM BuyerCompliances BC (NOLOCK)
	                                INNER JOIN PurchaseOrderContacts POC (NOLOCK) ON POC.OrganizationId = BC.OrganizationId 
		                                AND POC.OrganizationRole = 'Principal'
                                        AND BC.Stage = 1
			                            AND POC.PurchaseOrderId = PO.Id
                            ) BCOM
                            OUTER APPLY
                            (
							        SELECT TOP 1 ProductCode, GridValue, LineOrder
							        FROM POLineItems POL
							        WHERE POL.PurchaseOrderId = PO.Id AND BCOM.IsAllowShowAdditionalInforPOListing = 1							
    	                    ) POL
                            OUTER APPLY 
                            (
	                            SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
					                            WHEN Stage = 20 THEN 'label.released'
					                            WHEN Stage = 30 THEN 'label.booked'
					                            WHEN Stage = 40 THEN 'label.bookingConfirmed'
					                            WHEN Stage = 45 THEN 'label.cargoReceived'
					                            WHEN Stage = 50 THEN 'label.shipmentDispatch'
					                            WHEN Stage = 60 THEN 'label.closed'
					                            WHEN Stage = 70 THEN 'label.completed'
					                            ELSE '' END AS [StageName]

                            ) STG
                            OUTER APPLY 
                            (
	                            SELECT CASE		WHEN Status = 1 THEN 'label.active'
					                            WHEN Status = 0 THEN 'label.cancel'
					                            ELSE '' END AS [StatusName]

                            ) STA";
                    if (!string.IsNullOrEmpty(itemNo))
                    {
                        sql += @$"
                            WHERE EXISTS (SELECT 1 FROM POLineItems POI WHERE  POI.PurchaseOrderId = PO.Id AND POI.ProductCode like '%{itemNo.Split("~")[1]}%')
                           ";
                    }
                }
                // user role = shipper
                else
                {
                    if (id != null)
                    {
                        sql =
                        @$"
                            SELECT 
                                 po.Id, 
                                 IIF(BCOM.IsAllowShowAdditionalInforPOListing = 1, CONCAT(po.PONumber,'~',ProductCode,'~',GridValue,'~',LineOrder), po.PONumber) as PONumber,
                                 po.CustomerReferences, po.POIssueDate,
                                 po.Status, po.Stage, po.CargoReadyDate, po.CreatedDate, po.CreatedBy,
                                 po.OrganizationRole, po.POType,
                                 po.ProductionStarted,
                                 po.ModeOfTransport,
                                 po.ShipFrom,
                                 po.ShipTo,
                                 po.Incoterm,
                                 po.ExpectedDeliveryDate,
                                 po.ExpectedShipDate,
                                 po.ContainerType,
                                 po.PORemark,
                                SUP.*, PRIN.*, BCOM.*, STG.StageName, STA.StatusName,
		                        --BCOM.AllowToBookIn,
		                        CASE
                                WHEN
                                (
                                    PO.Status = 1
            
                                    AND (PO.POType = (Select POType from PurchaseOrders where Id = {id}))
                                    AND (BCOM.IsProgressCargoReadyDates = 0 OR NOT (BCOM.IsProgressCargoReadyDates = 1 AND BCOM.IsCompulsory = 1 AND po.ProductionStarted = 0))
			                        AND POLB.BalanceUnitQty > 0
			                        AND (SPCM.AllowMultiplePOPerFulfillment = 1 OR PO.Id = {id})
			                        AND(SUP.SupplierId =(SELECT TOP(1) sc.OrganizationId FROM PurchaseOrderContacts sc WHERE sc.PurchaseOrderId = {id} AND sc.OrganizationRole = 'Supplier'))
			                        AND(PRIN.CustomerId =(SELECT TOP(1) sc.OrganizationId FROM PurchaseOrderContacts sc WHERE sc.PurchaseOrderIdsc.PurchaseOrderId = {id} AND sc.OrganizationRole = 'Principal'))
			                        AND(CONS.ConsigneeId =(SELECT TOP(1) sc.OrganizationId FROM PurchaseOrderContacts sc WHERE sc.PurchaseOrderId = {id} AND sc.OrganizationRole = 'Consignee'))
			                        AND
				                        -- Check buyer compliance settings on purchase order verifications
				                        (
					                        {id} != PO.Id
					                        AND	((VERS.ExpectedShipDateVerification != 10) OR (PO.ExpectedShipDate = (Select ExpectedShipDate From PurchaseOrders where Id={id})))
					                        AND ((VERS.ExpectedDeliveryDateVerification != 10) OR (PO.ExpectedDeliveryDate = (Select ExpectedDeliveryDate From PurchaseOrders where Id={id})))
					                        AND ((VERS.ConsigneeVerification != 10) OR (CONS.ConsigneeId =(SELECT TOP(1) sc.OrganizationId FROM PurchaseOrderContacts sc WHERE sc.PurchaseOrderId = {id} AND sc.OrganizationRole = 'Consignee')))
					                        AND ((VERS.ShipFromLocationVerification != 10) OR ( PO.ShipFromId = (Select ShipFromId From PurchaseOrders where Id={id})))
					                        AND ((VERS.ShipToLocationVerification != 10) OR ( PO.ShipToId = (Select ShipToId From PurchaseOrders where Id={id})))
					                        AND ((VERS.ModeOfTransportVerification != 10) OR (PO.ModeOfTransport = (Select ModeOfTransport From PurchaseOrders where Id={id})))
					                        AND ((VERS.IncotermVerification != 10) OR ( PO.Incoterm = (Select Incoterm From PurchaseOrders where Id={id})))
					                        AND ((VERS.PreferredCarrierVerification != 10) OR ( PO.CarrierCode= (Select CarrierCode From PurchaseOrders where Id={id})))
					                        AND ((BT.CheckPOExWorkDate = 0) OR (PO.CargoReadyDate IS NULL) 
						                        OR ((Select CargoReadyDate From PurchaseOrders where Id={id}) IS NULL) 
						                        OR (CAST(PO.CargoReadyDate AS DATE) = CAST((Select CargoReadyDate From PurchaseOrders where Id={id}) AS DATE)))
				                        )
                                ) THEN 1
                                ELSE 0
                             END as [check]
                             FROM
                             (
                                 SELECT 
                                     po.Id, po.PONumber,po.CustomerReferences, po.POIssueDate,
                                     po.Status, po.Stage, po.CargoReadyDate, po.CreatedDate, po.CreatedBy,
                                     pc.OrganizationRole, po.POType,
                                     po.ProductionStarted,
                                     po.ModeOfTransport,
                                     po.ShipFrom,
		                             po.ShipFromId,
                                     po.ShipTo,
		                             po.ShipToId,
                                     po.Incoterm,
		                             po.CarrierCode,
                                     po.ExpectedDeliveryDate,
                                     po.ExpectedShipDate,
                                     po.ContainerType,
                                     po.PORemark
                                 FROM PurchaseOrders po WITH (NOLOCK)
                                 INNER JOIN PurchaseOrderContacts pc WITH (NOLOCK) on po.Id = pc.PurchaseOrderId
                                 WHERE pc.OrganizationId = {delegatedOrganizationId} 
                             ) PO
                             CROSS APPLY
                             (
                                 SELECT TOP(1) sc.CompanyName AS Supplier, sc.OrganizationId AS SupplierId
                                 FROM PurchaseOrderContacts sc WITH (NOLOCK)
                                 WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
                             ) SUP
                             CROSS APPLY
                             (
                                 SELECT TOP(1) sc.OrganizationId AS CustomerId
                                 FROM PurchaseOrderContacts sc WITH (NOLOCK)
                                 WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
                             ) PRIN
                             CROSS APPLY
                             (
                                 SELECT TOP(1) sc.OrganizationId AS ConsigneeId
                                 FROM PurchaseOrderContacts sc WITH (NOLOCK)
                                 WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Consignee'
                             ) CONS
                             CROSS APPLY
                             (
                                 SELECT 
		                             BC.Id,
                                     BC.IsProgressCargoReadyDate AS IsProgressCargoReadyDates,
                                     BC.ProgressNotifyDay,
                                     BC.IsAllowShowAdditionalInforPOListing,
		                             BC.IsCompulsory,
		                             BC.AllowToBookIn
                                 FROM BuyerCompliances BC (NOLOCK)
                                 WHERE BC.OrganizationId = PRIN.CustomerId
                                     AND BC.Stage = 1
                             ) BCOM
                             OUTER APPLY
                             (
	                                    SELECT TOP 1 ProductCode, GridValue, LineOrder , BalanceUnitQty
	                                    FROM POLineItems POL 
	                                    WHERE POL.PurchaseOrderId = PO.Id AND BCOM.IsAllowShowAdditionalInforPOListing = 1		
			                            order by BalanceUnitQty desc
                             ) POL
                             OUTER APPLY 
                             (
                                 SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
                                                 WHEN Stage = 20 THEN 'label.released'
                                                 WHEN Stage = 30 THEN 'label.booked'
                                                 WHEN Stage = 40 THEN 'label.bookingConfirmed'
                                                 WHEN Stage = 45 THEN 'label.cargoReceived'
                                                 WHEN Stage = 50 THEN 'label.shipmentDispatch'
                                                 WHEN Stage = 60 THEN 'label.closed'
                                                 WHEN Stage = 70 THEN 'label.completed'
                                                 ELSE '' END AS [StageName]

                             ) STG
                             OUTER APPLY 
                             (
                                 SELECT CASE		WHEN Status = 1 THEN 'label.active'
                                                 WHEN Status = 0 THEN 'label.cancel'
                                                 ELSE '' END AS [StatusName]

                             ) STA
                              OUTER APPLY
                             (
	                                    SELECT TOP 1  BalanceUnitQty
	                                    FROM POLineItems POL 
	                                    WHERE POL.PurchaseOrderId = PO.Id
			                            order by BalanceUnitQty desc
                             ) POLB
                             Outer Apply
                             (
                             SELECT SPC.AllowMultiplePOPerFulfillment 
		                            FROM ShippingCompliances SPC WITH (NOLOCK)
			                            INNER JOIN BuyerCompliances BC ON SPC.BuyerComplianceId = BC.Id AND BC.[Status] = 1 AND BC.OrganizationId = PRIN.CustomerId
                             )SPCM
                             Outer Apply
                             (
                             SELECT			VER.ExpectedShipDateVerification,
				                            VER.ExpectedDeliveryDateVerification,
				                            VER.ConsigneeVerification,
				                            VER.ShipperVerification,
				                            VER.ShipFromLocationVerification,
				                            VER.ShipToLocationVerification,
				                            VER.ModeOfTransportVerification,
				                            VER.IncotermVerification,
				                            VER.PreferredCarrierVerification
		                            FROM PurchaseOrderVerificationSettings VER WITH (NOLOCK)
		                            WHERE VER.BuyerComplianceId = (Select BC.ID From BuyerCompliances BC inner join PurchaseOrderContacts PC on PC.OrganizationId = BC.OrganizationId AND PC.OrganizationRole = 'Principal' where PC.PurchaseOrderId ={id})
                             )VERS
                             OUTER APPLY
                             (SELECT Count(1) AS CheckPOExWorkDate
                                    FROM BookingTimelesses BT
                                    WHERE
			                            BT.BuyerComplianceId = (Select BC.ID From BuyerCompliances BC inner join PurchaseOrderContacts PC on PC.OrganizationId = BC.OrganizationId AND PC.OrganizationRole = 'Principal' where PC.PurchaseOrderId ={id})
			                            AND BT.DateForComparison = 10
                                        AND EXISTS
                                            (    SELECT 1
                                                FROM BookingPolicies BP
                                                WHERE BT.BuyerComplianceId = BP.BuyerComplianceId AND BP.BookingTimeless != 0
                                            )
                            )BT
                             WHERE
                                 (PO.OrganizationRole = 'Delegation'
                                 OR (
                                     PO.OrganizationRole = 'Supplier' 
                                     AND CAST(SUP.SupplierID AS NVARCHAR(20)) + ',' + CAST(PRIN.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] 
                                     FROM dbo.fn_SplitStringToTable('{supplierCustomerRelationships}', ';') tmp )
                          ))";


                    }
                    else
                    {
                        sql =
                        @$"
                            SELECT 
                                    po.Id, 
                                    IIF(BCOM.IsAllowShowAdditionalInforPOListing = 1, CONCAT(po.PONumber,'~',ProductCode,'~',GridValue,'~',LineOrder), po.PONumber) as PONumber,
                                    po.CustomerReferences, po.POIssueDate,
                                    po.Status, po.Stage, po.CargoReadyDate, po.CreatedDate, po.CreatedBy,
                                    po.OrganizationRole, po.POType,
                                    po.ProductionStarted,
                                    po.ModeOfTransport,
                                    po.ShipFrom,
                                    po.ShipTo,
                                    po.Incoterm,
                                    po.ExpectedDeliveryDate,
                                    po.ExpectedShipDate,
                                    po.ContainerType,
                                    po.PORemark,
                                   SUP.*, PRIN.*, BCOM.*, STG.StageName, STA.StatusName,
                                    CASE
                                    WHEN
                                        (
                                        PO.Status = 1
            
                                        AND (PO.POType = 10 OR PO.POType =BCOM.AllowToBookIn)
                                        AND (BCOM.IsProgressCargoReadyDates = 0 OR NOT (BCOM.IsProgressCargoReadyDates = 1 AND BCOM.IsCompulsory = 1 AND po.ProductionStarted = 0))
			                            AND POLB.BalanceUnitQty > 0
                                        ) THEN 1
                                    ELSE 0
                                    END as [check]

                            FROM
                            (
	                            SELECT 
                                    po.Id, po.PONumber,po.CustomerReferences, po.POIssueDate,
                                    po.Status, po.Stage, po.CargoReadyDate, po.CreatedDate, po.CreatedBy,
                                    pc.OrganizationRole, po.POType,
                                    po.ProductionStarted,
                                    po.ModeOfTransport,
                                    po.ShipFrom,
                                    po.ShipTo,
                                    po.Incoterm,
                                    po.ExpectedDeliveryDate,
                                    po.ExpectedShipDate,
                                    po.ContainerType,
                                    po.PORemark
	                            FROM PurchaseOrders po WITH (NOLOCK)
                                INNER JOIN PurchaseOrderContacts pc WITH (NOLOCK) on po.Id = pc.PurchaseOrderId
                                WHERE pc.OrganizationId = {delegatedOrganizationId}
                            ) PO
                            CROSS APPLY
                            (
			                    SELECT TOP(1) sc.CompanyName AS Supplier, sc.OrganizationId AS SupplierId
			                    FROM PurchaseOrderContacts sc WITH (NOLOCK)
			                    WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
                            ) SUP
                            CROSS APPLY
                            (
                                SELECT TOP(1) sc.OrganizationId AS CustomerId
                                FROM PurchaseOrderContacts sc WITH (NOLOCK)
                                WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
                            ) PRIN
                            CROSS APPLY
                            (
		                        SELECT 
			                        BC.IsProgressCargoReadyDate AS IsProgressCargoReadyDates,
			                        BC.ProgressNotifyDay,
                                    BC.IsAllowShowAdditionalInforPOListing,
		                            BC.IsCompulsory,
		                            BC.AllowToBookIn
		                        FROM BuyerCompliances BC (NOLOCK)
		                        WHERE BC.OrganizationId = PRIN.CustomerId
                                    AND BC.Stage = 1
                            ) BCOM
                            OUTER APPLY
                            (
							        SELECT TOP 1 ProductCode, GridValue, LineOrder
							        FROM POLineItems POL
							        WHERE POL.PurchaseOrderId = PO.Id AND BCOM.IsAllowShowAdditionalInforPOListing = 1							
    	                    ) POL
                            OUTER APPLY 
                            (
	                            SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
					                            WHEN Stage = 20 THEN 'label.released'
					                            WHEN Stage = 30 THEN 'label.booked'
					                            WHEN Stage = 40 THEN 'label.bookingConfirmed'
					                            WHEN Stage = 45 THEN 'label.cargoReceived'
					                            WHEN Stage = 50 THEN 'label.shipmentDispatch'
					                            WHEN Stage = 60 THEN 'label.closed'
					                            WHEN Stage = 70 THEN 'label.completed'
					                            ELSE '' END AS [StageName]

                            ) STG
                            OUTER APPLY 
                            (
	                            SELECT CASE		WHEN Status = 1 THEN 'label.active'
					                            WHEN Status = 0 THEN 'label.cancel'
					                            ELSE '' END AS [StatusName]

                            ) STA
                              OUTER APPLY
                             (
	                                    SELECT TOP 1  BalanceUnitQty
	                                    FROM POLineItems POL 
	                                    WHERE POL.PurchaseOrderId = PO.Id
			                            order by BalanceUnitQty desc
                             ) POLB
                            WHERE
	                            (PO.OrganizationRole = 'Delegation'
	                            OR (
		                            PO.OrganizationRole = 'Supplier' 
		                            AND CAST(SUP.SupplierID AS NVARCHAR(20)) + ',' + CAST(PRIN.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] 
		                            FROM dbo.fn_SplitStringToTable('{supplierCustomerRelationships}', ';') tmp )
	                            ))";
                        if (!string.IsNullOrEmpty(itemNo))
                        {
                            sql += @$"
                            AND EXISTS (SELECT 1 FROM POLineItems POI WHERE  POI.PurchaseOrderId = PO.Id AND POI.ProductCode like '%{itemNo.Split("~")[1]}%')
                           ";
                        }
                    }

                }
            }

            query = _dataQuery.GetQueryable<PurchaseOrderQueryModel>(sql);

            if (isExport == false && string.IsNullOrEmpty(statisticKey) && request.FilterValuesOnColumn("poNumber") != null && request.FilterValuesOnColumn("poNumber").Any())
            {
                var singleQuery = _dataQuery.GetQueryable<PurchaseOrderSingleQueryModel>(sql);
                // Mappping: transform query model (from sql script) to data model (to grid/list)
                Func<PurchaseOrderSingleQueryModel, PurchaseOrderQueryModel> mappingData = (x) => { return Mapper.Map<PurchaseOrderSingleQueryModel, PurchaseOrderQueryModel>(x); };

                // Call to get grid data from current grid request
                var result = await _dataQuery.ToDataSourceResultAsSingleQueryAsync(singleQuery, request, mappingData);
                return result;
            }

            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<IEnumerable<SelectPurchaseOrderViewModel>> GetPurchaseOrderSelectionsByPrincipalIdAsync(int skip, int take, string searchTerm, long selectedPOId, string supplierCustomerRelationships, long principalOrganizationId, long supplierOrganizationId, int roleId, string affiliates, bool isInternal)
        {
            var storedProcedureName = "";
            List<SqlParameter> filterParameter;
            if (isInternal)
            {
                // It is internal user
                storedProcedureName = "spu_GetPurchaseOrderSelectionList_InternalUsers";
                filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@PrincipalOrganizationId",
                        Value = principalOrganizationId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SearchTerm",
                        Value = searchTerm,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Skip",
                        Value = skip,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@Take",
                        Value = take,
                        DbType = DbType.Int32,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@SelectedPOId",
                        Value = selectedPOId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            }
            else
            {
                if (roleId == (int)Role.Agent)
                {
                    string organizationIds = string.Empty;
                    if (!string.IsNullOrEmpty(affiliates))
                    {
                        var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                        organizationIds = string.Join(",", listOfAffiliates);
                    }
                    storedProcedureName = "spu_GetPurchaseOrderSelectionList_Agent";
                    filterParameter = new List<SqlParameter>
                    {
                        new SqlParameter
                        {
                            ParameterName = "@PrincipalOrganizationId",
                            Value = principalOrganizationId,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@SearchTerm",
                            Value = searchTerm,
                            DbType = DbType.String,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@Skip",
                            Value = skip,
                            DbType = DbType.Int32,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@Take",
                            Value = take,
                            DbType = DbType.Int32,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@SelectedPOId",
                            Value = selectedPOId,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        },
                         new SqlParameter
                        {
                            ParameterName = "@Affiliates",
                            Value = organizationIds,
                            DbType = DbType.String,
                            Direction = ParameterDirection.Input
                        }
                    };
                }
                else
                {
                    storedProcedureName = "spu_GetPurchaseOrderSelectionList_Shippers";
                    filterParameter = new List<SqlParameter>
                    {
                        new SqlParameter
                        {
                            ParameterName = "@PrincipalOrganizationId",
                            Value = principalOrganizationId,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@SearchTerm",
                            Value = searchTerm,
                            DbType = DbType.String,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@SupplierOrganizationId",
                            Value = supplierOrganizationId,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@SupplierCustomerRelationships",
                            Value = supplierCustomerRelationships,
                            DbType = DbType.String,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@Skip",
                            Value = skip,
                            DbType = DbType.Int32,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@Take",
                            Value = take,
                            DbType = DbType.Int32,
                            Direction = ParameterDirection.Input
                        },
                        new SqlParameter
                        {
                            ParameterName = "@SelectedPOId",
                            Value = selectedPOId,
                            DbType = DbType.Int64,
                            Direction = ParameterDirection.Input
                        }
                    };
                }
            }

            Func<DbDataReader, IEnumerable<SelectPurchaseOrderViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<SelectPurchaseOrderViewModel>();
                while (reader.Read())
                {
                    var tmpId = reader[0];
                    var tmpItemsCount = reader[1];
                    var tmpPONumber = reader[2];
                    var tmpRowCount = reader[3];

                    var newRow = new SelectPurchaseOrderViewModel
                    {
                        Id = (long)tmpId,
                        ItemsCount = (int)tmpItemsCount,
                        PONumber = tmpPONumber?.ToString() ?? "",
                        RecordCount = (long)(tmpRowCount != DBNull.Value ? tmpRowCount : 0)
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }

        public async Task<IEnumerable<SelectUnmappedPurchaseOrderViewModel>> GetUnmappedPurchaseOrderSelectionsAsync(int skip, int take, string searchTerm, long principalOrganizationId, long supplierOrganizationId)
        {
            var storedProcedureName = "spu_GetUnmappedPurchaseOrderSelectionList";
            List<SqlParameter> filterParameter = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@PrincipalOrganizationId",
                    Value = principalOrganizationId,
                    DbType = DbType.Int64,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@SupplierOrganizationId",
                    Value = supplierOrganizationId,
                    DbType = DbType.Int64,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@SearchTerm",
                    Value = searchTerm,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@Skip",
                    Value = skip,
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@Take",
                    Value = take,
                    DbType = DbType.Int32,
                    Direction = ParameterDirection.Input
                }
            };

            Func<DbDataReader, IEnumerable<SelectUnmappedPurchaseOrderViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<SelectUnmappedPurchaseOrderViewModel>();
                while (reader.Read())
                {
                    var tmpId = reader[0];
                    var tmpPONumber = reader[1];
                    var tmpShipFrom = reader[2];
                    var tmpRowCount = reader[3];
                    var displayText = reader[4];

                    var newRow = new SelectUnmappedPurchaseOrderViewModel
                    {
                        Id = (long)tmpId,
                        PONumber = tmpPONumber?.ToString() ?? "",
                        ShipFrom = tmpShipFrom?.ToString() ?? "",
                        RecordCount = (long)(tmpRowCount != DBNull.Value ? tmpRowCount : 0),
                        DisplayText = displayText?.ToString() ?? "",
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, filterParameter.ToArray());
            return result;
        }
        #region Shipped purchase orders

        public async Task<DataSourceResult> ShippedListAsync(DataSourceRequest request, bool isInternal, string affiliates, string supplierCustomerRelationships, long? delegatedOrganizationId = 0)
        {
            var listOfAffiliates = new List<long>();

            if (!string.IsNullOrEmpty(affiliates))
            {
                listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            }

            IQueryable<ShippedPurchaseOrderQueryModel> query;
            string sql;
            if (isInternal)
            {
                sql =
                    @" SELECT 
                                PO.Id, 
                                PO.PONumber, 
                                PO.POIssueDate, 
                                PO.[Status], 
                                PO.Stage, 
                                PO.CargoReadyDate, 
                                PO.CreatedDate, 
                                PO.CreatedBy, 
                                SUP.Supplier, 
                                PO.POType, 
                                NULL AS IsProgressCargoReadyDates, 
                                NULL AS ProgressNotifyDay, 
                                PO.ProductionStarted,
                                STG.StageName,
                                STA.StatusName

                                FROM PurchaseOrders PO WITH (NOLOCK)
                                OUTER APPLY

                                (
	                                SELECT TOP (1) PC.CompanyName Supplier
	                                FROM PurchaseOrderContacts PC WITH (NOLOCK)
	                                WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationRole = 'Supplier'
                                ) SUP

                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
					                                WHEN Stage = 20 THEN 'label.released'
					                                WHEN Stage = 30 THEN 'label.booked'
					                                WHEN Stage = 40 THEN 'label.bookingConfirmed'
					                                WHEN Stage = 45 THEN 'label.cargoReceived'
					                                WHEN Stage = 50 THEN 'label.shipmentDispatch'
					                                WHEN Stage = 60 THEN 'label.closed'
					                                WHEN Stage = 70 THEN 'label.completed'
					                                ELSE '' END AS [StageName]

                                ) STG
                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN Status = 1 THEN 'label.active'
					                                WHEN Status = 0 THEN 'label.cancel'
					                                ELSE '' END AS [StatusName]

                                ) STA

                                WHERE 
	                                -- Stage = Shipment Dispatch / Closed
	                                PO.Stage IN (50, 60)

	                                AND EXISTS (
		                                SELECT 1
		                                FROM CargoDetails CD  WITH (NOLOCK)
		                                WHERE PO.Id = CD.OrderId AND CD.OrderType = 1 AND CD.ShipmentId IS NOT NULL
	                                )";
            }
            else
            {
                // user role = agent/ principal
                if (string.IsNullOrEmpty(supplierCustomerRelationships))
                {
                    sql =
                        $@"SELECT 
                                PO.Id, 
                                PO.PONumber, 
                                PO.POIssueDate, 
                                PO.[Status], 
                                PO.Stage, 
                                PO.CargoReadyDate, 
                                PO.CreatedDate, 
                                PO.CreatedBy, 
                                SUP.Supplier, 
                                PO.POType, 
                                NULL AS IsProgressCargoReadyDates, 
                                NULL AS ProgressNotifyDay, 
                                PO.ProductionStarted,
                                STG.StageName,
                                STA.StatusName

                                FROM PurchaseOrders PO WITH (NOLOCK)
                                OUTER APPLY

                                (
	                                SELECT TOP (1) PC.CompanyName Supplier
	                                FROM PurchaseOrderContacts PC WITH (NOLOCK)
	                                WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationRole = 'Supplier'
                                ) SUP

                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
					                                WHEN Stage = 20 THEN 'label.released'
					                                WHEN Stage = 30 THEN 'label.booked'
					                                WHEN Stage = 40 THEN 'label.bookingConfirmed'
					                                WHEN Stage = 45 THEN 'label.cargoReceived'
					                                WHEN Stage = 50 THEN 'label.shipmentDispatch'
					                                WHEN Stage = 60 THEN 'label.closed'
					                                WHEN Stage = 70 THEN 'label.completed'
					                                ELSE '' END AS [StageName]

                                ) STG
                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN Status = 1 THEN 'label.active'
					                                WHEN Status = 0 THEN 'label.cancel'
					                                ELSE '' END AS [StatusName]

                                ) STA

                                WHERE 
	                                -- Stage = Shipment Dispatch / Closed
	                                PO.Stage IN (50, 60)

	                                AND PO.Id IN (
                                        SELECT PC.PurchaseOrderId FROM PurchaseOrderContacts PC WITH (NOLOCK)
                                        WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationId IN ({string.Join(",", listOfAffiliates)}))

	                                AND EXISTS (
		                                SELECT 1
		                                FROM CargoDetails CD WITH (NOLOCK)
		                                WHERE PO.Id = CD.OrderId AND CD.OrderType = 1 AND CD.ShipmentId IS NOT NULL
	                                ) ";
                }
                // user role = shipper
                else
                {
                    sql =
                        @"
                            SELECT PO.*, SUP.*, PRIN.*, STG.StageName, STA.StatusName
                                FROM
                                (
	                                SELECT po.Id, po.PONumber, po.POIssueDate, po.Status, po.Stage, po.CargoReadyDate, po.CreatedDate, po.CreatedBy, pc.OrganizationRole, po.POType, NULL AS IsProgressCargoReadyDates, NULL AS ProgressNotifyDay, po.ProductionStarted
	                                FROM PurchaseOrders po WITH (NOLOCK)
                                    INNER JOIN PurchaseOrderContacts pc on po.Id = pc.PurchaseOrderId
                                    WHERE pc.OrganizationId = {0} 
                                        AND EXISTS (
		                                        SELECT 1
		                                        FROM CargoDetails CD 
		                                        WHERE PO.Id = CD.OrderId AND CD.OrderType = 1 AND CD.ShipmentId IS NOT NULL
	                                        )
                                        AND 
                                            -- Stage = Shipment Dispatch / Closed
	                                        PO.Stage IN (50, 60)
                                ) PO
                                CROSS APPLY
                                (
			                        SELECT TOP(1) sc.CompanyName AS Supplier, sc.OrganizationId AS SupplierId
			                        FROM PurchaseOrderContacts sc WITH (NOLOCK)
			                        WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
                                ) SUP
                                CROSS APPLY
                                (
                                    SELECT TOP(1) sc.OrganizationId AS CustomerId
                                    FROM PurchaseOrderContacts sc WITH (NOLOCK)
                                    WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
                                ) PRIN
                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN Stage = 10 THEN 'label.draft'
					                                WHEN Stage = 20 THEN 'label.released'
					                                WHEN Stage = 30 THEN 'label.booked'
					                                WHEN Stage = 40 THEN 'label.bookingConfirmed'
					                                WHEN Stage = 45 THEN 'label.cargoReceived'
					                                WHEN Stage = 50 THEN 'label.shipmentDispatch'
					                                WHEN Stage = 60 THEN 'label.closed'
					                                WHEN Stage = 70 THEN 'label.completed'
					                                ELSE '' END AS [StageName]

                                ) STG
                                OUTER APPLY 
                                (
	                                SELECT CASE		WHEN Status = 1 THEN 'label.active'
					                                WHEN Status = 0 THEN 'label.cancel'
					                                ELSE '' END AS [StatusName]

                                ) STA
                                WHERE
								    
	                                PO.OrganizationRole = 'Delegation' 
									    OR (
										    PO.OrganizationRole = 'Supplier' 
										    AND CAST(SUP.SupplierID AS NVARCHAR(20)) + ',' + CAST(PRIN.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] 
										    FROM dbo.fn_SplitStringToTable({1}, ';') tmp )
									    )";
                }
            }

            query = _dataQuery.GetQueryable<ShippedPurchaseOrderQueryModel>(sql, delegatedOrganizationId, supplierCustomerRelationships);
            return await query.ToDataSourceResultAsync(request);
        }

        #endregion Shipped purchase orders
    }
}
