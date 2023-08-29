import { Component, OnInit } from '@angular/core';
import { OrganizationListService } from './organization-list.service';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { OrganizationStatus } from 'src/app/core/models/enums/enums';
import { DropDowns } from 'src/app/core';
import { faCloudDownloadAlt, faFileImport, faPlus  } from '@fortawesome/free-solid-svg-icons';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { environment } from 'src/environments/environment';

@Component({
    selector: 'app-organization-list',
    templateUrl: './organization-list.component.html',
    styleUrls: ['./organization-list.component.scss']
})
export class OrganizationListComponent extends ListComponent implements OnInit {

    AppPermissions = AppPermissions;
    faCloudDownloadAlt = faCloudDownloadAlt;
    faFileImport = faFileImport;
    importFormOpened = false;
    uploadSaveUrl = `${environment.apiUrl}/importdataprogress/organizations`;

    listName = 'organizations';
    organizationStatus = OrganizationStatus;

    organizationTypeList = DropDowns.OrganizationTypeList.filter(o => o.isOfficial);
    faPlus = faPlus;

    constructor(service: OrganizationListService, route: ActivatedRoute, location: Location) {
        super(service, route, location);
    }

    public downloadTemplate(): void {
        this.service.downloadTemplate(`${environment.apiUrl}/importdataprogress/template/organization`,
        'OrganizationImportTemplate.xlsx').subscribe();
    }

    public importFormClosedHandler(): void {
        this.importFormOpened = false;

    }

    openImportDataPopup(): void {
        this.importFormOpened = true;
    }
}
