/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { TemplateRef } from '@angular/core';
/**
 * A directive which renders the content of the DateRange Popup. To define the cell template, nest an
 * `<ng-template>` tag with the `kendoRangePopupTemplate` directive inside the component tag.
 */
export declare class DateRangePopupTemplateDirective {
    templateRef: TemplateRef<any>;
    constructor(templateRef: TemplateRef<any>);
}
