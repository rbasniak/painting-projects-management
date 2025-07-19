/** @type {import('tailwindcss').Config} */
import PrimeUI from 'tailwindcss-primeui';

export default {
  darkMode: ['selector', '[class="app-dark"]'],
  content: [
    './apps/painting-projects-management/src/**/*.{html,ts,scss,css}',
    'node_modules/@smz-ui/core/**/*.{html,ts,tsx,js,jsx,mjs}',
    'node_modules/@smz-ui/layout/**/*.{html,ts,tsx,js,jsx,mjs}',
  ],
  plugins: [PrimeUI],
  theme: {
    screens: {
      sm: '576px',
      md: '768px',
      lg: '992px',
      xl: '1200px',
      '2xl': '1920px',
    },
    extend: {
      colors: {
        'surface-border': 'var(--surface-border)',
      },
    },
  },
};
