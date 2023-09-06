/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { ChangeDetectorRef, ElementRef, EventEmitter, OnChanges, OnDestroy, Renderer2, AfterViewChecked, AfterViewInit, TemplateRef } from '@angular/core';
import { IntlService } from '@progress/kendo-angular-intl';
import { BusViewService } from './services/bus-view.service';
import { CalendarDOMService } from './services/dom.service';
import { CalendarView } from './models/view.type';
import { CalendarViewEnum } from './models/view.enum';
import { ViewService } from './models/view-service.interface';
import { VirtualizationComponent } from '../virtualization/virtualization.component';
import { PageAction, ScrollAction } from '../virtualization/services/scroller.service';
/**
 * @hidden
 */
export declare class NavigationComponent implements OnChanges, OnDestroy, AfterViewInit, AfterViewChecked {
    private bus;
    private dom;
    private intl;
    private cdr;
    private renderer;
    activeView: CalendarViewEnum;
    min: Date;
    max: Date;
    focusedDate: Date;
    templateRef: TemplateRef<any>;
    valueChange: EventEmitter<Date>;
    pageChange: EventEmitter<any>;
    virtualization: VirtualizationComponent;
    list: ElementRef;
    readonly getComponentClass: boolean;
    activeViewValue: CalendarView;
    service: ViewService;
    dates: Date[];
    style: any;
    take: number;
    skip: number;
    total: number;
    itemHeight: number;
    topOffset: number;
    bottomOffset: number;
    maxViewHeight: number;
    private indexToScroll;
    private intlSubscription;
    constructor(bus: BusViewService, dom: CalendarDOMService, intl: IntlService, cdr: ChangeDetectorRef, renderer: Renderer2);
    ngOnInit(): void;
    ngOnChanges(changes: any): void;
    ngOnDestroy(): void;
    ngAfterViewInit(): void;
    ngAfterViewChecked(): void;
    onPageChange({ skip }: PageAction): void;
    scrollChange({ offset }: ScrollAction): void;
    handleDateChange(args: any): void;
    private getTake;
    private intlChange;
}
