import { Component, OnInit, ViewChild } from '@angular/core';
import { ConsolidationModel } from '../models/consolidation.model';
import { faPowerOff, faPlus, faEllipsisV, faPencilAlt, faTrashAlt, faCheck, faCog, faMinus } from '@fortawesome/free-solid-svg-icons';
import { DATE_FORMAT } from 'src/app/core/helpers/date.helper';
import { ConsolidationStage, FormMode, StringHelper } from 'src/app/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ConsolidationService } from '../consolidation.service';
import { ConsolidationConsignmentFormComponent } from '../consolidation-consignment-form/consolidation-consignment-form.component';
import { ConsolidationCargoDetailListComponent } from '../consolidation-cargo-detail-list/consolidation-cargo-detail-list.component';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { forkJoin, Subject, Subscription } from 'rxjs';
import { IntegrationData } from 'src/app/core/models/forms/integration-data';
import { filter } from 'rxjs/operators';
import { CargoDetailLoadModel } from 'src/app/core/models/cargo-details/cargo-detail-load.model';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@Component({
    selector: 'app-consolidation-details',
    templateUrl: './consolidation-details.component.html',
    styleUrls: ['./consolidation-details.component.scss']
})
export class ConsolidationDetailsComponent implements OnInit {
    @ViewChild(ConsolidationCargoDetailListComponent, { static: false }) consolidationCargoDetailListComponent: ConsolidationCargoDetailListComponent;

    model: ConsolidationModel = new ConsolidationModel;
    consignmentList: any[];
    faTrashAlt = faTrashAlt;
    faMinus = faMinus;
    faPencilAlt = faPencilAlt;
    faEllipsisV = faEllipsisV;
    faPlus = faPlus;
    faPowerOff = faPowerOff;
    faCheck = faCheck;
    faCog = faCog;
    DATE_FORMAT = DATE_FORMAT;
    consolidationStage = ConsolidationStage;
    consolidationId: number;
    isInitDataLoaded: boolean;
    isCollapseMode: boolean = false;
    readonly AppPermissions = AppPermissions;

    /**Used to interact between components in consolidation by emitting or subscribing to an event and handling it. */
    integration$: Subject<IntegrationData> = new Subject();

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    constructor(private route: ActivatedRoute,
        public translateService: TranslateService,
        private router: Router,
        private notification: NotificationPopup,
        private consolidationService: ConsolidationService,
        private _gaService: GoogleAnalyticsService) {
    }

    ngOnInit() {
        this.route.params.subscribe(params => {
            this.consolidationId = params['id'];
            const obs1$ = this.consolidationService.getConsolidation(this.consolidationId);
            const obs2$ = this.consolidationService.getConsignments(this.consolidationId);
            forkJoin([obs1$, obs2$]).subscribe((results:any) => {
                if (results[0] == null) {
                    this.router.navigate(['/error/404']);
                }
                this.model = results[0];
                this.consignmentList = results[1];
                this.isInitDataLoaded = true;
            })
        });
        this._registerEventHandlers();
    }

