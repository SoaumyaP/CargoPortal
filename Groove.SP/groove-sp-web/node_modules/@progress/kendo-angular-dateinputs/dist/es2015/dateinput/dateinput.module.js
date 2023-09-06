/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { DateInputComponent } from './dateinput.component';
import { CommonModule } from '@angular/common';
import { IntlModule } from '@progress/kendo-angular-intl';
import { DateInputLocalizedMessagesDirective } from './localization/dateinput-localized-messages.directive';
import { DateInputCustomMessagesComponent } from './localization/dateinput-custom-messages.component';
import { EventsModule } from '@progress/kendo-angular-common';
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the DateInput component.
 */
export class DateInputModule {
}
DateInputModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    DateInputComponent,
                    DateInputCustomMessagesComponent,
                    DateInputLocalizedMessagesDirective
                ],
                exports: [
                    DateInputComponent,
                    DateInputCustomMessagesComponent,
                    DateInputLocalizedMessagesDirective
                ],
                imports: [CommonModule, IntlModule, EventsModule]
            },] },
];
