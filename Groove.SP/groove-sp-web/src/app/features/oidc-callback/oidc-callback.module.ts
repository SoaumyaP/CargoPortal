import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OidcCallbackComponent } from './oidc-callback.component';
import { RouterModule } from '@angular/router';

const route = [
    {
        path: '',
        component: OidcCallbackComponent,
        data: {
            pageName: 'oidcCallback'
        }
      }
  ];

@NgModule({
  declarations: [OidcCallbackComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(route),
  ]
})
export class OidcCallbackModule { }
