import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiModule } from 'src/app/ui';
import { CommonService } from 'src/app/core/services/common.service';
import { ArticleMasterRoutingModule } from './article-master-routing.module';
import { ArticleMasterListComponent } from './article-master-list/article-master-list.component';
import { ArticleMasterListService } from './article-master-list/article-master-list.service';
import { ArticleMasterFormComponent } from './article-master-form/article-master-form.component';
import { ArticleMasterFormService } from './article-master-form/article-master-form.service';


@NgModule({
    declarations: [
        ArticleMasterListComponent,
        ArticleMasterFormComponent
    ],
    imports: [
        CommonModule,
        ArticleMasterRoutingModule,
        UiModule
    ],
    providers: [
        ArticleMasterListService,
        ArticleMasterFormService,
        CommonService
    ]
})
export class ArticleMasterModule { }