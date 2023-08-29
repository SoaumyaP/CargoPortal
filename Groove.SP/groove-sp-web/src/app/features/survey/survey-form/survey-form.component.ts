import { Component, NgZone, OnDestroy, Renderer2 } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faPencilAlt, faPlus, faFileAlt, faChartBar, faPowerOff } from '@fortawesome/free-solid-svg-icons';
import { TranslateService } from '@ngx-translate/core';
import { fromEvent, Observable, Subject, Subscription } from 'rxjs';
import { debounceTime, map, tap } from 'rxjs/operators';
import {
  DateHelper,
  DropDownListItemModel,
  DropdownListModel,
  FormComponent,
  FormMode,
  OrganizationType,
  Roles,
  StringHelper,
  SurveyParticipantType,
  SurveySendToUserType,
  SurveyStatus
} from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { startWithTap } from 'src/app/core/helpers/operators.helper';
import { closest, MultipleEmailValidationPattern, Separator, tableRow } from 'src/app/core/models/constants/app-constants';
import { SurveyQuestionModel } from 'src/app/core/models/survey/survey-question.model';
import { SurveyModel } from 'src/app/core/models/survey/survey.model';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { SurveyFormService } from './survey-form.service';
import * as cloneDeep from 'lodash/cloneDeep';
import { DialogResult } from '@progress/kendo-angular-dialog';
import { RowClassArgs } from '@progress/kendo-angular-grid';
import { ArrayHelper } from 'src/app/core/helpers/array.helper';
import { SurveyQuestionAnswerService } from '../survey-question-answer/survey-question-answer.service';

@Component({
  selector: 'app-survey-form',
  templateUrl: './survey-form.component.html',
  styleUrls: ['./survey-form.component.scss']
})
export class SurveyFormComponent extends FormComponent implements OnDestroy {
  modelName = "surveys"
  readonly applyValidateRequiredField: boolean = false;

  // Init for add new mode
  model: SurveyModel = {
    id: 0,
    name: "",
    description: null,
    participantType: SurveyParticipantType.UserRole,
    userRole: null,
    organizationType: OrganizationType.Principal,
    specifiedOrganization: null,
    specifiedUser: null,
    sendToUser: null,
    isIncludeAffiliate: false,
    status: SurveyStatus.Draft,
    publishedDate: null,
    closedDate: null,
    questions: [],

    createdDate: null,
    updatedDate: null,
    createdBy: null,
    updatedBy: null
  }
  onFetchingOrgOptions: boolean = false;
  orgOptions$: Observable<DropdownListModel<number>[]>;
  selectedOrgIds: any[] = [];

  onFetchingUserOptions: boolean = false;
  filteredUserOptions: DropDownListItemModel<number>[];
  selectedUsers: any[] = [];

  faPlus = faPlus;
  faPencilAlt = faPencilAlt;
  faFileAlt = faFileAlt;
  faChartBar = faChartBar;
  faPowerOff = faPowerOff;
  emailPattern = MultipleEmailValidationPattern;
  readonly surveyStatus = SurveyStatus;
  readonly participantType = SurveyParticipantType;
  readonly organizationType = OrganizationType;
  readonly appPermissions = AppPermissions;
  userRoleOptions: DropdownListModel<number>[] = [
    {
      label: 'label.agent',
      value: Roles.Agent
    },
    {
      label: 'label.cruiseAgent',
      value: Roles.CruiseAgent
    },
    {
      label: 'label.cruisePrincipal',
      value: Roles.CruisePrincipal
    },
    {
      label: 'CSR',
      value: Roles.CSR
    },
    {
      label: 'label.principal',
      value: Roles.Principal
    },
    {
      label: 'label.shipper',
      value: Roles.Shipper
    },
    {
      label: 'label.systemAdmin',
      value: Roles.System_Admin
    },
    {
      label: 'label.warehouse',
      value: Roles.Warehouse
    },
  ];

  orgTypeOptions: DropdownListModel<number>[] = [
    {
      label: 'label.general',
      value: OrganizationType.General
    },
    {
      label: 'label.agent',
      value: OrganizationType.Agent,
    },
    {
      label: 'label.principal',
      value: OrganizationType.Principal,
    }
  ];

