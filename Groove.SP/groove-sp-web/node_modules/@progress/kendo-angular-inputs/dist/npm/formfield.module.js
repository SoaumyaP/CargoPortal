/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var common_1 = require("@angular/common");
var formfield_component_1 = require("./formfield/formfield.component");
var hint_component_1 = require("./formfield/hint.component");
var error_component_1 = require("./formfield/error.component");
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the FormField, Error and Hint components.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the FormField module
 * import { FormFieldModule } from '@progress/kendo-angular-inputs';
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
 *     imports:      [BrowserModule, FormFieldModule], // import FormField module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
var FormFieldModule = /** @class */ (function () {
    function FormFieldModule() {
    }
    FormFieldModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [hint_component_1.HintComponent, error_component_1.ErrorComponent, formfield_component_1.FormFieldComponent],
                    exports: [hint_component_1.HintComponent, error_component_1.ErrorComponent, formfield_component_1.FormFieldComponent],
                    imports: [common_1.CommonModule]
                },] },
    ];
    return FormFieldModule;
}());
exports.FormFieldModule = FormFieldModule;
