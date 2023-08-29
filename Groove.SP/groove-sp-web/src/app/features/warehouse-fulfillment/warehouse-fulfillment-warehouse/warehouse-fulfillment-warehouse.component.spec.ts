import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WarehouseFulfillmentWarehouseComponent } from './warehouse-fulfillment-warehouse.component';

describe('WarehouseFulfillmentWarehouseComponent', () => {
  let component: WarehouseFulfillmentWarehouseComponent;
  let fixture: ComponentFixture<WarehouseFulfillmentWarehouseComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WarehouseFulfillmentWarehouseComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WarehouseFulfillmentWarehouseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