  sendToUserTypeOptions = [];

  validationRules = {
    surveyName: {
      'required': 'label.surveyName'
    },
    orgSelections: {
      required: 'label.organization'
    },
    sendToUserType: {
      required: 'label.sendToUserEmail'
    },
    userRole: {
      required: 'label.role'
    },
    specifiedUser: {
      required: 'label.specifiedUser',
      pattern: 'label.specifiedUser'
    }
  };

  questionFromModel: SurveyQuestionModel;
  openQuestionForm: boolean = false;
  questionFormMode: FormMode = FormMode.View;

  private subscriptions: Subscription[] = [];
  private dragDropQuestionSub: Subscription;
  acpTimeout: any;
  gridBindingTimeout: any;
  canDragDropQuestion: boolean = false;

  userSearchTermKeyUp$ = new Subject<string>();

  constructor(
    protected route: ActivatedRoute,
    public notification: NotificationPopup,
    public router: Router,
    public service: SurveyFormService,
    public surveyQuestionAnswerService: SurveyQuestionAnswerService,
    public translateService: TranslateService,
    private renderer: Renderer2,
    public zone: NgZone
  ) {
    super(route, service, notification, translateService, router);

    const sub = this.userSearchTermKeyUp$.pipe(
      debounceTime(1000),
      tap((searchTerm: string) => {
        if (!StringHelper.isNullOrEmpty(searchTerm) && searchTerm.length >= 3) {
          this.filterUserSelections(searchTerm);
        }
      }
      )).subscribe();
    this.subscriptions.push(sub);
  }

  onInitDataLoaded(data) {
    this.bindSelection();

    ArrayHelper.sortBy(this.model.questions, 'sequence');

    this.orgOptions$ = this.getOrgOptions$(this.model.organizationType);
    this.updateSendToUserTypeOptions();

    if (!this.isViewMode) {
      if (this.model.questions.length > 0) {
        this.invokeQuestionDragDrop();
      }
    }

    // data observer on preview form
    this.surveyQuestionAnswerService.previewFormSubject?.subscribe(
      data => {
        if (data) {
          this.model = Object.assign({}, data);
          this.bindSelection();
        }
      }
    )

    super.onInitDataLoaded(data);
  }

  updateSendToUserTypeOptions(): void {
    switch (this.model?.organizationType) {
      case OrganizationType.General:
        this.sendToUserTypeOptions = [
          {
            label: 'label.usersUnderSelectedOrganizations',
            value: SurveySendToUserType.User
          },
          {
            label: 'label.usersUnderCustomersRelationship',
            value: SurveySendToUserType.UserInRelationship,
          }
        ];
        break;
      case OrganizationType.Principal:
        this.sendToUserTypeOptions = [
          {
            label: 'label.usersUnderSelectedOrganizations',
            value: SurveySendToUserType.User
          },
          {
            label: 'label.usersUnderSuppliersRelationship',
            value: SurveySendToUserType.UserInRelationship,
          }
        ];
        break;
      default:
        this.sendToUserTypeOptions = [
          {
            label: 'label.usersUnderSelectedOrganizations',
            value: SurveySendToUserType.User,
          }
        ];
        break;
    }
  }

  bindSelection(): void {
    if (!StringHelper.isNullOrWhiteSpace(this.model.specifiedOrganization)) {
      this.selectedOrgIds = this.model.specifiedOrganization.split(Separator.Semicolon).map(x => Number.parseInt(x));
    }
    if (!StringHelper.isNullOrWhiteSpace(this.model.specifiedUser)) {
      this.selectedUsers = this.model.specifiedUser.split(Separator.COMMA).map(x => x.trim());
      this.filteredUserOptions = this.selectedUserOptions;
    }
  }

  getOrgOptions$(orgType: OrganizationType) {
    return this.service.getOrgSelectionDataSourceByType(orgType).pipe(
      startWithTap(() => this.onFetchingOrgOptions = true),
      tap(() => this.onFetchingOrgOptions = false)
    );
  }

  getUserOptions$(searchEmail: string) {
    return this.service.getUserSelectionDataSource(searchEmail).pipe(
      startWithTap(() => this.onFetchingUserOptions = true),
      tap(() => this.onFetchingUserOptions = false)
    );
  }

