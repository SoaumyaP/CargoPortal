import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ShippingContainerRoutingModule } from './shipping-container-routing.module';
import { ShippingContainerListComponent } from './shipping-container-list/shipping-container-list.component';
import { ShippingContainerListService } from './shipping-container-list/shipping-container-list.service';
import { UiModule } from 'src/app/ui';
import { ContainerTrackingComponent } from './container-tracking/container-tracking.component';
import { ContainerFormService } from './container-tracking/container-form.service';
import { AttachmentUploadPopupService } from 'src/app/ui/attachment-upload-popup/attachment-upload-popup.service';
import { ContainerActivityFormComponent } from './container-activity-form/container-activity-form.component';


@NgModule({
    declarations: [
      ShippingContainerListComponent,
      ContainerTrackingComponent,
      ContainerActivityFormComponent,
    ],
    imports: [
        CommonModule,
        ShippingContainerRoutingModule,
        UiModule
    ],
    providers : [
        ShippingContainerListService,
        ContainerFormService,
        AttachmentUploadPopupService
    ]
})
export class ShippingContainerModule { }
