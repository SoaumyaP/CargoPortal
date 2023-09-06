/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { AfterContentInit, OnDestroy } from '@angular/core';
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
export declare class DateRangeComponent implements AfterContentInit, OnDestroy {
    /**
     * @hidden
     */
    showDefault: boolean;
    private contentPopup;
    private readonly hasContentPopup;
    private subscription;
    ngAfterContentInit(): void;
    ngOnDestroy(): void;
}