  orgTypeChange(value: OrganizationType): void {
    this.selectedOrgIds = [];
    this.model.sendToUser = null;
    this.orgOptions$ = this.getOrgOptions$(value);
    this.updateSendToUserTypeOptions();

    if (value == OrganizationType.Agent) {
      this.model.sendToUser = SurveySendToUserType.User;
    }
  }

  onOrgSelectionsFilter(value): void {
    this.orgOptions$ = this.getOrgOptions$(this.model.organizationType).pipe(
      map(x => x.filter(y => y.label.toLowerCase().indexOf(value.toLowerCase()) !== -1))
    );
  }

  filterUserSelections(value): void {
    this.getUserOptions$(value).pipe(
      map(x => x.filter(y => y.text.toLowerCase().indexOf(value.toLowerCase()) !== -1))
    ).subscribe(
      (x) => this.filteredUserOptions = x
    );
  }

  onUserSelectionsOpen(): void {
    this.filteredUserOptions = this.selectedUserOptions;
  }

  get selectedUserOptions(): DropDownListItemModel<number>[] {
    let res: DropDownListItemModel<number>[] = [];
    this.selectedUsers.forEach(e => {
      res.push({
        text: e,
        value: 0
      })
    });

    return res;
  }

  addQuestion(event: any): void {
    this.questionFromModel = {
      id: 0,
      content: "",
      highValueLabel: null,
      lowValueLabel: null,
      isIncludeOpenEndedText: false,
      sequence: this.model.questions.length + 1,
      starRating: null,
      starRatingArray: null,
      placeHolderText: null,
      type: null,
      typeName: null,
      surveyId: this.model.id,
      options: [],
      dragging: false
    };
    this.openQuestionForm = true;
    this.questionFormMode = FormMode.Add;
  }

  editQuestion(question: SurveyQuestionModel): void {
    this.questionFromModel = cloneDeep(question);
    this.openQuestionForm = true;
    this.questionFormMode = FormMode.Edit;
  }

  deleteQuestion(rowIndex): void {
    this.showConfirmPopup('delete.saveConfirmation').subscribe(
      (res: any) => {
        if (res.value) {
          this.model.questions.splice(rowIndex, 1);
          this.updateQuestionSequence();

          if (this.model.questions.length > 0) {
            this.invokeQuestionDragDrop();
          }
          else {
            this.destroyQuestionDragDrop();
          }
        }
      }
    );
  }

  viewQuestion(question: SurveyQuestionModel): void {
    this.questionFromModel = cloneDeep(question);
    this.openQuestionForm = true;
    this.questionFormMode = FormMode.View;
  }

  onQuestionFormClose(event: any): void {
    this.openQuestionForm = false;
  }

  onQuestionFormSave(data: SurveyQuestionModel): void {
    if (this.questionFormMode === FormMode.Edit) {
      let question = this.model.questions.find(x => x.sequence === data.sequence);
      question.content = data.content;
      question.highValueLabel = data.highValueLabel;
      question.lowValueLabel = data.lowValueLabel;
      question.isIncludeOpenEndedText = data.isIncludeOpenEndedText;
      question.starRating = data.starRating;
      question.placeHolderText = data.placeHolderText;
      question.type = data.type;
      question.typeName = data.typeName;
      question.options = data.options;
    }
    else if (this.questionFormMode === FormMode.Add) {
      this.model.questions.push(data);
    }

    this.openQuestionForm = false;
    this.invokeQuestionDragDrop();
  }

  _clearParticipantFormErrors(): void {
    delete this.formErrors['orgSelections'];
    delete this.formErrors['sendToUserType'];
    delete this.formErrors['userRole'];
    delete this.formErrors['specifiedUser'];
  }

