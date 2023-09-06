/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var textbox_directive_1 = require("./textbox/textbox.directive");
var textarea_directive_1 = require("./textbox/textarea.directive");
var textbox_container_component_1 = require("./textbox/textbox-container.component");
var textbox_component_1 = require("./textbox/textbox.component");
var common_1 = require("@angular/common");
var textbox_separator_component_1 = require("./textbox/textbox-separator.component");
var kendo_angular_common_1 = require("@progress/kendo-angular-common");
var textbox_suffix_directive_1 = require("./textbox/textbox-suffix.directive");
var textbox_prefix_directive_1 = require("./textbox/textbox-prefix.directive");
var custom_messages_component_1 = require("./textbox/localization/custom-messages.component");
var localized_textbox_messages_directive_1 = require("./textbox/localization/localized-textbox-messages.directive");
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
var TextBoxModule = /** @class */ (function () {
    function TextBoxModule() {
    }
    TextBoxModule.decorators = [
        { type: core_1.NgModule, args: [{
                    declarations: [
                        textbox_directive_1.TextBoxDirective,
                        textarea_directive_1.TextAreaDirective,
                        textbox_container_component_1.TextBoxContainerComponent,
                        textbox_component_1.TextBoxComponent,
                        textbox_separator_component_1.TextBoxSeparatorComponent,
                        textbox_suffix_directive_1.TextBoxSuffixTemplateDirective,
                        textbox_prefix_directive_1.TextBoxPrefixTemplateDirective,
                        custom_messages_component_1.TextBoxCustomMessagesComponent,
                        localized_textbox_messages_directive_1.LocalizedTextBoxMessagesDirective
                    ],
                    exports: [
                        textbox_directive_1.TextBoxDirective,
                        textarea_directive_1.TextAreaDirective,
                        textbox_container_component_1.TextBoxContainerComponent,
                        textbox_component_1.TextBoxComponent,
                        textbox_separator_component_1.TextBoxSeparatorComponent,
                        textbox_suffix_directive_1.TextBoxSuffixTemplateDirective,
                        textbox_prefix_directive_1.TextBoxPrefixTemplateDirective,
                        kendo_angular_common_1.EventsModule,
                        custom_messages_component_1.TextBoxCustomMessagesComponent,
                        localized_textbox_messages_directive_1.LocalizedTextBoxMessagesDirective
                    ],
                    imports: [common_1.CommonModule, kendo_angular_common_1.EventsModule]
                },] },
    ];
    return TextBoxModule;
}());
exports.TextBoxModule = TextBoxModule;
