import { Routes } from '@angular/router';

import { LayoutComponent } from './layout/layout.component';

export const AppRoutes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: '',
        redirectTo: '/users',
        pathMatch: 'full'
      },
      {
        path: '',
        loadChildren:
          () => import('./main/main.module').then(m => m.MainComponentsModule),
      },
    ]
  }
];
