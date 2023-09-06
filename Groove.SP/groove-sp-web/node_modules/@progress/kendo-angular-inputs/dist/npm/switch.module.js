/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var switch_component_1 = require("./switch/switch.component");
var common_1 = require("@angular/common");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
var localized_switch_messages_directive_1 = require("./switch/localization/localized-switch-messages.directive");
var custom_messages_component_1 = require("./switch/localization/custom-messages.component");
var kendo_angular_common_2 = require("@progress/kendo-angular-common");
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the Switch component.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Switch module
 * import { SwitchModule } from '@progress/kendo-angular-inputs';
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
 *     imports:      [BrowserModule, SwitchModule], // import Switch module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
var SwitchModule = /** @class */ (function () {
    function SwitchModule() {
    }
    SwitchModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [
                        switch_component_1.SwitchComponent,
                        custom_messages_component_1.SwitchCustomMessagesComponent,
                        localized_switch_messages_directive_1.LocalizedSwitchMessagesDirective
                    ],
                    exports: [
                        switch_component_1.SwitchComponent,
                        custom_messages_component_1.SwitchCustomMessagesComponent,
                        localized_switch_messages_directive_1.LocalizedSwitchMessagesDirective
                    ],
                    imports: [common_1.CommonModule, kendo_angular_common_2.EventsModule, kendo_angular_common_1.ResizeSensorModule]
                },] },
    ];
    return SwitchModule;
}());
exports.SwitchModule = SwitchModule;
