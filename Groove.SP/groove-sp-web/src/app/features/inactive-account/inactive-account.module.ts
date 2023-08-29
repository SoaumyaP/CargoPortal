import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InactiveAccountComponent } from './inactive-account.component';
import { RouterModule } from '@angular/router';
import { UiModule } from 'src/app/ui';

const route = [
  { path: '', component: InactiveAccountComponent }
];

@NgModule({
  declarations: [InactiveAccountComponent],
  imports: [
    CommonModule,
    UiModule,
    RouterModule.forChild(route)
  ]
})
export class InactiveAccountModule { }
