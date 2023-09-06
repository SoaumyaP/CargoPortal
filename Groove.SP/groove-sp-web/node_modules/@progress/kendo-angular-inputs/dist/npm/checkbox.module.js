/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var checkbox_directive_1 = require("./checkbox/checkbox.directive");
var common_1 = require("@angular/common");
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the CheckBox directive.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the CheckBox module
 * import { CheckBoxModule } from '@progress/kendo-angular-inputs';
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
 *     imports:      [BrowserModule, CheckBoxModule], // import CheckBox module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
var CheckBoxModule = /** @class */ (function () {
    function CheckBoxModule() {
    }
    CheckBoxModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [checkbox_directive_1.CheckBoxDirective],
                    exports: [checkbox_directive_1.CheckBoxDirective],
                    imports: [common_1.CommonModule]
                },] },
    ];
    return CheckBoxModule;
}());
exports.CheckBoxModule = CheckBoxModule;
