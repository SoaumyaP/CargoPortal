/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var core_1 = require("@angular/core");
/**
 * @hidden
 */
var FloatingLabelInputAdapter = /** @class */ (function () {
    function FloatingLabelInputAdapter(component, formControl) {
        this.component = component;
        var isObservableOrEventEmitter = function (event) { return event instanceof rxjs_1.Observable || event instanceof core_1.EventEmitter; };
        if (isObservableOrEventEmitter(component.onFocus)) {
            this.onFocus = component.onFocus;
        }
        if (isObservableOrEventEmitter(component.autoFillStart)) {
            this.autoFillStart = component.autoFillStart;
        }
        if (isObservableOrEventEmitter(component.autoFillEnd)) {
            this.autoFillEnd = component.autoFillEnd;
        }
        if (isObservableOrEventEmitter(component.onBlur)) {
            this.onBlur = component.onBlur;
        }
        if (formControl) {
            this.onValueChange = formControl.valueChanges;
        }
        else if (component.onValueChange) {
            this.onValueChange = component.onValueChange;
        }
    }
    Object.defineProperty(FloatingLabelInputAdapter.prototype, "focusableId", {
        get: function () {
            var component = this.component;
            if ('focusableId' in component) {
                return component.focusableId;
            }
            else if ('id' in component) {
                return component.id;
            }
            return "";
        },
        set: function (value) {
            var component = this.component;
            if ('focusableId' in component) {
                component.focusableId = value;
            }
            else if ('id' in component) {
                component.id = value;
            }
        },
        enumerable: true,
        configurable: true
    });
    return FloatingLabelInputAdapter;
}());
exports.FloatingLabelInputAdapter = FloatingLabelInputAdapter;
