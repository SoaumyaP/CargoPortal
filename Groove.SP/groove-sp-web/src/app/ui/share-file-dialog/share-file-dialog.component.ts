import { Component, OnInit, ViewChild, ElementRef, Input } from '@angular/core';
import { HttpService, StringHelper } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { TranslateService } from '@ngx-translate/core';
import * as $ from 'jquery';
import * as bootstrap from 'bootstrap';
import { NotificationPopup } from '../notification-popup/notification-popup';
import { NgForm } from '@angular/forms';
import { TagInputComponent } from 'ngx-chips';
@Component({
    selector: 'app-share-file-dialog',
    templateUrl: './share-file-dialog.component.html',
    styleUrls: ['./share-file-dialog.component.scss']
})
export class ShareFileDialogComponent implements OnInit {
    @ViewChild('shareFileModal', { static: true }) shareFileModal: ElementRef;
    @ViewChild('tagInput', { static: true }) tagInputControl: TagInputComponent;

    public shareRequest = {
        mailingList: [],
        selectedAttachments: []
    };
    public mailingList: Array<any> = [];
    @Input('fileList') fileList: Array<any>;

    public selectedAttachments: Array<any> = [];
    public fileExtentions: Array<string> = ['doc', 'xls', 'ppt', 'pdf'];

    public errorMessages = {};

    constructor(private _httpService: HttpService, private translateService: TranslateService,
        public notification: NotificationPopup) { }
    ngOnInit() {
        this.selectedAttachments = this.fileList;
        this.translateService.stream('validation.invalidEmail').subscribe(msg => {
            this.errorMessages['invalidEmail'] = msg;
        });
        
        this.errorMessages['required'] = this.translateService.instant('validation.requiredThisField');
    }

    public checkFileName(fileName: string) {
        let result = '';
        if (fileName != null) {
            result = fileName.substr(fileName.lastIndexOf('.') + 1, fileName.length);
        }
        if (result.length > 3) {
            result = result.substr(0, 3);
        }
        if (this.fileExtentions.findIndex(x => x === result) === -1) {
            result = 'default';
        }
        return result;
    }

    public onClickRemove(fileId) {
        this.selectedAttachments = this.selectedAttachments.filter(x => x.id !== fileId);
    }

    public onCancel() {
        this.mailingList = [];
        this.selectedAttachments = this.fileList;
    }

    public validators(control) {
        if (StringHelper.isNullOrEmpty(control.value)) {
            return { 'required': true };
        }else {
            if (StringHelper.validateEmail(control.value)) {
                return null;
            }
            return { 'invalidEmail': true };
        }
    }

    isShowBorderError(){
        if ((this.tagInputControl.errors?.length > 0  && this.mailingList.length === 0) || (this.tagInputControl.errors?.length > 0 && this.tagInputControl.errors[0] !== this.translateService.instant('validation.requiredThisField'))) {
            return true;
        }
    }

    isShowRequiredError(){
        if (this.mailingList.length === 0 && this.tagInputControl.errors[0] === this.translateService.instant('validation.requiredThisField')) {
            return true;
        }
    }

    public onSubmit() {
        if (this.mailingList.length === 0 && !this.tagInputControl.inputTextValue && this.tagInputControl.errors.length === 0 ) {
            this.tagInputControl.errors[0] = this.translateService.instant('validation.requiredThisField');
        }

        if (this.mailingList.length === 0 || this.selectedAttachments.length === 0) {
            return;
        }

        this.shareRequest = {
            selectedAttachments: this.selectedAttachments,
            mailingList: this.mailingList.map(
                (item) => {
                    return item.value;
                })
        };
        if (this.shareRequest.mailingList.length !== 0 && this.shareRequest.selectedAttachments.length !== 0) {
            this._httpService.create(`${environment.apiUrl}/attachments/Share`, this.shareRequest).subscribe((success) => {
                this.notification.showSuccessPopup(
                    "msg.shareFileSuccessfully",
                    "label.shareFile"
                );
            }, (error) => {
                this.notification.showErrorPopup(
                    "msg.shareFileUnsuccessfully",
                    "label.shareFile"
                );
            });
            jQuery('#shareFileModal').modal('hide');
            this.onCancel();
        }
    }

    truncateFilename(filename: string, length: number) {
        if (!filename) {
            return '';
        }

        if (filename.length > length) {
            return `${filename.substring(0, length)}...`;
        }

        return filename;
    }
}
