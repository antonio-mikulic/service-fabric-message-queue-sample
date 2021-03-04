import { Routes } from '@angular/router';
import { UsersComponent } from './users/users.component';
import { RolesComponent } from './roles/roles.component';

export const MainRoutes: Routes = [
  {
    path: 'users',
    component: UsersComponent
  },
  {
    path: 'roles',
    component: RolesComponent
  },

];
