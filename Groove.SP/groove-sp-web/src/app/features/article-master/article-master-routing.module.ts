import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { ArticleMasterFormComponent } from './article-master-form/article-master-form.component';
import { ArticleMasterListComponent } from './article-master-list/article-master-list.component';


const routes: Routes = [
    {
        path: '',
        component: ArticleMasterListComponent,
        data:
        {
            permission: AppPermissions.Organization_ArticleMaster_List,
            pageName: 'listOfArticles'
        }
    },
    {
        path: ':mode/:id',
        component: ArticleMasterFormComponent,
        data:
        {
            pageName: 'articleDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ArticleMasterRoutingModule { }