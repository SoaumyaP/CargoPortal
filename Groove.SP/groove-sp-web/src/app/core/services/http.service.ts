import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of, Subject } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { ConstructorBase } from '../models/model-base.model';
import { StringHelper } from '../helpers';
import { AttachmentModel } from '../models/attachment.model';
import { environment } from 'src/environments/environment';

@Injectable({
    providedIn: 'root'
})
export class HttpService {

    /**To store caches of returned data */
    private _cacheStorage: Array<HttpResponseCacheStorage> = [];
    /**To store subscriptions in case request is pending */
    private _cacheRegistries: Array<HttpResponseCacheRegister> = [];

    constructor(
        private _httpClient: HttpClient
    ) { }

    public get<T>(api: string, params?: any): Observable<T> {
        return this.doGet(false, api, params);
    }

    public getWithCache<T>(api: string, params?: any): Observable<T> {
        return this.doGet(true, api, params);
    }

    private doGet<T>(enableCache: boolean, endpoint: string, params?: any): Observable<T> {
        let httpParams = new HttpParams();

        for (const key in params) {
            if (params.hasOwnProperty(key)) {
                httpParams = httpParams.append(key, params[key]);
            }
        }
        const url = endpoint;
        if (enableCache) {
            const caches = this._cacheStorage.filter(x => x.url === endpoint);
            // cache available by url
            if (caches && caches.length > 0) {
                // get the last one for safety
                const theLastCache = caches[caches.length - 1];
                // if request done, return data
                if (theLastCache.mark === CacheMark.Done) {
                    return of(theLastCache.response);
                }
                //  if request in progress, add registry and call later after the current request done
                else if (theLastCache.mark === CacheMark.Progress) {
                    const registry = new Subject<T>();
                    this._cacheRegistries.push(new HttpResponseCacheRegister(url, registry));
                    return registry;
                }
            } else {
                this._cacheStorage.push(new HttpResponseCacheStorage(url, CacheMark.Progress));
            }
        }

        return this._httpClient.get(endpoint, { params: httpParams })
            .pipe(
                map(result => {
                    let data;
                    if (endpoint.indexOf('api/translations/') === -1) {
                        data = this.decorateResponse(result);
                    } else {
                        data = result;
                    }

                    if (enableCache) {
                        const caches = this._cacheStorage.filter(x => x.url === endpoint && x.mark === CacheMark.Progress);
                        if (caches && caches.length > 0) {
                            // get the last one
                           const theLastCache = caches[caches.length - 1];
                           theLastCache.response = data;
                           // mark it as done
                           theLastCache.mark = CacheMark.Done;
                        } else {
                            this._cacheStorage.push(new HttpResponseCacheStorage(url, CacheMark.Done, data));
                        }

                        // If there is registry, then fire event
                        const registries = this._cacheRegistries.filter(x => x.url === endpoint && x.mark ===  CacheMark.Progress);
                        if (registries && registries.length > 0) {
                            registries.forEach(registry => {
                                registry.mark = CacheMark.Done;
                                registry.subject.next(data);
                            });

                            // remove registries which are fired
                            this._cacheRegistries = this._cacheRegistries.filter(x => x.url !== endpoint);
                        }
                    }
                    return data;
                }),
                catchError(this.handleError)
            );
    }

    /**
     * To fire http request with method POST
     * @param api url
     * @param payload body content
     * @param isSilent if it is running in silent mode, no notification displayed on UI
     */
    public create<T>(api: string, payload?: any, isSilent: boolean = true): Observable<T> {
        if (isSilent) {
            return this._httpClient.post(api, payload)
                .pipe(
                    map(result => {
                            if (api.indexOf('api/translations/') === -1) {
                                return this.decorateResponse(result);
                            }
                            return result;
                        }
                    ),
                    catchError(this.handleError)
                );
        } else {
            return this._httpClient.post(api, payload, {reportProgress: false})
                .pipe(
                    map(result => {
                            if (api.indexOf('api/translations/') === -1) {
                                return this.decorateResponse(result);
                            }
                            return result;
                        }
                    ),
                    catchError(this.handleError)
                );
        }
    }

