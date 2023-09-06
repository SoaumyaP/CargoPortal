import {
  Component,
  EventEmitter,
  Input,
  Output
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { StringHelper } from 'src/app/core';
import { DropdownListModel } from 'src/app/core/models/dropDowns/dropdown-item-model';
import { ShipmentItineraryConfirmPopupService } from './shipment-itinerary-confirm.service';
export interface ConfirmItineraryModel {
  cyClosingDate: string | null;
  cfsClosingDate: string | null;
  cyEmptyPickupTerminalCode: string | null;
  cyEmptyPickupTerminalDescription: string | null;
  cfsWarehouseCode: string | null;
  cfsWarehouseDescription: string | null;
}

export enum ShipmentItineraryConfirmPopupMode {
  Confirm,
  Update
}
@Component({
  selector: 'app-shipment-itinerary-confirm-popup',
  templateUrl: './shipment-itinerary-confirm-popup.component.html',
  styleUrls: ['./shipment-itinerary-confirm-popup.component.scss']
})
export class ShipmentItineraryConfirmPopupComponent {
  @Input() isOpen: boolean = false;
  @Input() mode: ShipmentItineraryConfirmPopupMode;
  @Input() isCFSMovement: boolean;
  @Input() schedulerId: number;
  @Input() model: ConfirmItineraryModel;

  @Output()
  close: EventEmitter<any> = new EventEmitter<any>();

  @Output()
  confirm: EventEmitter<any> = new EventEmitter<any>();

  @Output()
  save: EventEmitter<any> = new EventEmitter<any>();

  readonly modeType = ShipmentItineraryConfirmPopupMode;

  defaultDropDownItem: DropdownListModel<string> = {
    label: 'label.select',
    value: null
  };

  terminalDataSource$: Observable<DropdownListModel<string>[]>;
  warehouseDataSource$: Observable<DropdownListModel<string>[]>;
  terminalDataSource: DropdownListModel<string>[];
  warehouseDataSource: DropdownListModel<string>[];

  constructor(private service: ShipmentItineraryConfirmPopupService, public translateService: TranslateService) {
    this.terminalDataSource$ = this.service.getTerminalDataSource().pipe(tap(dt => this.terminalDataSource = dt));
    this.warehouseDataSource$ = this.service.getWarehouseDataSource().pipe(tap(dt => this.warehouseDataSource = dt));
  }

  _bindingModel(): void {
    if (!this.isCFSMovement && !StringHelper.isNullOrEmpty(this.model.cyEmptyPickupTerminalCode)) {
      let terminal = this.terminalDataSource.find(x => x.value === this.model.cyEmptyPickupTerminalCode);
      this.model.cyEmptyPickupTerminalDescription = terminal.label;
    }
    else {
      this.model.cyEmptyPickupTerminalCode = null;
      this.model.cyEmptyPickupTerminalDescription = null;
    }

    if (this.isCFSMovement && !StringHelper.isNullOrEmpty(this.model.cfsWarehouseCode)) {
      let warehouse = this.warehouseDataSource.find(x => x.value === this.model.cfsWarehouseCode);
      this.model.cfsWarehouseDescription = warehouse.label;
    }
    else {
      this.model.cfsWarehouseCode = null;
      this.model.cfsWarehouseDescription = null;
    }
  }

  onConfirm(skipFormUpdates: boolean): void {
    this._bindingModel();
    this.confirm.emit({
      skipConfirmUpdates: skipFormUpdates,
      model: this.model
    });
  }

  onSave(): void {
    this._bindingModel();
    this.save.emit(this.model);
  }

  onFormClose() {
    this.close.emit();
  }

  get title() {
    return this.mode === ShipmentItineraryConfirmPopupMode.Confirm ? 'label.confirmItinerary' : 'label.editItinerary';
  }

}