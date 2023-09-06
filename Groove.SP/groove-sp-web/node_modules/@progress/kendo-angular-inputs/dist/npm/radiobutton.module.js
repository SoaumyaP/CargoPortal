/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var radiobutton_directive_1 = require("./radiobutton/radiobutton.directive");
var common_1 = require("@angular/common");
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the RadioButton directive.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the RadioButton module
 * import { RadioButtonModule } from '@progress/kendo-angular-inputs';
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
 *     imports:      [BrowserModule, RadioButtonModule], // import RadioButton module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
var RadioButtonModule = /** @class */ (function () {
    function RadioButtonModule() {
    }
    RadioButtonModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [radiobutton_directive_1.RadioButtonDirective],
                    exports: [radiobutton_directive_1.RadioButtonDirective],
                    imports: [common_1.CommonModule]
                },] },
    ];
    return RadioButtonModule;
}());
exports.RadioButtonModule = RadioButtonModule;