    public update<T>(api: string, payload?: any): Observable<T> {
        return this._httpClient.put(api, payload)
            .pipe(
                map(result => {
                    if (api.indexOf('api/translations/') === -1) {
                        return this.decorateResponse(result);
                    }
                    return result;
                }
            ),
                catchError(this.handleError)
            );
    }

    /**
     * To convert input model to specific class then put into request payload.
     * Use the method to make sure request payload is right exact to provided model.
     * Common use case is that data type of property is custom.
     * Ex: class ClassA {
     *      @DataType(LocalDate)
     *      approvedDate: LocalDate | null = undefined;
     *      }
     */
    public convertModelThenUpdate<InputModel>(type: ConstructorBase, api: string,  payload?: InputModel): Observable<any> {
        let convertedPayload;
        if (!StringHelper.isNullOrEmpty(payload)) {
            convertedPayload = new type(payload);
        }

        return this._httpClient.put(api, convertedPayload ?? payload)
            .pipe(
                map(result => {
                        if (api.indexOf('api/translations/') === -1) {
                            return this.decorateResponse(result);
                        }
                        return result;
                    }
                ),
                catchError(this.handleError)
        );
    }

    public delete<T>(api: string): Observable<T> {
        return this._httpClient.delete(api)
            .pipe(
                map(result => {
                        if (api.indexOf('api/translations/') === -1) {
                            return this.decorateResponse(result);
                        }
                        return result;
                    }
                ),
                catchError(this.handleError)
            );
    }

    public uploadFile(api: string, file: File, type: string): Observable<any> {
        const formData: FormData = new FormData();
        formData.append('type', type);
        formData.append('fileName', file.name);
        formData.append('file', file, file.name);
        return this._httpClient.post(api, formData);
    }

    public downloadFile(url: string, fileName): Observable<any> {
        return this._httpClient.get(url, { responseType: 'blob' })
            .pipe(tap(result => {
                if (result) {
                    const downloadUrl = window.URL.createObjectURL(result);
                    const fileLink = document.createElement('a');
                    fileLink.href = downloadUrl;
                    fileLink.download = fileName;

                    document.body.appendChild(fileLink);
                    fileLink.click();

                    // make sure fileLink is clicked before removing
                    setTimeout(() => {
                        document.body.removeChild(fileLink);
                        window.URL.revokeObjectURL(downloadUrl);
                    }, 100);
                }
            }));
    }

    /**
     * Download all attachments as a ZIP file.
     */
    public downloadAttachments(
        fileName: string,
        attachments: Array<AttachmentModel>): Observable<any> {
            return this._httpClient.post(`${environment.apiUrl}/attachments/download`, attachments, {responseType: "blob"})
                .pipe(tap(result => {
                    if (result) {
                        var textUrl = URL.createObjectURL(result);
                        var element = document.createElement('a');
                        element.setAttribute('href', textUrl);
                        element.setAttribute('download', `${fileName}.zip`);
                        element.style.display = 'none';
                        document.body.appendChild(element);
                        element.click();
                        document.body.removeChild(element);
                    }
                }));
    }

    /**
     * Properties with name ending with 'Date' will be converted to Date object.
     */
    private decorateResponse(object: any): any {
        try {
            for (const item in object) {
                if (object[item] !== null) {
                    if (typeof (object[item]) === 'object') {
                        this.decorateResponse(object[item]);
                    } else if (item.endsWith('Date')) {
                        object[item] = new Date(object[item]);
                    }
                }
            }
        } catch (ex) {
            return null;
        }
        return object;
    }

    private handleError(response: Response): Observable<any> {
        // nothing to do now
        return Observable.throw(response);
    }

    private queryStringParameterize(data) {
        return Object.keys(data).map(key => `${key}=${encodeURIComponent(data[key])}`).join('&');
    }
}

/**Cache model for http */
export class HttpResponseCacheStorage {
    constructor(public url: string, public mark: CacheMark, public response?: any) {

    }
}

/**Status mark for http cache model */
export enum CacheMark {
    Progress,
    Done,
    Failed
}

/**Register model for http cache */
export class HttpResponseCacheRegister {
    public mark: CacheMark;
    constructor(public url: string, public subject: Subject<any>) {
        this.mark = CacheMark.Progress;
    }
}
