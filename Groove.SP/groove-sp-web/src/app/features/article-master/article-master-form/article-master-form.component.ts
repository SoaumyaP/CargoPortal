import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { ArticleMasterStatus, FormComponent } from 'src/app/core';
import { DefaultValue2Hyphens } from 'src/app/core/models/constants/app-constants';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { ArticleMasterFormService } from './article-master-form.service';

@Component({
  selector: 'app-article-master-form',
  templateUrl: './article-master-form.component.html',
  styleUrls: ['./article-master-form.component.scss']
})
export class ArticleMasterFormComponent extends FormComponent {
  modelName = "articleMasters";
  defaultValue = DefaultValue2Hyphens;
  readonly articleMasterStatus = ArticleMasterStatus;
  
  constructor(protected route: ActivatedRoute,
    public notification: NotificationPopup,
    public router: Router,
    public service: ArticleMasterFormService,
    public translateService: TranslateService) {
    super(route, service, notification, translateService, router);
  }

  backList() {
    this.router.navigate(['/article-masters']);
  }

}