  onSubmit() {
    let isValid: boolean = true;
    if (!this.mainForm.valid) {
      isValid = false;
      this.validateAllFields(false);
    }

    if (!isValid) {
      return;
    }

    // In case there is any error but not belonging to any tab
    const errors = Object.keys(this.formErrors);
    errors.map((key) => {
      const err = Reflect.get(this.formErrors, key);
      if (err && !StringHelper.isNullOrEmpty(err)) {
        return;
      }
    });
    
    let model: SurveyModel = { ...this.model };

    model = DateHelper.formatDate(model);

    // clear redundant participant fields
    if (this.model.participantType === SurveyParticipantType.UserRole) {
      this.clearParticipantOrgSelect(model);
      this.clearParticipantUserSelect(model);
    }
    else if (this.model.participantType === SurveyParticipantType.Organization) {
      model.specifiedOrganization = [...this.selectedOrgIds].map(x => `${x}`).join(Separator.Semicolon);

      this.clearParticipantUserRoleSelect(model);
      this.clearParticipantUserSelect(model);
    }
    else if (this.model.participantType === SurveyParticipantType.SpecifiedUser) {
      model.specifiedUser = [...this.selectedUsers].map(x => `${x}`).join(`${Separator.COMMA} `);

      this.clearParticipantUserRoleSelect(model);
      this.clearParticipantOrgSelect(model);
    }

    this.service.saveSurvey(model).subscribe(
      (data: any) => {
        this.notification.showSuccessPopup(
          "save.sucessNotification",
          "label.survey"
        );
        this.surveyQuestionAnswerService.previewFormSubject?.complete();
        if (this.isAddMode) {
          this.router.navigate([`/surveys`]);
        } else {
          this.modelId = data.id;
          this.router.navigate([
            `/surveys/view/${data.id}`
          ]);
          this.resetForm();
          this.ngOnInitForAddMode();
        }
      },
      err => {
        this.notification.showErrorPopup(
          "save.failureNotification",
          "label.survey"
        );
      }
    )
  }

  clearParticipantOrgSelect(model): void {
    model.organizationType = OrganizationType.Principal;
    model.sendToUser = null;
    model.isIncludeAffiliate = false;
    model.specifiedOrganization = null;
  }
  clearParticipantUserSelect(model): void {
    model.specifiedUser = null;
  }
  clearParticipantUserRoleSelect(model): void {
    model.userRole = null;
  }

  validateMandatoryFields(): boolean {
    let isValid = true;
    var mandatoryFields = [
      'name'
    ];
    switch (this.model.participantType) {
      case SurveyParticipantType.UserRole:
        mandatoryFields.push('userRole');

        break;
      case SurveyParticipantType.Organization:
        if (this.model.organizationType !== OrganizationType.Agent) {
          mandatoryFields.push('sendToUser');
        }

        if (this.selectedOrgIds.length === 0) {
          isValid = false;
        }
        break;
      case SurveyParticipantType.SpecifiedUser:
        if (this.selectedUsers.length === 0) {
          isValid = false;
        }
        break;
      default:
        break;
    }
    for (let i = 0; i < mandatoryFields.length; i++) {
      let fieldName = mandatoryFields[i];
      let field = this.model[fieldName];
      if (typeof field === "number" && (field === 0 || !field)) {
        isValid = false;
      }
      else if (StringHelper.isNullOrWhiteSpace(field)) {
        isValid = false;
      }
    }

    if (!this.model.questions || this.model.questions.length === 0) {
      isValid = false;
    }

    return isValid;
  }

  cancel() {
    this.showConfirmPopup("edit.cancelConfirmation").subscribe((result: any) => {
      if (result.value) {
        if (this.isAddMode) {
          this.backToList();
        } else if (this.isEditMode) {
          this.router.navigate([
            `/surveys/view/${this.model.id}`
          ]);

          this.resetForm();
          this.ngOnInit();
        }
      }
    });
  }

  /**
   * Destroy drag drop; clear form errors; clear selected value 
   */
  resetForm(): void {
    this.destroyQuestionDragDrop();
    this.formErrors = {};
    this.selectedOrgIds = [];
    this.selectedUsers = [];
  }

  editSurvey() {
    this.router.navigate([
      `/surveys/edit/${this.model.id}`
    ]);
    this.ngOnInit();
  }

  closeSurvey() {
    this.service.closeSurvey(this.model.id).subscribe(
      (done) => {
        this.notification.showSuccessPopup(
          "save.sucessNotification",
          "label.survey"
        );
        this.ngOnInit();
      },
      (err) => this.notification.showErrorPopup(
        "save.failureNotification",
        "label.survey"
      )
    )
  }

