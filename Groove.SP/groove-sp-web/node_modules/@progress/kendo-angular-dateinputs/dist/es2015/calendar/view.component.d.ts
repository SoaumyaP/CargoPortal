/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ChangeDetectorRef, EventEmitter, OnChanges, OnDestroy, TemplateRef, Renderer2, NgZone, ElementRef } from '@angular/core';
import { CellContext } from './models/cell-context.interface';
import { ViewService } from './models/view-service.interface';
import { CalendarViewEnum } from './models/view.enum';
import { SelectionRangeEnd } from './models/selection-range-end.type';
import { SelectionRange } from './models/selection-range.interface';
import { BusViewService } from './services/bus-view.service';
import { WeekNamesService } from './services/weeknames.service';
import { DisabledDatesService } from './services/disabled-dates.service';
import { IntlService } from '@progress/kendo-angular-intl';
/**
 * @hidden
 */
export declare class ViewComponent implements OnChanges, OnDestroy {
    bus: BusViewService;
    intl: IntlService;
    private cdr;
    private weekService;
    private element;
    private zone;
    private renderer;
    private disabledDatesService;
    direction: 'horizontal' | 'vertical';
    isActive: boolean;
    activeView: CalendarViewEnum;
    cellUID: string;
    focusedDate: Date;
    selectedDate: Date;
    viewDate: Date;
    activeRangeEnd: SelectionRangeEnd;
    selectionRange: SelectionRange;
    min: Date;
    max: Date;
    weekNumber: boolean;
    viewIndex: number;
    templateRef: TemplateRef<any>;
    weekNumberTemplateRef: TemplateRef<any>;
    change: EventEmitter<Date>;
    cellEnter: EventEmitter<Date>;
    cellLeave: EventEmitter<Date>;
    weekNames: string[];
    colSpan: number;
    data: CellContext[][];
    service: ViewService;
    title: string;
    private subscriptions;
    private showWeekNumbers;
    private domEvents;
    private currentCellIndex;
    constructor(bus: BusViewService, intl: IntlService, cdr: ChangeDetectorRef, weekService: WeekNamesService, element: ElementRef, zone: NgZone, renderer: Renderer2, disabledDatesService: DisabledDatesService);
    ngOnInit(): void;
    ngOnChanges(changes: any): void;
    ngOnDestroy(): void;
    isHorizontal(): boolean;
    isMonthView(): boolean;
    firstDate(rowCtx: CellContext[]): Date;
    getWeekNumber(date: Date): number;
    getWeekNumberContext(rowCtx: CellContext[]): CellContext;
    getStyles(context: CellContext): any;
    tableCellIndex(rowIndex: number, cellIndex: number): string;
    private firstWeekDateContext;
    private updateData;
    private intlChange;
    private disabledDatesChange;
    private bindEvents;
    private clickHandler;
    private mouseLeaveHandler;
    private cellMouseoverHandler;
    private closestCell;
    private emitCellLeave;
    private cellByIndex;
}
