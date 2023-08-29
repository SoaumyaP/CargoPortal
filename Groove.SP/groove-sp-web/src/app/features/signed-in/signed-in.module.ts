import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SignedInComponent } from './signed-in.component';
import { RouterModule } from '@angular/router';

const route = [
  {
      path: '',
      component: SignedInComponent,
      data: {
          pageName: 'signedIn'
      }
    }
];

@NgModule({
  declarations: [SignedInComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(route),
  ]
})

export class SignedInModule { }
