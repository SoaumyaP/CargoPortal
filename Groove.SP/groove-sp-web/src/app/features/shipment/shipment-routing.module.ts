import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ShipmentListComponent } from './shipment-list/shipment-list.component';
import { AppPermissions } from '../../core/auth/auth-constants';
import { ShipmentTrackingComponent } from './shipment-tracking/shipment-tracking.component';
import { ShipmentFormComponent } from './shipment-form/shipment-form.component';

const routes: Routes = [
    {
        path: '',
        component: ShipmentListComponent,
        data:
        {
            permission: AppPermissions.Shipment_List,
            pageName: 'shipments'
        }
    },
    {
        path: 'search/:referenceNo',
        component: ShipmentListComponent,
        data:
        {
            permission: AppPermissions.Shipment_Detail,
            pageName: 'shipments'
        }
    },
    {
        path: ':id',
        component: ShipmentTrackingComponent,
        data:
        {
            permission: AppPermissions.Shipment_Detail,
            pageName: 'shipmentDetail'
        }
    },
    {
        path: 'edit/:id',
        component: ShipmentFormComponent,
        data:
        {
            permission: AppPermissions.Shipment_Detail_Edit,
            pageName: 'shipmentDetail'
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ShipmentsRoutingModule { }
