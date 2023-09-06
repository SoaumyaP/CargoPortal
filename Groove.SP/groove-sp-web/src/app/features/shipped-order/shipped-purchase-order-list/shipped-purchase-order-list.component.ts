import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpService, POType, POTypeText, PurchaseOrderStatus, UserContextService } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ColumnSetting, ListComponent } from 'src/app/core/list';
import { Location } from '@angular/common';
import { ShippedPurchaseOrderListService } from './shipped-purchase-order-list.service';

@Component({
  selector: 'app-shipped-purchase-order-list',
  templateUrl: './shipped-purchase-order-list.component.html',
  styleUrls: ['./shipped-purchase-order-list.component.scss']
})
export class ShippedPurchaseOrderListComponent extends ListComponent implements OnInit {

    listName = 'shipped-purchase-orders';
    readonly AppPermissions = AppPermissions;
    currentUser: any;
    purchaseOrderStatus = PurchaseOrderStatus;
    poType = POType;
    poTypeText = POTypeText;
    public poStageTypes: Array<{ text: string, value: number }> = [
        { text: 'label.shipmentDispatch', value: 50 },
        { text: 'label.closed', value: 60 },
    ];
    columns: ColumnSetting[] = [
        {
            field: 'poNumber',
            title: 'label.poNumber',
            filter: 'text',
            class: 'link-code',
            width: '20%'
        },
        {
            field: 'createdDate',
            title: 'label.poDates',
            filter: 'date',
            format: this.DATE_FORMAT,
            width: '15%'
        },
        {
            field: 'statusName',
            title: 'label.status',
            filter: 'text',
            width: '10%',
        },
        {
            field: 'cargoReadyDate',
            title: 'label.cargoReadyDates',
            filter: 'date',
            format: this.DATE_FORMAT,
            width: '15%'
        },
        {
            field: 'supplier',
            title: 'label.supplier',
            filter: 'text',
            width: '25%'
        },
        {
            field: 'stageName',
            title: 'label.stage',
            filter: 'text',
            width: '15%',
        }
    ];

    constructor(service: ShippedPurchaseOrderListService,
        route: ActivatedRoute,
        location: Location,
        private _userContext: UserContextService,
        private router: Router) {
            super(service, route, location);
            this._userContext.getCurrentUser().subscribe(user => {
                if (user) {
                    this.currentUser = user;
                    if (!user.isInternal) {
                        this.service.affiliates = user.affiliates;
                        this.service.state = Object.assign({}, this.service.defaultState);
                    }
                }
            });
         }

    async ngOnInit() {
        if (!this.currentUser.isInternal) {
            // apply for shipper/supplier
            if (this.currentUser.customerRelationships) {
                // get current customer-relationships
                const customerRelationshipParams = await this._userContext.getCustomerRelationships(this.currentUser).toPromise();
                this.service.otherQueryParams.customerRelationships = customerRelationshipParams;
            }
            this.service.otherQueryParams.organizationId = this.currentUser.organizationId;
        }
        super.ngOnInit();
    }

}
