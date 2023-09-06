/**-----------------------------------------------------------------------------------------
* Copyright © 2020 Progress Software Corporation. All rights reserved.
* Licensed under commercial license. See LICENSE.md in the project root for more information
*-------------------------------------------------------------------------------------------*/
import { EventEmitter } from '@angular/core';
/**
 * @hidden
 */
export class PickerService {
    constructor() {
        this.onFocus = new EventEmitter();
        this.onBlur = new EventEmitter();
        this.sameDateSelected = new EventEmitter();
        this.dateCompletenessChange = new EventEmitter();
    }
}
