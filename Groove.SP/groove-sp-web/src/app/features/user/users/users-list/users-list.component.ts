import { Component, OnInit } from '@angular/core';
import { UsersListService } from '../users-list/users-list.service';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent, UserStatus, DropDowns } from 'src/app/core';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faCloudDownloadAlt, faFileImport } from '@fortawesome/free-solid-svg-icons';
import { environment } from 'src/environments/environment';

@Component({
    selector: 'app-user-list',
    templateUrl: './users-list.component.html',
    styleUrls: ['./users-list.component.scss']
})
export class UsersListComponent extends ListComponent implements OnInit {
    listName = 'users';
    organizationList: any;
    userStatus = UserStatus;
    uploadValidateUrl = `${environment.apiUrl}/users/validateExcelImport`;
    uploadSaveUrl = `${environment.apiUrl}/users/import`;
    importFormOpened = false;

    readonly AppPermissions = AppPermissions;
    faCloudDownloadAlt = faCloudDownloadAlt;
    faFileImport = faFileImport;

    organizationTypeList = DropDowns.OrganizationTypeList;

    constructor(public service: UsersListService, route: ActivatedRoute, location: Location) {
        super(service, route, location);
    }

    ngOnInit() {
        super.ngOnInit();
    }

    public downloadTemplate(): void {
        this.service.downloadTemplate(`${environment.apiUrl}/importdataprogress/template/user`,
        'UserImportTemplate.xlsx').subscribe();
    }

    public importFormClosedHandler(): void {
        this.importFormOpened = false;

    }
}
