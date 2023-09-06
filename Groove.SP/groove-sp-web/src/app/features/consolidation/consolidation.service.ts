import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { HttpService } from 'src/app/core/services/http.service';
import { ConsolidationModel } from './models/consolidation.model';
import { CargoDetailLoadModel } from 'src/app/core/models/cargo-details/cargo-detail-load.model';
import { UserContextService } from 'src/app/core';

@Injectable()
export class ConsolidationService {
    public currentUser: any;
    constructor(protected httpService: HttpService, private _userContext: UserContextService) {
        this._userContext.getCurrentUser().subscribe(user => {
            if (user) {
                this.currentUser = user;
            }
          });
    }

    getConsolidation(id: number): Observable<ConsolidationModel> {
        if (!this.currentUser.isInternal) {
            return this.httpService.get<ConsolidationModel>(
                `${environment.apiUrl}/consolidations/internal/${id}?affiliates=${this.currentUser.affiliates}`
            );
        }
        return this.httpService.get<ConsolidationModel>(`${environment.apiUrl}/consolidations/internal/${id}`);
    }

    getConsignments(
        consolidationId: number
    ) {
        const {isInternal, affiliates} = this.currentUser;
        if (!isInternal) {
            return this.httpService.get(`${environment.apiUrl}/consolidations/internal/${consolidationId}/consignments?affiliates=${affiliates}`);
        }
        return this.httpService.get(`${environment.apiUrl}/consolidations/internal/${consolidationId}/consignments`);
    }

    getUnloadedCargoDetails (
        id: number
    ): Observable<CargoDetailLoadModel[]> {
        return this.httpService.get<CargoDetailLoadModel[]>(`${environment.apiUrl}/consolidations/internal/${id}/cargoDetails`);
    }

    loadCargoDetails(
        id: number,
        model: Array<CargoDetailLoadModel>
    ) {
        return this.httpService.create(`${environment.apiUrl}/consolidations/internal/${id}/cargoDetails`, model);
    }

    trialConfirmConsolidation(
        id: number
    ) {
        return this.httpService.get<ConsolidationModel>(`${environment.apiUrl}/consolidations/internal/${id}/trialConfirmConsolidation`);
    }

    confirmConsolidation(
        id: number
    ) : Observable<ConsolidationModel> {
        return this.httpService.create<ConsolidationModel>(`${environment.apiUrl}/consolidations/internal/${id}/confirm`);
    }

    unconfirmConsolidation(
        id: number
    ) : Observable<ConsolidationModel> {
        return this.httpService.create<ConsolidationModel>(`${environment.apiUrl}/consolidations/internal/${id}/unconfirm`);
    }

    deleteConsolidation(
        id: number
    ) {
        return this.httpService.delete(`${environment.apiUrl}/consolidations/internal/${id}`);
    }
}