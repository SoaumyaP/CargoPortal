import { Component, Input, Output, EventEmitter, OnInit, OnChanges } from '@angular/core';
import { FileRestrictions, SuccessEvent, UploadEvent, FileState, SelectEvent } from '@progress/kendo-angular-upload';
import { process, State } from '@progress/kendo-data-query';
import { GridDataResult, DataStateChangeEvent } from '@progress/kendo-angular-grid';
import { TranslateService } from '@ngx-translate/core';
import { BackgroundJobMonitor } from '../background-job/background-job';
import { HttpService, StringHelper } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { faDownload } from '@fortawesome/free-solid-svg-icons';
import { Separator } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-import-form',
    templateUrl: './import-form.component.html',
    styleUrls: ['./import-form.component.scss']
})
export class ImportFormComponent implements OnInit, OnChanges {
    status: string;
    statusCode: number;
    processing: boolean = null;
    errorLogs: Array<any> = <any>[];
    importStep: number;
    widthPopup: number;
    heightPopup: number;
    isDropEnteredFile: boolean = false;
    ImportStepState = {
        Selecting: 0,
        Selected: 1,
        Importing: 2,
        ShowError: 3,
        Done: 4
    };

    faDownload = faDownload;

    /**Current import progress id. */
    bjId: number;

    importRestrictions: FileRestrictions = {
        allowedExtensions: ['.xlsx']
    };

    isNotAllow = false;
    isLimitedItem = false;

    private state: State = {
        skip: 0,
        take: 5
    };

    private totalSteps: number;
    private completedSteps: number;
    private progressTitle: string;
    private selectedFileName = '';
    private gridData: GridDataResult = <any>[];

    @Input() public importFormOpened: boolean = false;
    @Input() public uploadSaveUrl: string = '';
    @Input() public templateUrl: string = '';
    @Input() public templateName: string = '';
    @Input() public importNote = '';
    @Output() close: EventEmitter<any> = new EventEmitter();

    constructor(private translation: TranslateService,
        private httpService: HttpService,
        private backgroundJobMonitor: BackgroundJobMonitor) {
    }

    ngOnInit() {
        this.status = this.translation.instant('label.processing');
        this.updateStyle(this.ImportStepState.Selecting);
        this.completedSteps = 100;
        this.processing = true;
    }
    ngOnChanges() {
        if (this.importFormOpened === true) {
            this.isDropEnteredFile = false;
        }
    }

    get isSelectingFile() {
        return this.importStep === this.ImportStepState.Selecting;
    }

    get isSelectedFile() {
        return this.importStep === this.ImportStepState.Selected;
    }

    get isImporting() {
        return this.importStep === this.ImportStepState.Importing;
    }

    get isShowError() {
        return this.importStep === this.ImportStepState.ShowError;
    }

    get isDone() {
        return this.importStep === this.ImportStepState.Done;
    }

    updateStyle(state) {
        this.importStep = state;
        const dialog: HTMLCollectionOf<Element> = document.getElementsByClassName('import-dialog');
        const dialogEl = dialog[0] as HTMLElement;
        switch (this.importStep) {
            case this.ImportStepState.ShowError:
                this.widthPopup = 700;
                this.heightPopup = 710;
                // error color
                const processBars: HTMLCollectionOf<Element> = document.getElementsByClassName('progress-bar');
                if (processBars.length > 0) {
                    const processBar = processBars[0] as HTMLElement;
                    processBar.style.backgroundColor = '#F0023A';
                }

                dialogEl.classList.add('max-screen-1280x768');
                break;
            default:
                this.widthPopup = 610;
                this.heightPopup = 400;
                dialogEl?.classList.remove('max-screen-1280x768');
                break;
        }
    }

    onImport() {
        const uploadButtons: HTMLCollectionOf<Element> = document.getElementsByClassName('k-upload-selected');
        if (uploadButtons.length > 0) {
            const uploadButton = uploadButtons[0] as HTMLElement;
            uploadButton.click();
            this.updateStyle(this.ImportStepState.Importing);
        }
    }

