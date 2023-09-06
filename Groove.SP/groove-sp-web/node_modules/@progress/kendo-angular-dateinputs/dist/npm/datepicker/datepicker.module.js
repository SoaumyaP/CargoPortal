/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var datepicker_component_1 = require("./datepicker.component");
var common_1 = require("@angular/common");
var kendo_angular_intl_1 = require("@progress/kendo-angular-intl");
var kendo_angular_popup_1 = require("@progress/kendo-angular-popup");
var calendar_module_1 = require("../calendar/calendar.module");
var templates_module_1 = require("../calendar/templates.module");
var dateinput_module_1 = require("../dateinput/dateinput.module");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
var datepicker_localized_messages_directive_1 = require("./localization/datepicker-localized-messages.directive");
var datepicker_custom_messages_component_1 = require("./localization/datepicker-custom-messages.component");
var kendo_common_1 = require("@progress/kendo-common");
var touch_enabled_1 = require("../touch-enabled");
var ɵ0 = kendo_common_1.touchEnabled;
exports.ɵ0 = ɵ0;
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the DatePicker component.
 */
var DatePickerModule = /** @class */ (function () {
    function DatePickerModule() {
    }
    DatePickerModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [
                        datepicker_component_1.DatePickerComponent,
                        datepicker_custom_messages_component_1.DatePickerCustomMessagesComponent,
                        datepicker_localized_messages_directive_1.DatePickerLocalizedMessagesDirective
                    ],
                    exports: [
                        datepicker_component_1.DatePickerComponent,
                        datepicker_custom_messages_component_1.DatePickerCustomMessagesComponent,
                        datepicker_localized_messages_directive_1.DatePickerLocalizedMessagesDirective,
                        templates_module_1.TemplatesModule
                    ],
                    imports: [
                        common_1.CommonModule,
                        dateinput_module_1.DateInputModule,
                        calendar_module_1.CalendarModule,
                        kendo_angular_intl_1.IntlModule,
                        kendo_angular_popup_1.PopupModule,
                        templates_module_1.TemplatesModule,
                        kendo_angular_common_1.EventsModule
                    ],
                    providers: [{ provide: touch_enabled_1.TOUCH_ENABLED, useValue: ɵ0 }]
                },] },
    ];
    return DatePickerModule;
}());
exports.DatePickerModule = DatePickerModule;
