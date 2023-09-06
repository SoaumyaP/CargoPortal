/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
/**
 * @hidden
 */
var LocalDataChangesService = /** @class */ (function () {
    function LocalDataChangesService() {
        this.changes = new core_1.EventEmitter();
    }
    LocalDataChangesService.decorators = [
        { type: core_1.Injectable },
    ];
    return LocalDataChangesService;
}());
exports.LocalDataChangesService = LocalDataChangesService;
