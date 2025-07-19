import { Component, inject } from '@angular/core';
import { LayoutService } from '@smz-ui/layout';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'app-home-page',
    imports: [ButtonModule],
    template: `
    <div class="absolute inset-0 flex justify-center items-center">
      <img class="w-3/4 sm:w-2/3 md:w-1/2 lg:w-2/5 xl:w-1/3" alt="PPM Logo" [src]="layoutService.isDarkTheme() ? 'ppm-logo-full-dark.svg' : 'ppm-logo-full-light.svg'"/>
    </div>
    `
})
export class HomePageComponent {
    public readonly layoutService = inject(LayoutService);
}