import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { HttpService, DATE_HOUR_FORMAT, DATE_FORMAT, FormModeType } from 'src/app/core';
import { environment } from 'src/environments/environment';
@Component({
  selector: 'app-booking-validation-log-popup',
  templateUrl: './booking-validation-log-popup.component.html',
  styleUrls: ['./booking-validation-log-popup.component.scss']
})
export class BookingValidationLogPopupComponent implements OnInit {

    isShowPopup: boolean = false;
    model = null;
    DATE_FORMAT = DATE_FORMAT;
    DATE_HOUR_FORMAT = DATE_HOUR_FORMAT;
    formModeType = FormModeType;

    constructor(private httpService: HttpService, private notification: NotificationPopup) {
    }

    ngOnInit() {
    }

    private getData(id: number): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/bookingValidationLogs/${id}`);
    }
    public open(id: number) {
        this.getData(id).subscribe(
            data => {
                this.model = data;
                this.isShowPopup = true;
            },
            err => {
                this.notification.showErrorPopup(err.errorMessage, 'label.bookingValidationLogs');
            });
    }
    public close() {
        this.isShowPopup = false;
    }
}
