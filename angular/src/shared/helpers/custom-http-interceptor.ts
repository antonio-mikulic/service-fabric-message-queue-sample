import { Injectable } from '@angular/core';
import { Observable, Subject, of } from 'rxjs';

import {  HttpInterceptor, HttpHandler, HttpRequest, HttpEvent, HttpResponse,  HttpHeaders } from '@angular/common/http';

// Inspired by abp-ng2-module https://github.com/aspnetboilerplate/abp-ng2-module
export interface IValidationErrorInfo {
    message: string;
    members: string[];
}

export interface IErrorInfo {
    code: number;
    message: string;
    details: string;
    validationErrors: IValidationErrorInfo[];
}

export interface IAjaxResponse {
    success: boolean;
    result?: any;
    targetUrl?: string;
    error?: IErrorInfo;
    unAuthorizedRequest: boolean;
}

@Injectable()
export class HttpConfiguration {

    defaultError = <IErrorInfo>{
        message: 'An error has occurred!',
        details: 'Error details were not sent by server.'
    };

    handleTargetUrl(targetUrl: string): void {
        if (!targetUrl) {
            location.href = '/';
        } else {
            location.href = targetUrl;
        }
    }

    handleUnAuthorizedRequest(messagePromise: any, targetUrl?: string) {
        if (messagePromise) {
            messagePromise.done(() => {
                this.handleTargetUrl(targetUrl || '/');
            });
        } else {
            this.handleTargetUrl(targetUrl || '/');
        }
    }


    processResponse(response: HttpResponse<any>, ajaxResponse: IAjaxResponse): HttpResponse<any> {
        var newResponse: HttpResponse<any>;
        
        if (ajaxResponse.success) {
            
            newResponse = response.clone({
                body: ajaxResponse.result
            });

            if (ajaxResponse.targetUrl) {
                this.handleTargetUrl(ajaxResponse.targetUrl);;
            }
        } else {

            newResponse = response.clone({
                body: ajaxResponse.result
            });

            if (!ajaxResponse.error) {
                ajaxResponse.error = this.defaultError;
            }

            if (response.status === 401) {
                ajaxResponse.error = this.defaultError;
                this.handleUnAuthorizedRequest(null, ajaxResponse.targetUrl);
            }
        }

        return newResponse;
    }

    getAjaxResponseOrNull(response: HttpResponse<any>): IAjaxResponse | null {
        if(!response || !response.headers) {
            return null;
        }

        var responseObj = JSON.parse(JSON.stringify(response.body));
        return responseObj as IAjaxResponse;
    }

    handleResponse(response: HttpResponse<any>): HttpResponse<any> {
        var ajaxResponse = this.getAjaxResponseOrNull(response);
        if (ajaxResponse == null) {
            return response;
        }

        return this.processResponse(response, ajaxResponse);
    }

    blobToText(blob: any): Observable<string> {
        return new Observable<string>((observer: any) => {
            if (!blob) {
                observer.next("");
                observer.complete();
            } else {
                let reader = new FileReader(); 
                reader.onload = function() { 
                    observer.next(this.result);
                    observer.complete();
                }
                reader.readAsText(blob); 
            }
        });
    }
}

@Injectable()
export class CustomHttpInterceptor implements HttpInterceptor {
  
    protected configuration: HttpConfiguration;

    constructor(configuration: HttpConfiguration) {
        this.configuration = configuration;
    }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    
    var interceptObservable = new Subject<HttpEvent<any>>();
    var modifiedRequest = this.normalizeRequestHeaders(request);
    
    next.handle(modifiedRequest)
        .subscribe((event: HttpEvent<any>) => {
            this.handleSuccessResponse(event, interceptObservable );
        }, (error: any) => {
            return this.handleErrorResponse(error, interceptObservable);
        });

    return interceptObservable;
  }

  protected normalizeRequestHeaders(request: HttpRequest<any>):HttpRequest<any> {
        var modifiedHeaders = new HttpHeaders();
        modifiedHeaders = request.headers.set("Pragma","no-cache")
                                            .set("Cache-Control","no-cache")
                                            .set("Expires", "Sat, 01 Jan 2000 00:00:00 GMT");
        
        modifiedHeaders = this.addXRequestedWithHeader(modifiedHeaders);
        modifiedHeaders = this.addAuthorizationHeaders(modifiedHeaders);

        return request.clone({
            headers: modifiedHeaders
        });
    }

    protected addXRequestedWithHeader(headers:HttpHeaders):HttpHeaders {
        if (headers) {
            headers = headers.set('X-Requested-With', 'XMLHttpRequest');
        }

        return headers;
    }

    protected addAuthorizationHeaders(headers:HttpHeaders): HttpHeaders {
        let authorizationHeaders = headers ? headers.getAll('Authorization'): null;
        if (!authorizationHeaders) {
            authorizationHeaders = [];
        }

        if (!this.itemExists(authorizationHeaders, (item: string) => item.indexOf('Bearer ') == 0)) {
            //let token = this._tokenService.getToken();
            let token = "emptyForNow";
            if (headers && token) {
                headers = headers.set('Authorization', 'Bearer ' + token);
            }
        }

        return headers;
    }

    protected handleSuccessResponse(event: HttpEvent<any>, interceptObservable: Subject<HttpEvent<any>>): void{
        if (event instanceof HttpResponse) {
            if (event.body instanceof Blob && event.body.type && event.body.type.indexOf("application/json") >= 0){
                this.configuration.blobToText(event.body).subscribe(json => {
                    const responseBody = json == "null" ? {}: JSON.parse(json);
                    
                    var modifiedResponse = this.configuration.handleResponse(event.clone({
                        body: responseBody
                    }));
                    
                    interceptObservable.next(modifiedResponse.clone({
                        body: new Blob([JSON.stringify(modifiedResponse.body)], {type : 'application/json'})
                    }));

                    interceptObservable.complete();
                });
            } else {
                interceptObservable.next(event);
                interceptObservable.complete();
            }
        } else {
            interceptObservable.next(event);
        }
    }

    protected handleErrorResponse(error: any, interceptObservable: Subject<HttpEvent<any>>): Observable<any> {
        var errorObservable = new Subject<any>();

        if(!(error.error instanceof Blob)){
            interceptObservable.error(error);
            interceptObservable.complete();
            return of({});
        }

        this.configuration.blobToText(error.error).subscribe(json => {
            const errorBody = (json == "" || json == "null") ? {}: JSON.parse(json);
            const errorResponse = new HttpResponse({
                headers: error.headers,
                status: error.status,
                body: errorBody
            });

            var ajaxResponse = this.configuration.getAjaxResponseOrNull(errorResponse);
            
            if (ajaxResponse != null) {
                this.configuration.processResponse(errorResponse, ajaxResponse);
            } 

            errorObservable.complete();
            
            interceptObservable.error(error);
            interceptObservable.complete();
        });
        
        return errorObservable;
    }

    private itemExists<T>(items: T[], predicate: (item: T) => boolean): boolean {
        for (let i = 0; i < items.length; i++) {
            if (predicate(items[i])) {
                return true;
            }
        }

        return false;
    }
}
