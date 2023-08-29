import { Routes, RouterModule } from '@angular/router';
import { AuthorizationGuard } from '../core/auth/auth.guard';
import { LayoutComponent } from './layout.component';
import { NgModule } from '@angular/core';
import { NotFoundComponent } from '../ui/error/not-found/not-found.component';
import { UnauthorizedComponent } from '../ui/error/unauthorized/unauthorized.component';
import { MaintenanceComponent } from '../ui/error/maintenance/maintenance.component';
import { NoResultComponent } from '../ui/error/no-result/no-result.component';
import { InvalidSearchComponent } from '../ui/error/invalid-search/invalid-search.component';

const LAYOUT_ROUTES: Routes = [
    {
        path: '',
        component: LayoutComponent,
        canActivate: [AuthorizationGuard],
        canActivateChild: [AuthorizationGuard],
        children: [
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            {
                path: 'home',
                loadChildren: () =>
                    import('../features/home/home.module').then(
                        (m) => m.HomeModule
                    ),
            },
            {
                path: 'purchase-orders',
                loadChildren: () =>
                    import('../features/order/order.module').then(
                        (m) => m.OrderModule
                    ),
            },
            {
                path: 'list-purchase-orders',
                loadChildren: () =>
                    import('../features/order/order.module').then(
                        (m) => m.OrderModule
                    ),
            },
            {
                path: 'routing-orders',
                loadChildren: () =>
                    import('../features/routing-order/routing-order.module').then(
                        (m) => m.RoutingOrderModule
                    ),
            },
            {
                path: 'po-progress-check',
                loadChildren: () =>
                    import('../features/po-progress-check/po-progress-check.module').then(
                        (m) => m.POProgressCheckModule
                    ),
            },
            {
                path: 'cruise-orders',
                loadChildren: () =>
                    import('../features/cruise-order/cruise-order.module').then(
                        (m) => m.CruiseOrderModule
                    ),
            },
            {
                path: 'po-fulfillments',
                loadChildren: () =>
                    import(
                        '../features/po-fulfillment/po-fulfillment.module'
                    ).then((m) => m.POFulfillmentModule),
            },
            {
                path: 'missing-po-fulfillments',
                loadChildren: () =>
                    import(
                        '../features/missing-po-fulfillment/missing-po-fulfillment.module'
                    ).then((m) => m.MissingPOFulfillmentModule),
            },
            {
                path: 'bulk-fulfillments',
                loadChildren: () =>
                    import(
                        '../features/bulk-fulfillment/bulk-fulfillment.module'
                    ).then((m) => m.BulkFulfillmentModule),
            },
            {
                path: 'warehouse-bookings',
                loadChildren: () =>
                    import(
                        '../features/warehouse-fulfillment/warehouse-fulfillment.module'
                    ).then((m) => m.WarehouseFulfillmentModule),
            },
            {
                path: 'warehouse-bookings-confirm',
                loadChildren: () =>
                    import(
                        '../features/warehouse-fulfillment/warehouse-fulfillment-confirm/warehouse-fulfillment-confirm.module'
                    ).then((m) => m.WarehouseFulfillmentConfirmModule),
            },
            {
                path: 'buyer-approvals',
                loadChildren: () =>
                    import(
                        '../features/buyer-approval/buyer-approval.module'
                    ).then((m) => m.BuyerApprovalModule),
            },
            {
                path: 'shipments',
                loadChildren: () =>
                    import('../features/shipment/shipment.module').then(
                        (m) => m.ShipmentModule
                    ),
            },
            {
                path: 'list-shipments',
                loadChildren: () =>
                    import('../features/shipment/shipment.module').then(
                        (m) => m.ShipmentModule
                    ),
            },
            {
                path: 'shortships',
                loadChildren: () =>
                    import(
                        '../features/shortship/shortship.module'
                    ).then((m) => m.ShortshipModule),
            },
            {
                path: 'shipped-purchase-orders',
                loadChildren: () =>
                    import('../features/shipped-order/shipped-order.module').then(
                        (m) => m.ShippedOrderModule
                    ),
            },
            {
                path: 'containers',
                loadChildren: () =>
                    import('../features/shipping-container/shipping-container.module').then(
                        (m) => m.ShippingContainerModule
                    ),
            },
            {
                path: 'freight-schedulers',
                loadChildren: () =>
                    import('../features/freight-scheduler/freight-scheduler.module').then(
                        (m) => m.FreightSchedulerModule
                    ),
            },
            {
                path: 'organizations',
                loadChildren: () =>
                    import('../features/organization/organization.module').then(
                        (m) => m.OrganizationModule
                    ),
            },
            {
                path: 'compliances',
                loadChildren: () =>
                    import('../features/compliance/compliance.module').then(
                        (m) => m.ComplianceModule
                    ),
            },
            {
                path: 'vessels',
                loadChildren: () =>
                    import('../features/vessel/vessel.module').then(
                        (m) => m.VesselModule
                    ),
            },
            {
                path: 'vessel-arrivals',
                loadChildren: () =>
                    import('../features/vessel-arrival/vessel-arrival.module').then(
                        (m) => m.VesselArrivalModule
                    ),
            },
            {
                path: 'carriers',
                loadChildren: () =>
                    import('../features/carrier/carrier.module').then(
                        (m) => m.CarrierModule
                    )
            },
            {
                path: 'contracts',
                loadChildren: () =>
                    import('../features/contract/contract.module').then(
                        (m) => m.ContractModule
                    )
            },
            {
                path: 'locations',
                loadChildren: () =>
                    import('../features/location/location.module').then(
                        (m) => m.LocationModule
                    ),
            },
            {
                path: 'events',
                loadChildren: () =>
                    import('../features/master-event/master-event.module').then(
                        (m) => m.MasterEventModule
                    ),
            },
            {
                path: 'warehouse-locations',
                loadChildren: () =>
                    import('../features/warehouse-location/warehouse-location.module').then(
                        (m) => m.WarehouseLocationModule
                    ),
            },
            {
                path: 'article-masters',
                loadChildren: () =>
                    import(
                        '../features/article-master/article-master.module'
                    ).then((m) => m.ArticleMasterModule),
            },
            {
                path: 'surveys',
                loadChildren: () =>
                    import(
                        '../features/survey/survey.module'
                    ).then((m) => m.SurveyModule),
            },
            {
                path: 'events',
                loadChildren: () =>
                    import(
                        '../features/master-event/master-event.module'
                    ).then((m) => m.MasterEventModule),
            },
            {
                path: 'user-requests',
                loadChildren: () =>
                    import('../features/user-request/user-request.module').then(
                        (m) => m.UserRequestModule
                    ),
            },
            {
                path: 'user-profile',
                loadChildren: () =>
                    import(
                        '../features/user/user-profile/user-profile.module'
                    ).then((m) => m.UserProfileModule),
            },
            {
                path: 'users',
                loadChildren: () =>
                    import('../features/user/users/users.module').then(
                        (m) => m.UsersModule
                    ),
            },
            {
                path: 'roles',
                loadChildren: () =>
                    import('../features/user/roles/roles.module').then(
                        (m) => m.RolesModule
                    ),
            },
            {
                path: 'signed-in',
                loadChildren: () =>
                    import('../features/signed-in/signed-in.module').then(
                        (m) => m.SignedInModule
                    ),
            },
            {
                path: 'invoices',
                loadChildren: () =>
                    import('../features/invoice/invoice.module').then(
                        (m) => m.InvoiceModule
                    ),
            },
            {
                path: 'roles',
                loadChildren: () =>
                    import('../features/user/roles/roles.module').then(
                        (m) => m.RolesModule
                    ),
            },
            {
                path: 'bill-of-ladings',
                loadChildren: () =>
                    import(
                        '../features/shipment/bill-of-lading/bill-of-lading.module'
                    ).then((m) => m.BillOfLadingModule),
            },
            {
                path: 'master-bill-of-ladings',
                loadChildren: () =>
                    import(
                        '../features/shipment/master-bill-of-lading/master-bill-of-lading.module'
                    ).then((m) => m.MasterBillOfLadingModule),
            },
            {
                path: 'consignments',
                loadChildren: () =>
                    import('../features/consignment/consignment.module').then(
                        (m) => m.ConsignmentModule
                    ),
            },
            {
                path: 'master-dialogs',
                loadChildren: () =>
                    import('../features/master-dialog/master-dialog.module').then(
                        (m) => m.MasterDialogModule
                    ),
            },
            {
                path: 'integration-logs',
                loadChildren: () =>
                    import(
                        '../features/integration-log/integration-log.module'
                    ).then((m) => m.IntegrationLogModule),
            },
            {
                path: 'reports',
                loadChildren: () =>
                    import('../features/report/report.module').then(
                        (m) => m.ReportModule
                    ),
            },
            {
                path: 'scheduling',
                loadChildren: () =>
                    import('../features/scheduling/scheduling.module').then(
                        (m) => m.SchedulingModule
                    ),
            },
            {
                path: 'error/404',
                component: NotFoundComponent,
                data: {
                    pageName: 'notFound',
                },
            },
            {
                path: 'error/401',
                component: UnauthorizedComponent,
                data: {
                    pageName: 'unauthorized',
                },
            },
            {
                path: 'maintenance',
                component: MaintenanceComponent,
                data: {
                    pageName: 'maintenance',
                },
            },
            {
                path: 'search/no-result',
                component: NoResultComponent,
                data: {
                    pageName: 'noResult',
                },
            },
            {
                path: 'search/invalid',
                component: InvalidSearchComponent,
                data: {
                    pageName: 'invalidSearch',
                },

            },
            {
                path: 'consolidations',
                loadChildren: () =>
                    import('../features/consolidation/consolidation.module').then(
                        (m) => m.ConsolidationModule)
            },
            {
                path: 'balance-of-goods',
                loadChildren: () =>
                    import(
                        '../features/balance-of-goods/balance-of-goods.module'
                    ).then((m) => m.BalanceOfGoodsModule),
            },
        ],
    },
    {
        path: 'inactive-account',
        loadChildren: () =>
            import('../features/inactive-account/inactive-account.module').then(
                (m) => m.InactiveAccountModule
            ),
        data: {
            allowAnonymous: true,
            pageName: 'inactiveAccount',
        },
    },
    {
        path: 'inactive-role',
        loadChildren: () =>
            import('../features/inactive-account/inactive-account.module').then(
                (m) => m.InactiveAccountModule
            ),
        data: {
            allowAnonymous: true,
            pageName: 'inactiveRole',
        },
    },
    // {
    //     path: 'balance-of-goods',
    //     loadChildren: () =>
    //         import(
    //             '../features/balance-of-goods/balance-of-goods.module'
    //         ).then((m) => m.BalanceOfGoodsModule),
    // },
];

@NgModule({
    imports: [RouterModule.forChild(LAYOUT_ROUTES)],
    exports: [RouterModule],
})
export class LayoutRoutingModule { }
