/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Injectable } from '@angular/core';
import { isDocumentAvailable } from '@progress/kendo-angular-common';
import { domContainerFactory as containerFactory } from '../../util';
var div = containerFactory('div');
var ul = containerFactory('ul');
var li = containerFactory('li');
var listItem = function () { return li('<span>02</span>', 'k-item'); };
var ɵ0 = listItem;
var list = function () { return ul([listItem()], 'k-reset'); };
var ɵ1 = list;
var scrollable = function () { return (div([list()], 'k-time-container k-content k-scrollable')); };
var ɵ2 = scrollable;
var timeListWrapper = function () {
    if (!isDocumentAvailable()) {
        return null;
    }
    return div([div([scrollable()], 'k-time-list')], 'k-time-list-wrapper', { left: '-10000px', position: 'absolute' });
};
var ɵ3 = timeListWrapper;
var TIMELIST_WRAPPER = timeListWrapper();
/**
 * @hidden
 */
var TimePickerDOMService = /** @class */ (function () {
    function TimePickerDOMService() {
    }
    TimePickerDOMService.prototype.ensureHeights = function () {
        if (this.timeListHeight !== undefined) {
            return;
        }
        this.calculateHeights();
    };
    TimePickerDOMService.prototype.calculateHeights = function (container) {
        if (!isDocumentAvailable()) {
            return;
        }
        var listContainer = container && container.querySelector('.k-time-list-container');
        var hostContainer = listContainer || document.body;
        var wrapper = hostContainer.appendChild(TIMELIST_WRAPPER);
        this.timeListHeight = wrapper.querySelector('.k-scrollable').getBoundingClientRect().height;
        this.itemHeight = wrapper.querySelector('li').getBoundingClientRect().height;
        hostContainer.removeChild(wrapper);
    };
    TimePickerDOMService.prototype.isActive = function (element) {
        if (!isDocumentAvailable() || !element) {
            return false;
        }
        return (element.nativeElement || element) === document.activeElement;
    };
    TimePickerDOMService.decorators = [
        { type: Injectable },
    ];
    return TimePickerDOMService;
}());
export { TimePickerDOMService };
export { ɵ0, ɵ1, ɵ2, ɵ3 };
