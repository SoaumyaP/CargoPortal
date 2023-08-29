import {
    Component,
    OnInit,
    Output,
    EventEmitter,
    Input,
    OnChanges,
    OnDestroy,
    ElementRef,
    ViewChild,
} from '@angular/core';
import { faTimes, faEllipsisV } from '@fortawesome/free-solid-svg-icons';
import { SelectPosFormService } from './select-pos-form.service';
import { UserContextService, StringHelper } from 'src/app/core';
import {
    tap,
    debounceTime,
} from 'rxjs/operators';
import { Observable, of, Subscription, Subject } from 'rxjs';

interface SelectPurchaseOrderModel {
    id: number;
    poNumber: string;
    poKey: string;
    itemsCount: number;
}

@Component({
    selector: 'app-select-pos-form',
    templateUrl: './select-pos-form.component.html',
    styleUrls: ['./select-pos-form.component.scss'],
})
export class SelectPOsFormComponent
    implements OnInit, OnChanges, OnDestroy {
    @Input() popupOpened: boolean;
    @Output() popupClosing: EventEmitter<any> = new EventEmitter();

    faTimes = faTimes;
    faEllipsisV = faEllipsisV;

    selectedPOs: Array<SelectPurchaseOrderModel> = [];
    sourcePOs: Array<SelectPurchaseOrderModel> = [];
    sourcePOsFiltered: Array<SelectPurchaseOrderModel> = [];

    principalDataSource = [];
    selectedPrincipalValue = 0;

    @ViewChild('searchInput', { read: ElementRef, static: false })
    searchInput: ElementRef;
    searchTerm: string = '';
    treeData: any = null;
    selectedDragItem: SelectPurchaseOrderModel = null;

    treeViewPagination: ILoadMoreRequestArgs = {
        skip: 0,
        take: 20,
        loadedRecordCount: 0,
        maximumRecordCount: 0,
        loadingData: false
    };

    public searchTearmkeyUp$ = new Subject<string>();

    private _currentUser: any = null;
    private _subscriptions: Array<Subscription> = [];

    constructor(
        private _service: SelectPosFormService,
        private _userContextService: UserContextService
    ) {

        const sub = this.searchTearmkeyUp$.pipe(
            debounceTime(1000),
            tap((searchTearm: string) => {
                if (StringHelper.isNullOrEmpty(searchTearm) || searchTearm.length === 0 || searchTearm.length >= 3) {
                    this._fetchSourcePOsDataSource(false, searchTearm);
                }
            }
        )).subscribe();
        this._subscriptions.push(sub);
    }

    ngOnInit() {}

    ngOnChanges() {
        // Clean-up current values as opening popup
        if (this.popupOpened) {
            this._cleanupWorkingState();
            const sub = this._getCurrentUser$
                .pipe(
                    tap(() => {
                        this._fetchPrincipalDataSource();
                    })
                )
                .subscribe();
            this._subscriptions.push(sub);
        }
    }

    ngOnDestroy() {
        this._subscriptions.map((x) => x.unsubscribe());
    }

    // Event handlers on poup
    onDragStart(event) {
        event.dataTransfer.setData('text', '');
    }

    onDragEnd() {
        this.selectedDragItem = null;
    }

    clickItem(selectedPO) {
        this.selectedDragItem = selectedPO;
    }

    // need statements to drop work
    allowDrop(event) {
        event.stopPropagation();
        event.preventDefault();
    }

    onDrop() {
        const value = this.selectedDragItem;
        this.selectedPOs.push(value);
        this._filterSourcePOs(true);
        // this._fetchSourcePOsDataSource();
    }

    unselectPO(currentPO: {
        id: number;
        poNumber: string;
        poKey: string;
        itemsCount: number;
    }) {
        const po = this.selectedPOs.filter((item) => item.id === currentPO.id);

        this.selectedPOs = this.selectedPOs.filter(
            (item) => item.id !== currentPO.id
        );

        const check = this.sourcePOs.some((item) => item.id === currentPO.id);
        if (!check) {
            this.sourcePOs.push(po[0]);
        }

        this._filterSourcePOs();
        // this._fetchSourcePOsDataSource();
    }

    principalSelectionChanged() {

        // clean-up selected POs
        this.selectedPOs = [];

        // clean-up searching text
        this.searchTerm = '';

        this._fetchSourcePOsDataSource();
    }

    onClosing() {
        this.popupClosing.emit();
    }

    onBooking() {
        if ((!this.selectedPOs || this.selectedPOs.length === 0) && !this.isAllowMissingPO) {
            return;
        }
        
        const seletedPOs = this.selectedPOs.map((x) => x.id);
        this.popupClosing.emit({
            selectedPOIds: seletedPOs,
            selectedPrincipalId: this.selectedPrincipalValue,
            isAllowMissingPO: this.isAllowMissingPO
        });
    }

    private _fetchPrincipalDataSource() {
        const roleId = this._currentUser.role ? this._currentUser.role.id : 0;
        const organizationId = this._currentUser.organizationId || 0;
        const affiliates = this._currentUser.affiliates;
        const sub = this._service
            .getPrincipalDataSource(roleId, organizationId, affiliates)
            .pipe(
                tap((data) => {
                    this.principalDataSource = data;
                    this.selectedPrincipalValue =
                        this.principalDataSource &&
                        this.principalDataSource.length > 0
                            ? this.principalDataSource[0].value
                            : 0;
                }),
                tap(() => {
                    // this._getBuyerCompliance();
                    this._fetchSourcePOsDataSource();
                })
            )
            .subscribe();
        this._subscriptions.push(sub);
    }

    loadMorePO() {
        if (
            this.treeViewPagination.loadedRecordCount <
            this.treeViewPagination.maximumRecordCount
        ) {
            this._fetchSourcePOsDataSource(true);
        }
    }

    private _fetchSourcePOsDataSource(
        loadMoreMode?: boolean,
        searchValue?: string
    ) {
        // Set status here to make show loading icon
        this.treeViewPagination.loadingData = true;

        // Reset data if it is not loading more POs
        if (!loadMoreMode) {

            this.sourcePOs = [];
            this.sourcePOsFiltered = [];

            this.treeViewPagination.skip = 0;
            this.treeViewPagination.loadedRecordCount = 0;
            this.treeViewPagination.maximumRecordCount = 0;
        }

        const skip = this.treeViewPagination.skip;
        const take = this.treeViewPagination.take;
        const selectedPrincipalValue = this.selectedPrincipalValue;
        const currentOrganizationId = this._currentUser.organizationId;
        const theFirstSelectedPOId =
            this.selectedPOs && this.selectedPOs[0] && this.selectedPOs[0].id;

        if (selectedPrincipalValue === 0) {
            this.sourcePOs = [];
            this.sourcePOsFiltered = [];
        } else {
            const customerRelationships = this._currentUser
                .customerRelationships;
            const sub = this._service
                .getSourcePOsDataSource(
                    selectedPrincipalValue,
                    currentOrganizationId,
                    customerRelationships,
                    skip,
                    take,
                    // searchTeam will be from direct value of input or data model
                    StringHelper.isNullOrEmpty(searchValue)
                        ? this.searchTerm
                        : searchValue,
                    this._currentUser.role.id,
                    theFirstSelectedPOId,
                    this._currentUser.affiliates
                )
                .pipe(
                    tap((data) => {
                        const filterData = data.filter(
                            (x) => !this.selectedPOs.some((y) => y.id === x.id)
                        );

                        if (loadMoreMode) {
                            this.sourcePOs = this.sourcePOs.concat(filterData);
                            this.sourcePOsFiltered = this.sourcePOsFiltered.concat(
                                filterData
                            );
                        } else {
                            this.sourcePOs = filterData;
                            this.sourcePOsFiltered = filterData;
                            this.treeViewPagination.maximumRecordCount =
                                StringHelper.isNullOrEmpty(data) ||
                                StringHelper.isNullOrEmpty(data[0])
                                    ? 0
                                    : data[0].recordCount;
                        }

                        // update treeview pagging
                        this.treeViewPagination.loadedRecordCount += data.length;
                        this.treeViewPagination.skip = this.treeViewPagination.loadedRecordCount;

                        // set other status
                        this.treeViewPagination.loadingData = false;

                    })
                )
                .subscribe();
            this._subscriptions.push(sub);
        }
    }

    private get _getCurrentUser$(): Observable<any> {
        if (this._currentUser) {
            return of(this._currentUser);
        } else {
            return this._userContextService.getCurrentUser().pipe(
                tap((user: any) => {
                    this._currentUser = user;
                })
            );
        }
    }

    private _filterSourcePOs(addingMode?: boolean) {

        // If selected POs count > 1, then locally filter
        if (this.selectedPOs.length > 1) {
            // apply filter on selected POs
            this.sourcePOsFiltered = this.sourcePOs.filter(
                (x) => !this.selectedPOs.some((y) => y.id === x.id)
            );
        } else if (this.selectedPOs.length === 1) {
            // If it is selecting PO, then server filter
            if (addingMode) {
                this._fetchSourcePOsDataSource();
            } else {
            // If it is unselecting PO, then local filter
                // apply filter on selected POs
                this.sourcePOsFiltered = this.sourcePOs.filter(
                    (x) => !this.selectedPOs.some((y) => y.id === x.id)
                );
            }
        } else {
            // Server filter
            this._fetchSourcePOsDataSource();
        }
    }

    get isAllowMissingPO() {
        var selectedPrincipal = this.principalDataSource?.find(x => x.value === this.selectedPrincipalValue);

        if (!selectedPrincipal) {
            return false;
        }
        return selectedPrincipal.isAllowMissingPO;
    }

    private _cleanupWorkingState() {
        this.selectedPrincipalValue = 0;
        this.selectedPOs = [];
        this.sourcePOsFiltered = [];
        this.sourcePOs = [];
        this.selectedDragItem = null;
        this.searchTerm = '';
    }
}
