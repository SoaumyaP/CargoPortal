/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,missingOverride,missingReturn,unusedPrivateMembers,uselessCode} checked by tsc
 */
import { Injectable } from '@angular/core';
var EqualityHelperService = /** @class */ (function () {
    function EqualityHelperService() {
    }
    /**
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    EqualityHelperService.prototype.areEqual = /**
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    function (value1, value2) {
        if (!value1 || !value2) {
            return false;
        }
        if (this.bothValuesAreArrays(value1, value2)) {
            return this.arraysEqual((/** @type {?} */ (value1)), (/** @type {?} */ (value2)));
        }
        if (this.bothValuesAreStrings(value1, value2)) {
            return value1 === value2;
        }
        if (this.bothValuesAreObjects(value1, value2)) {
            return JSON.stringify(value1).toLowerCase() === JSON.stringify(value2).toLowerCase();
        }
        if (this.oneValueIsStringAndTheOtherIsArray(value1, value2)) {
            if (Array.isArray(value1) && this.valueIsString(value2)) {
                return value1[0] === value2;
            }
            if (Array.isArray(value2) && this.valueIsString(value1)) {
                return value2[0] === value1;
            }
        }
    };
    /**
     * @private
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    EqualityHelperService.prototype.oneValueIsStringAndTheOtherIsArray = /**
     * @private
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    function (value1, value2) {
        return (Array.isArray(value1) && this.valueIsString(value2)) || (Array.isArray(value2) && this.valueIsString(value1));
    };
    /**
     * @private
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    EqualityHelperService.prototype.bothValuesAreObjects = /**
     * @private
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    function (value1, value2) {
        return this.valueIsObject(value1) && this.valueIsObject(value2);
    };
    /**
     * @private
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    EqualityHelperService.prototype.bothValuesAreStrings = /**
     * @private
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    function (value1, value2) {
        return this.valueIsString(value1) && this.valueIsString(value2);
    };
    /**
     * @private
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    EqualityHelperService.prototype.bothValuesAreArrays = /**
     * @private
     * @param {?} value1
     * @param {?} value2
     * @return {?}
     */
    function (value1, value2) {
        return Array.isArray(value1) && Array.isArray(value2);
    };
    /**
     * @private
     * @param {?} value
     * @return {?}
     */
    EqualityHelperService.prototype.valueIsString = /**
     * @private
     * @param {?} value
     * @return {?}
     */
    function (value) {
        return typeof value === 'string' || value instanceof String;
    };
    /**
     * @private
     * @param {?} value
     * @return {?}
     */
    EqualityHelperService.prototype.valueIsObject = /**
     * @private
     * @param {?} value
     * @return {?}
     */
    function (value) {
        return typeof value === 'object';
    };
    /**
     * @private
     * @param {?} arr1
     * @param {?} arr2
     * @return {?}
     */
    EqualityHelperService.prototype.arraysEqual = /**
     * @private
     * @param {?} arr1
     * @param {?} arr2
     * @return {?}
     */
    function (arr1, arr2) {
        if (arr1.length !== arr2.length) {
            return false;
        }
        for (var i = arr1.length; i--;) {
            if (arr1[i] !== arr2[i]) {
                return false;
            }
        }
        return true;
    };
    EqualityHelperService.decorators = [
        { type: Injectable }
    ];
    return EqualityHelperService;
}());
export { EqualityHelperService };
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoib2lkYy1lcXVhbGl0eS1oZWxwZXIuc2VydmljZS5qcyIsInNvdXJjZVJvb3QiOiJuZzovL2FuZ3VsYXItYXV0aC1vaWRjLWNsaWVudC8iLCJzb3VyY2VzIjpbImxpYi9zZXJ2aWNlcy9vaWRjLWVxdWFsaXR5LWhlbHBlci5zZXJ2aWNlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7QUFBQSxPQUFPLEVBQUUsVUFBVSxFQUFFLE1BQU0sZUFBZSxDQUFDO0FBRTNDO0lBQUE7SUFrRUEsQ0FBQzs7Ozs7O0lBaEVHLHdDQUFROzs7OztJQUFSLFVBQVMsTUFBa0QsRUFBRSxNQUFrRDtRQUMzRyxJQUFJLENBQUMsTUFBTSxJQUFJLENBQUMsTUFBTSxFQUFFO1lBQ3BCLE9BQU8sS0FBSyxDQUFDO1NBQ2hCO1FBRUQsSUFBSSxJQUFJLENBQUMsbUJBQW1CLENBQUMsTUFBTSxFQUFFLE1BQU0sQ0FBQyxFQUFFO1lBQzFDLE9BQU8sSUFBSSxDQUFDLFdBQVcsQ0FBQyxtQkFBQSxNQUFNLEVBQVMsRUFBRSxtQkFBQSxNQUFNLEVBQVMsQ0FBQyxDQUFDO1NBQzdEO1FBRUQsSUFBSSxJQUFJLENBQUMsb0JBQW9CLENBQUMsTUFBTSxFQUFFLE1BQU0sQ0FBQyxFQUFFO1lBQzNDLE9BQU8sTUFBTSxLQUFLLE1BQU0sQ0FBQztTQUM1QjtRQUVELElBQUksSUFBSSxDQUFDLG9CQUFvQixDQUFDLE1BQU0sRUFBRSxNQUFNLENBQUMsRUFBRTtZQUMzQyxPQUFPLElBQUksQ0FBQyxTQUFTLENBQUMsTUFBTSxDQUFDLENBQUMsV0FBVyxFQUFFLEtBQUssSUFBSSxDQUFDLFNBQVMsQ0FBQyxNQUFNLENBQUMsQ0FBQyxXQUFXLEVBQUUsQ0FBQztTQUN4RjtRQUVELElBQUksSUFBSSxDQUFDLGtDQUFrQyxDQUFDLE1BQU0sRUFBRSxNQUFNLENBQUMsRUFBRTtZQUN6RCxJQUFJLEtBQUssQ0FBQyxPQUFPLENBQUMsTUFBTSxDQUFDLElBQUksSUFBSSxDQUFDLGFBQWEsQ0FBQyxNQUFNLENBQUMsRUFBRTtnQkFDckQsT0FBTyxNQUFNLENBQUMsQ0FBQyxDQUFDLEtBQUssTUFBTSxDQUFDO2FBQy9CO1lBQ0QsSUFBSSxLQUFLLENBQUMsT0FBTyxDQUFDLE1BQU0sQ0FBQyxJQUFJLElBQUksQ0FBQyxhQUFhLENBQUMsTUFBTSxDQUFDLEVBQUU7Z0JBQ3JELE9BQU8sTUFBTSxDQUFDLENBQUMsQ0FBQyxLQUFLLE1BQU0sQ0FBQzthQUMvQjtTQUNKO0lBQ0wsQ0FBQzs7Ozs7OztJQUVPLGtFQUFrQzs7Ozs7O0lBQTFDLFVBQTJDLE1BQStCLEVBQUUsTUFBK0I7UUFDdkcsT0FBTyxDQUFDLEtBQUssQ0FBQyxPQUFPLENBQUMsTUFBTSxDQUFDLElBQUksSUFBSSxDQUFDLGFBQWEsQ0FBQyxNQUFNLENBQUMsQ0FBQyxJQUFJLENBQUMsS0FBSyxDQUFDLE9BQU8sQ0FBQyxNQUFNLENBQUMsSUFBSSxJQUFJLENBQUMsYUFBYSxDQUFDLE1BQU0sQ0FBQyxDQUFDLENBQUM7SUFDMUgsQ0FBQzs7Ozs7OztJQUVPLG9EQUFvQjs7Ozs7O0lBQTVCLFVBQTZCLE1BQStCLEVBQUUsTUFBK0I7UUFDekYsT0FBTyxJQUFJLENBQUMsYUFBYSxDQUFDLE1BQU0sQ0FBQyxJQUFJLElBQUksQ0FBQyxhQUFhLENBQUMsTUFBTSxDQUFDLENBQUM7SUFDcEUsQ0FBQzs7Ozs7OztJQUVPLG9EQUFvQjs7Ozs7O0lBQTVCLFVBQTZCLE1BQStCLEVBQUUsTUFBK0I7UUFDekYsT0FBTyxJQUFJLENBQUMsYUFBYSxDQUFDLE1BQU0sQ0FBQyxJQUFJLElBQUksQ0FBQyxhQUFhLENBQUMsTUFBTSxDQUFDLENBQUM7SUFDcEUsQ0FBQzs7Ozs7OztJQUVPLG1EQUFtQjs7Ozs7O0lBQTNCLFVBQTRCLE1BQStCLEVBQUUsTUFBK0I7UUFDeEYsT0FBTyxLQUFLLENBQUMsT0FBTyxDQUFDLE1BQU0sQ0FBQyxJQUFJLEtBQUssQ0FBQyxPQUFPLENBQUMsTUFBTSxDQUFDLENBQUM7SUFDMUQsQ0FBQzs7Ozs7O0lBRU8sNkNBQWE7Ozs7O0lBQXJCLFVBQXNCLEtBQVU7UUFDNUIsT0FBTyxPQUFPLEtBQUssS0FBSyxRQUFRLElBQUksS0FBSyxZQUFZLE1BQU0sQ0FBQztJQUNoRSxDQUFDOzs7Ozs7SUFFTyw2Q0FBYTs7Ozs7SUFBckIsVUFBc0IsS0FBVTtRQUM1QixPQUFPLE9BQU8sS0FBSyxLQUFLLFFBQVEsQ0FBQztJQUNyQyxDQUFDOzs7Ozs7O0lBRU8sMkNBQVc7Ozs7OztJQUFuQixVQUFvQixJQUFtQixFQUFFLElBQW1CO1FBQ3hELElBQUksSUFBSSxDQUFDLE1BQU0sS0FBSyxJQUFJLENBQUMsTUFBTSxFQUFFO1lBQzdCLE9BQU8sS0FBSyxDQUFDO1NBQ2hCO1FBRUQsS0FBSyxJQUFJLENBQUMsR0FBRyxJQUFJLENBQUMsTUFBTSxFQUFFLENBQUMsRUFBRSxHQUFJO1lBQzdCLElBQUksSUFBSSxDQUFDLENBQUMsQ0FBQyxLQUFLLElBQUksQ0FBQyxDQUFDLENBQUMsRUFBRTtnQkFDckIsT0FBTyxLQUFLLENBQUM7YUFDaEI7U0FDSjtRQUVELE9BQU8sSUFBSSxDQUFDO0lBQ2hCLENBQUM7O2dCQWpFSixVQUFVOztJQWtFWCw0QkFBQztDQUFBLEFBbEVELElBa0VDO1NBakVZLHFCQUFxQiIsInNvdXJjZXNDb250ZW50IjpbImltcG9ydCB7IEluamVjdGFibGUgfSBmcm9tICdAYW5ndWxhci9jb3JlJztcblxuQEluamVjdGFibGUoKVxuZXhwb3J0IGNsYXNzIEVxdWFsaXR5SGVscGVyU2VydmljZSB7XG4gICAgYXJlRXF1YWwodmFsdWUxOiBzdHJpbmcgfCBhbnlbXSB8IG9iamVjdCB8IG51bGwgfCB1bmRlZmluZWQsIHZhbHVlMjogc3RyaW5nIHwgYW55W10gfCBvYmplY3QgfCBudWxsIHwgdW5kZWZpbmVkKSB7XG4gICAgICAgIGlmICghdmFsdWUxIHx8ICF2YWx1ZTIpIHtcbiAgICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgfVxuXG4gICAgICAgIGlmICh0aGlzLmJvdGhWYWx1ZXNBcmVBcnJheXModmFsdWUxLCB2YWx1ZTIpKSB7XG4gICAgICAgICAgICByZXR1cm4gdGhpcy5hcnJheXNFcXVhbCh2YWx1ZTEgYXMgYW55W10sIHZhbHVlMiBhcyBhbnlbXSk7XG4gICAgICAgIH1cblxuICAgICAgICBpZiAodGhpcy5ib3RoVmFsdWVzQXJlU3RyaW5ncyh2YWx1ZTEsIHZhbHVlMikpIHtcbiAgICAgICAgICAgIHJldHVybiB2YWx1ZTEgPT09IHZhbHVlMjtcbiAgICAgICAgfVxuXG4gICAgICAgIGlmICh0aGlzLmJvdGhWYWx1ZXNBcmVPYmplY3RzKHZhbHVlMSwgdmFsdWUyKSkge1xuICAgICAgICAgICAgcmV0dXJuIEpTT04uc3RyaW5naWZ5KHZhbHVlMSkudG9Mb3dlckNhc2UoKSA9PT0gSlNPTi5zdHJpbmdpZnkodmFsdWUyKS50b0xvd2VyQ2FzZSgpO1xuICAgICAgICB9XG5cbiAgICAgICAgaWYgKHRoaXMub25lVmFsdWVJc1N0cmluZ0FuZFRoZU90aGVySXNBcnJheSh2YWx1ZTEsIHZhbHVlMikpIHtcbiAgICAgICAgICAgIGlmIChBcnJheS5pc0FycmF5KHZhbHVlMSkgJiYgdGhpcy52YWx1ZUlzU3RyaW5nKHZhbHVlMikpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4gdmFsdWUxWzBdID09PSB2YWx1ZTI7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBpZiAoQXJyYXkuaXNBcnJheSh2YWx1ZTIpICYmIHRoaXMudmFsdWVJc1N0cmluZyh2YWx1ZTEpKSB7XG4gICAgICAgICAgICAgICAgcmV0dXJuIHZhbHVlMlswXSA9PT0gdmFsdWUxO1xuICAgICAgICAgICAgfVxuICAgICAgICB9XG4gICAgfVxuXG4gICAgcHJpdmF0ZSBvbmVWYWx1ZUlzU3RyaW5nQW5kVGhlT3RoZXJJc0FycmF5KHZhbHVlMTogc3RyaW5nIHwgb2JqZWN0IHwgYW55W10sIHZhbHVlMjogc3RyaW5nIHwgb2JqZWN0IHwgYW55W10pIHtcbiAgICAgICAgcmV0dXJuIChBcnJheS5pc0FycmF5KHZhbHVlMSkgJiYgdGhpcy52YWx1ZUlzU3RyaW5nKHZhbHVlMikpIHx8IChBcnJheS5pc0FycmF5KHZhbHVlMikgJiYgdGhpcy52YWx1ZUlzU3RyaW5nKHZhbHVlMSkpO1xuICAgIH1cblxuICAgIHByaXZhdGUgYm90aFZhbHVlc0FyZU9iamVjdHModmFsdWUxOiBzdHJpbmcgfCBvYmplY3QgfCBhbnlbXSwgdmFsdWUyOiBzdHJpbmcgfCBvYmplY3QgfCBhbnlbXSkge1xuICAgICAgICByZXR1cm4gdGhpcy52YWx1ZUlzT2JqZWN0KHZhbHVlMSkgJiYgdGhpcy52YWx1ZUlzT2JqZWN0KHZhbHVlMik7XG4gICAgfVxuXG4gICAgcHJpdmF0ZSBib3RoVmFsdWVzQXJlU3RyaW5ncyh2YWx1ZTE6IHN0cmluZyB8IG9iamVjdCB8IGFueVtdLCB2YWx1ZTI6IHN0cmluZyB8IG9iamVjdCB8IGFueVtdKSB7XG4gICAgICAgIHJldHVybiB0aGlzLnZhbHVlSXNTdHJpbmcodmFsdWUxKSAmJiB0aGlzLnZhbHVlSXNTdHJpbmcodmFsdWUyKTtcbiAgICB9XG5cbiAgICBwcml2YXRlIGJvdGhWYWx1ZXNBcmVBcnJheXModmFsdWUxOiBzdHJpbmcgfCBvYmplY3QgfCBhbnlbXSwgdmFsdWUyOiBzdHJpbmcgfCBvYmplY3QgfCBhbnlbXSkge1xuICAgICAgICByZXR1cm4gQXJyYXkuaXNBcnJheSh2YWx1ZTEpICYmIEFycmF5LmlzQXJyYXkodmFsdWUyKTtcbiAgICB9XG5cbiAgICBwcml2YXRlIHZhbHVlSXNTdHJpbmcodmFsdWU6IGFueSkge1xuICAgICAgICByZXR1cm4gdHlwZW9mIHZhbHVlID09PSAnc3RyaW5nJyB8fCB2YWx1ZSBpbnN0YW5jZW9mIFN0cmluZztcbiAgICB9XG5cbiAgICBwcml2YXRlIHZhbHVlSXNPYmplY3QodmFsdWU6IGFueSkge1xuICAgICAgICByZXR1cm4gdHlwZW9mIHZhbHVlID09PSAnb2JqZWN0JztcbiAgICB9XG5cbiAgICBwcml2YXRlIGFycmF5c0VxdWFsKGFycjE6IEFycmF5PHN0cmluZz4sIGFycjI6IEFycmF5PHN0cmluZz4pIHtcbiAgICAgICAgaWYgKGFycjEubGVuZ3RoICE9PSBhcnIyLmxlbmd0aCkge1xuICAgICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICB9XG5cbiAgICAgICAgZm9yIChsZXQgaSA9IGFycjEubGVuZ3RoOyBpLS07ICkge1xuICAgICAgICAgICAgaWYgKGFycjFbaV0gIT09IGFycjJbaV0pIHtcbiAgICAgICAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4gdHJ1ZTtcbiAgICB9XG59XG4iXX0=