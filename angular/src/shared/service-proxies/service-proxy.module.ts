import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import * as ApiServiceProxies from './service-proxies';
import { CustomHttpInterceptor } from '../helpers/custom-http-interceptor';

@NgModule({
    providers: [
        ApiServiceProxies.TokenAuthServiceProxy,
        ApiServiceProxies.UserManagementServiceProxy,
        ApiServiceProxies.RoleManagementServiceProxy,
        ApiServiceProxies.BrokerServiceProxy,
        { provide: HTTP_INTERCEPTORS, useClass: CustomHttpInterceptor, multi: true }
    ]
})
export class ServiceProxyModule { }
