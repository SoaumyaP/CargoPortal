/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgModule } from '@angular/core';
import { DatePickerComponent } from './datepicker.component';
import { CommonModule } from '@angular/common';
import { IntlModule } from '@progress/kendo-angular-intl';
import { PopupModule } from '@progress/kendo-angular-popup';
import { CalendarModule } from '../calendar/calendar.module';
import { TemplatesModule } from '../calendar/templates.module';
import { DateInputModule } from '../dateinput/dateinput.module';
import { EventsModule } from '@progress/kendo-angular-common';
import { DatePickerLocalizedMessagesDirective } from './localization/datepicker-localized-messages.directive';
import { DatePickerCustomMessagesComponent } from './localization/datepicker-custom-messages.component';
import { touchEnabled } from '@progress/kendo-common';
import { TOUCH_ENABLED } from '../touch-enabled';
const ɵ0 = touchEnabled;
/**
 * Represents the [NgModule]({{ site.data.urls.angular['ngmoduleapi'] }})
 * definition for the DatePicker component.
 */
export class DatePickerModule {
}
DatePickerModule.decorators = [
    { type: NgModule, args: [{
                declarations: [
                    DatePickerComponent,
                    DatePickerCustomMessagesComponent,
                    DatePickerLocalizedMessagesDirective
                ],
                exports: [
                    DatePickerComponent,
                    DatePickerCustomMessagesComponent,
                    DatePickerLocalizedMessagesDirective,
                    TemplatesModule
                ],
                imports: [
                    CommonModule,
                    DateInputModule,
                    CalendarModule,
                    IntlModule,
                    PopupModule,
                    TemplatesModule,
                    EventsModule
                ],
                providers: [{ provide: TOUCH_ENABLED, useValue: ɵ0 }]
            },] },
];
export { ɵ0 };