  invokeQuestionDragDrop() {
    clearTimeout(this.gridBindingTimeout);
    this.acpTimeout = setTimeout(() => {
      this.destroyQuestionDragDrop();
      this.dragDropQuestionSub = this.handleQuestionDragAndDrop();
      this.canDragDropQuestion = true;
    }, 1);
  }

  private destroyQuestionDragDrop() {
    if (this.dragDropQuestionSub) {
      this.dragDropQuestionSub.unsubscribe();
      this.canDragDropQuestion = false;
    }
  }

  private handleQuestionDragAndDrop(): Subscription {
    const sub = new Subscription(() => { });
    let draggedItemIndex;

    const tableRows = Array.from(document.querySelectorAll(".question-grid.k-grid tr"));
    tableRows.forEach((item) => {
      this.renderer.setAttribute(item, "draggable", "true");
      const dragStart = fromEvent<DragEvent>(item, "dragstart");
      const dragOver = fromEvent(item, "dragover");
      const dragEnd = fromEvent(item, "dragend");

      sub.add(
        dragStart
          .pipe(
            tap(({ dataTransfer }) => {
              try {
                const dragImgEl = document.createElement("span");
                dragImgEl.setAttribute(
                  "style",
                  "position: absolute; display: block; top: 0; left: 0; width: 0; height: 0;"
                );
                document.body.appendChild(dragImgEl);
                dataTransfer.setDragImage(dragImgEl, 0, 0);
              } catch (err) {
                // IE doesn't support setDragImage
              }
              try {
                // Firefox won't drag without setting data
                dataTransfer.setData("application/json", "");
              } catch (err) {
                // IE doesn't support MIME types in setData
              }
            })
          )
          .subscribe(({ target }) => {
            const row: HTMLTableRowElement = <HTMLTableRowElement>target;
            draggedItemIndex = row.rowIndex;
            const dataItem = this.model.questions[draggedItemIndex];
            dataItem.dragging = true;
          })
      );

      sub.add(
        dragOver.subscribe((e: any) => {
          e.preventDefault();
          const dataItem = this.model.questions.splice(draggedItemIndex, 1)[0];
          const dropIndex = closest(e.target, tableRow).rowIndex;

          draggedItemIndex = dropIndex;
          this.zone.run(() =>
            this.model.questions.splice(dropIndex, 0, dataItem)
          );
        })
      );

      sub.add(
        dragEnd.subscribe((e: any) => {
          e.preventDefault();
          const dataItem = this.model.questions[draggedItemIndex];
          dataItem.dragging = false;
          this.updateQuestionSequence();
        })
      );
    });

    return sub;
  }

  updateQuestionSequence(): void {
    this.model.questions.forEach((e, i) => {
      e.sequence = i + 1;
    });
  }

  public rowCallback(context: RowClassArgs) {
    return {
      dragging: context.dataItem.dragging,
    };
  }

  showConfirmPopup(msg: string, title: string = 'label.survey'): Observable<DialogResult> {
    const confirmDlg = this.notification.showConfirmationDialog(
      msg,
      title
    );
    return confirmDlg.result;
  }

  onPreview() {
    this.router.navigateByUrl(`surveys/${this.model.id}/question-answer?mode=preview`)
    if (this.isAddMode) {
      this.model.specifiedUser = [...this.selectedUsers].map(x => `${x}`).join(`${Separator.COMMA} `);
      this.model.specifiedOrganization = [...this.selectedOrgIds].map(x => `${x}`).join(Separator.Semicolon);
      this.service.surveyFormSubject.next(this.model);
    } else {
      this.service.surveyFormSubject.next(null);
    }
  }

  get isHiddenPreview() {
    return this.model.status !== this.surveyStatus.Draft || this.isEditMode
  }

  backToList(): void {
    this.router.navigate(["surveys"]);
    this.surveyQuestionAnswerService.previewFormSubject?.complete();
  }

  ngOnDestroy(): void {
    this.subscriptions.map(x => x.unsubscribe());
  }
}