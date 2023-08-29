using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Entities;
using static Groove.SP.Application.ApplicationBackgroundJob.SendMailBackgroundJobs;

namespace Groove.SP.Application.Attachment.Services.Interfaces
{
    public interface IAttachmentService : IServiceBase<AttachmentModel, AttachmentViewModel>
    {
        Task<AttachmentViewModel> GetAsync(long id);

        /// <summary>
        /// To get list of attachments by entity type: SHI, BOL, CTN, ...
        /// </summary>
        /// <param name="entityType">Refer to class EntityType</param>
        /// <param name="entityId">Id of entity</param>
        /// <param name="roleId">Role id. Please leave null/zero to get all available attachments, ignore attachment permissions</param>
        /// <returns></returns>
        Task<IEnumerable<AttachmentViewModel>> GetAttachmentsAsync(string entityType, long entityId, long? roleId = 0);

        /// <summary>
        /// Display all attachments of the related modules in each one.
        /// <br></br>
        /// Sample: when viewing Booking page, user can see all documents of Booking, Shipment, Container, House BL, Master BL if there are records associated.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task<IEnumerable<AttachmentViewModel>> GetAttachmentsCrossModuleAsync(string entityType, long entityId, long? roleId = 0, long? organizationId = 0);

        /// <summary>
        /// To import attachment into the system via (1) application GUI or (2) API
        /// </summary>
        /// <param name="viewModel">Data of attachment</param>
        /// <param name="verifyPermission">True to check by AttachmentTypePermission (upload attachment via application GUI). It should be false if triggered by system/work flow/third party.</param>
        /// <param name="roleId">Role id. Please leave null/zero to get all available attachments, ignore attachment permissions</param>
        /// <returns>Zero if no attachment added, else id of new attachment.</returns>
        Task<long> ImportAttachmentAsync(AttachmentViewModel viewModel, bool? verifyPermission = false, long? roleId = 0);

        /// <summary>
        /// To update attachment from application GUI <em>(Applied attachment permissions)</em>
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="roleId">Role id. Please leave null/zero to get all available attachments, ignore attachment permissions</param>
        /// <returns></returns>
        Task<long> UpdateAttachmentAsync(AttachmentViewModel viewModel, long? roleId = 0);

        /// <summary>
        /// To delete attachment from application GUI <em>(Applied attachment permissions)</em>
        /// <br></br>Remove attachment record after all links (GlobalIdAttachments) removed
        /// </summary>
        /// <param name="globalId">Global id</param>
        /// <param name="attachmentId">Attachment id</param>
        /// <param name="roleId">Role id. Please leave null/zero to get all available attachments, ignore attachment permissions</param>
        /// <returns></returns>
        Task<bool> DeleteAttachmentAsync(string globalId, long attachmentId, long? roleId = 0);

        /// <summary>
        /// To mark the current attachment is deleted
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task SoftDelete(long id);

        /// <summary>
        /// Get email attachments for NewShippingDocumentsNotification (Booking_NewShippingDocumentsNotificationToOriginAgent)<em>
        /// <list type="bullet">
        /// <item>All files of "Others" type</item>
        /// <item>The latest version of other types, except from Shipping Order form (Not send SO Form)</item>
        /// </list>
        /// <br></br>(Not applied attachment type permissions/classifications)</em>
        /// </summary>
        /// <param name="poffId">POFF Id</param>
        /// <returns></returns>
        Task<IEnumerable<SPEmailAttachment>> GetNewShippingEmailAttachmentsAsync(long poffId);

        /// <summary>
        /// To import attachment for sea house bill from CSED (House bill of lading and shipment)
        /// </summary>
        /// <param name="shippingDocument"></param>
        /// <param name="blobId"></param>
        /// <returns></returns>
        Task ImportCSEDSeaHouseBillAsync(CSEDShippingDocumentViewModel shippingDocument, string blobId);

        /// <summary>
        /// To import attachment for sea manifest from CSED (Container)
        /// </summary>
        /// <param name="shippingDocument"></param>
        /// <param name="blobId"></param>
        /// <returns></returns>
        Task ImportCSEDSeaManifestAsync(CSEDShippingDocumentViewModel shippingDocument, string blobId);

        /// <summary>
        /// To upload attachment for appropriate object entity: Shipment, Master Bill or House Bill. The attachment will be displayed on Attachment tab
        /// </summary>
        /// <param name="shippingDocument"></param>
        /// <param name="blobId"></param>
        /// <returns></returns>
        Task ImportCSEDAttachmentAsync(CSEDShippingDocumentViewModel shippingDocument, string blobId);

        /// <summary>
        /// To get accessible/allowed document types by current user role and entity type
        /// </summary>
        /// <param name="roleId">Role id. See more values at <see cref="Groove.SP.Core.Models.Role"/></param>
        /// <param name="entityType">Entity type. See more values at <see cref="Groove.SP.Core.Models.EntityType"/></param>
        /// <returns></returns>
        Task<string[]> GetAccessibleDocumentTypesAsync(long roleId, string entityType);

        Task<string[]> GetAccessibleDocumentTypesAsync(long roleId, string entityType, long entityId, long organizationId);
    }
}
