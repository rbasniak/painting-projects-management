import { Route } from '@angular/router';
import { AccessPageComponent, AppLayoutComponent, ErrorPageComponent, NotfoundPageComponent } from '@smz-ui/layout';
import { HomePageComponent } from './ui/pages/home-page.component';
import { AuthStoreDemoComponent } from './ui/pages/auth-store/auth-store-demo.component';
import { StoreHistoryComponent } from './ui/pages/store-history/store-history.component';

export const appRoutes: Route[] = [
  {
    path: '',
    data: {
      smzUiRoot: true,
    },
    children: [
      {
        path: '',
        component: AppLayoutComponent,
        data: {
          smzUiRoot: true,
        },
        children: [
          { path: 'home', component: HomePageComponent, data: { title: 'Home' } },
          { path: 'store-history', component: StoreHistoryComponent, data: { title: 'Store History' } },
          { path: 'auth-demo', component: AuthStoreDemoComponent, data: { title: 'Auth Demo' } },
          { path: '', redirectTo: 'home', pathMatch: 'full' },
        ]
      },
      { path: 'notfound', component: NotfoundPageComponent },
      { path: 'access', component: AccessPageComponent },
      { path: 'error', component: ErrorPageComponent },
    ]
  },
  { path: '**', redirectTo: '/notfound' }
];
