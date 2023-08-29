import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { DataStateChangeEvent, GridDataResult } from '@progress/kendo-angular-grid';
import { FileRestrictions, SelectEvent, SuccessEvent, UploadEvent } from '@progress/kendo-angular-upload';
import { process, State } from '@progress/kendo-data-query';
import { HttpService, StringHelper } from 'src/app/core';
import { BackgroundJobMonitor } from '../background-job/background-job';
import { NotificationPopup } from '../notification-popup/notification-popup';
import { faDownload } from '@fortawesome/free-solid-svg-icons';
import { environment } from 'src/environments/environment';
import { Separator } from 'src/app/core/models/constants/app-constants';

@Component({
  selector: 'app-two-step-import-data-popup',
  templateUrl: './two-step-import-data-popup.component.html',
  styleUrls: ['./two-step-import-data-popup.component.scss']
})
export class TwoStepImportDataPopupComponent implements OnInit, OnChanges {

  ImportDataProgressStatus = ImportDataProgressStatus;
  statusDescription: string;
  /** Indicate back-end server import data progress status*/
  importDataProcessStatus: ImportDataProgressStatus;
  importDataProgressLogs: Array<any> = <any>[];
  /** Indicate GUI popup step */
  currentImportStep: TwoImportDataStep;
  widthPopup: number;
  heightPopup: number;
  
  faDownload = faDownload;

  /** Uses drag - drop file to upload */
  isDropEnteredFile: boolean = false;

  /** Indicate if it should display log table */
  isShowLogTableDetails: boolean = false;

  /** File extension restrictions */
  importFileExtensionRestrictions: FileRestrictions = {
    allowedExtensions: []
  };
  isFileExtensionForbidden = false;

  /** State of log table */
  state: State = {
    skip: 0,
    take: 5
  };

  /** Progress values */
  overallProgress: number;
  completedProgress: number;
  /** Value will be blank always as no need to display yet */
  progressTitle: string;
  fileName = '';
  selectedFile: any;

  /**Current import progress id. */
  bjId: number;

  /** Data for log table */
  gridData: GridDataResult = <any>[];

  /** Back-end server API to handle file uploading */
  @Input() public uploadValidateUrl: string = '';

  @Input() public uploadSaveUrl: string = '';

  @Input() public popupTitle: string = 'label.importData';

  /** Acceptable file extensions */
  @Input() public allowedFileExtensions: Array<string> = [];
  /** Some notes for popup that will display at the bottom */
  @Input() public popupNote: string = '';

  /** Event callback as closing popup*/
  @Output() close: EventEmitter<any> = new EventEmitter();

  constructor(private translation: TranslateService,
    private backgroundJobMonitor: BackgroundJobMonitor,
    private httpClient: HttpClient,
    private httpService: HttpService,
    private notification: NotificationPopup) {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.importFileExtensionRestrictions = {
      allowedExtensions: this.allowedFileExtensions,
    };
  }

  ngOnInit() {
    this.statusDescription = '...';
    this._updatePopupStyle(TwoImportDataStep.Initial);
  }

  get isSelectingFile() {
    return this.currentImportStep === TwoImportDataStep.Initial;
  }

  get isSelectedFile() {
    return this.currentImportStep === TwoImportDataStep.FileSelected;
  }

  get isValidating() {
    return this.currentImportStep === TwoImportDataStep.Validating;
  }

  get isDone() {
    return this.currentImportStep === TwoImportDataStep.Done;
  }

  get isDoneSuccess() {
    return this.isDone && this.importDataProcessStatus === ImportDataProgressStatus.Success;
  }

  get isDoneNotSuccess() {
    return this.isDone && this.importDataProcessStatus !== ImportDataProgressStatus.Success;
  }

  private _updatePopupStyle(step: TwoImportDataStep, isShowLogs?: boolean) {
    this.currentImportStep = step;
    const dialog: HTMLCollectionOf<Element> = document.getElementsByClassName('two-step-import-data-dialog');
    const dialogEl = dialog[0] as HTMLElement;
    switch (this.currentImportStep) {
      default:
        this.widthPopup = 610;
        this.heightPopup = 400;
        break;
    }

    if (isShowLogs) {
      this.widthPopup = 700;
      this.heightPopup = 610;
      dialogEl.classList.add('max-screen-1280x768');
    } else {
      dialogEl?.classList.remove('max-screen-1280x768');

    }
  }

  dataStateChange(state: DataStateChangeEvent): void {
    this.state = state;
    this.gridData = process(this.importDataProgressLogs, this.state);
  }

  uploadEventHandler(e: UploadEvent) {
    this.importDataProgressLogs = [];
    this.gridData = process(this.importDataProgressLogs, this.state);
  }

