/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
var UNTOUCHED = 'ng-untouched';
var toClassList = function (classNames) { return String(classNames).trim().split(' '); };
var ɵ0 = toClassList;
/**
 * @hidden
 */
export var hasClass = function (element, className) {
    return Boolean(toClassList(element.className).find(function (name) { return name === className; }));
};
/**
 * @hidden
 */
export function invokeElementMethod(element, name) {
    var args = [];
    for (var _i = 2; _i < arguments.length; _i++) {
        args[_i - 2] = arguments[_i];
    }
    if (element && element.nativeElement) {
        return element.nativeElement[name].apply(element.nativeElement, args);
    }
}
/**
 * @hidden
 */
export var isUntouched = function (element) {
    return element && element.nativeElement && hasClass(element.nativeElement, UNTOUCHED);
};
/**
 * @hidden
 */
export var containsFocus = function (hostElement, contender) {
    return hostElement && contender && (hostElement === contender || hostElement.contains(contender));
};
/**
 * @hidden
 */
export var closest = function (node, predicate) {
    while (node && !predicate(node)) {
        node = node.parentNode;
    }
    return node;
};
export { ɵ0 };
