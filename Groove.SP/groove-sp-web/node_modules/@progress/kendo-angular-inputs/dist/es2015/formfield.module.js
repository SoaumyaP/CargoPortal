/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormFieldComponent } from './formfield/formfield.component';
import { HintComponent } from './formfield/hint.component';
import { ErrorComponent } from './formfield/error.component';
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
export class FormFieldModule {
}
FormFieldModule.decorators = [
    { type: NgModule, args: [{
                declarations: [HintComponent, ErrorComponent, FormFieldComponent],
                exports: [HintComponent, ErrorComponent, FormFieldComponent],
                imports: [CommonModule]
            },] },
];
