import { signal, WritableSignal } from '@angular/core';
import { Topbar } from '@smz-ui/layout';

export const appTopbar: WritableSignal<Topbar> = signal<Topbar>({
  showMenuToggle: true,
  showBackButton: true,
  logoPath: {
    light: 'ppm-logo-horizontal-light.svg',
    dark: 'ppm-logo-horizontal-dark.svg'
  },
  appName: undefined,
  showConfigurator: true,
  showDarkModeToggle: true,
  menuItems: [
    // {
    //   component: {
    //     component: SmzTenantSwitchComponent,
    //     inputs: [],
    //     outputs: []
    //   }
    // },
    {
        icon: 'pi pi-calendar',
        label: 'Calendar',
        routerLink: '/calendar',
        callback: () => {
            console.log('Calendar');
        }
    },
    {
        icon: 'pi pi-user',
        label: 'Profile',
        callback: () => {
            console.log('Profile');
        }
    },
    {
        icon: 'pi pi-inbox',
        label: 'Messages',
        callback: () => {
            console.log('Messages');
        }
    }
  ]
})