import { Injectable } from '@angular/core';
import { HttpService, FormService, StringHelper } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { NgForm } from '@angular/forms';

@Injectable()
export class ConsignmentItineraryFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/consignments`);
    }

    getAllCarriers(): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/carriers`);
    }

    getAllLocations(): any {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }

    getSchedules(queryParams: string): Observable<any[]> {
        return this.httpService.get(`${environment.apiUrl}/freightSchedulers/filter?${queryParams}`);
    }

    validateFlightNo(model:any, mainForm: NgForm) {
        if (model.scac && model.flightNumber) {
            const lengthOfCarrierCode = model.scac.length;
            const xx = model.flightNumber.substring(0, lengthOfCarrierCode);

            if (xx !== model.scac) {
                mainForm.controls['flightNo'].setErrors({ 'invalidCode': true });
                mainForm.controls['flightNo'].markAsTouched();
                return;
            }
            else{
                const yyy = model.flightNumber.substring(lengthOfCarrierCode, model.flightNumber.length)
                if (!StringHelper.isDigit(yyy) || !(yyy.length === 3 || yyy.length === 4)){
                    mainForm.controls['flightNo'].setErrors({ 'invalidCode': true });
                    mainForm.controls['flightNo'].markAsTouched();
                    return;
                }
            }
            mainForm?.controls['flightNo']?.updateValueAndValidity();
        }
    }
}
