import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { FormHelper } from 'src/app/core/helpers/form.helper';
import { AddMissingPOFormModel } from '../missing-po-fulfillment-customer/missing-po-fulfillment-customer.component';

@Component({
  selector: 'app-add-missing-customer-po-form',
  templateUrl: './add-missing-customer-po-form.component.html',
  styleUrls: ['./add-missing-customer-po-form.component.scss']
})
export class AddMissingCustomerPoFormComponent implements OnInit {
  @Input() public isOpen: boolean = false;
  @Input() public model: AddMissingPOFormModel;
  @Output() close: EventEmitter<any> = new EventEmitter();
  @Output() add: EventEmitter<any> = new EventEmitter<any>();

  @ViewChild('mainForm', { static: false }) mainForm: NgForm;

  customerPO: string;

  constructor() { }

  ngOnInit() {
  }

  onFormClosed(): void {
    this.isOpen = false;
    this.close.emit();
  }

  onFormSaved(): void {
    FormHelper.ValidateAllFields(this.mainForm);
    if (this.mainForm.invalid) {
      return;
    }

    this.isOpen = false;
    this.add.emit(this.model);
  }

}