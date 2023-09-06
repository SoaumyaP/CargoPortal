/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes,extraRequire,missingOverride,missingReturn,unusedPrivateMembers,uselessCode} checked by tsc
 */
var UriEncoder = /** @class */ (function () {
    function UriEncoder() {
    }
    /**
     * @param {?} key
     * @return {?}
     */
    UriEncoder.prototype.encodeKey = /**
     * @param {?} key
     * @return {?}
     */
    function (key) {
        return encodeURIComponent(key);
    };
    /**
     * @param {?} value
     * @return {?}
     */
    UriEncoder.prototype.encodeValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) {
        return encodeURIComponent(value);
    };
    /**
     * @param {?} key
     * @return {?}
     */
    UriEncoder.prototype.decodeKey = /**
     * @param {?} key
     * @return {?}
     */
    function (key) {
        return decodeURIComponent(key);
    };
    /**
     * @param {?} value
     * @return {?}
     */
    UriEncoder.prototype.decodeValue = /**
     * @param {?} value
     * @return {?}
     */
    function (value) {
        return decodeURIComponent(value);
    };
    return UriEncoder;
}());
export { UriEncoder };
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoidXJpLWVuY29kZXIuanMiLCJzb3VyY2VSb290Ijoibmc6Ly9hbmd1bGFyLWF1dGgtb2lkYy1jbGllbnQvIiwic291cmNlcyI6WyJsaWIvc2VydmljZXMvdXJpLWVuY29kZXIudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6Ijs7OztBQUVBO0lBQUE7SUFnQkEsQ0FBQzs7Ozs7SUFmRyw4QkFBUzs7OztJQUFULFVBQVUsR0FBVztRQUNqQixPQUFPLGtCQUFrQixDQUFDLEdBQUcsQ0FBQyxDQUFDO0lBQ25DLENBQUM7Ozs7O0lBRUQsZ0NBQVc7Ozs7SUFBWCxVQUFZLEtBQWE7UUFDckIsT0FBTyxrQkFBa0IsQ0FBQyxLQUFLLENBQUMsQ0FBQztJQUNyQyxDQUFDOzs7OztJQUVELDhCQUFTOzs7O0lBQVQsVUFBVSxHQUFXO1FBQ2pCLE9BQU8sa0JBQWtCLENBQUMsR0FBRyxDQUFDLENBQUM7SUFDbkMsQ0FBQzs7Ozs7SUFFRCxnQ0FBVzs7OztJQUFYLFVBQVksS0FBYTtRQUNyQixPQUFPLGtCQUFrQixDQUFDLEtBQUssQ0FBQyxDQUFDO0lBQ3JDLENBQUM7SUFDTCxpQkFBQztBQUFELENBQUMsQUFoQkQsSUFnQkMiLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQgeyBIdHRwUGFyYW1ldGVyQ29kZWMgfSBmcm9tICdAYW5ndWxhci9jb21tb24vaHR0cCc7XG5cbmV4cG9ydCBjbGFzcyBVcmlFbmNvZGVyIGltcGxlbWVudHMgSHR0cFBhcmFtZXRlckNvZGVjIHtcbiAgICBlbmNvZGVLZXkoa2V5OiBzdHJpbmcpOiBzdHJpbmcge1xuICAgICAgICByZXR1cm4gZW5jb2RlVVJJQ29tcG9uZW50KGtleSk7XG4gICAgfVxuXG4gICAgZW5jb2RlVmFsdWUodmFsdWU6IHN0cmluZyk6IHN0cmluZyB7XG4gICAgICAgIHJldHVybiBlbmNvZGVVUklDb21wb25lbnQodmFsdWUpO1xuICAgIH1cblxuICAgIGRlY29kZUtleShrZXk6IHN0cmluZyk6IHN0cmluZyB7XG4gICAgICAgIHJldHVybiBkZWNvZGVVUklDb21wb25lbnQoa2V5KTtcbiAgICB9XG5cbiAgICBkZWNvZGVWYWx1ZSh2YWx1ZTogc3RyaW5nKTogc3RyaW5nIHtcbiAgICAgICAgcmV0dXJuIGRlY29kZVVSSUNvbXBvbmVudCh2YWx1ZSk7XG4gICAgfVxufVxuIl19