  uploadSuccessHandler(event: SuccessEvent) {
    const rs = event.response;
    this.bjId = rs.body;

    // monitor background job setup
    this.backgroundJobMonitor.monitor(this.bjId);

    this.backgroundJobMonitor.onTimeout = () => {
      this.statusDescription = this.translation.instant('timeout');
      this.importDataProcessStatus = 4;
    };

    // Fire as import data progress is in success/failed/aborted/warning from server
    this.backgroundJobMonitor.onFinished = (data) => {
      this.statusDescription = this.translateLabel(data.result);
      this.importDataProcessStatus = data.status;
      this.currentImportStep = TwoImportDataStep.Done;

      try {
        const needToShowLogDetails = !StringHelper.isNullOrEmpty(data.log);
        this.isShowLogTableDetails = needToShowLogDetails;
        this._updatePopupStyle(TwoImportDataStep.Done, needToShowLogDetails);
        this.importDataProgressLogs = JSON.parse(data.log);
        this.gridData = process(this.importDataProgressLogs, this.state);

      } catch (ex) {
        console.log(ex);
      }
    };

    // Frequently check status of import data progress from server
    this.backgroundJobMonitor.onProgressChanged = (data) => {
      this.overallProgress = data.totalSteps;
      this.completedProgress = data.completedSteps;
      // if (this.completedProgress !== 0 && this.overallProgress !== 0) {
      //     this.progressTitle = `${Math.ceil(this.completedProgress * 100 / this.overallProgress)}%`;
      // } else {
      //     this.progressTitle = '0%';
      // }
      this.statusDescription = StringHelper.isNullOrEmpty(data.result) ? this.translation.instant('label.initializing') + '...' : this.translateLabel(data.result);
      this.currentImportStep = TwoImportDataStep.Validating;
      this.importDataProcessStatus = data.status;
    };

    this.backgroundJobMonitor.onError = (error) => {
      this.statusDescription = error.errorMessage;
      this.importDataProcessStatus = 5;
      this.currentImportStep = TwoImportDataStep.Done;
    };
  }

  uploadErrorHandler(event: SuccessEvent) {
    console.log(event);
  }

  selectEventHandler(e: SelectEvent) {
    this.isFileExtensionForbidden = e.files[0].validationErrors && e.files[0].validationErrors.length > 0;
    this.fileName = e.files[0].name;
    this.selectedFile = e.files[0];
    this._updatePopupStyle(TwoImportDataStep.FileSelected);
  }

  onSelectFile() {
    const uploadFile: HTMLElement = document.querySelector('input[kendofileselect]');
    uploadFile.click();
  }

  closePopup() {
    if (!this.isValidating) {
      this.initPopup();
    }
    this.close.emit();
  }

  initPopup() {
    this.statusDescription = '';
    this._updatePopupStyle(TwoImportDataStep.Initial);
    this.state.skip = 0;
    this.gridData = null;
    this.completedProgress = 0;
    this.overallProgress = 1;
    this.fileName = '';
    this.progressTitle = '';
    this.isDropEnteredFile = false;
    this.isShowLogTableDetails = false;
    this.importDataProcessStatus = null;
  }

  getFileType() {
    const fileName = this.fileName;
    if (StringHelper.isNullOrEmpty(fileName)) {
      return '';
    }
    const ext = fileName.split('.').pop();
    return ext;
  }

  getFileIcon(): String {
    const fileName = this.fileName;
    if (StringHelper.isNullOrEmpty(fileName)) {
      return 'icon-default';
    }
    const ext = fileName.split('.').pop();
    switch (ext.toLowerCase()) {
      case 'xls':
        return 'icon-xls';
      case 'xlsx':
        return 'icon-xlsx';
      case 'csv':
        return 'icon-csv';
      case 'pdf':
        return 'icon-pdf';
      case 'doc':
        return 'icon-doc';
      case 'docx':
        return 'icon-docx';
      case 'png':
        return 'icon-png';
      case 'jpg':
        return 'icon-jpg';
    }
    return 'icon-default';

  }

  getFileName() {
    const ext = this.fileName.split('.').pop();
    return ext;
  }

  validateData(): void {
    const uploadButtons: HTMLCollectionOf<Element> = document.getElementsByClassName('k-upload-selected');
    if (uploadButtons.length > 0) {
      const uploadButton = uploadButtons[0] as HTMLElement;
      uploadButton.click();
      this._updatePopupStyle(TwoImportDataStep.Validating);
    }
  }

  importData(): void {
    if (!this.selectedFile || !this.isDoneSuccess) {
      return;
    }
    const formData: FormData = new FormData();
    formData.append("files", this.selectedFile.rawFile);
    this.httpClient.post(this.uploadSaveUrl, formData).subscribe();
    this.closePopup();
    this.notification.showSuccessPopup('msg.backgroundImportingNotification', 'label.importData', true);
  }

  downloadErrors(): void {
    const url = `${environment.apiUrl}/importDataProgress/${this.bjId}/downloadErrors?lang=${this.translation.currentLang}`;
    this.httpService.downloadFile(url, "ErrorList.xlsx").subscribe();
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
}

/**
 * Indicate current step of popup (GUI popup)
 */
export enum TwoImportDataStep {
  Initial = 0,
  FileSelected = 1,
  Validating = 2,
  Done = 3
}

/**
 * Indicate status of import data progress (on back-end server)
 */
export enum ImportDataProgressStatus {
  Started = 1,
  Success = 2,
  Failed = 3,
  Aborted = 4,
  Warning = 5
}
