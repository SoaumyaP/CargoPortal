import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { StringHelper, UserContextService } from 'src/app/core';
import { ListComponent } from 'src/app/core/list/list.component';
import { VesselArrivalListService } from './vessel-arrival-list.service';
import { Location } from '@angular/common';
import { map } from 'rxjs/operators';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faFileDownload } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-vessel-arrival-list',
  templateUrl: './vessel-arrival-list.component.html',
  styleUrls: ['./vessel-arrival-list.component.scss']
})
export class VesselArrivalListComponent extends ListComponent implements OnInit, OnDestroy {
  @ViewChild('excelexport', { static: false }) excelExportElement: any;
  listName = 'vessel-arrivals';
  currentUser: any;
  AppPermissions = AppPermissions;
  faFileDownload = faFileDownload;
  isCanExport: boolean;
  vesselArrivalsExportModel: any[];
  isCanClickPONumber: boolean;

  constructor(service: VesselArrivalListService, route: ActivatedRoute, location: Location,
    private _userContext: UserContextService, private router: Router) {
    super(service, route, location);
    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
        this.checkPermission();
        if (!user.isInternal) {
          this.service.affiliates = user.affiliates;
          this.service.state = Object.assign({}, this.service.defaultState);
        }
      }
    });
  }


  ngOnInit() {
    if (!this.currentUser.isInternal) {
      this.service.otherQueryParams.organizationId = this.currentUser.organizationId;
      if (this.currentUser.customerRelationships) {
        this.service.otherQueryParams.customerRelationships = this.currentUser.customerRelationships;
      }
    }

    this.route.paramMap
      .pipe(map(() => window.history.state))
      .subscribe(
        (state) => {
          if (StringHelper.isNullOrEmpty(state.statisticKey)) {
          } else {
            this.service.otherQueryParams.statisticKey
              = this.service.statisticKey
              = state.statisticKey;

            this.service.otherQueryParams.statisticFilter
              = this.service.statisticFilter
              = state.statisticFilter;
          }
          super.ngOnInit();
        }
      );
  }

  checkPermission() {
    this.isCanClickPONumber  =  this.currentUser?.permissions?.some(c => c.name === AppPermissions.PO_Detail);
  }

  exportExcel() {
    this.isCanExport = false;
    this.service.queryToExport().subscribe(
      r => {
        this.isCanExport = true;
        this.vesselArrivalsExportModel = r.data;
        setTimeout(() => {
          this.excelExportElement.save();
        }, 50);
      }
    );
  }

  ngOnDestroy(): void {

  }
}
