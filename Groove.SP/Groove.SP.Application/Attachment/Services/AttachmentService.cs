using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using static Groove.SP.Application.ApplicationBackgroundJob.SendMailBackgroundJobs;

namespace Groove.SP.Application.Attachment.Services
{
    public class AttachmentService : ServiceBase<AttachmentModel, AttachmentViewModel>, IAttachmentService
    {
        private readonly IDataQuery _dataQuery;
        private readonly IBlobStorage _blobStorage;
        private readonly IRepository<ShipmentModel> _shipmentRepository;
        private readonly IRepository<BillOfLadingModel> _billOfLadingRepository;
        private readonly IRepository<MasterBillOfLadingModel> _masterBillOfLadingRepository;
        private readonly IRepository<AttachmentTypePermissionModel> _attachmentTypePermissionRepository;
        private readonly IRepository<AttachmentTypeClassificationModel> _attachmentTypeClassificationRepository;

        /// <summary>
        /// Ignore overriding attachment type if Other and Miscellaneous
        /// </summary>
        private readonly string[] _ignoreAttachmentTypes = new[] { AttachmentType.OTHERS, AttachmentType.MISCELLANEOUS };

        public AttachmentService(IUnitOfWorkProvider unitOfWorkProvider,
                                IDataQuery dataQuery,
                                IBlobStorage blobStorage,
                                IRepository<ShipmentModel> shipmentRepository,
                                IRepository<BillOfLadingModel> billOfLadingRepository,
                                IRepository<MasterBillOfLadingModel> masterBillOfLadingRepository,
                                IRepository<AttachmentTypePermissionModel> attachmentTypePermissionRepository,
                                IRepository<AttachmentTypeClassificationModel> attachmentTypeClassificationRepository)
        : base(unitOfWorkProvider)
        {
            _dataQuery = dataQuery;
            _blobStorage = blobStorage;
            _shipmentRepository = shipmentRepository;
            _billOfLadingRepository = billOfLadingRepository;
            _masterBillOfLadingRepository = masterBillOfLadingRepository;
            _attachmentTypePermissionRepository = attachmentTypePermissionRepository;
            _attachmentTypeClassificationRepository = attachmentTypeClassificationRepository;
        }

        public async Task<AttachmentViewModel> GetAsync(long id)
        {
            var model = await this.Repository.GetAsync(x => x.Id == id);
            var viewModel = Mapper.Map<AttachmentViewModel>(model);
            return viewModel;
        }

        public async Task<IEnumerable<AttachmentViewModel>> GetAttachmentsAsync(string entityType, long entityId, long? roleId = 0)
        {
            var globalId = CommonHelper.GenerateGlobalId(entityId, entityType);
            var attachmentQuery = Repository.Query(s => s.GlobalIdAttachments.Any(g => g.GlobalId == globalId), n => n.OrderByDescending(a => a.UploadedDate));
            var atpQuery = GetAccessibleDocumentTypesAsIQueryable(roleId.Value, entityType);
            var result = from attachment in attachmentQuery
                         where roleId == 0 || atpQuery.Any(s => s == attachment.AttachmentType)
                         select Mapper.Map<AttachmentViewModel>(attachment);
            return await result.ToListAsync();
        }

        public async Task<IEnumerable<AttachmentViewModel>> GetAttachmentsCrossModuleAsync(string entityType, long entityId, long? roleId = 0, long? organizationId = 0)
        {
            var storedProcedureName = "spu_GetAttachmentCrossModule";
            var parameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@roleId",
                        Value = roleId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@organizationId",
                        Value = organizationId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@poFulfillmentId",
                        Value = entityType == EntityType.POFullfillment ? entityId : 0,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@shipmentId",
                        Value = entityType == EntityType.Shipment ? entityId : 0,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@containerId",
                        Value = entityType == EntityType.Container ? entityId : 0,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@houseBLId",
                        Value = entityType == EntityType.BillOfLading ? entityId : 0,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@masterBLId",
                        Value = entityType == EntityType.MasterBill ? entityId : 0,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };
            Func<DbDataReader, IEnumerable<AttachmentViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<AttachmentViewModel>();

