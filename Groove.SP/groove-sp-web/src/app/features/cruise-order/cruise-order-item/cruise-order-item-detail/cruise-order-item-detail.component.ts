import { Component, Input, OnDestroy, OnInit } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { of, Subscription } from "rxjs";
import { delay } from "rxjs/operators";
import { DATE_FORMAT, StringHelper } from "src/app/core";
import { CruiseOrderItemModel } from "../../models/cruise-order-item.model";

@Component({
    selector: "app-cruise-order-item-detail",
    templateUrl: "./cruise-order-item-detail.component.html",
    styleUrls: ["./cruise-order-item-detail.component.scss"],
})
export class CruiseOrderItemDetailComponent implements OnInit, OnDestroy {
    @Input("model") cruiseOrderItemModel: CruiseOrderItemModel;
    @Input() loadingGrids: {};
    @Input() rowIndex: number;

    defaultValue = '--';
    DATE_FORMAT = DATE_FORMAT;

    // Store all subscriptions, then should un-subscribe at the end
    private _subscriptions: Array<Subscription> = [];

    constructor(private _translateService: TranslateService) {}


    ngOnInit(): void {

        // To add visual animation on loading data
        this.loadingGrids[this.rowIndex] = true;
        of(1)
            .pipe(delay(500))
            .subscribe((x) => delete this.loadingGrids[this.rowIndex]);
    }

    returnText(field: string) {
        return this.cruiseOrderItemModel && this.cruiseOrderItemModel[field]
            ? this.cruiseOrderItemModel[field]
            : this.defaultValue;
    }

    returnCommercialInvoiceText() {
        if (!StringHelper.isNullOrEmpty(this.cruiseOrderItemModel?.commercialInvoice)) {
            return this._translateService.instant(this.cruiseOrderItemModel.commercialInvoice ? 'label.yes' : 'label.no');
        }
       return this.defaultValue;
    }

    ngOnDestroy(): void {
        this._subscriptions.map((x) => x.unsubscribe());
      }
}
