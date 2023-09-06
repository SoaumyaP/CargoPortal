/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SlidersCommonModule } from './sliders-common/sliders-common.module';
import { RangeSliderComponent } from './rangeslider/rangeslider.component';
import { LocalizedRangeSliderMessagesDirective } from './rangeslider/localization/localized-rangeslider-messages.directive';
import { RangeSliderCustomMessagesComponent } from './rangeslider/localization/custom-messages.component';
import { LabelTemplateDirective } from './sliders-common/label-template.directive';
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
export class RangeSliderModule {
}
RangeSliderModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    RangeSliderComponent,
                    RangeSliderCustomMessagesComponent,
                    LocalizedRangeSliderMessagesDirective
                ],
                exports: [
                    RangeSliderComponent,
                    RangeSliderCustomMessagesComponent,
                    LocalizedRangeSliderMessagesDirective,
                    LabelTemplateDirective
                ],
                imports: [CommonModule, SlidersCommonModule]
            },] },
];
