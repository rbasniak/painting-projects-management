import { signal, WritableSignal } from '@angular/core';
import { Footer } from '@smz-ui/layout';

export const appFooter: WritableSignal<Footer> = signal<Footer>({
    leftLogoPath: {
        light: 'ppm-logo-horizontal-light.svg',
        dark: 'ppm-logo-horizontal-dark.svg'
    },
    version: '1.0.0',
    copyright: 'Copyright 2025 SMZ'
})