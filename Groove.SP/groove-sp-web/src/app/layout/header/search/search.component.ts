import { Component, OnInit } from '@angular/core';
import { FormModeType, StringHelper, UserContextService } from 'src/app/core';
import { Router } from '@angular/router';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';

@Component({
    selector: 'app-search',
    templateUrl: './search.component.html',
    styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {
    quickTrackNumber = '';
    isSearching: boolean = false;
    currentUser: any;

    public QuickTrackOptionEnum = {
        POFulfillmentNo: "poFulfillmentNumber",
        MissingPOFulfillmentNo: "missingPOFulfillmentNumber",
        BulkFulfillmentNo: "bulkFulfillmentNumber",
        WarehouseFulfillmentNo: "warehouseFulfillmentNumber",
        ShipmentNo: "shipmentNo",
        ContainerNo: "containerNo",
        HouseBillNo: "houseBillNumber",
        ShipmentRefNo: "ShipmentRefNo",
        ItemNo: "itemNo"
    };

    listItems: Array<{ text: string, value: number }> = [
        { text: 'label.customerReferenceNo', value: 1 },
        { text: 'label.agentReferenceNo', value: 4 },
        { text: 'label.containerNo', value: 2 },
        { text: 'label.houseBillNo', value: 3 },
        { text: 'label.itemNo', value: 5 },
    ];

    constructor(
        private router: Router,
        private _userContextService: UserContextService,
        private _httpClient: HttpClient,
        private _gaService: GoogleAnalyticsService) {
            _userContextService.getCurrentUser().subscribe((user) => {
                this.currentUser = user;
            });
    }

    ngOnInit() {
    }

    onValueSubmit() {
        this.quickTrackNumber = this.quickTrackNumber.trim();
        if (!StringHelper.isNullOrEmpty(this.quickTrackNumber)) {
            const refresh = Date.now();

            let url = `${environment.apiUrl}/quickTracks/${this.quickTrackNumber}/track-option`;
            if (this.currentUser && !this.currentUser.isInternal) {
                let affiliates = this.currentUser.affiliates || '';
                let customerRelationships = this.currentUser.customerRelationships || '';
                url +=  `?affiliates=${affiliates}&supplierCustomerRelationships=${customerRelationships}`;
            }

            this.isSearching = true;
            this._httpClient.get(url).subscribe((data:any) => {
                this.isSearching = false;
                if (data.matchedOption) {
                    switch (data.matchedOption) {
                        case this.QuickTrackOptionEnum.POFulfillmentNo:
                            this.router.navigate([`/po-fulfillments/view/${data.matchedValue}`], {
                                queryParams: {
                                    formType: FormModeType.View,
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By Booking No.', GAEventCategory.QuickSearch);
                            break;
                        case this.QuickTrackOptionEnum.MissingPOFulfillmentNo:
                            this.router.navigate([`/missing-po-fulfillments/view/${data.matchedValue}`], {
                                queryParams: {
                                    formType: FormModeType.View,
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By Booking No.', GAEventCategory.QuickSearch);
                            break;
                        case this.QuickTrackOptionEnum.BulkFulfillmentNo:
                            this.router.navigate([`/bulk-fulfillments/view/${data.matchedValue}`], {
                                queryParams: {
                                    formType: FormModeType.View,
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By Booking No.', GAEventCategory.QuickSearch);
                            break;
                        case this.QuickTrackOptionEnum.WarehouseFulfillmentNo:
                            this.router.navigate([`/warehouse-bookings/view/${data.matchedValue}`], {
                                queryParams: {
                                    formType: FormModeType.View,
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By Booking No.', GAEventCategory.QuickSearch);
                            break;
                        case this.QuickTrackOptionEnum.ShipmentNo:
                            this.router.navigate([`/shipments/${data.matchedValue}`], {
                                queryParams: {
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By Shipment No.', GAEventCategory.QuickSearch);
                            break;
                        case this.QuickTrackOptionEnum.ContainerNo:
                            this.router.navigate([`/containers/${data.matchedValue}`], {
                                queryParams: {
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By Container No.', GAEventCategory.QuickSearch);
                            break;
                        case this.QuickTrackOptionEnum.HouseBillNo:
                            this.router.navigate([`/bill-of-ladings/${data.matchedValue}`], {
                                queryParams: {
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By House Bill No.', GAEventCategory.QuickSearch);
                            break;
                        case this.QuickTrackOptionEnum.ShipmentRefNo:
                            this.router.navigate(['/shipments/search/', `refNo~${this.quickTrackNumber}`], {
                                queryParams: {
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By Shipment Ref. No.', GAEventCategory.QuickSearch);
                            break;
                        case this.QuickTrackOptionEnum.ItemNo:
                            this.router.navigate(['/purchase-orders/search/', `itemNo~${this.quickTrackNumber}`], {
                                queryParams: {
                                    state: 'tracking',
                                    refresh: refresh
                                }
                            });
                            this._gaService.emitAction('Search By Item No.', GAEventCategory.QuickSearch);
                            break;
                        default:
                            this.router.navigate(['/search/no-result']);
                            break;
                    }
                }
                else {
                    this.router.navigate(['/search/no-result']);
                }
            });
        } else {
            this.router.navigate(['/search/invalid']);
        }
    }

}
