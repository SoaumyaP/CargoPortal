import { Observable } from 'rxjs';
import { HttpService } from '../services/http.service';
import { environment } from 'src/environments/environment';

export abstract class FormService<T> {

    affiliateCodes: any = null;

    // apply for shipper to fetch PO
    customerRelationships: any = null;
    organizationId: number = 0;

    constructor(protected httpService: HttpService, public apiUrl: string ) {

    }

    public get(params?: any): Observable<T> {
        return this.httpService.get<T>(this.apiUrl, params);
    }

    public create(model: T): Observable<T> {
        return this.httpService.create(`${this.apiUrl}`, model);
    }

    public update(id: number|string, model: T): Observable<T> {
        return this.httpService.update(`${this.apiUrl}/${id}`, model);
    }

    public delete(id: string): Observable<T> {
        return this.httpService.delete<T>(`${this.apiUrl}/${id}`);
    }

    public getData(sourceUrl, params?: any): Observable<T> {
        return this.httpService.get<T>(sourceUrl, params);
    }

    public checkApiPrefix(api: string) {
        const affiliateApiPrefix = [
            'shipments',
            'containers',
            'billofladings',
            'masterbillofladings',
            'purchaseorders',
            'pofulfillments',
            'bulkfulfillments',
            'warehouseFulfillments',
            'buyerapprovals',
            'consignments',
            'consolidations/internal',
            'routingorders'
        ];

        if (affiliateApiPrefix.map(x => `${environment.apiUrl}/${x}`.toLocaleLowerCase()).indexOf(api.toLowerCase()) >= 0) {
            return true;
        }
        return false;
    }

    protected _onlyUnique = function(value, index, self) {
        return self.indexOf(value) === index;
    };
}
