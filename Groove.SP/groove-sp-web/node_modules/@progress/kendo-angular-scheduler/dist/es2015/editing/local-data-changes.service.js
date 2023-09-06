/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { Injectable, EventEmitter } from '@angular/core';
/**
 * @hidden
 */
export class LocalDataChangesService {
    constructor() {
        this.changes = new EventEmitter();
    }
}
LocalDataChangesService.decorators = [
    { type: Injectable },
];
