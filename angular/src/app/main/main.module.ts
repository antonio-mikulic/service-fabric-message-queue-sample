import 'hammerjs';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';

import { MaterialModule } from '../material-module';
import { CdkTableModule } from '@angular/cdk/table';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FlexLayoutModule } from '@angular/flex-layout';
import { MainRoutes } from './main.routing';
import { UsersComponent } from './users/users.component';
import { ServiceProxyModule } from '../../shared/service-proxies/service-proxy.module';
import { CreateOrEditUserDialogComponent } from './users/create-or-edit-user.component';
import { CreateOrEditRoleDialogComponent } from './roles/create-or-edit-role.component';
import { RolesComponent } from './roles/roles.component';

@NgModule({
  imports: [
    CommonModule,
    ServiceProxyModule, 
    RouterModule.forChild(MainRoutes),
    MaterialModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    FlexLayoutModule,
    CdkTableModule
  ],
  providers: [],
  entryComponents: [
    CreateOrEditUserDialogComponent,
    CreateOrEditRoleDialogComponent
  ],
  declarations: [
    UsersComponent,
    CreateOrEditUserDialogComponent,
    RolesComponent,
    CreateOrEditRoleDialogComponent
  ]
})
export class MainComponentsModule {}
