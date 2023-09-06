import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ListComponent, SurveyStatus, UserContextService } from 'src/app/core';
import { SurveyListService } from './survey-list.service';
import { Location } from '@angular/common';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';

@Component({
  selector: 'app-survey-list',
  templateUrl: './survey-list.component.html',
  styleUrls: ['./survey-list.component.scss']
})
export class SurveyListComponent extends ListComponent implements OnInit {
  listName = 'surveys';
  SurveyStatus = SurveyStatus;
  faPlus = faPlus;
  isCanClickSurveyName: boolean;
  currentUser: UserProfileModel;

  constructor(service: SurveyListService, route: ActivatedRoute, private router: Router, location: Location,
    private _userContext: UserContextService
  ) {
    super(service, route, location);
    this._userContext.getCurrentUser().subscribe(user => {
      if (user) {
        this.currentUser = user;
        this.isCanClickSurveyName = user.permissions?.some(c => c.name === AppPermissions.Organization_SurveyDetail);
      }
    });
  }

  get isAddNewSurvey() {
    const isHasPermission = this.currentUser.permissions?.some(c => c.name === AppPermissions.Organization_SurveyDetail_Add);
    return isHasPermission;
  }

  ngOnInit() {
    super.ngOnInit();
  }

  onAddNewSurvey() {
    this.router.navigate(['surveys/add/0']);
  }
}
