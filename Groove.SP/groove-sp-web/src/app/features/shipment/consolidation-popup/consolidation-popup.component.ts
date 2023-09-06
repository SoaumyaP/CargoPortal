import { Component, EventEmitter, Input, OnChanges, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { DropdownListModel, ModeOfTransportType, StringHelper } from 'src/app/core';
@Component({
    selector: 'app-consolidation-popup',
    templateUrl: './consolidation-popup.component.html',
    styleUrls: ['./consolidation-popup.component.scss']
})
export class ConsolidationPopupComponent implements OnChanges {
    /** Define whether popup should open */
    @Input()
    popupOpened: boolean = false;

    @Input()
    consignments: any;

    /** Execute as the popup closed */
    @Output()
    close: EventEmitter<any> = new EventEmitter<any>();

    /** Execute as the add clicked */
    @Output()
    add: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild('mainForm', { static: false }) currentForm: NgForm;

    optionsDataSource: DropdownListModel<number>;
    selectedConsignmentId: number;

    public defaultDropDownItem: { label: string, value: number } =
        {
            label: this.translateService.instant('label.select'),
            value: null
        };

    constructor(public translateService: TranslateService) { }

    ngOnChanges() {
        // Filter on mode of transport Sea/Air
        this.consignments = this.consignments?.filter(c => StringHelper.caseIgnoredCompare(c.modeOfTransport, ModeOfTransportType.Sea) || StringHelper.caseIgnoredCompare(c.modeOfTransport, ModeOfTransportType.Air));
        this.optionsDataSource = this.consignments?.map(x => new DropdownListModel<number>(x.executionAgent, x.id));
    }

    onAddClick() {
        this.add.emit(this.selectedConsignmentId);
    }

    /** Based handlers as the popup closed */
    onFormClosed() {
        this.popupOpened = false;
        this.selectedConsignmentId = null;
        this.close.emit();
    }

    // convenience getter for easy access to form fields
    get f() { return this.currentForm?.controls; }
}
