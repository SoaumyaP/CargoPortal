import { Routes, RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { ExpiredComponent } from './ui/error/expired/expired.component';

const ROUTES: Routes = [
    { path: '', loadChildren: () => import('./layout/layout.module').then(m => m.LayoutModule) },
    {
        path: 'error/expired', component: ExpiredComponent,
        data:
        {
            allowAnonymous: true,
            pageName: 'titleExpired',
        }
    },
    {
        path: 'quick-track',
        loadChildren: () => import('./features/quick-track-anonymous/quick-track-anonymous.module').then(m => m.QuickTrackAnonymousModule),
        data:
        {
            allowAnonymous: true,   // to handle loading background
        }
    },

    // move these routing declarations into here
    // NOT put in layout-routing because there are some secured http api calls using access token
    // and that may cause problem as OIDC library not finished initialization or authorization yet
    {
        path: 'oidc-callback',
        loadChildren: () => import('./features/oidc-callback/oidc-callback.module').then(m => m.OidcCallbackModule),
        data:
        {
            allowAnonymous: true,
            pageName: 'oidcCallback'
        }
    },
    {
        path: 'login',
        loadChildren: () => import('./features/login/login.module').then(m => m.LoginModule),
        data:
        {
            allowAnonymous: true,
            pageName: 'login'
        }
    },
    {
        path: 'signed-out',
        loadChildren: () =>
            import('./features/signed-out/signed-out.module').then(m => m.SignedOutModule),
        data: {
            allowAnonymous: true,
            pageName: 'signedOut',
        },
    },
];

@NgModule({
    imports: [RouterModule.forRoot(ROUTES)],
    exports: [RouterModule]
})
export class AppRoutingModule { }
