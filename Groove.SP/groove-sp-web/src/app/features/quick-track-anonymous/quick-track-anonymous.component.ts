import { Component, OnInit } from '@angular/core';
import { StringHelper } from 'src/app/core';
import { Router } from '@angular/router';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';

@Component({
    selector: 'app-quick-tracking',
    templateUrl: './quick-track-anonymous.component.html',
    styleUrls: ['./quick-track-anonymous.component.scss']
})

export class QuickTrackAnonymousComponent implements OnInit {
    quickTrackNumber = '';

    public SearchTypeEnum = {
        Shipment: 1,
        Container: 2,
        BillLading: 3
    };

    listItems: Array<{ text: string, value: number }> = [
        { text: 'label.shipmentNo', value: 1 },
        { text: 'label.containerNo', value: 2 },
        { text: 'label.houseBillNo', value: 3 }
    ];

    searchType: any;
    quickLang: string = '';

    constructor(
        private router: Router,
        private _gaService: GoogleAnalyticsService) {
    }

    ngOnInit() {
        this.searchType = this.SearchTypeEnum.Shipment;
    }

    onValueSubmit() {
        this.quickTrackNumber = this.quickTrackNumber.trim();
        if (!StringHelper.isNullOrEmpty(this.quickTrackNumber)) {
            const refresh = Date.now();
            switch (this.searchType) {
                case this.SearchTypeEnum.Shipment:
                    this.router.navigate(['/quick-track/shipments', this.quickTrackNumber],
                        {queryParams: {state: 'tracking', lang: this.quickLang, refresh: refresh }});
                    this._gaService.emitAction('Search By Shipment No.', GAEventCategory.QuickTrack);

                break;
                case this.SearchTypeEnum.Container:
                    this.router.navigate(['/quick-track/containers', this.quickTrackNumber],
                        {queryParams: {state: 'tracking', lang: this.quickLang, refresh: refresh }});
                    this._gaService.emitAction('Search By Container No.', GAEventCategory.QuickTrack);

                break;
                case this.SearchTypeEnum.BillLading:
                    this.router.navigate(['/quick-track/bill-of-ladings', this.quickTrackNumber],
                        {queryParams: {state: 'tracking', lang: this.quickLang, refresh: refresh }});
                    this._gaService.emitAction('Search By House Bill No.', GAEventCategory.QuickTrack);

                break;
            }
        } else {
            this.router.navigate(['/quick-track/invalid'], {queryParams: {lang: this.quickLang }});
        }
    }

}
