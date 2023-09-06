/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IntlModule } from '@progress/kendo-angular-intl';
import { PopupModule } from '@progress/kendo-angular-popup';
import { TimeListComponent } from './timelist.component';
import { TimePickerComponent } from './timepicker.component';
import { TimeSelectorComponent } from './timeselector.component';
import { VirtualizationModule } from '../virtualization/virtualization.module';
import { TimePickerDOMService } from './services/dom.service';
import { HoursService } from './services/hours.service';
import { MinutesService } from './services/minutes.service';
import { SecondsService } from './services/seconds.service';
import { DayPeriodService } from './services/dayperiod.service';
import { DateInputModule } from '../dateinput/dateinput.module';
import { TimePickerLocalizedMessagesDirective } from './localization/timepicker-localized-messages.directive';
import { TimePickerCustomMessagesComponent } from './localization/timepicker-custom-messages.component';
import { TimeSelectorLocalizedMessagesDirective } from './localization/timeselector-localized-messages.directive';
import { TimeSelectorCustomMessagesComponent } from './localization/timeselector-custom-messages.component';
import { touchEnabled } from '@progress/kendo-common';
import { TOUCH_ENABLED } from '../touch-enabled';
import { EventsModule } from '@progress/kendo-angular-common';
var COMPONENT_DIRECTIVES = [
    TimePickerLocalizedMessagesDirective,
    TimeListComponent,
    TimePickerCustomMessagesComponent,
    TimePickerComponent,
    TimeSelectorLocalizedMessagesDirective,
    TimeSelectorCustomMessagesComponent,
    TimeSelectorComponent
];
var COMPONENT_MODULES = [
    DateInputModule,
    IntlModule,
    PopupModule,
    VirtualizationModule,
    EventsModule
];
var ɵ0 = touchEnabled;
var providers = [
    TimePickerDOMService,
    HoursService,
    MinutesService,
    SecondsService,
    DayPeriodService,
    {
        provide: TOUCH_ENABLED,
        useValue: ɵ0
    }
];
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the TimePicker component.
 */
var TimePickerModule = /** @class */ (function () {
    function TimePickerModule() {
    }
    TimePickerModule.decorators = [
        { type: NgModule, args: [{
                    declarations: [COMPONENT_DIRECTIVES],
                    exports: [COMPONENT_DIRECTIVES],
                    imports: [CommonModule].concat(COMPONENT_MODULES),
                    providers: providers
                },] },
    ];
    return TimePickerModule;
}());
export { TimePickerModule };
export { ɵ0 };
