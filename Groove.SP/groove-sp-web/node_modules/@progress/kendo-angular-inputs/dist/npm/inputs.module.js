/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var slider_module_1 = require("./slider.module");
var rangeslider_module_1 = require("./rangeslider.module");
var switch_module_1 = require("./switch.module");
var numerictextbox_module_1 = require("./numerictextbox.module");
var maskedtextbox_module_1 = require("./maskedtextbox.module");
var textbox_module_1 = require("./textbox.module");
var common_1 = require("@angular/common");
var colorpicker_module_1 = require("./colorpicker.module");
var checkbox_module_1 = require("./checkbox.module");
var radiobutton_module_1 = require("./radiobutton.module");
var formfield_module_1 = require("./formfield.module");
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the Inputs components.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the Inputs module
 * import { InputsModule } from '@progress/kendo-angular-inputs';
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
 *     imports:      [BrowserModule, BrowserAnimationsModule, InputsModule], // import Inputs module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
var InputsModule = /** @class */ (function () {
    function InputsModule() {
    }
    InputsModule.decorators = [
        { type: core_1.NgModule, args: [{
                    exports: [textbox_module_1.TextBoxModule, slider_module_1.SliderModule, rangeslider_module_1.RangeSliderModule, switch_module_1.SwitchModule, numerictextbox_module_1.NumericTextBoxModule, maskedtextbox_module_1.MaskedTextBoxModule, colorpicker_module_1.ColorPickerModule, checkbox_module_1.CheckBoxModule, radiobutton_module_1.RadioButtonModule, formfield_module_1.FormFieldModule],
                    imports: [common_1.CommonModule]
                },] },
    ];
    return InputsModule;
}());
exports.InputsModule = InputsModule;
