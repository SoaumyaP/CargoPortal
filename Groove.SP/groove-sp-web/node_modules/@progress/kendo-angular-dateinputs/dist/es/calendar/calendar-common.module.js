/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { KForOf } from './for.directive';
import { HeaderComponent } from './header.component';
import { ViewComponent } from './view.component';
import { EventsModule } from '@progress/kendo-angular-common';
/**
 * @hidden
 *
 * The exported package module.
 *
 * The package exports:
 * - `HeaderComponent`&mdash;The component that renders the UI for vertical navigation.
 * - `ViewComponent`&mdash;The component that renders the active Calendar view.
 */
var CalendarCommonModule = /** @class */ (function () {
    function CalendarCommonModule() {
    }
    CalendarCommonModule.decorators = [
        { type: NgModule, args: [{
                    declarations: [
                        KForOf,
                        HeaderComponent,
                        ViewComponent
                    ],
                    exports: [
                        KForOf,
                        HeaderComponent,
                        ViewComponent
                    ],
                    imports: [CommonModule, EventsModule]
                },] },
    ];
    return CalendarCommonModule;
}());
export { CalendarCommonModule };
