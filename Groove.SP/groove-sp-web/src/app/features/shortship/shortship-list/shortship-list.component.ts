import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { DATE_FORMAT, FormModeType, ListComponent, StringHelper, UserContextService } from 'src/app/core';
import { ShortshipListService } from './shortship-list.service';
import { Location } from '@angular/common';
import { faEnvelopeOpen, faEnvelope, faFileDownload } from '@fortawesome/free-solid-svg-icons';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-shortship-list',
  templateUrl: './shortship-list.component.html',
  styleUrls: ['./shortship-list.component.scss']
})
export class ShortshipListComponent extends ListComponent implements OnInit {
  @ViewChild('excelexport', { static: false }) excelExportElement: any;

  listName = 'shortships';
  DATE_FORMAT = DATE_FORMAT;
  formModeType = FormModeType;
  faEnvelopeOpen = faEnvelopeOpen;
  faEnvelope = faEnvelope;
  faFileDownload = faFileDownload;
  isCanExport: boolean;
  shortShipExportModel: any[];

  isHasPOFulfillmentDetailPermission: boolean = false;
  isHasPurchaseOrderDetailPermission: boolean = false;

  constructor(
    public notification: NotificationPopup,
    public service: ShortshipListService, route: ActivatedRoute, location: Location,
    private _userContext: UserContextService,
    public translateService: TranslateService,
  ) {
    super(service, route, location);

    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        if (!user.isInternal) {
          this.service.affiliates = user.affiliates;
          this.service.otherQueryParams.organizationId = user.organizationId;
        }

        this.isHasPOFulfillmentDetailPermission = user.permissions?.some(c => c.name === AppPermissions.PO_Fulfillment_Detail);
        this.isHasPurchaseOrderDetailPermission = user.permissions?.some(c => c.name === AppPermissions.PO_Detail);
      }
    });
  }

  ngOnInit() {
    this.route.paramMap.pipe(map(() => window.history.state))
      .subscribe(
        (state) => {
          if (StringHelper.isNullOrEmpty(state.statisticKey)) {
          } else {
            this.service.otherQueryParams.statisticKey
              = this.service.statisticKey
              = state.statisticKey;
          }
        }
      );

    super.ngOnInit();
  }

  readOrUnreadShortship(dataItem: any) {
    this.service.readOrUnread(dataItem.id, dataItem).subscribe(
      (data) => {
        this.notification.showSuccessPopup('save.sucessNotification', 'label.shortShip');
        dataItem.isRead = !dataItem.isRead;
      },
      error => {
        this.notification.showErrorPopup('save.failureNotification', 'label.shortShip');
      }
    )
  }

  exportExcel() {
    this.isCanExport = false;
    this.service.queryToExport().subscribe(
      r => {
        this.isCanExport = true;
        this.shortShipExportModel = r.data;
        setTimeout(() => {
          this.excelExportElement.save();
        }, 50);
      }
    );
  }
}
