/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SliderComponent } from './slider/slider.component';
import { SlidersCommonModule } from './sliders-common/sliders-common.module';
import { LocalizedSliderMessagesDirective } from './slider/localization/localized-slider-messages.directive';
import { SliderCustomMessagesComponent } from './slider/localization/custom-messages.component';
import { LabelTemplateDirective } from './sliders-common/label-template.directive';
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
export class SliderModule {
}
SliderModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    SliderComponent,
                    SliderCustomMessagesComponent,
                    LocalizedSliderMessagesDirective
                ],
                exports: [
                    SliderComponent,
                    SliderCustomMessagesComponent,
                    LabelTemplateDirective,
                    LocalizedSliderMessagesDirective
                ],
                imports: [CommonModule, SlidersCommonModule]
            },] },
];