                // Map data for attachment information
                while (reader.Read())
                {
                    var newRow = new AttachmentViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];
                    tmpValue = reader[1];
                    newRow.FileName = tmpValue.ToString();
                    tmpValue = reader[2];
                    newRow.AttachmentType = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[3];
                    newRow.BlobId = tmpValue.ToString();
                    tmpValue = reader[4];
                    newRow.Description = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[5];
                    newRow.ReferenceNo = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[6];
                    newRow.UploadedBy = tmpValue.ToString();
                    tmpValue = reader[7];
                    newRow.UploadedDateTime = (DateTime)tmpValue;
                    tmpValue = reader[8];
                    newRow.CreatedDate = (DateTime)tmpValue;
                    tmpValue = reader[9];
                    newRow.CreatedBy = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[10];
                    newRow.UpdatedBy = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[11];
                    newRow.UpdatedDate = DBNull.Value == tmpValue ? (DateTime?)null : (DateTime)tmpValue;
                    tmpValue = reader[12];
                    switch (tmpValue.ToString())
                    {
                        case EntityType.POFullfillment:
                            newRow.DocumentLevel = DocumentLevel.POFulfillment;
                            break;
                        case EntityType.Shipment:
                            newRow.DocumentLevel = DocumentLevel.Shipment;
                            break;
                        case EntityType.Container:
                            newRow.DocumentLevel = DocumentLevel.Container;
                            break;
                        case EntityType.BillOfLading:
                            newRow.DocumentLevel = DocumentLevel.BillOfLading;
                            break;
                        case EntityType.MasterBill:
                            newRow.DocumentLevel = DocumentLevel.MasterBill;
                            break;
                        default:
                            newRow.DocumentLevel = string.Empty;
                            break;
                    }
                    tmpValue = reader[13];
                    newRow.Alias = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping, parameters.ToArray());
            return result;
        }

        public async Task<long> ImportAttachmentAsync(AttachmentViewModel viewModel, bool? verifyPermission = false, long? roleId = 0)
        {
            // Need to check on current attachment type by classifications and permissions
            if (verifyPermission.Value)
            {
                var entityType = viewModel.GetEntityType();
                var isAllowed = CheckAccessibleDocumentType(viewModel.AttachmentType, roleId.Value, entityType);
                if (!isAllowed)
                {
                    throw new ApplicationException("You are not authorized to upload document (classification/permission).");
                }
            }

            var utcNow = DateTime.UtcNow;
            var attachment = new AttachmentModel
            {
                Id = viewModel.Id,
                AttachmentType = viewModel.AttachmentType,
                ReferenceNo = viewModel.ReferenceNo,
                Description = viewModel.Description,
                UploadedBy = viewModel.UploadedBy,
                UploadedDate = viewModel.UploadedDateTime,
                FileName = viewModel.FileName,
                BlobId = viewModel.BlobId,
                CreatedDate = utcNow,
                GlobalIdAttachments = new List<GlobalIdAttachmentModel>()
                {
                    new GlobalIdAttachmentModel()
                    {
                        GlobalId = viewModel.GetGlobalId(),
                        AttachemntId = viewModel.Id,
                        CreatedDate = utcNow,
                        CreatedBy = viewModel.UploadedBy,
                        UpdatedDate = utcNow,
                        UpdatedBy = viewModel.UploadedBy
                    }
                }
            };

            // Ignore logic on attachment type re-upload if uploaded by System
            if (viewModel.UploadedBy != AppConstant.SYSTEM_USERNAME)
            {

                // check logic re-uploading on attachment type by uploaded by
                // get all attachments which are belonging to current globalId SHI_123, CTN_456
                var globalId = viewModel.GetGlobalId();
                var storedAttachments = Repository.QueryAsNoTracking(x => x.GlobalIdAttachments.Any(y => y.GlobalId == globalId), includes: x => x.Include(y => y.GlobalIdAttachments));

                // get other attachments which are same globalId + document type + reference number
                // ignore for Other and Miscellaneous
                var otherSameTypeAttachments = storedAttachments.Where(x => !_ignoreAttachmentTypes.Any(y => y == x.AttachmentType)
                                                                            && x.AttachmentType == viewModel.AttachmentType
                                                                            && (x.ReferenceNo ?? string.Empty) == (viewModel.ReferenceNo ?? string.Empty));

                // clone all linkages to current attachment
                if (otherSameTypeAttachments?.Count() > 0)
                {
                    attachment.GlobalIdAttachments = new List<GlobalIdAttachmentModel>();
                    foreach (var globalIdAttachment in otherSameTypeAttachments.SelectMany(x => x.GlobalIdAttachments).Distinct())
                    {
                        attachment.GlobalIdAttachments.Add(new GlobalIdAttachmentModel
                        {
                            AttachemntId = attachment.Id,
                            GlobalId = globalId,
                            CreatedDate = utcNow,
                            CreatedBy = attachment.UploadedBy,
                            UpdatedDate = utcNow,
                            UpdatedBy = attachment.UploadedBy
                        });
                    }

                    // After all linkages clones, remove others
                    Repository.RemoveRange(otherSameTypeAttachments.ToArray());
                }
            }

            await Repository.AddAsync(attachment);
            await UnitOfWork.SaveChangesAsync();

            return attachment.Id;
        }

        public async Task<long> UpdateAttachmentAsync(AttachmentViewModel viewModel, long? roleId = 0)
        {
            // get all attachments which are belonging to current globalId SHI_123, CTN_456
            var globalId = viewModel.GetGlobalId();
            var storedAttachments = Repository.QueryAsNoTracking(x => x.GlobalIdAttachments.Any(y => y.GlobalId == globalId), includes: x => x.Include(y => y.GlobalIdAttachments)).ToList();

            // current being saved attachment
            var currentStoredAttachment = storedAttachments.First(x => x.Id == viewModel.Id);

            // check permission to update attachment
            
            var entityType = viewModel.GetEntityType();           
            if (roleId > 0)
            {
                var accessibledDocumentTypes = await GetAccessibleDocumentTypesAsync(roleId.Value, entityType);
                // current and new values of document type must be accessible
                var isDocumentypeAccessible = accessibledDocumentTypes.Any(x => x.Equals(currentStoredAttachment.AttachmentType)) 
                                            && accessibledDocumentTypes.Any(x => x.Equals(viewModel.AttachmentType));
                if (!isDocumentypeAccessible)
                {
                    throw new ApplicationException("You are not authorized to update document (classification/permission).");
                }
            }

            Mapper.Map(viewModel, currentStoredAttachment);
            Repository.Update(currentStoredAttachment);

            // get other attachments which are same globalId + document type + uploaded by + not current editing attachment
            // ignore for Other and Miscellaneous
            var otherSameTypeAttachments = storedAttachments.Where(x => !_ignoreAttachmentTypes.Any(y => y == x.AttachmentType)
                                                                        && x.AttachmentType == viewModel.AttachmentType
                                                                        && (x.ReferenceNo ?? string.Empty) == (viewModel.ReferenceNo ?? string.Empty)
                                                                        && x.Id != currentStoredAttachment.Id);

            // clone all linkages to current attachment
            if (otherSameTypeAttachments?.Count() > 0)
            {
                if (currentStoredAttachment.GlobalIdAttachments != null) {
                    currentStoredAttachment.GlobalIdAttachments.Clear();
                }
                else
                {
                    currentStoredAttachment.GlobalIdAttachments = new List<GlobalIdAttachmentModel>();
                }

                // Need to commit to prevents concurrency exceptions
                Repository.Update(currentStoredAttachment);
                await UnitOfWork.SaveChangesAsync();

                foreach (var globalIdAttachment in otherSameTypeAttachments.SelectMany(x => x.GlobalIdAttachments).Distinct())
                {
                    var utcNow = DateTime.UtcNow;
                    currentStoredAttachment.GlobalIdAttachments.Add(new GlobalIdAttachmentModel
                    {
                        AttachemntId = currentStoredAttachment.Id,
                        GlobalId = globalId,
                        CreatedDate = utcNow,
                        CreatedBy = currentStoredAttachment.UploadedBy,
                        UpdatedDate = utcNow,
                        UpdatedBy = currentStoredAttachment.UploadedBy
                    });
                }
                

                // After all linkages clones, remove others
                Repository.RemoveRange(otherSameTypeAttachments.ToArray());
            }

            await UnitOfWork.SaveChangesAsync();

            return currentStoredAttachment.Id;
        }

        public async Task<bool> DeleteAttachmentAsync(string globalId, long attachmentId, long? roleId = 0)
        {
            var storedDocument = await Repository.GetAsync(x => x.GlobalIdAttachments.Any(y => y.GlobalId == globalId && y.AttachemntId == attachmentId), 
                                includes: x => x.Include(y => y.GlobalIdAttachments));
            
            if (storedDocument == null)
            {
                throw new AppEntityNotFoundException($"Object with the globalId {globalId} and  attachmentId {attachmentId} not found!");
            }

            bool accessibledDocumentTypes = false;
            // check permission to remove attachment (attachment type classification/permission)
            if (roleId > 0)
            {
                var entityType = CommonHelper.GetGlobalIdParts(globalId).Type;                
                accessibledDocumentTypes = CheckAccessibleDocumentType(storedDocument.AttachmentType, roleId.Value, entityType);
            }
            if (roleId == 0 || accessibledDocumentTypes)
            {
                var globalIdAttachment = storedDocument.GlobalIdAttachments.First(x => x.GlobalId == globalId && x.AttachemntId == attachmentId);

                var needRemoveAzureFiles = false;
                // If there is the last GlobalIdAttachment, remove attachment record               
                if (storedDocument.GlobalIdAttachments.Count() == 1)
                {
                    needRemoveAzureFiles = true;
                    Repository.Remove(storedDocument);                    
                }
                // Else, just remove the current link GlobalIdAttachment
                else
                {
                    storedDocument.GlobalIdAttachments.Remove(globalIdAttachment);
                }
                await UnitOfWork.SaveChangesAsync();

                if (needRemoveAzureFiles)
                {
                    // Only remove from Azure blobs if the file is not in use anymore
                    await _blobStorage.DeleteBlobAsync(storedDocument.BlobId);
                }

                return true;
            }
            throw new ApplicationException("You are not authorized to remove document.");
        }


        public async Task SoftDelete(long id)
        {
            var model = await this.Repository.FindAsync(id);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            model.IsDeleted = true;

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            this.OnEntityUpdated(model);
        }

        public async Task<IEnumerable<SPEmailAttachment>> GetNewShippingEmailAttachmentsAsync(long poffId)
        {
            var result = new List<SPEmailAttachment>();
            var attachments = await GetAttachmentsAsync(EntityType.POFullfillment, poffId);
            if(attachments == null || !attachments.Any())
            {
                return result;
            }

            // Ignore Shipping Order Form
            attachments = attachments.Where(x => x.AttachmentType != AttachmentType.SHIPPING_ORDER_FORM).ToList();

            if (attachments == null || !attachments.Any())
            {
                return result;
            }

            var filteredAttachments = new List<AttachmentViewModel>();

            var notOthers = attachments.Where(x => x.AttachmentType != AttachmentType.OTHERS).GroupBy(x => x.AttachmentType).Select(x => x.OrderByDescending(y => y.UploadedDateTime).FirstOrDefault()).ToList();
            var others = attachments.Where(x => x.AttachmentType == AttachmentType.OTHERS);
                
            if (notOthers != null && notOthers.Any())
            {
                filteredAttachments.AddRange(notOthers);
            }
            if (others != null && others.Any())
            {
                filteredAttachments.AddRange(others);
            }
            if (!filteredAttachments.Any())
            {
                return result;
            }

            foreach (var attachment in filteredAttachments)
            {
                var content = await _blobStorage.GetBlobAsByteArrayAsync(attachment.BlobId);
                result.Add(new SPEmailAttachment
                {
                    AttachmentName = attachment.FileName,
                    AttachmentContent = content
                });
            }
            return result;
            
        }

        public async Task ImportCSEDSeaHouseBillAsync(CSEDShippingDocumentViewModel shippingDocument, string blobId)
        {
            // check if document id existed
            // if yes, not store new attachment record
            var filteringBlobId = _blobStorage.GetCSEDDocumentBlobFilter(blobId);
            var storedAttachment = await Repository.GetAsync(x => x.AttachmentType == AttachmentType.HOUSE_BL && EF.Functions.Like(x.BlobId, filteringBlobId), includes: x => x.Include(y => y.GlobalIdAttachments));
            if (storedAttachment == null)
            {
                storedAttachment = new AttachmentModel
                {
                    ReferenceNo = shippingDocument.documentCode,
                    AttachmentType = AttachmentType.HOUSE_BL,

                    FileName = shippingDocument.documentName,
                    BlobId = blobId,

                    // fulfill some audit information
                    CreatedBy = shippingDocument.uploadBy,
                    CreatedDate = shippingDocument.createdDate,
                    UpdatedBy = shippingDocument.uploadBy,
                    UpdatedDate = shippingDocument.createdDate,
                    UploadedBy = shippingDocument.uploadBy,
                    UploadedDate = shippingDocument.createdDate
                };
            }

            // try to get linked house bill of lading and shipment
            var houseBL = await _billOfLadingRepository.GetAsNoTrackingAsync(x => x.BillOfLadingNo == shippingDocument.documentCode);
            var shipment = await _shipmentRepository.GetAsNoTrackingAsync(x => x.ShipmentNo == shippingDocument.blShipmentNumber);

            if (houseBL == null || shipment == null)
            {
                throw new ApplicationException($"Cannot import as either House BL '{shippingDocument.documentCode}' or Shipment '{shippingDocument.blShipmentNumber}' not available.");
            }

            if (storedAttachment.GlobalIdAttachments == null)
            {
                storedAttachment.GlobalIdAttachments = new List<GlobalIdAttachmentModel>();
            }

            var storedGlobalIds = storedAttachment.GlobalIdAttachments.Select(x => x.GlobalId);

            if (houseBL != null)
            {
                var globalId = CommonHelper.GenerateGlobalId(houseBL.Id, EntityType.BillOfLading);
                // ignore if already stored
                if (!storedGlobalIds.Contains(globalId)) {
                    storedAttachment.GlobalIdAttachments.Add(new GlobalIdAttachmentModel()
                    {
                        GlobalId = globalId,
                        CreatedDate = shippingDocument.createdDate,
                        CreatedBy = shippingDocument.uploadBy,
                        UpdatedDate = shippingDocument.createdDate,
                        UpdatedBy = shippingDocument.uploadBy
                    });
                }
            }

            if (shipment != null)
            {
                var globalId = CommonHelper.GenerateGlobalId(shipment.Id, EntityType.Shipment);
                // ignore if already stored
                if (!storedGlobalIds.Contains(globalId))
                {
                    storedAttachment.GlobalIdAttachments.Add(new GlobalIdAttachmentModel()
                    {
                        GlobalId = globalId,
                        CreatedDate = shippingDocument.createdDate,
                        CreatedBy = shippingDocument.uploadBy,
                        UpdatedDate = shippingDocument.createdDate,
                        UpdatedBy = shippingDocument.uploadBy
                    });
                }
            }

            // create new
            if (storedAttachment.Id == 0)
            {
                await Repository.AddAsync(storedAttachment);

            }
            // update current
            else
            {
                Repository.Update(storedAttachment);
            }
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task ImportCSEDSeaManifestAsync(CSEDShippingDocumentViewModel shippingDocument, string blobId)
        {           
            var sql = $@"
                            SELECT DISTINCT CON.Id AS [Id]
                            FROM Containers CON
                            WHERE 
	                            CON.ContainerNo = @containerNumber
	                            AND 
		                            (
			                            -- Non-direct
			                            -- FCL
			                            EXISTS (
				                            SELECT 1
				                            FROM BillOfLadingShipmentLoads BSL
				                            INNER JOIN MasterBills MB ON BSL.MasterBillOfLadingId = MB.Id
				                            WHERE	BSL.ContainerId = CON.Id						
						                            AND CON.IsFCL = 1
						                            AND MB.MasterBillOfLadingNo = @masterBLNumber
			                            )
			                            -- Non-direct
			                            -- Not FCL
			                            OR EXISTS (
				                            SELECT 1
				                            FROM Consolidations CSO
				                            INNER JOIN BillOfLadingShipmentLoads BSL ON CSO.Id = BSL.ConsolidationId
				                            INNER JOIN MasterBills MB ON BSL.MasterBillOfLadingId = MB.Id
				                            WHERE	BSL.ContainerId = CON.Id
						                            AND CON.IsFCL = 0
						                            AND MB.MasterBillOfLadingNo = @masterBLNumber
			                            )
			                            -- Direct
			                            OR EXISTS (
				                            SELECT 1
				                            FROM ShipmentLoads SHL
				                            INNER JOIN Consignments CSM ON SHL.ConsignmentId = CSM.Id
				                            INNER JOIN MasterBills MB ON CSM.MasterBillId = MB.Id
				                            WHERE	SHL.ContainerId = CON.Id 
						                            AND MB.MasterBillOfLadingNo = @masterBLNumber
			                            ) 
		                            ) 
            ";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@containerNumber",
                        Value = shippingDocument.documentCode,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@masterBLNumber",
                        Value = shippingDocument.oceanBlNumber,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                };

            Func<DbDataReader, IEnumerable<long>> mapping = (reader) =>
            {
                var mappedData = new List<long>();

                while (reader.Read())
                {
                    var newRow = (long)reader[0];
                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var containerIds = _dataQuery.GetDataBySql(sql, mapping, filterParameters.ToArray());

            // try to link to appropriate containers
            if (containerIds == null || !containerIds.Any())
            {
                throw new ApplicationException($"Cannot import as either Container '{shippingDocument.documentCode}' or Master BL '{shippingDocument.oceanBlNumber}' not available.");
            }

            // check if document id existed
            // if yes, not store new attachment record
            var filteringBlobId = _blobStorage.GetCSEDDocumentBlobFilter(blobId);
            var storedAttachment = await Repository.GetAsync(x => x.AttachmentType == AttachmentType.MANIFEST && EF.Functions.Like(x.BlobId, filteringBlobId), includes: x => x.Include(y => y.GlobalIdAttachments));
            if (storedAttachment == null)
            {
                storedAttachment = new AttachmentModel
                {
                    ReferenceNo = shippingDocument.documentCode,
                    AttachmentType = AttachmentType.MANIFEST,

                    FileName = shippingDocument.documentName,
                    BlobId = blobId,

                    // fulfill some audit information
                    CreatedBy = shippingDocument.uploadBy,
                    CreatedDate = shippingDocument.createdDate,
                    UpdatedBy = shippingDocument.uploadBy,
                    UpdatedDate = shippingDocument.createdDate,
                    UploadedBy = shippingDocument.uploadBy,
                    UploadedDate = shippingDocument.createdDate
                };
            }

            if (storedAttachment.GlobalIdAttachments == null)
            {
                storedAttachment.GlobalIdAttachments = new List<GlobalIdAttachmentModel>();
            }

            var storedGlobalIds = storedAttachment.GlobalIdAttachments.Select(x => x.GlobalId);

            foreach (var containerId in containerIds)
            {
                var globalId = CommonHelper.GenerateGlobalId(containerId, EntityType.Container);
                // ignore if already stored
                if (!storedGlobalIds.Contains(globalId))
                {
                    storedAttachment.GlobalIdAttachments.Add(new GlobalIdAttachmentModel()
                    {
                        GlobalId = globalId,
                        CreatedDate = shippingDocument.createdDate,
                        CreatedBy = shippingDocument.uploadBy,
                        UpdatedDate = shippingDocument.createdDate,
                        UpdatedBy = shippingDocument.uploadBy
                    });
                }
            }

            // create new
            if (storedAttachment.Id == 0)
            {
                await Repository.AddAsync(storedAttachment);

            }
            // update current
            else
            {
                Repository.Update(storedAttachment);
            }
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task ImportCSEDAttachmentAsync(CSEDShippingDocumentViewModel shippingDocument, string blobId)
        {
            var utcNow = DateTime.UtcNow;
            string attachmentType;
            if (CSEDShippingDocumentDocumentSubType.ValueMapping.TryGetValue(shippingDocument.documentSubType, out attachmentType))
            {

            } else
            {
                throw new ApplicationException($"Cannot import as attachment type for documentSubType: '{shippingDocument.documentSubType}' not available.");
            }

            string globalId = "";
            if (CSEDShippingDocumentBillType.OceanBill.Equals(shippingDocument.billType, StringComparison.OrdinalIgnoreCase))
            {
                var masterBL = await _masterBillOfLadingRepository.GetAsNoTrackingAsync(x => x.MasterBillOfLadingNo == shippingDocument.documentCode);
                if (masterBL != null)
                {
                    globalId = CommonHelper.GenerateGlobalId(masterBL.Id, EntityType.MasterBill);
                }
            }
            else if (CSEDShippingDocumentBillType.HouseBill.Equals(shippingDocument.billType, StringComparison.OrdinalIgnoreCase))
            {
                var houseBL = await _billOfLadingRepository.GetAsNoTrackingAsync(x => x.BillOfLadingNo == shippingDocument.documentCode);
                if (houseBL != null)
                {
                    globalId = CommonHelper.GenerateGlobalId(houseBL.Id, EntityType.BillOfLading);
                }
            }
            else if (CSEDShippingDocumentBillType.ShippingOrder.Equals(shippingDocument.billType, StringComparison.OrdinalIgnoreCase))
            {
                var shipment = await _shipmentRepository.GetAsNoTrackingAsync(x => x.ShipmentNo == shippingDocument.documentCode);
                if (shipment != null)
                {
                    globalId = CommonHelper.GenerateGlobalId(shipment.Id, EntityType.Shipment);
                }
            }
           
            if (string.IsNullOrEmpty(globalId))
            {
                throw new ApplicationException($"Cannot import as document code: '{shippingDocument.documentCode}' of bill type: '{shippingDocument.billType}' not available.");
            }

            var attachment = new AttachmentModel
            {
                AttachmentType = attachmentType,
                ReferenceNo = null,
                Description = null,
                FileName = shippingDocument.documentName,
                BlobId = blobId,

                // fulfill some audit information
                CreatedBy = shippingDocument.uploadBy,
                CreatedDate = shippingDocument.createdDate,
                UpdatedBy = shippingDocument.uploadBy,
                UpdatedDate = shippingDocument.createdDate,
                UploadedBy = shippingDocument.uploadBy,
                UploadedDate = shippingDocument.createdDate,

                GlobalIdAttachments = new List<GlobalIdAttachmentModel>()
                {
                    new GlobalIdAttachmentModel()
                    {
                        GlobalId = globalId,
                        CreatedDate = utcNow,
                        CreatedBy = AppConstant.SYSTEM_USERNAME,
                        UpdatedDate = utcNow,
                        UpdatedBy = AppConstant.SYSTEM_USERNAME
                    }
                }
            };

            // check logic re-uploading on attachment type by uploaded by
            // get all attachments which are belonging to current globalId SHI_123, CTN_456
            var storedAttachments = Repository.QueryAsNoTracking(x => x.GlobalIdAttachments.Any(y => y.GlobalId == globalId), includes: x => x.Include(y => y.GlobalIdAttachments));

            // get other attachments which are same globalId + document type + reference number
            // ignore for Other and Miscellaneous
            var otherSameTypeAttachments = storedAttachments.Where(x => !_ignoreAttachmentTypes.Any(y => y == x.AttachmentType)
                                                                        && x.AttachmentType == attachmentType
                                                                        && (x.ReferenceNo ?? string.Empty) == string.Empty);

            // clone all linkages to current attachment
            if (otherSameTypeAttachments?.Count() > 0)
            {
                attachment.GlobalIdAttachments = new List<GlobalIdAttachmentModel>();
                foreach (var globalIdAttachment in otherSameTypeAttachments.SelectMany(x => x.GlobalIdAttachments).Distinct())
                {
                    attachment.GlobalIdAttachments.Add(new GlobalIdAttachmentModel
                    {
                        AttachemntId = attachment.Id,
                        GlobalId = globalId,
                        CreatedDate = utcNow,
                        CreatedBy = attachment.UploadedBy,
                        UpdatedDate = utcNow,
                        UpdatedBy = attachment.UploadedBy
                    });
                }

                // After all linkages clones, remove others
                Repository.RemoveRange(otherSameTypeAttachments.ToArray());
            }            

            await Repository.AddAsync(attachment);
            await UnitOfWork.SaveChangesAsync();

        }

        public async Task<string[]> GetAccessibleDocumentTypesAsync(long roleId, string entityType, long entityId, long organizationId)
        {
            var filterParameter = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@roleId",
                        Value = roleId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@organizationId",
                        Value = organizationId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@entityType",
                        Value = entityType,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                    new SqlParameter
                    {
                        ParameterName = "@entityId",
                        Value = entityId,
                        DbType = DbType.Int64,
                        Direction = ParameterDirection.Input
                    }
                };

            string[] mappingCallback(DbDataReader reader)
            {
                var attachmentTypes = new List<string>();
                while (reader.Read())
                {
                    var attachmentType = reader[0] as string;
                    attachmentTypes.Add(attachmentType);
                }
                return attachmentTypes.ToArray();
            }

            var data = await _dataQuery.GetDataByStoredProcedureAsync("spu_GetAccessibleDocumentTypes", mappingCallback, filterParameter.ToArray());

            return data;
        }
        /// <summary>
        /// To get accessible document types <em>(ordered by <see cref="AttachmentTypeClassificationModel.Order"/>)</em>
        /// </summary>
        /// <param name="roleId">Role id. See more values at <see cref="Groove.SP.Core.Models.Role"/></param>
        /// <param name="entityType">Entity type. See more values at <see cref="Groove.SP.Core.Models.EntityType"/></param>
        /// <returns></returns>
        public Task<string[]> GetAccessibleDocumentTypesAsync(long roleId, string entityType)
        {
            var query = GetAccessibleDocumentTypesAsIQueryable(roleId, entityType);
            var result = query.ToArrayAsync();
            return result;
        }

        /// <summary>
        /// To get IQueryable of accessible document types <em>(ordered by <see cref="AttachmentTypeClassificationModel.Order"/>)</em>
        /// </summary>
        /// <param name="roleId">Role id. See more values at <see cref="Groove.SP.Core.Models.Role"/></param>
        /// <param name="entityType">Entity type. See more values at <see cref="Groove.SP.Core.Models.EntityType"/></param>
        /// <returns></returns>
        protected IQueryable<string> GetAccessibleDocumentTypesAsIQueryable(long roleId, string entityType)
        {
            var atpQuery = _attachmentTypePermissionRepository.Query(s => s.RoleId == roleId).Select(x => x.AttachmentType);

            // Order by AttachmentTypeClassification.Order
            var atcQuery = _attachmentTypeClassificationRepository.Query(s => s.EntityType == entityType, orderBy: x=>x.OrderBy(y=>y.Order));
            var result = atcQuery.Where(x => atpQuery.Contains(x.AttachmentType)).Select(x => x.AttachmentType);
            return result;
        }

        /// <summary>
        /// To check whether current document type is accessible by current user role id and entity type
        /// </summary>
        /// <param name="documentType">Document/attachment type</param>
        /// <param name="roleId">Role id. See more values at <see cref="Groove.SP.Core.Models.Role"/></param>
        /// <param name="entityType">Entity type. See more values at <see cref="Groove.SP.Core.Models.EntityType"/></param>
        /// <returns></returns>
        protected bool CheckAccessibleDocumentType(string documentType, long roleId, string entityType)
        {
            // Check on role id
            var atpQuery = _attachmentTypePermissionRepository.Query(s => s.RoleId == roleId).Select(x => x.AttachmentType);

            // Check document type and entity type
            var atcQuery = _attachmentTypeClassificationRepository.Query(s => s.EntityType == entityType && s.AttachmentType == documentType).Select(x => x.AttachmentType);
            var result = atpQuery.Any(x => atcQuery.Any(y => y == x));
            return result;
        }


    }
}
