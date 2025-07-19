import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient } from '@angular/common/http';
import { withFetch } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { appRoutes } from './app.routes';
import { appLogging } from './layout/app.logging';
import { provideSmzUILayout, SMZ_UI_LAYOUT_CONFIG } from '@smz-ui/layout';
import { appLayoutState } from './layout/app.state';
import { appSidebar } from './layout/app.sidebar';
import { appFooter } from './layout/app.footer';
import { appTopbar } from './layout/app.topbar';
import { appLayout } from './layout/app.layout';
import { authStoreProvider } from './ui/pages/auth-store/auth-store.provider';
import { provideStoreHistory } from '@smz-ui/store';
import { provideLogging } from '@smz-ui/core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    // provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes, withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' }), withEnabledBlockingInitialNavigation()),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch()),
    provideAnimationsAsync(),
    providePrimeNG({ theme: { preset: Aura, options: { darkModeSelector: '.app-dark' } } }),
    provideSmzUILayout(() => [
      {
        sidebar: appSidebar,
        footer: appFooter,
        topbar: appTopbar,
        layout: appLayout,
        state: appLayoutState
      }
    ]),
    provideAppInitializer(async () => {
      const config = inject(SMZ_UI_LAYOUT_CONFIG);
      config.hasClaim = () => true;
    }),
    provideStoreHistory(),
    provideLogging(appLogging),
    authStoreProvider,
  ],
};
