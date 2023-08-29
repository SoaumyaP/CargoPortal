import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { BuyerComplianceStatus, UserContextService } from 'src/app/core';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ArticleMasterListService } from './article-master-list.service';

@Component({
  selector: 'app-article-master-list',
  templateUrl: './article-master-list.component.html',
  styleUrls: ['./article-master-list.component.scss']
})
export class ArticleMasterListComponent extends ListComponent implements OnInit {

  listName = 'article-masters';
  faPlus = faPlus;
  buyerComplianceStatus = BuyerComplianceStatus;
  readonly AppPermissions = AppPermissions;
  constructor(service: ArticleMasterListService, route: ActivatedRoute, location: Location, _userContext: UserContextService) {
    super(service, route, location);
    
    _userContext.getCurrentUser().subscribe(user => {
      if (user) {
        if (!user.isInternal) {
          this.service.affiliates = user.affiliates;
          this.service.state = Object.assign({}, this.service.defaultState);
        }
      }
    });
  }
}