/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var common_1 = require("@angular/common");
var slider_component_1 = require("./slider/slider.component");
var sliders_common_module_1 = require("./sliders-common/sliders-common.module");
var localized_slider_messages_directive_1 = require("./slider/localization/localized-slider-messages.directive");
var custom_messages_component_1 = require("./slider/localization/custom-messages.component");
var label_template_directive_1 = require("./sliders-common/label-template.directive");
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the Slider component.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Inputs module
 * import { SliderModule } from '@progress/kendo-angular-inputs';
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
 *     imports:      [BrowserModule, BrowserAnimationsModule, SliderModule], // import Slider module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
var SliderModule = /** @class */ (function () {
    function SliderModule() {
    }
    SliderModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [
                        slider_component_1.SliderComponent,
                        custom_messages_component_1.SliderCustomMessagesComponent,
                        localized_slider_messages_directive_1.LocalizedSliderMessagesDirective
                    ],
                    exports: [
                        slider_component_1.SliderComponent,
                        custom_messages_component_1.SliderCustomMessagesComponent,
                        label_template_directive_1.LabelTemplateDirective,
                        localized_slider_messages_directive_1.LocalizedSliderMessagesDirective
                    ],
                    imports: [common_1.CommonModule, sliders_common_module_1.SlidersCommonModule]
                },] },
    ];
    return SliderModule;
}());
exports.SliderModule = SliderModule;
