import { Injectable } from '@angular/core';
import { HttpService, FormService, EventLevelMapping, BuyerComplianceServiceType, EntityType, DropDownListItemModel, ModeOfTransportType, DropdownListModel, POType, POStageType } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { mergeMap, map, reduce, first } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { NoteModel } from 'src/app/core/models/note.model';
import { PurchaseOrderTabModel } from '../models/purchase-order-tab.model';

@Injectable()
export class PurchaseOrderDetailService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/purchaseorders`);
    }

    getCarrier(carrierCode): any {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/carriers`, { code: carrierCode }).pipe(
            map(result => result[0])
        );
    }

    getGateway(portCode): any {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/ports`, { code: portCode }).pipe(
            map(result => result[0])
        );
    }

    getActivity(id: number, request: any): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/globalidactivities/get-by-po/${id}`, request);
    }

    getOrganization(id): Observable<any> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}`);
    }

    getInformationFromArticleMaster(id, polineItemId): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/purchaseorders/${id}/polineitems/${polineItemId}/articlemaster`);
    }

    getNotes(purchaseOrderId: number): Observable<NoteModel[]> {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/${purchaseOrderId}/notes`);
    }

    getMasterNotes(purchaseOrderId: number) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/${purchaseOrderId}/masterDialogs`);
    }

    getAllLocations(): any {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }

    getEvents(): any {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/eventCodes/eventByLevel?level=${EventLevelMapping.PurchaseOrder}`);
    }

    createActivity(poId, model) {
        return this.httpService.create(`${environment.apiUrl}/purchaseorders/${poId}/activities`, model);
    }

    deleteActivity(poId, activityId) {
        return this.httpService.delete(`${environment.apiUrl}/purchaseorders/${poId}/activities/${activityId}`);
    }

    updateActivity(poId, activityId, model) {
        return this.httpService.update(`${environment.apiUrl}/purchaseorders/${poId}/activities/${activityId}`,
            model);
    }

    close(id, model) {
        return this.httpService.update(`${environment.apiUrl}/purchaseorders/${id}/close`, model);
    }

    getDefaultMilestone(customerServiceType?: BuyerComplianceServiceType | null, modeOfTransport?: ModeOfTransportType) {
        let milestone: any[] =
            [
                {
                    activityCode: '1051',
                    milestone: 'label.forwarderBookingRequest'
                },
                {
                    activityCode: '1061',
                    milestone: 'label.forwarderBookingConfirmed'
                }
            ];

        switch (customerServiceType) {
            case BuyerComplianceServiceType.Freight:

                milestone.push({
                    activityCode: '7001',
                    milestone: 'label.shipmentDispatch'
                });
                milestone.push({
                    activityCode: '7003',
                    milestone: 'label.shipmentDispatch'
                });

                milestone = milestone.concat(
                    [
                        {
                            activityCode: '1010',
                            milestone: 'label.closed'
                        },
                    ]
                )
                break;

            case BuyerComplianceServiceType.WareHouse:
                milestone = milestone.concat(
                    [
                        {
                            activityCode: '1063',
                            milestone: 'label.cargoReceived'
                        },
                    ]
                )
                break;

            default:
                break;
        }
        return milestone.reverse();
    }

    getActivityOptionFilter(customerServiceType: BuyerComplianceServiceType): DropdownListModel<string>[] {
        if (customerServiceType === BuyerComplianceServiceType.WareHouse) {
            return [
                {
                    label: 'label.fulfillmentNumber',
                    value: 'BookingNo'
                }
            ]
        }

        return [
            {
                label: 'label.fulfillmentNumber',
                value: 'BookingNo'
            },
            {
                label: 'label.shipmentNo',
                value: 'ShipmentNo'
            },
            {
                label: 'label.containerNo',
                value: 'ContainerNo'
            },
            {
                label: 'label.vesselName',
                value: 'VesselName'
            }
        ]
    }

    getFilterActivityValueDropdown(filterBy: string, poId: number): Observable<DropDownListItemModel<string>[]> {
        return this.httpService.get<DropDownListItemModel<string>[]>(`${environment.apiUrl}/globalidactivities/filter-value-dropdown?filterBy=${filterBy}&entityType=${EntityType.CustomerPO}&entityId=${poId}`);
    }

    /**
    * To get list of tabs available on UI
    * @param isAddMode If it is add mode
    * @param bookingStage Current booking stage
    * @returns
    */
    createNavigation(model: any): Array<PurchaseOrderTabModel> {
        const allSections = [
            {
                text: 'label.general',
                sectionId: 'general',
                selected: false,
                readonly: false
            },
            {
                text: 'label.product',
                sectionId: 'product',
                selected: false,
                readonly: false
            },
            {
                text: 'label.contact',
                sectionId: 'contact',
                selected: false,
                readonly: false
            },
            {
                text: 'label.termsAndInstructions',
                sectionId: 'termsAndInstructions',
                selected: false,
                readonly: false
            },
            {
                text: 'label.allocatedPO',
                sectionId: 'allocatedPO',
                selected: false,
                readonly: false
            },
            {
                text: 'label.fulfillment',
                sectionId: 'fulfillment',
                selected: false,
                readonly: false
            },
            {
                text: 'label.activity',
                sectionId: 'activity',
                selected: false,
                readonly: true
            },
            {
                text: 'label.dialog',
                sectionId: 'dialog',
                selected: false,
                readonly: false
            }
        ];
        let hiddenSectionIds = [];

        if (model.poType !== POType.Blanket) {
            hiddenSectionIds.push('allocatedPO');
        }

        if (!(model.poType === POType.Bulk || model.poType === model.allowToBookIn)) {
            hiddenSectionIds.push('fulfillment');
        }

        return allSections.filter(section => !hiddenSectionIds.includes(section.sectionId));
    }
}
