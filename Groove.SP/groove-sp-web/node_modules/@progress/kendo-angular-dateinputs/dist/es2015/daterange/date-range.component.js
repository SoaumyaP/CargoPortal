/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Component, ContentChildren, QueryList } from '@angular/core';
import { DateRangePopupComponent } from './date-range-popup.component';
import { DateRangeService } from './date-range.service';
/**
 * Represents the Kendo UI DateRange component for Angular.
 *
 * @example
 * ```ts
 * _@Component({
 * selector: 'my-app',
 * template: `
 *  <kendo-daterange>
 *      <kendo-dateinput kendoDateRangeStartInput [(value)]="dateRange.start"></kendo-dateinput>
 *      <kendo-dateinput kendoDateRangeEndInput [(value)]="dateRange.end"></kendo-dateinput>
 *  </kendo-daterange>
 * `
 * })
 * export class AppComponent {
 *   public dateRange: any = { start: null, end: null };
 * }
 * ```
 */
export class DateRangeComponent {
    constructor() {
        /**
         * @hidden
         */
        this.showDefault = false;
    }
    get hasContentPopup() {
        return this.contentPopup.length > 0;
    }
    ngAfterContentInit() {
        this.showDefault = !this.hasContentPopup;
        this.subscription = this.contentPopup.changes.subscribe(() => {
            this.showDefault = !this.hasContentPopup;
        });
    }
    ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }
}
DateRangeComponent.decorators = [
    { type: Component, args: [{
                providers: [DateRangeService],
                selector: 'kendo-daterange',
                template: `
        <ng-content></ng-content>
        <kendo-daterange-popup *ngIf="showDefault"></kendo-daterange-popup>
    `
            },] },
];
DateRangeComponent.propDecorators = {
    contentPopup: [{ type: ContentChildren, args: [DateRangePopupComponent,] }]
};
