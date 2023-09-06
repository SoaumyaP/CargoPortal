/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from "@angular/core";
import { TextBoxDirective } from "./textbox/textbox.directive";
import { TextAreaDirective } from "./textbox/textarea.directive";
import { TextBoxContainerComponent } from "./textbox/textbox-container.component";
import { TextBoxComponent } from "./textbox/textbox.component";
import { CommonModule } from "@angular/common";
import { TextBoxSeparatorComponent } from "./textbox/textbox-separator.component";
import { EventsModule } from "@progress/kendo-angular-common";
import { TextBoxSuffixTemplateDirective } from "./textbox/textbox-suffix.directive";
import { TextBoxPrefixTemplateDirective } from './textbox/textbox-prefix.directive';
import { TextBoxCustomMessagesComponent } from './textbox/localization/custom-messages.component';
import { LocalizedTextBoxMessagesDirective } from './textbox/localization/localized-textbox-messages.directive';
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the TextBox directive.
 *
 * @example
 *
 * ```ts-no-run
 * // Import the TextBox module
 * import { TextBoxModule } from '@progress/kendo-angular-inputs';
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
 *     imports:      [BrowserModule, TextBoxModule], // import TextBox module
 *     bootstrap:    [AppComponent]
 * })
 * export class AppModule {}
 *
 * // Compile and launch the module
 * platformBrowserDynamic().bootstrapModule(AppModule);
 *
 * ```
 */
export class TextBoxModule {
}
TextBoxModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    TextBoxDirective,
                    TextAreaDirective,
                    TextBoxContainerComponent,
                    TextBoxComponent,
                    TextBoxSeparatorComponent,
                    TextBoxSuffixTemplateDirective,
                    TextBoxPrefixTemplateDirective,
                    TextBoxCustomMessagesComponent,
                    LocalizedTextBoxMessagesDirective
                ],
                exports: [
                    TextBoxDirective,
                    TextAreaDirective,
                    TextBoxContainerComponent,
                    TextBoxComponent,
                    TextBoxSeparatorComponent,
                    TextBoxSuffixTemplateDirective,
                    TextBoxPrefixTemplateDirective,
                    EventsModule,
                    TextBoxCustomMessagesComponent,
                    LocalizedTextBoxMessagesDirective
                ],
                imports: [CommonModule, EventsModule]
            },] },
];
