import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService, FormService, DropDownListItemModel, StringHelper } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class ConsolidationFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/consolidations/internal`);
    }

    getAllLocations() {
        return this.httpService.get(`${environment.commonApiUrl}/countries/AllLocations`);
    }

    create(model: InputConsolidationModel) {
        return this.httpService.create(this.apiUrl, model);
    }

    update(consolidationId, model: UpdateConsolidationModel) {
        return this.httpService.update(`${this.apiUrl}/${consolidationId}`, model);
    }

    isDuplicatedContainer(
        containerId: number,
        containerNo: string,
        carrierSONo: string
    ): Observable<boolean> {
        return this.httpService.get<boolean>(`${environment.apiUrl}/containers/internal/${StringHelper.isNullOrEmpty(containerId) ? 0 : containerId}/checkDuplicateContainer?containerNo=${containerNo}&carrierSONo=${carrierSONo}`);
    }

    getDefaultOriginCFS(
        consignmentId: number
    ): Observable<DropDownListItemModel<string>> {
        return this.httpService.get<DropDownListItemModel<string>>(`${environment.apiUrl}/consignments/${consignmentId}/origincfs`);
    }
}

export class UpdateConsolidationModel {

    private id: number;

    private originCFS: string;

    private cfsCutoffDate: Date;

    private equipmentType: string;

    private carrierSONo: string;

    private loadingDate: Date;

    // Container detail info

    private containerNo: string;

    private sealNo: string;

    private sealNo2: string;

    // /**
    //  *
    //  */
    constructor(id: number,
                originCFS: string,
                cfsCutoffDate: Date,
                equipmentType: string,
                carrierSONo: string,
                containerNo: string,
                sealNo: string,
                sealNo2: string,
                loadingDate: Date) {

        this.id = id;
        this.originCFS = originCFS;
        this.cfsCutoffDate = cfsCutoffDate;
        this.equipmentType = equipmentType;
        this.carrierSONo = carrierSONo;
        this.containerNo = containerNo;
        this.sealNo = sealNo;
        this.sealNo2 = sealNo2;
        this.loadingDate = loadingDate;
    }
}

export class InputConsolidationModel {
    
    private consignmentId: number;

    private originCFS: string;

    private cfsCutoffDate: Date;

    private equipmentType: string;

    private carrierSONo: string;

    private loadingDate: Date;

    private containerNo: string;

    private sealNo: string;

    private sealNo2: string;

    // /**
    //  *
    //  */
    constructor(consignmentId: number,
                originCFS: string,
                cfsCutoffDate: Date,
                equipmentType: string,
                carrierSONo: string,
                containerNo: string,
                sealNo: string,
                sealNo2: string,
                loadingDate: Date) {
        
        this.consignmentId = consignmentId;
        this.originCFS = originCFS;
        this.cfsCutoffDate = cfsCutoffDate;
        this.equipmentType = equipmentType;
        this.carrierSONo = carrierSONo;
        this.containerNo = containerNo;
        this.sealNo = sealNo;
        this.sealNo2 = sealNo2;
        this.loadingDate = loadingDate;
    }
}