/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Directive, TemplateRef } from '@angular/core';
/**
 * Used for rendering the header title of the Calendar. To define the header title template, nest an `<ng-template>` tag
 * with the `kendoCalendarHeaderTitleTemplate` directive inside the component tag. The template context is set to the
 * current component. To get a reference to the current title, use the `let-title` directive. To provide more details about
 * the current title, get a reference to the current `date` by using the `let-date` directive or get a reference to the
 * current active view  by using the `let-activeView` directive.
 *
 * For more examples, refer to the article on [templates]({% slug templates_calendar %}).
 *
 * @example
 * ```ts
 * _@Component({
 * selector: 'my-app',
 * styles: ['.custom { color: red; }'],
 * template: `
 *  <kendo-calendar>
 *    <ng-template kendoCalendarHeaderTitleTemplate let-title>
 *      <span class="custom">{{title}}</span>
 *    </ng-template>
 *  </kendo-calendar>
 * `
 * })
 * export class AppComponent { }
 * ```
 */
export class HeaderTitleTemplateDirective {
    constructor(templateRef) {
        this.templateRef = templateRef;
    }
}
HeaderTitleTemplateDirective.decorators = [
    { type: Directive, args: [{
                selector: '[kendoCalendarHeaderTitleTemplate]'
            },] },
];
/** @nocollapse */
HeaderTitleTemplateDirective.ctorParameters = () => [
    { type: TemplateRef }
];
