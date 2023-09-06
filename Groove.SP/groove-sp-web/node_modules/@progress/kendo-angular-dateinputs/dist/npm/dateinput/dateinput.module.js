/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var dateinput_component_1 = require("./dateinput.component");
var common_1 = require("@angular/common");
var kendo_angular_intl_1 = require("@progress/kendo-angular-intl");
var dateinput_localized_messages_directive_1 = require("./localization/dateinput-localized-messages.directive");
var dateinput_custom_messages_component_1 = require("./localization/dateinput-custom-messages.component");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the DateInput component.
 */
var DateInputModule = /** @class */ (function () {
    function DateInputModule() {
    }
    DateInputModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [
                        dateinput_component_1.DateInputComponent,
                        dateinput_custom_messages_component_1.DateInputCustomMessagesComponent,
                        dateinput_localized_messages_directive_1.DateInputLocalizedMessagesDirective
                    ],
                    exports: [
                        dateinput_component_1.DateInputComponent,
                        dateinput_custom_messages_component_1.DateInputCustomMessagesComponent,
                        dateinput_localized_messages_directive_1.DateInputLocalizedMessagesDirective
                    ],
                    imports: [common_1.CommonModule, kendo_angular_intl_1.IntlModule, kendo_angular_common_1.EventsModule]
                },] },
    ];
    return DateInputModule;
}());
exports.DateInputModule = DateInputModule;
