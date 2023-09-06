import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { GridModule } from '@progress/kendo-angular-grid';
import { DropDownListModule, ComboBoxModule, AutoCompleteModule, DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { PopupModule } from '@progress/kendo-angular-popup';
import { DateInputModule, DateInputsModule, DatePickerModule } from '@progress/kendo-angular-dateinputs';
import { TreeViewModule } from '@progress/kendo-angular-treeview';
import { CustomGridPagerComponent } from './custom-grid-pager/custom-grid-pager.component';
import { DropDownListFilterComponent } from './drop-down-list-filter/drop-down-list-filter.component';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { StatusLabelComponent } from './status-label/status-label.component';
import { TooltipModule } from '@progress/kendo-angular-tooltip';
import { ShareFileDialogComponent } from './share-file-dialog/share-file-dialog.component';
import { TagInputModule } from 'ngx-chips';
import { MilestoneComponent } from './milestone/milestone.component';
import { LayoutModule as KendoLayoutModule, PanelBarModule } from '@progress/kendo-angular-layout';
import { SortableModule } from '@progress/kendo-angular-sortable';
import { TabStripModule } from '@progress/kendo-angular-layout';
import { ExpiredComponent } from './error/expired/expired.component';
import { InvalidSearchComponent } from './error/invalid-search/invalid-search.component';
import { MaintenanceComponent } from './error/maintenance/maintenance.component';
import { NoResultComponent } from './error/no-result/no-result.component';
import { NotFoundComponent } from './error/not-found/not-found.component';
import { UnauthorizedComponent } from './error/unauthorized/unauthorized.component';
import { RouterModule } from '@angular/router';
import { ImportFormComponent } from './import-form/import-form.component';
import { UploadModule } from '@progress/kendo-angular-upload';
import { ProgressBarModule } from 'ngx-progress-bar';
import { BackgroundJobMonitor } from './background-job/background-job';
import { HasPermissionDirective } from '../core/directives/has-permission.directive';
import { PolicyFormComponent } from '../features/compliance/policy-form/policy-form.component';
import { POCustomerFormComponent } from '../features/po-fulfillment/po-customer-form/po-customer-form.component';
import { ProductDetailPopupComponent } from '../features/order/product-detail-popup/product-detail-popup.component';
import { LoaderComponent } from './loader/loader.component';
import { LoaderService } from './loader/loader.service';
import { ClipboardDirective } from '../core/directives/clipboard.directive';
import { AttachmentUploadPopupComponent } from './attachment-upload-popup/attachment-upload-popup.component';
import { ChartsModule } from '@progress/kendo-angular-charts';
import { DefaultPipe } from '../core/pipes/default.pipe';
import { SelectPOsFormComponent } from './select-pos-form/select-pos-form.component';
import { SelectPosFormService } from './select-pos-form/select-pos-form.service';
import { SpinnerComponent } from './spinner/spinner.component';
import { SpinnerDirective } from '../core/directives/spinner.directive';
import { ConvertUTCDateToLocalDatePipe } from '../core/pipes/convert-utc-to-local-date.pipe';
import { PositionUpDownDirective } from '../core/directives/position-up-down.directive';
import { OrderByPipe } from '../core/pipes/order-by.pipe';
import { AppGreaterThanDirective } from '../core/directives/value-validator.directive';
import { POFulfillmentCustomerOrderPipe } from '../features/po-fulfillment/po-fulfillment-customer/po-fulfillment-customer-order.pipe';
import { MissingPOFulfillmentCustomerOrderPipe } from '../features/missing-po-fulfillment/missing-po-fulfillment-customer/missing-po-fulfillment-customer-order.pipe';
import { LineBreakPipe } from '../core/pipes/line-break.pipe';
import { EditableFieldComponent } from './editable-field/editable-field.component';
import { ActivityListComponent } from './activity-list/activity-list.component';
import { ActivityFormComponent } from './activity-list/activity-form/activity-form.component';
import { UTCDateTimeFormatPipe } from '../core/pipes/utcDateTime.pipe';
import { PopupComponent } from './popup/popup.component';
import { CruiseOrderItemDetailPopupComponent } from '../features/cruise-order/cruise-order-item/popups/cruise-order-item-detail-popup/cruise-order-item-detail-popup.component';
import { ShowLessPipe } from '../core/pipes/show-less.pipe';
import { TabContentLoadOnDemandDirective } from '../core/directives/lazyload.directive';
import { CargoDescriptionDetailPopupComponent } from './cargo-description-detail-popup/cargo-description-detail-popup.component';
import { ContactSequencePipe } from '../core/pipes/contactSequence.pipe';
import { AssignPOsFormComponent } from './assign-pos-form/assign-pos-form.component';
import { AssignPOsFormService } from './assign-pos-form/assign-pos-form.service';
import { MAWBNumberFormatPipe } from '../core/pipes/mawb-number-format.pipe';
import { ImportDataPopupComponent } from './import-data-popup/import-data-popup.component';
import { TwoStepImportDataPopupComponent } from './two-step-import-data-popup/two-step-import-data-popup.component';
import { PagerComponent } from './pager/pager.component';
import { BreakOnPipe } from '../core/pipes/break-on.pipe';
import { BreakLineDirective } from '../core/directives/break-line.directive';
import { ScrollToTopComponent } from './scroll-to-top/scroll-to-top.component';
import { DynamicNotificationPopupComponent } from './dynamic-notification-popup/dynamic-notification-popup.component';
import { DynamicMilestoneComponent } from './dynamic-milestone/dynamic-milestone.component';
import { ActivityTimelineComponent } from './activity-timeline/activity-timeline.component';
import { ActivityTimelineService } from './activity-timeline/activity-timeline.service';

/*
    Here is the place to put Reusable UI components
*/
@NgModule({
    declarations: [
        CustomGridPagerComponent,
        DropDownListFilterComponent,
        StatusLabelComponent,
        ShareFileDialogComponent,
        MilestoneComponent,
        ExpiredComponent,
        InvalidSearchComponent,
        MaintenanceComponent,
        NoResultComponent,
        NotFoundComponent,
        UnauthorizedComponent,
        ImportFormComponent,
        AttachmentUploadPopupComponent,
        CargoDescriptionDetailPopupComponent,
        PolicyFormComponent,
        ProductDetailPopupComponent,
        CruiseOrderItemDetailPopupComponent,
        POCustomerFormComponent,
        HasPermissionDirective,
        SpinnerDirective,
        ClipboardDirective,
        LoaderComponent,
        DefaultPipe,
        OrderByPipe,
        MAWBNumberFormatPipe,
        UTCDateTimeFormatPipe,
        POFulfillmentCustomerOrderPipe,
        MissingPOFulfillmentCustomerOrderPipe,
        ConvertUTCDateToLocalDatePipe,
        LineBreakPipe,
        BreakOnPipe,
        ShowLessPipe,
        ContactSequencePipe,
        SelectPOsFormComponent,
        AssignPOsFormComponent,
        SpinnerComponent,
        PositionUpDownDirective,
        AppGreaterThanDirective,
        EditableFieldComponent,
        ActivityListComponent,
        ActivityFormComponent,
        PopupComponent,
        TabContentLoadOnDemandDirective,
        ImportDataPopupComponent,
        TwoStepImportDataPopupComponent,
        PagerComponent,
        BreakLineDirective,
        ScrollToTopComponent,
        DynamicNotificationPopupComponent,
        DynamicMilestoneComponent,
        ActivityTimelineComponent
        ],
    imports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        GridModule,
        DropDownListModule,
        AutoCompleteModule,
        ComboBoxModule,
        TreeViewModule,
        InputsModule,
        TabStripModule,
        ButtonsModule,
        DatePickerModule,
        DialogModule,
        PopupModule,
        DateInputModule,
        FontAwesomeModule,
        TooltipModule,
        DropDownsModule,
        TagInputModule,
        RouterModule,
        UploadModule,
        ProgressBarModule,
        ChartsModule,
        DateInputsModule,
        PanelBarModule
    ],
    exports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        GridModule,
        DropDownListModule,
        AutoCompleteModule,
        ComboBoxModule,
        TreeViewModule,
        InputsModule,
        TabStripModule,
        ButtonsModule,
        DatePickerModule,
        DialogModule,
        PopupModule,
        DateInputModule,
        FontAwesomeModule,
        DropDownsModule,
        CustomGridPagerComponent,
        DropDownListFilterComponent,
        StatusLabelComponent,
        TooltipModule,
        ShareFileDialogComponent,
        MilestoneComponent,
        KendoLayoutModule,
        ImportFormComponent,
        SelectPOsFormComponent,
        AssignPOsFormComponent,
        AttachmentUploadPopupComponent,
        CargoDescriptionDetailPopupComponent,
        PolicyFormComponent,
        ProductDetailPopupComponent,
        CruiseOrderItemDetailPopupComponent,
        POCustomerFormComponent,
        HasPermissionDirective,
        SpinnerDirective,
        ClipboardDirective,
        UploadModule,
        LoaderComponent,
        ChartsModule,
        DefaultPipe,
        OrderByPipe,
        MAWBNumberFormatPipe,
        UTCDateTimeFormatPipe,
        POFulfillmentCustomerOrderPipe,
        MissingPOFulfillmentCustomerOrderPipe,
        ConvertUTCDateToLocalDatePipe,
        LineBreakPipe,
        BreakOnPipe,
        ShowLessPipe,
        ContactSequencePipe,
        SpinnerComponent,
        PositionUpDownDirective,
        AppGreaterThanDirective,
        EditableFieldComponent,
        ActivityListComponent,
        PopupComponent,
        DateInputsModule,
        TabContentLoadOnDemandDirective,
        PanelBarModule,
        SortableModule,
        ImportDataPopupComponent,
        TwoStepImportDataPopupComponent,
        PagerComponent,
        BreakLineDirective,
        ScrollToTopComponent,
        DynamicNotificationPopupComponent,
        DynamicMilestoneComponent,
        ActivityTimelineComponent
    ],
    entryComponents: [
        SpinnerComponent
    ]
})
export class UiModule {
    static forRoot(): ModuleWithProviders  {
        return {
            ngModule: UiModule,
            providers: [
                // Provide services needed by UI module itself
                DatePipe,
                BackgroundJobMonitor,
                LoaderService,
                SelectPosFormService,
                AssignPOsFormService,
                ActivityTimelineService
            ]
        };
    }
}
