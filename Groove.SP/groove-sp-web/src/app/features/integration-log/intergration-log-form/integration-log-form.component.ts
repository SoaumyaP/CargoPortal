import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { HttpService, IntegrationLogStatus, DATE_FORMAT } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { IntegrationLogModel } from '../models/integration-log.model';
@Component({
  selector: 'app-integration-log-form',
  templateUrl: './integration-log-form.component.html',
  styleUrls: ['./integration-log-form.component.scss']
})
export class IntegrationLogFormComponent implements OnInit {

    isShowPopup: boolean = false;
    integrationLogStatus = IntegrationLogStatus;
    model: IntegrationLogModel;
    DATE_FORMAT = DATE_FORMAT;

    constructor(private httpService: HttpService, private notification: NotificationPopup) {
    }

    ngOnInit() {
    }

    private getData(id: number): Observable<IntegrationLogModel> {
        return this.httpService.get(`${environment.apiUrl}/integrationLogs/${id}`);
    }
    public open(id: number) {
        this.getData(id).subscribe(
            data => {
                this.model = data;
                this.model.responseJsonPreview = this.previewJsonString(this.model.response);
                this.isShowPopup = true;
            },
            err => {
                this.notification.showErrorPopup(err.errorMessage, 'label.integrationLogs');
            });
    }
    public close() {
        this.isShowPopup = false;
    }

    public previewJsonString(json: string): string {
        try {
            const obj = JSON.parse(
                                json
                                .replace(/\\\\"/g, '\\\\\\"')
                                .replace(/"{/g, '{')
                                .replace(/}"/g, '}')
                                .replace(/\\"/g, '"')
                    );
            const purifiedString = JSON.stringify(obj, undefined, 4);
            return purifiedString.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
                let cls = 'number';
                if (/^"/.test(match)) {
                    if (/:$/.test(match)) {
                        cls = 'key';
                    } else {
                        cls = 'string';
                    }
                } else if (/true|false/.test(match)) {
                    cls = 'boolean';
                } else if (/null/.test(match)) {
                    cls = 'null';
                }
                return '<span class="' + cls + '">' + match + '</span>';
            });
        } catch {
            return json;
        }
    }
}
