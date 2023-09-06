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
var DateRangeComponent = /** @class */ (function () {
    function DateRangeComponent() {
        /**
         * @hidden
         */
        this.showDefault = false;
    }
    Object.defineProperty(DateRangeComponent.prototype, "hasContentPopup", {
        get: function () {
            return this.contentPopup.length > 0;
        },
        enumerable: true,
        configurable: true
    });
    DateRangeComponent.prototype.ngAfterContentInit = function () {
        var _this = this;
        this.showDefault = !this.hasContentPopup;
        this.subscription = this.contentPopup.changes.subscribe(function () {
            _this.showDefault = !_this.hasContentPopup;
        });
    };
    DateRangeComponent.prototype.ngOnDestroy = function () {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    };
    DateRangeComponent.decorators = [
        { type: Component, args: [{
                    providers: [DateRangeService],
                    selector: 'kendo-daterange',
                    template: "\n        <ng-content></ng-content>\n        <kendo-daterange-popup *ngIf=\"showDefault\"></kendo-daterange-popup>\n    "
                },] },
    ];
    DateRangeComponent.propDecorators = {
        contentPopup: [{ type: ContentChildren, args: [DateRangePopupComponent,] }]
    };
    return DateRangeComponent;
}());
export { DateRangeComponent };
