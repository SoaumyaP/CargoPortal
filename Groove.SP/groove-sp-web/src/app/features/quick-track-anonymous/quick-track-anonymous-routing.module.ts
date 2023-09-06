import { Routes, RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { QuickTrackAnonymousComponent } from './quick-track-anonymous.component';
import { ShipmentAnonymousComponent } from './shipment-anonymous/shipment-anonymous.component';
import { InvalidSearchComponent } from 'src/app/ui/error/invalid-search/invalid-search.component';
import { NoResultComponent } from 'src/app/ui/error/no-result/no-result.component';
import { ContainerAnonymousComponent } from './container-anonymous/container-anonymous.component';
import { BillOfLadingAnonymousComponent } from './bill-of-lading-anonymous/bill-of-lading-anonymous.component';

const ROUTES: Routes = [
    {
        path: '',
        component: QuickTrackAnonymousComponent,
        data: {
            pageName: 'quickTrack'
        },
        children: [
            {
                path: 'shipments/:id',
                component: ShipmentAnonymousComponent,
                data:
                {
                    allowAnonymous: true,
                    pageName: 'shipmentDetail'
                }
            },
            {
                path: 'containers/:id',
                component: ContainerAnonymousComponent,
                data:
                {
                    allowAnonymous: true,
                    pageName: 'containerDetail'
                }
            },
            {
                path: 'bill-of-ladings/:id',
                component: BillOfLadingAnonymousComponent,
                data:
                {
                    allowAnonymous: true,
                    pageName: 'BOLDetail'
                }
            },
            {
                path: 'invalid',
                component: InvalidSearchComponent,
                data:
                {
                    allowAnonymous: true,
                    pageName: 'invalidSearch'
                }
            },
            {
                path: 'no-result',
                component: NoResultComponent,
                data:
                {
                    allowAnonymous: true,
                    pageName: 'noResult'
                }
            },
        ]
    }
];

@NgModule({
    imports: [RouterModule.forChild(ROUTES)],
    exports: [RouterModule]
})
export class QuickTrackingAnonymousRoutingModule { }