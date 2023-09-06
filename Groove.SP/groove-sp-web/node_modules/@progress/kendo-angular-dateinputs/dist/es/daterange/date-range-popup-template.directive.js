/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, TemplateRef } from '@angular/core';
/**
 * A directive which renders the content of the DateRange Popup. To define the cell template, nest an
 * `<ng-template>` tag with the `kendoRangePopupTemplate` directive inside the component tag.
 */
var DateRangePopupTemplateDirective = /** @class */ (function () {
    function DateRangePopupTemplateDirective(templateRef) {
        this.templateRef = templateRef;
    }
    DateRangePopupTemplateDirective.decorators = [
        { type: Directive, args: [{
                    selector: '[kendoDateRangePopupTemplate]'
                },] },
    ];
    /** @nocollapse */
    DateRangePopupTemplateDirective.ctorParameters = function () { return [
        { type: TemplateRef }
    ]; };
    return DateRangePopupTemplateDirective;
}());
export { DateRangePopupTemplateDirective };
