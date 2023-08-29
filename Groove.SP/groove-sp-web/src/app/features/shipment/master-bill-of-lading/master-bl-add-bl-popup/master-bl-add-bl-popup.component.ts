import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChange, SimpleChanges, ViewChild } from '@angular/core';
import { AbstractControl, NgForm } from '@angular/forms';
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { EMPTY, of } from 'rxjs';
import { Subject, Subscription } from 'rxjs';
import { delay, switchMap, tap } from 'rxjs/operators';
import { DATE_FORMAT, StringHelper, UserContextService } from 'src/app/core';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';
import { BillOfLadingFormService } from '../../bill-of-lading/bill-of-lading-form.service';
import { BillOfLadingListService } from '../../bill-of-lading/bill-of-lading-list.service';
import { BillOfLadingModel } from '../../bill-of-lading/models/bill-of-lading.model';

@Component({
    selector: 'app-master-bl-add-bl-popup',
    templateUrl: './master-bl-add-bl-popup.component.html',
    styleUrls: ['./master-bl-add-bl-popup.component.scss']
})
export class MasterBLAddBLPopupComponent implements OnInit, OnChanges, OnDestroy {
    @ViewChild('mainForm', { static: false }) mainForm: NgForm;
    
    /**
     * Metadata will be inputted into the component
     */
    @Input()
    inputMetaData: MasterBLAddBLPopupComponentMetadata;

    /**
     * As a popup is closing
     */
    @Output()
    close: EventEmitter<number> = new EventEmitter<number>();

    /**
     * To define whether a popup should be opened
     */
    isFormOpened: boolean;

    /**
     * To define current working house bill of lading id
     */
    masterBOLId: number;

    /**
     * Additional data for list of house bill of ladings
     */
    houseBLList: Array<BillOfLadingModel> = [];

    // Master BL number
    @ViewChild('houseBLAutoComplete', { static: false })
    public houseBLAutoComplete: AutoCompleteComponent;

    selectedHouseBLNumber?: string;
    selectedHouseBL?: BillOfLadingModel;
    houseBLKeyUp$ = new Subject<string>();
    houseBLDataSource: Array<BillOfLadingModel> = [];
    isHouseBLLoading: boolean = false;
    model: BillOfLadingModel;

    currentUser: UserProfileModel;
    defaultValue = DefaultValue2Hyphens;
    DATE_FORMAT = DATE_FORMAT;

    private _subscriptions: Array<Subscription> = [];


    get isSelectedHouseBLBlank(): boolean {
        return StringHelper.isNullOrEmpty(this.selectedHouseBL);
    }

    constructor(
        public listService: BillOfLadingListService,
        private _userContext: UserContextService) {
        this._registerEventHandlers();
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
            }
        });
    }

    ngOnInit() {
    }

    ngOnDestroy(): void {
        this._subscriptions.map(x => x.unsubscribe());
    }

    ngOnChanges(changes: SimpleChanges) {
        // to fetch data for attachment type options
        const model: SimpleChange = changes['inputMetaData'];
        const previousValue = model?.previousValue as MasterBLAddBLPopupComponentMetadata;
        const newValue = model?.currentValue as MasterBLAddBLPopupComponentMetadata;
        this.isFormOpened = newValue.isFormOpened;

        // the popup is opening, set values for some properties/variables
        if (newValue?.isFormOpened && !previousValue?.isFormOpened) {
            // Store into internal variables
            this.masterBOLId = newValue.masterBLId;
            this.houseBLList = newValue.houseBLList || [];

            this._resetPopup();
        }

    }

    onHouseBLValueChange(houseBLNumber: string) {
        this.selectedHouseBLNumber = houseBLNumber;
        this.selectedHouseBL = this.houseBLDataSource.find(x => x.billOfLadingNo === houseBLNumber);
        this.model = { ...this.selectedHouseBL };
    }

    onBtnSelectClick() {
        FormHelper.ValidateAllFields(this.mainForm);
        if (!(this.mainForm.valid && this.selectedHouseBL)) {
            return;
        }
        // Fire event to parent component with id of selected master bl
        this.close.emit(this.model.id);

    }

    onFormClosed() {
        this.close.emit();
    }

    /**
     * To handle events
     */
    private _registerEventHandlers(): void {
        const sub = this.houseBLKeyUp$.pipe(
            tap(() => {
                this.houseBLAutoComplete.toggle(false);
            }),
            switchMap((searchTerm: string) => {
                if (!StringHelper.isNullOrEmpty(searchTerm) && searchTerm.length >= 3) {
                    return of(searchTerm).pipe(delay(1000));
                } else {
                    return EMPTY;
                }
            }
        )).subscribe((searchTerm: string) => {
            this._houseBLFilterChange(searchTerm);
        });
        this._subscriptions.push(sub);
    }

    private _houseBLFilterChange(searchTerm: string) {
        const isInternal = this.currentUser.isInternal;
        const affiliates = this.currentUser.affiliates;
        this.isHouseBLLoading = true;

        this.listService.searchHouseBLsByNumber(searchTerm, isInternal, affiliates).subscribe(
            (data) => {
                // Remove existing house bill of ladings which linked to master bill of lading
                data = data.filter(x => !this.houseBLList.some(y => StringHelper.caseIgnoredCompare(y.billOfLadingNo, x.billOfLadingNo)));
                this.houseBLDataSource = data;
                this.houseBLAutoComplete.toggle(true);
                this.isHouseBLLoading = false;
            }
        );

    }

    private _resetPopup(resetVesselVoyage: boolean = true) {
        this.selectedHouseBLNumber = null;
        this.selectedHouseBL = null;
        this.selectedHouseBLNumber = null;
        this._resetModel();
    }

    private _resetModel() {
        this.model = ({} as BillOfLadingModel);
    }

    getControl(controlName: string): AbstractControl {
        return this.mainForm?.form?.controls[controlName];
      }
}

/**
 * To define metadata to input component MasterBLAddBLPopupComponent
 */
export interface MasterBLAddBLPopupComponentMetadata {
    /**
     * To define whether a popup should be opened
     */
    isFormOpened: boolean;

    /**
     * To define current working master bill of lading
     */
    masterBLId: number;

    /**
     * Additional data for list of house bill of ladings
     */
    houseBLList?: Array<BillOfLadingModel>;
}

