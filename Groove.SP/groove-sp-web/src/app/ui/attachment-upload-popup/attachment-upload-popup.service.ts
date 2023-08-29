import { Injectable } from '@angular/core';
import { HttpService, FormService, DropdownListModel, EntityType, StringHelper } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { AttachmentModel } from 'src/app/core/models/attachment.model';
import { toBase64String } from '@angular/compiler/src/output/source_map';

@Injectable()
export class AttachmentUploadPopupService extends FormService<AttachmentModel> {

    constructor(
        httpService: HttpService,
        private _translateService: TranslateService) {
        super(httpService, `${environment.apiUrl}/attachments`);
    }

    // Override methods
    public create(model: AttachmentModel): Observable<any> {
        // remove redundant data
        delete model.otherDocumentTypes;
        return this.httpService.create(`${this.apiUrl}`, model);
    }

    public update(id: number | string, model: AttachmentModel): Observable<any> {
        // remove redundant data
        delete model.otherDocumentTypes;
        return this.httpService.update(`${this.apiUrl}/${id}`, model);
    }

    /**
     * To delete attachment, applied attachment permissions + classifications.
     * Remove attachment record after all links (GlobalIdAttachments) removed </em>
     * @param globalId GlobalId of current entity. Ex: SHI_1, POF_2
     * @param attachmentId Id of current attachment
     */
    public deleteAttachment(globalId: string, attachmentId: number): Observable<any> {
        return this.httpService.delete(`${this.apiUrl}?globalId=${globalId}&attachmentId=${attachmentId}`);
    }

    public downloadFile(id, fileName) {
        return this.httpService.downloadFile(`${this.apiUrl}/${id}/download/${encodeURIComponent(fileName)}`, fileName);
    }

    downloadAttachments(
        fileName: string,
        selectedAttachments: Array<any>
    ) {
        if (!selectedAttachments || selectedAttachments.length <= 0) {
            return;
        }
        if (selectedAttachments.length === 1) {
            fileName = selectedAttachments[0].fileName.split('.')[0];
        }
        return this.httpService.downloadAttachments(fileName, selectedAttachments);
    }

    /**
     * To get document type options  accessible by current entity type (page) and role id.
     * Label of option is "label.attachmentType.[AttachmentType to lower case then remove all spaces]". Ex: Shipping Order Form -> label.attachmentType.shippingorderform
     * @param roleId role id
     * @param entityType refer to EntityType
     * @returns Observable<Array<DropdownListModel<string>>>
     * Incase the translate "of Label of option" unavailable, label will be equal to value.
     */
    public getAccessibleDocumentTypeOptions(roleId: number, entityType: EntityType, entityId: number): Observable<Array<DropdownListModel<string>>> {
        return this.httpService.get<string[]>(`${this.apiUrl}/attachmentTypes?entityType=${entityType}&roleId=${roleId}&entityId=${entityId}`)
            .map(
                (data: string[]) => {
                    return data.map(
                        (item: string) => {
                            return new DropdownListModel(item, item);
                        }
                    );
                }
            );
    }

    /**
     * To get translation for provided document type
     * @param documentType document type
     * @returns translation of inputted document type
     */
    public translateDocumentType(documentType: string): string {
        const translateKey = documentType.replace(/ /g, '').toLowerCase();
        const translateValue: string = this._translateService.instant(`label.attachmentType.${translateKey}`);
        return translateValue;
    }

    /**
     * To get translation for provided document level
     * @param documentLevel document level
     * @returns translation of inputted document level
     */
    public translateDocumentLevel(documentLevel: string): string {
        const translateKey = documentLevel.replace(/ /g, '').toLowerCase();
        const translateValue: string = this._translateService.instant(`label.documentLevel.${translateKey}`);
        return translateValue;
    }
}