    private _registerEventHandlers() {
        /*Handle when the loaded cargo detail list has been updated successfully.
        */
        const sub1 = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[consolidation-cargo-detail-list]cargoDetailListUpdated'
            )).subscribe((eventContent: IntegrationData) => {
                const updatedList = eventContent.content;
                if (updatedList) {
                    this.model.totalGrossWeight = updatedList.map(c => c.grossWeight).reduce(function (a, b) { return a + b; }, 0);
                    this.model.totalNetWeight = updatedList.filter(c => c.netWeight)?.map(c => c.netWeight).reduce(function (a, b) { return a + b; }, 0);
                    this.model.totalPackage = updatedList.map(c => c.package).reduce(function (a, b) { return a + b; }, 0);
                    this.model.totalVolume = updatedList.map(c => c.volume).reduce(function (a, b) { return a + b; }, 0);
                    this.model.totalGrossWeightUOM = updatedList[0]?.grossWeightUOM ?? null;
                    this.model.totalNetWeightUOM = updatedList[0]?.netWeightUOM ?? null;
                    this.model.totalPackageUOM = updatedList[0]?.packageUOM ?? null;
                    this.model.totalVolumeUOM = updatedList[0]?.volumeUOM ?? null;
                }
            });

        this._subscriptions.push(sub1);

        /* Handle when the cargo detail(s) has been loaded successfully.
        */
        const sub2 = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[load-cargo-detail-popup]cargoDetailLoaded'
            )).subscribe((eventContent: IntegrationData) => {
                const loadedList: CargoDetailLoadModel[] = eventContent.content;
                if (loadedList) {
                    this.model.totalGrossWeight = loadedList.map(c => c.grossWeight).reduce(function (a, b) { return a + b; }, 0);
                    this.model.totalNetWeight = loadedList.filter(c => c.netWeight)?.map(c => c.netWeight).reduce(function (a, b) { return a + b; }, 0);
                    this.model.totalPackage = loadedList.map(c => c.package).reduce(function (a, b) { return a + b; }, 0);
                    this.model.totalVolume = loadedList.map(c => c.volume).reduce(function (a, b) { return a + b; }, 0);
                    this.model.totalGrossWeightUOM = loadedList[0]?.grossWeightUOM ?? null;
                    this.model.totalNetWeightUOM = loadedList[0]?.netWeightUOM ?? null;
                    this.model.totalPackageUOM = loadedList[0]?.packageUOM ?? null;
                    this.model.totalVolumeUOM = loadedList[0]?.volumeUOM ?? null;
                }
            });

        this._subscriptions.push(sub2);

        /* Handle when the consignment linking to this consolidation has been deleted successfully.
        */
        const sub3 = this.integration$.pipe(
            filter((eventContent: IntegrationData) =>
                eventContent.name === '[consolidation-consignment-form]consignmentDeleted'
            )).subscribe((eventContent: IntegrationData) => {
                const deletedConsignment = eventContent.content;
                this.model.totalGrossWeight -= deletedConsignment.grossWeight;
                this.model.totalNetWeight -= deletedConsignment.netWeight;
                this.model.totalPackage -= deletedConsignment.package;
                this.model.totalVolume -= deletedConsignment.volume;
            });

        this._subscriptions.push(sub3);
    }

    confirmConsolidation() {

        this.consolidationService.trialConfirmConsolidation(this.model.id).subscribe(
            (success) => {
                const confirmDlg = this.notification.showConfirmationDialog('msg.confirmConsolidationConfirmation', 'label.consolidation');
                confirmDlg.result.subscribe(
                    (result: any) => {
                        if (result.value) {
                            this.consolidationService.confirmConsolidation(this.model.id).subscribe(
                                (res: ConsolidationModel) => {
                                    this.notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
                                    this.model = res;
                                    this._gaService.emitAction('Confirm', GAEventCategory.Consolidation);
                                },
                                (err) => this.notification.showErrorPopup('save.failureNotification', 'label.consolidation')
                            );
                        }
                    });
            },
            (fail) => {
                if (fail.error) {
                    const errMessage = fail.error.errors;
                    if (!StringHelper.isNullOrEmpty(errMessage)) {
                        const errHashTags = errMessage.split('#').filter(x => !StringHelper.isNullOrEmpty(x));
                        switch (errHashTags[0]) {
                            case 'noCargoWasLoaded':
                                this.notification.showInfoDialog('msg.pleaseLoadCargoDetail', 'label.consolidation');
                                break;
                            case 'totalPackageIsEqualToZero':
                                this.notification.showInfoDialog('msg.subtotalPackageMustBeGreaterThanZero', 'label.consolidation');
                                break;
                            case 'missingContainerInfo':
                                const { carrierSONo, containerNo, sealNo } = this.model
                                if (StringHelper.isNullOrEmpty(carrierSONo)) {
                                    this.notification.showErrorPopup('validation.carrierSONoIsMandatoryToConfirm', 'label.consolidation');
                                } else if (StringHelper.isNullOrEmpty(containerNo)) {
                                    this.notification.showErrorPopup('validation.containerNoIsMandatoryToConfirm', 'label.consolidation');
                                } else if (StringHelper.isNullOrEmpty(sealNo)) {
                                    this.notification.showErrorPopup('validation.sealNoIsMandatoryToConfirm', 'label.consolidation');
                                }
                                this.router.navigate([`/consolidations/edit/${this.model.id}`], {queryParams: {state: 'confirmfailed'}});
                                break;
                            case 'duplicateOnContainerNoAndCarrierSONo':
                                this.notification.showErrorPopup('validation.duplicatedOnCarrierSONoAndContainerNo', 'label.consolidation');
                                this.router.navigate([`/consolidations/edit/${this.model.id}`], {queryParams: {state: 'confirmfailed'}});
                                break;
                            default:
                                this.notification.showErrorPopup(
                                    'save.failureNotification',
                                    'label.consolidation'
                                );
                                break;
                        }
                    }
                }
            }
        );
    }

    unconfirmConsolidation() {
        this.consolidationService.unconfirmConsolidation(this.model.id).subscribe(
            (res: ConsolidationModel) => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
                this.model = res;
                this._gaService.emitAction('Unconfirm', GAEventCategory.Consolidation);
            },
            (err) => this.notification.showErrorPopup('save.failureNotification', 'label.consolidation')
        );
    }

    deleteConsolidation() {
        const confirmDlg = this.notification.showConfirmationDialog('confirmation.deleteConsolidation', 'label.consolidation');
        confirmDlg.result.subscribe(
            (result:any) => {
            if (result.value) {
                this.consolidationService.deleteConsolidation(this.model.id).subscribe(
                    (success) => {
                        this._gaService.emitAction('Delete', GAEventCategory.Consolidation);
                        this.notification.showSuccessPopup('save.sucessNotification', 'label.consolidation');
                        this.backList();
                    },
                    (err) => this.notification.showErrorPopup('save.failureNotification', 'label.consolidation')
                );
            }
        });
    }

    get hiddenDeleteConsolidationBtn() {
        if (this.model.stage !== ConsolidationStage.New) {
            return true;
        }
        if (this.consignmentList?.filter(x => !x.isAddLine || StringHelper.isNullOrEmpty(x.isAddLine))?.length > 0) {
            return true;
        }
        return false;
    }

    showTextField(name) {
        return this.model[name] ? this.model[name] : '--';
    }

    backList() {
        this.router.navigate(['/consolidations']);
    }

    onCargoDetailListModeChange(mode: FormMode) {
        this.isCollapseMode = mode === FormMode.Edit ? true : false;
    }

}
