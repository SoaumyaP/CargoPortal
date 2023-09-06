import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ListComponent, LocalStorageService, SchedulingStatus, UserContextService } from 'src/app/core';
import { SchedulingListService } from './scheduling-list.service';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-scheduling-list',
  templateUrl: './scheduling-list.component.html',
  styleUrls: ['./scheduling-list.component.scss']
})
export class SchedulingListComponent extends ListComponent implements OnInit {

    readonly AppPermissions = AppPermissions;
    schedulingStatus = SchedulingStatus;

    faPlus = faPlus;
    listName = 'scheduling';
    
    constructor(
      public service: SchedulingListService,
      route: ActivatedRoute,
      location: Location,
      private _router: Router,
      private _userContext: UserContextService) {

        super(service, route, location);

        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
            if (!user.isInternal) {
                this.service.affiliates = user.affiliates;
                this.service.state = Object.assign({}, this.service.defaultState);
            }
            }
        });
    }

    ngOnInit() {
      super.ngOnInit();
    }

    onAddBtnClick() {

        // Remove local storage from telerik report preview
        LocalStorageService.remove('task-report-params');

        this._router.navigate(['/scheduling/add/0']);
    }

}
