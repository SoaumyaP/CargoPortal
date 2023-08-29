import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ListComponent, UserContextService } from 'src/app/core';
import { BillOfLadingListService } from './bill-of-lading-list.service';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faFileAlt } from '@fortawesome/free-regular-svg-icons';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { GAEventCategory } from 'src/app/core/models/constants/app-constants';
import { GoogleAnalyticsService } from 'src/app/core/services/google-analytics.service';

@Component({
  selector: 'app-bill-of-lading-list',
  templateUrl: './bill-of-lading-list.component.html',
  styleUrls: ['./bill-of-lading-list.component.scss']
})
export class BillOfLadingListComponent extends ListComponent implements OnInit {
  listName = 'bill-of-ladings';
  isCanClickMasterBLNo: boolean = false;
  faFileAlt = faFileAlt;

  constructor(
    public service: BillOfLadingListService,
    route: ActivatedRoute,
    location: Location,
    private attachmentService: AttachmentUploadPopupService,
    private _userContext: UserContextService,
    private _gaService: GoogleAnalyticsService) {
    super(service, route, location);
    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.service.roleId = user.role.id;
        this.service.organizationId = user.organizationId ?? 0;

        if (!user.isInternal) {
          this.service.affiliates = user.affiliates;
          this.service.state = Object.assign({}, this.service.defaultState);
        }
        this.isCanClickMasterBLNo = user.permissions?.some(c => c.name === AppPermissions.BillOfLading_MasterBLDetail);
      }
    });
  }

  ngOnInit() {
    super.ngOnInit();
  }

  showAllShipmentNo(dataItem) {
    dataItem.isShowAllShipmentNo = true;
  }

  downloadFile(id, fileName) {
    this.attachmentService.downloadFile(id, fileName).subscribe(
        value => {
            this._gaService.emitAction('Download', GAEventCategory.HouseBill);
        }
    );
  }
}
