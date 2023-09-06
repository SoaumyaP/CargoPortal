/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IntlModule } from '@progress/kendo-angular-intl';
import { PopupModule } from '@progress/kendo-angular-popup';
import { CalendarCommonModule } from './calendar-common.module';
import { TemplatesModule } from './templates.module';
import { HorizontalViewListComponent } from './horizontal-view-list.component';
import { MultiViewCalendarComponent } from './multiview-calendar.component';
import { MultiViewCalendarLocalizedMessagesDirective } from './localization/multiview-calendar-localized-messages.directive';
import { MultiViewCalendarCustomMessagesComponent } from './localization/multiview-calendar-custom-messages.component';
import { NavigationService } from './services/navigation.service';
import { CenturyViewService } from './services/century-view.service';
import { DecadeViewService } from './services/decade-view.service';
import { MonthViewService } from './services/month-view.service';
import { YearViewService } from './services/year-view.service';
import { WeekNamesService } from './services/weeknames.service';
/**
 * The package exports:
 * - `CellTemplateDirective`&mdash;The month cell template directive.
 * - `MonthCellTemplateDirective`&mdash;The month cell template directive.
 * - `YearCellTemplateDirective`&mdash;The year cell template directive.
 * - `DecadeCellTemplateDirective`&mdash;The decade cell template directive.
 * - `CenturyCellTemplateDirective`&mdash;The century cell template directive.
 * - `WeekNumberCellTemplateDirective`&mdash;The month week number cell template directive.
 * - `HeaderTitleTemplateDirective`&mdash;The header title template directive.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the MultiViewCalendar module
 * import { MultiViewCalendarModule } from '@progress/kendo-angular-dateinputs';
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
 *     imports:      [BrowserModule, MultiViewCalendarModule], // import MultiViewCalendar module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 */
var MultiViewCalendarModule = /** @class */ (function () {
    function MultiViewCalendarModule() {
    }
    MultiViewCalendarModule.decorators = [
        { type: NgModule, args: [{
                    declarations: [
                        HorizontalViewListComponent,
                        MultiViewCalendarLocalizedMessagesDirective,
                        MultiViewCalendarCustomMessagesComponent,
                        MultiViewCalendarComponent
                    ],
                    exports: [
                        HorizontalViewListComponent,
                        MultiViewCalendarLocalizedMessagesDirective,
                        MultiViewCalendarCustomMessagesComponent,
                        MultiViewCalendarComponent,
                        CalendarCommonModule,
                        TemplatesModule
                    ],
                    imports: [CommonModule, CalendarCommonModule, IntlModule, TemplatesModule, PopupModule],
                    providers: [
                        NavigationService,
                        CenturyViewService,
                        DecadeViewService,
                        MonthViewService,
                        YearViewService,
                        WeekNamesService
                    ]
                },] },
    ];
    return MultiViewCalendarModule;
}());
export { MultiViewCalendarModule };
