/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var common_1 = require("@angular/common");
var sliders_common_module_1 = require("./sliders-common/sliders-common.module");
var rangeslider_component_1 = require("./rangeslider/rangeslider.component");
var localized_rangeslider_messages_directive_1 = require("./rangeslider/localization/localized-rangeslider-messages.directive");
var custom_messages_component_1 = require("./rangeslider/localization/custom-messages.component");
var label_template_directive_1 = require("./sliders-common/label-template.directive");
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the RangeSlider component.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Inputs module
 * import { RangeSliderModule } from '@progress/kendo-angular-inputs';
 *
 * // The browser platform with a compiler
 * import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
 * import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
 *
 * import { NgModule } from '@angular/core';
 *
 * // Import the app component
 * import { AppComponent } from './app.component';
 *
 * // Define the app module
 * _@NgModule({
 *     declarations: [AppComponent], // declare app component
 *     imports:      [BrowserModule, BrowserAnimationsModule, RangeSliderModule], // import RangeSlider module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
var RangeSliderModule = /** @class */ (function () {
    function RangeSliderModule() {
    }
    RangeSliderModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [
                        rangeslider_component_1.RangeSliderComponent,
                        custom_messages_component_1.RangeSliderCustomMessagesComponent,
                        localized_rangeslider_messages_directive_1.LocalizedRangeSliderMessagesDirective
                    ],
                    exports: [
                        rangeslider_component_1.RangeSliderComponent,
                        custom_messages_component_1.RangeSliderCustomMessagesComponent,
                        localized_rangeslider_messages_directive_1.LocalizedRangeSliderMessagesDirective,
                        label_template_directive_1.LabelTemplateDirective
                    ],
                    imports: [common_1.CommonModule, sliders_common_module_1.SlidersCommonModule]
                },] },
    ];
    return RangeSliderModule;
}());
exports.RangeSliderModule = RangeSliderModule;
