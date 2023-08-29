import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { BalanceOfGoodsEnquiryComponent } from './balance-of-goods-enquiry.component';
import { GridModule } from '@progress/kendo-angular-grid';
import { RouterModule } from '@angular/router';
import { of } from 'rxjs';
import { UserContextService } from 'src/app/core/auth/user-context.service';
import { CoreModule, OrganizationType, UserStatus } from 'src/app/core';
import { BalanceOfGoodsService } from './balance-of-goods.service';

describe('BalanceOfGoodsEnquiryComponent', () => {
  let component: BalanceOfGoodsEnquiryComponent;
  let fixture: ComponentFixture<BalanceOfGoodsEnquiryComponent>;
  const profile = {
    isInternal: false,
    affiliates: '[1, 2]',
    id: 1,
    accountNumber: 'dummy value',
    username: 'dummy value',
    email: 'dummy value',
    name: 'dummy value',
    title: 'dummy value',
    department: 'dummy value',
    phone: 'dummy value',
    companyName: 'dummy value',
    profilePicture: 'dummy value',
    status: UserStatus.Active,
    lastSignInDate: 'dummy value',
    statusName: 'dummy value',
    countryId: 1,
    organizationId: 1,
    organizationRoleId: 1,
    organizationName: 'dummy value',
    organizationCode: 'dummy value',
    organizationType: OrganizationType.General,
    organizationTypeName: 'dummy value',
    userRoles: [],
    role: null,
    permissions: [],
    identityType: 'dummy value',
    identityTenant: 'dummy value',
    isUserRoleSwitch: true,
  };

  beforeEach(async(() => {
    const uc = jasmine.createSpyObj('UserContextService', ['getCurrentUser']);
    const getCurrentUserSpy = uc.getCurrentUser.and.returnValue(of(profile));

    const bs = jasmine.createSpyObj('BalanceOfGoodsService', ['query']);
    const query = bs.query.and.returnValue(of({totalRecords: 0, records: []}));

    TestBed.configureTestingModule({
      declarations: [
        BalanceOfGoodsEnquiryComponent,
     ],
     imports: [
        GridModule,
        RouterModule,
        TranslateModule,
        CoreModule
     ],
     providers: [
        {
            provide: UserContextService, useValue: uc
        },
        {
            provide: BalanceOfGoodsService, useValue: bs
        }
     ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BalanceOfGoodsEnquiryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('affiliates has value', () => {
    expect(component.affiliates === '[1, 2]').toBeTruthy();
  });

  it('query should contain affiliates', () => {
    component.loadData();
    expect(component.affiliates === '[1, 2]');
    expect(component.view.getValue().total === 0);
    expect(component.view.getValue().data.length === 0);
  });


});
