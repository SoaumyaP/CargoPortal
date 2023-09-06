import { Injectable } from '@angular/core';
import { HttpService, ImportDataProgressStatus } from 'src/app/core';
import { TranslateService } from '@ngx-translate/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class BackgroundJobMonitor {
    public onTimeout = () => { };
    public onFinished = (data: any) => { };
    public onError = (error: any) => { };
    public onProgressChanged = (data: any) => { };

    constructor(private httpService: HttpService, private translation: TranslateService) {
    }

    public monitor(backgroundId: number, timeout: number = null, startTime: number = null) {
        const delay = 2000; // polling delay time

        // time out duration
        if (!timeout) {
            timeout = 120000;
        }

        if (!startTime) {
            startTime = (new Date()).getTime();
        }

        const duration = (new Date()).getTime() - startTime;
        if (duration > timeout) {
            this.onTimeout();
            return;
        }

        this.httpService.get(`${environment.apiUrl}/ImportDataProgress/${backgroundId}`)
            .subscribe((data: any) => {
                this.onProgressChanged(data);
                const status = data['status'];
                if (status === ImportDataProgressStatus.Success
                    || status === ImportDataProgressStatus.Failed
                    || status === ImportDataProgressStatus.Aborted
                    || status === ImportDataProgressStatus.Warning) {
                    this.onFinished(data);
                    return;
                }

                setTimeout(() => this.monitor(data.id, startTime), delay);
            },
            error => {
                this.onError(error);
        });
    }
}