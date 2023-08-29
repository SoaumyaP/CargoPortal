import { Injectable } from '@angular/core';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class BalanceOfGoodsService {
    constructor(httpService: HttpService) {

    }

    public gridLoading = false;
    public affiliates: any = null;
}