    downloadErrors(): void {
        const url = `${environment.apiUrl}/importDataProgress/${this.bjId}/downloadPOErrors?lang=${this.translation.currentLang}`;
        this.httpService.downloadFile(url, "ErrorList.xlsx").subscribe();
    }

    protected dataStateChange(state: DataStateChangeEvent): void {
        this.state = state;
        this.gridData = process(this.errorLogs, this.state);
    }

    uploadEventHandler(e: UploadEvent) {
        this.errorLogs = [];
        this.gridData = process(this.errorLogs, this.state);
    }

    uploadSuccess(event: SuccessEvent) {
        const rs = event.response;
        this.bjId = rs.body;

        // monitor background job setup
        this.backgroundJobMonitor.monitor(this.bjId);

        this.backgroundJobMonitor.onTimeout = () => {
            this.status = this.translation.instant('timeout');
            this.statusCode = 4;
            this.processing = false;
        };

        this.backgroundJobMonitor.onFinished = (data) => {
            this.status = this.translateLabel(data.result);
            this.statusCode = data.status;
            this.processing = false;
            this.completedSteps = this.totalSteps;

            try {
                if (!StringHelper.isNullOrEmpty(data.log)) {
                    if (data.log === 'LIMIT_ITEM') {
                        this.updateStyle(this.ImportStepState.Selected);
                        this.isLimitedItem = true;
                    } else {
                        this.updateStyle(this.ImportStepState.ShowError);
                    }
                } else {
                    this.updateStyle(this.ImportStepState.Done);
                }
                this.errorLogs = JSON.parse(data.log);
                this.gridData = process(this.errorLogs, this.state);
            } catch (ex) {
                console.log(ex);
            }
        };

        this.backgroundJobMonitor.onProgressChanged = (data) => {
            this.totalSteps = data.totalSteps;
            this.completedSteps = data.completedSteps;
            this.progressTitle = `${this.completedSteps} / ${this.totalSteps} ${this.translation.instant('label.rows')}`;
            this.status = this.translation.instant('label.processing');
            this.statusCode = data.status;
            this.processing = true;
        };

        this.backgroundJobMonitor.onError = (error) => {
            this.status = error.errorMessage;
            this.statusCode = 5;
            this.processing = false;
        };
    }

    translateLabel(text: string): string {      
        let result = '';
        
        if (StringHelper.isNullOrWhiteSpace(text)) {
            return result;
        }

        let translatedText = [];
        let splitText = text.split(Separator.Semicolon);
        for (let i = 0; i < splitText.length; i++) {
            let el = splitText[i];
            translatedText.push(this.translation.instant(el));
        }
        const WHITE_SPACE = ' ';
        return translatedText.join(WHITE_SPACE);
    }

    uploadError(event: SuccessEvent) {
        console.log(event);
    }

    onFormClosed() {
        this.initPopup();
        this.importFormOpened = false;
        // Emit event with value imported successfully
        this.close.emit(this.statusCode === 2);
    }

    initPopup() {
        this.status = this.translation.instant('label.processing');
        this.updateStyle(this.ImportStepState.Selecting);
        this.gridData = null;
        this.totalSteps = 0;
        this.selectedFileName = '';
        this.completedSteps = 0;
        this.progressTitle = '';
        this.processing = null;
    }

    onSelectFile() {
        const uploadFile: HTMLElement = document.querySelector('input[kendofileselect]');
        uploadFile.click();
    }

    public showButton(state: FileState): boolean {
        return (state === FileState.Uploaded) ? true : false;
    }

    selectEventHandler(e: SelectEvent) {
        this.isLimitedItem = false;
        this.isNotAllow = e.files[0].validationErrors && e.files[0].validationErrors.length > 0;
        this.selectedFileName =  e.files[0].name;
        this.updateStyle(this.ImportStepState.Selected);
    }
}
