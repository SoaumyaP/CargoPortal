import { Component, OnInit, Input, Output, EventEmitter, ViewChild, OnDestroy } from '@angular/core';
import { FormGroup, FormControl} from '@angular/forms';
import { Subscription } from 'rxjs';
import { DropDownListItemModel } from 'src/app/core';
import { ReportPermissionModel } from '../models/report-permission.model';
import { ReportListComponent } from '../report-list/report-list.component';
import { ReportListService } from '../report-list/report-list.service';

@Component({
    selector: 'app-report-permission-form',
    templateUrl: './report-permission-form.component.html',
    styleUrls: ['./report-permission-form.component.scss']
})
export class ReportPermissionFormComponent implements OnInit, OnDestroy {
    @Output() reportPermissionFormClosed: EventEmitter<FormGroup> = new EventEmitter<FormGroup>();
    @Input() isViewOnly: boolean;

    public reportPermissionFormOpen: boolean = false;
    public organizationFilteredDataSource: Array<DropDownListItemModel<number>> = [];
    public organizationDataSource: Array<DropDownListItemModel<number>> = [];

    private _subscriptions: Array<Subscription> = [];

    formGroup = new FormGroup({
        reportId: new FormControl(0),
        organizations: new FormControl(''),
        grantInternal: new FormControl(true),
        grantPrincipal: new FormControl(false),
        grantShipper: new FormControl(false),
        grantAgent: new FormControl(false),
        grantWarehouse: new FormControl(false)
    });

    constructor(
        private reportListComponent: ReportListComponent,
        private reportListService: ReportListService) {
            if (this.isViewOnly) {
            this.formGroup.controls['grantInternal'].disable();
            this.formGroup.controls['grantPrincipal'].disable();
            this.formGroup.controls['grantShipper'].disable();
            this.formGroup.controls['grantAgent'].disable();
            this.formGroup.controls['grantWarehouse'].disable();
            }
    }

    ngOnInit() {
        this.reportListService.getOrganizationDataSource()
            .subscribe(value => {
                const result =
                value.map(element => {
                    const item = {
                        text: element.code + ' - ' + element.name,
                        value: element.id
                    };
                    return item;
                });
                this.organizationDataSource = result;
                this.organizationFilteredDataSource = result;

        });


        const sub = this.reportListComponent.reportPermissionOpening.subscribe((value: ReportPermissionModel) => {
            // If no organization selected, set it to dummy value
            const selectedOrganizationIds = (value.organizationIds || '').split(',').map(x => parseInt(x, 10));

            const selectedOrganizations = this.organizationDataSource.filter(x => selectedOrganizationIds.indexOf(x.value) > -1);

            this.formGroup.controls['reportId'].setValue(value.reportId);
            this.formGroup.controls['organizations'].setValue(selectedOrganizations);
            this.formGroup.controls['grantInternal'].setValue(value.grantInternal);
            this.formGroup.controls['grantPrincipal'].setValue(value.grantPrincipal);
            this.formGroup.controls['grantShipper'].setValue(value.grantShipper);
            this.formGroup.controls['grantAgent'].setValue(value.grantAgent);
            this.formGroup.controls['grantWarehouse'].setValue(value.grantWarehouse);

            this.reportPermissionFormOpen = true;
        });

        this._subscriptions.push(sub);
    }

    onFormClosed() {
        this.reportPermissionFormOpen = false;
        // fire event then report-list-component should handle
        this.reportPermissionFormClosed.emit();
    }

    onSaveClick() {
        this.reportPermissionFormOpen = false;
        // fire event then report-list-component should handle
        this.reportPermissionFormClosed.emit(this.formGroup);
    }

    onHandleOrganizationFilter(value) {
        this.organizationFilteredDataSource = this.organizationDataSource.filter((s) => s.text.toLowerCase().indexOf(value.toLowerCase()) !== -1);
    }

    onOrganizationValueChange(value) {
        // Set filtered data source to full list
        this.organizationFilteredDataSource = this.organizationDataSource;
    }

    ngOnDestroy() {
        this._subscriptions.map(x => x.unsubscribe());
    }
}
