/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
const UNTOUCHED = 'ng-untouched';
const toClassList = (classNames) => String(classNames).trim().split(' ');
const ɵ0 = toClassList;
/**
 * @hidden
 */
export const hasClass = (element, className) => Boolean(toClassList(element.className).find((name) => name === className));
/**
 * @hidden
 */
export function invokeElementMethod(element, name, ...args) {
    if (element && element.nativeElement) {
        return element.nativeElement[name].apply(element.nativeElement, args);
    }
}
/**
 * @hidden
 */
export const isUntouched = (element) => element && element.nativeElement && hasClass(element.nativeElement, UNTOUCHED);
/**
 * @hidden
 */
export const containsFocus = (hostElement, contender) => hostElement && contender && (hostElement === contender || hostElement.contains(contender));
/**
 * @hidden
 */
export const closest = (node, predicate) => {
    while (node && !predicate(node)) {
        node = node.parentNode;
    }
    return node;
};
export { ɵ0 };
