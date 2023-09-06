/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { SwitchComponent } from './switch/switch.component';
import { CommonModule } from '@angular/common';
import { ResizeSensorModule } from '@progress/kendo-angular-common';
import { LocalizedSwitchMessagesDirective } from './switch/localization/localized-switch-messages.directive';
import { SwitchCustomMessagesComponent } from './switch/localization/custom-messages.component';
import { EventsModule } from '@progress/kendo-angular-common';
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
        { type: NgModule, args: [{
                    declarations: [
                        SwitchComponent,
                        SwitchCustomMessagesComponent,
                        LocalizedSwitchMessagesDirective
                    ],
                    exports: [
                        SwitchComponent,
                        SwitchCustomMessagesComponent,
                        LocalizedSwitchMessagesDirective
                    ],
                    imports: [CommonModule, EventsModule, ResizeSensorModule]
                },] },
    ];
    return SwitchModule;
}());
export { SwitchModule };
