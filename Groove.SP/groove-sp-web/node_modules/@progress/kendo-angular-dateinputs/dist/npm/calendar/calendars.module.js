/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var calendar_module_1 = require("./calendar.module");
var multiview_calendar_module_1 = require("./multiview-calendar.module");
/**
 * The exported package module.
 *
 * The package exports:
 * - `CalendarModule`&mdash;The calendar module.
 * - `MultiViewCalendarModule`&mdash;The multi-view calendar module.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Calendars module
 * import { CalendarsModule } from '@progress/kendo-angular-dateinputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, CalendarsModule], // import the Calendars module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
var CalendarsModule = /** @class */ (function () {
    function CalendarsModule() {
    }
    CalendarsModule.decorators = [
        { type: core_1.NgModule, args: [{
                    exports: [
                        calendar_module_1.CalendarModule,
                        multiview_calendar_module_1.MultiViewCalendarModule
                    ],
                    imports: [
                        calendar_module_1.CalendarModule,
                        multiview_calendar_module_1.MultiViewCalendarModule
                    ]
                },] },
    ];
    return CalendarsModule;
}());
exports.CalendarsModule = CalendarsModule;
