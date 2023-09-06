/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Injectable, EventEmitter } from '@angular/core';
/**
 * @hidden
 */
var LocalDataChangesService = /** @class */ (function () {
    function LocalDataChangesService() {
        this.changes = new EventEmitter();
    }
    LocalDataChangesService.decorators = [
        { type: Injectable },
    ];
    return LocalDataChangesService;
}());
export { LocalDataChangesService };
