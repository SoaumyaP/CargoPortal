import { Component, Input } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { StatusStyle } from 'src/app/core';

@Component({
  selector: 'app-status-label',
  templateUrl: './status-label.component.html',
  styleUrls: ['./status-label.component.scss']
})

export class StatusLabelComponent {
    @Input() public status: number;
    @Input() public statusName: any;
    @Input() public statusEnum: any;

    constructor(private _translateService: TranslateService) {
    }
    get statusStyle() {
        const name = this.statusEnum[this.status];
        return StatusStyle[name];
    }

    get displayStatusName() {
        const statusValue = this.statusName.toString();
        // Try to get value from translation with key
        const translationKey = `label.${statusValue.charAt(0).toLowerCase() + statusValue.slice(1)}`;
        const result = this._translateService.instant(translationKey);

        // If not found in translation, return original value
        if (result.startsWith('label.')) {
            return this.statusName;
        }
        return result;
    }
}
