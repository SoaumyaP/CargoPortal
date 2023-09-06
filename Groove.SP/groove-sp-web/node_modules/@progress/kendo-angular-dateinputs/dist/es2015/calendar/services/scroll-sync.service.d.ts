/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { NgZone } from '@angular/core';
import { CalendarDOMService } from './dom.service';
import { Scrollable } from '../models/scrollable.interface';
import { CalendarViewEnum } from '../models/view.enum';
/**
 * @hidden
 */
export declare class ScrollSyncService {
    dom: CalendarDOMService;
    zone: NgZone;
    private divideByMagnitude;
    private powerByMagnitude;
    private navSubscription;
    private viewSubscription;
    private navigator;
    private view;
    constructor(dom: CalendarDOMService, zone: NgZone);
    configure(activeView: CalendarViewEnum): void;
    sync(navigator: Scrollable, view: Scrollable): void;
    scrollSiblingOf(scrolledElement: HTMLElement): void;
    siblingComponent(scrollableElement: HTMLElement): Scrollable;
    calculateScroll(component: Scrollable, scrollTop: number): number;
    destroy(): void;
    private unsubscribe;
}
