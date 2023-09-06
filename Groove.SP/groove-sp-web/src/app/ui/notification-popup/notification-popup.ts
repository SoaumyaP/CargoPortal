import { Injectable } from '@angular/core';
import { NotificationsService } from 'angular2-notifications';
import { DialogService } from '@progress/kendo-angular-dialog';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
    providedIn: 'root'
})
export class NotificationPopup {
    public messageTypes = {
        alert: 'alert',
        error: 'error',
        info: 'info',
        success: 'success',
        warning: 'warning'
    };

    constructor(public notifications: NotificationsService,
        private translateService: TranslateService,
        public dialog: DialogService) {
    }

    /* ==== POPUP ==== */
    public showAlertPopup(message, title, translate = true, override?: any) {
        this.showNotificationPopup(message, title, this.messageTypes.alert, this.notifications.icons.alert, translate, override);
    }

    public showErrorPopup(message, title, translate = true, override?: any) {
        this.showNotificationPopup(message, title, this.messageTypes.error, this.notifications.icons.error, translate, override);
    }

    public showInfoPopup(message, title, translate = true, override?: any) {
        this.showNotificationPopup(message, title, this.messageTypes.info, this.notifications.icons.info, translate, override);
    }

    public showSuccessPopup(message, title, translate = true, override?: any) {
        this.showNotificationPopup(message, title, this.messageTypes.success, this.notifications.icons.success, translate, override);
    }

    public showWarningPopup(message, title, translate = true, override?: any) {
        this.showNotificationPopup(message, title, this.messageTypes.warning, this.notifications.icons.warn, translate, override);
    }

    public showNotificationPopup(message, title, type, icon, translate = true, override?: any) {
        message = translate ? this.translateService.instant(message) : message;
        title = translate ? this.translateService.instant(title) : title;
        type = type !== 'warning' ? type : 'warn';
        this.notifications.set({ title: title, content: message, type: type, icon: icon, override: override }, true);
    }

    /* ==== DIALOG ==== */
    public showConfirmationDialog(message, title, cancel = false, translate = true, width = null) {
        message = translate ? this.translateService.instant(message) : message;
        title = translate ? this.translateService.instant(title) : title;

        const actions: Array<any> = [
            { text: this.translateService.instant('label.yes'), value: true, primary: true },
            { text: this.translateService.instant('label.no'), value: false }
        ];

        if (cancel) {
            actions.push({ text: this.translateService.instant('label.cancel') });
        }

        if (width) {
            return this.dialog.open({
                title: title,
                content: message,
                actions: actions,
                width: width
            });
        } else {
            return this.dialog.open({
                title: title,
                content: message,
                actions: actions
            });
        }
    }

    public showInfoDialog(message, title, cancel = false, translate = true, width = null) {
        message = translate ? this.translateService.instant(message) : message;
        title = translate ? this.translateService.instant(title) : title;

        const actions: Array<any> = [
            { text: this.translateService.instant('label.ok'), value: false, primary: true },
        ];

        if (cancel) {
            actions.push({ text: this.translateService.instant('label.cancel') });
        }

        if (width) {
            return this.dialog.open({
                title: title,
                content: message,
                actions: actions,
                width: width
            });
        } else {
            return this.dialog.open({
                title: title,
                content: message,
                actions: actions
            });
        }
    }
}
