/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ChangeDetectorRef, EventEmitter, OnChanges, OnDestroy, OnInit, TemplateRef } from '@angular/core';
import { LocalizationService } from '@progress/kendo-angular-l10n';
import { IntlService } from '@progress/kendo-angular-intl';
import { CalendarView } from './models/view.type';
import { CalendarViewEnum } from './models/view.enum';
import { BusViewService } from './services/bus-view.service';
import { DisabledDatesService } from './services/disabled-dates.service';
/**
 * @hidden
 */
export declare class HeaderComponent implements OnChanges, OnInit, OnDestroy {
    private bus;
    private cdr;
    localization: LocalizationService;
    private intl;
    private disabledDatesService;
    navigate: boolean;
    todayAvailable: boolean;
    activeViewValue: CalendarView;
    todayMessage: string;
    title: string;
    activeView: CalendarViewEnum;
    currentDate: Date;
    min: Date;
    max: Date;
    rangeLength: number;
    templateRef: TemplateRef<any>;
    today: EventEmitter<Date>;
    readonly getComponentClass: boolean;
    private subscriptions;
    constructor(bus: BusViewService, cdr: ChangeDetectorRef, localization: LocalizationService, intl: IntlService, disabledDatesService: DisabledDatesService);
    ngOnInit(): void;
    ngOnChanges(_: any): void;
    ngOnDestroy(): void;
    handleTodayClick(): void;
    handleNavigation(): void;
    private intlChange;
    private l10nChange;
    private getTitle;
    private setTodayAvailability;
}
