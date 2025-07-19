import { MenuItem } from 'primeng/api';

export const INITIAL_SIDEBAR: MenuItem[] = [
    {
        label: 'Features',
        items: [
            { label: 'Home', icon: 'fa-solid fa-house', routerLink: ['/'] },
        ]
    },
    {
      label: 'Extras',
      items: [
          { label: 'Auth Demo', icon: 'fa-solid fa-user-shield', routerLink: ['/auth-demo'] },
          { label: 'Store History', icon: 'fa-solid fa-clock', routerLink: ['/store-history'] },
      ]
    },
];
