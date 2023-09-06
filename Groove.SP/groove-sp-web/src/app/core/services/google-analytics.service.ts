import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { StringHelper } from '../helpers';

declare let gtag: Function;

@Injectable({
    providedIn: 'root'
})
export class GoogleAnalyticsService {

    constructor() { }

    /**
     * To config Google Analytics service.
     * Use this method to set up some additional parameters/user properties that belongs to current session.
     */
    public config(
        value: { [key: string]: any } = null) {

            if (value === null) {
                value = {};
            }

            value['debug_mode'] = environment.gaDebugMode;
            value['send_page_view'] = false;
            const gaTrackingId = environment.gaTrackingId;
            if (!StringHelper.isNullOrEmpty(gaTrackingId)) {
                gtag('config', gaTrackingId, value);
            }
    }

    /**
     * To fire event log to GA
     * @param name Name of event. Naming convention is lowercase without white-space. Ex: submit, submit_booking, cancel_shipment
     * @param category Category name that current event belongs to
     * @param action Name of event
     * @param label Label of event. It is optional
     * @param value Value of event. It is optional
     */
    public emitEvent(
        name: string,
        category: string,
        action: string,
        label: string = null,
        value: string = null) {
        if (!StringHelper.isNullOrWhiteSpace(name)) {
            name = name.replace(/\s\s+/g, ' ').replace(/\s/g, '_').toLocaleLowerCase();
        }
        gtag('event', name, {
            event_category: category,
            event_action: action,
            event_label: label,
            event_value: value
        });
    }

    /**
     * To fire event log to GA with event name is transformed from action value
     * @param action Name of event
     * @param category Category name that current event belongs to
     * @param label Label of event. It is optional
     * @param value Value of event. It is optional
     */
     public emitAction(
        action: string,
        category: string,
        label: string = null,
        value: string = null) {
        this.emitEvent(action, category, action, label, value);
    }

    /**
     * To fire page_view event
     * @param value Additional information for current event. It is key object
     */
    public emitPageView(value: {[key: string]: any}) {
        gtag('event', 'page_view', value);
    }
}
