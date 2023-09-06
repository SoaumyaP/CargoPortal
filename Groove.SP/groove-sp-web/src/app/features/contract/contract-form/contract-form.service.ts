import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { ContractType } from '../models/contract-type.model';
import { ContractModel } from '../models/contract.model';

@Injectable()
export class ContractFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/contractMasters`);
    }

    createNew(model: ContractModel): Observable<ContractModel> {
        return this.httpService.create<ContractModel>(`${environment.apiUrl}/contractMasters/internal`, model);
    }

    updateContract(model: ContractModel): Observable<ContractModel> {
        return this.httpService.update<ContractModel>(`${environment.apiUrl}/contractMasters/internal/${model.id}`, model);
    }

    getAllContractTypes() {
        return this.httpService.get<ContractType[]>(`${environment.apiUrl}/contractTypes`);
    }

    checkAlreadyExists(contractNo: string) {
        return this.httpService.get<boolean>(`${environment.apiUrl}/contractMasters/${contractNo}/already-exists`);
    }

    updateStatus(id: number, model: ContractModel) {
        return this.httpService.update(`${environment.apiUrl}/contractMasters/internal/${id}/updateStatus`, model);
    }
}
