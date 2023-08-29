import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BulkFulfillmentGeneralComponent } from './bulk-fulfillment-general.component';

describe('BulkFulfillmentGeneralComponent', () => {
  let component: BulkFulfillmentGeneralComponent;
  let fixture: ComponentFixture<BulkFulfillmentGeneralComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BulkFulfillmentGeneralComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BulkFulfillmentGeneralComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
