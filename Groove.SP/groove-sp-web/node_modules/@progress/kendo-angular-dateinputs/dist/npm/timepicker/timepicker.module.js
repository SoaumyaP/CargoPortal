/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var common_1 = require("@angular/common");
var kendo_angular_intl_1 = require("@progress/kendo-angular-intl");
var kendo_angular_popup_1 = require("@progress/kendo-angular-popup");
var timelist_component_1 = require("./timelist.component");
var timepicker_component_1 = require("./timepicker.component");
var timeselector_component_1 = require("./timeselector.component");
var virtualization_module_1 = require("../virtualization/virtualization.module");
var dom_service_1 = require("./services/dom.service");
var hours_service_1 = require("./services/hours.service");
var minutes_service_1 = require("./services/minutes.service");
var seconds_service_1 = require("./services/seconds.service");
var dayperiod_service_1 = require("./services/dayperiod.service");
var dateinput_module_1 = require("../dateinput/dateinput.module");
var timepicker_localized_messages_directive_1 = require("./localization/timepicker-localized-messages.directive");
var timepicker_custom_messages_component_1 = require("./localization/timepicker-custom-messages.component");
var timeselector_localized_messages_directive_1 = require("./localization/timeselector-localized-messages.directive");
var timeselector_custom_messages_component_1 = require("./localization/timeselector-custom-messages.component");
var kendo_common_1 = require("@progress/kendo-common");
var touch_enabled_1 = require("../touch-enabled");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
var COMPONENT_DIRECTIVES = [
    timepicker_localized_messages_directive_1.TimePickerLocalizedMessagesDirective,
    timelist_component_1.TimeListComponent,
    timepicker_custom_messages_component_1.TimePickerCustomMessagesComponent,
    timepicker_component_1.TimePickerComponent,
    timeselector_localized_messages_directive_1.TimeSelectorLocalizedMessagesDirective,
    timeselector_custom_messages_component_1.TimeSelectorCustomMessagesComponent,
    timeselector_component_1.TimeSelectorComponent
];
var COMPONENT_MODULES = [
    dateinput_module_1.DateInputModule,
    kendo_angular_intl_1.IntlModule,
    kendo_angular_popup_1.PopupModule,
    virtualization_module_1.VirtualizationModule,
    kendo_angular_common_1.EventsModule
];
var ɵ0 = kendo_common_1.touchEnabled;
exports.ɵ0 = ɵ0;
var providers = [
    dom_service_1.TimePickerDOMService,
    hours_service_1.HoursService,
    minutes_service_1.MinutesService,
    seconds_service_1.SecondsService,
    dayperiod_service_1.DayPeriodService,
    {
        provide: touch_enabled_1.TOUCH_ENABLED,
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
        { type: core_1.NgModule, args: [{
                    declarations: [COMPONENT_DIRECTIVES],
                    exports: [COMPONENT_DIRECTIVES],
                    imports: [common_1.CommonModule].concat(COMPONENT_MODULES),
                    providers: providers
                },] },
    ];
    return TimePickerModule;
}());
exports.TimePickerModule = TimePickerModule;
