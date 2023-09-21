import { Injectable } from '@angular/core';
import { HttpService, FormService, StringHelper, POType, DropDownListItemModel, EntityType, ModeOfTransportType, FormModeType } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { take, tap } from 'rxjs/operators';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { NoteModel } from 'src/app/core/models/note.model';
import { CarrierModel } from 'src/app/core/models/carrier.model';
import { OrganizationReferenceDataModel, UserOrganizationProfileModel } from 'src/app/core/models/organization.model';

@Injectable()
export class POFulfillmentFormService extends FormService<any> {

    constructor(httpService: HttpService) {
        super(httpService, `${environment.apiUrl}/pofulfillments`);
    }

    private _customerPOs: Array<any> = null;
    public currentCustomerPOs$ = new BehaviorSubject<Array<any>>(this._customerPOs);

    private _buyerCompliance: any = null;
    public buyerCompliance$ = new BehaviorSubject<any>(this._buyerCompliance);

    private _organizationsCached: Array<OrganizationReferenceDataModel> = [];
    private _carriersCached: Array<CarrierModel> = [];

    // 15-09-2023 /CR/
    public _buyerComplianceData$ = new BehaviorSubject([]);
    public _buyerComplianceData = this._buyerComplianceData$.asObservable();


    public currentCustomerPOs = () => this._customerPOs;

    getDefaultMilestone(modeOfTransport: ModeOfTransportType) {
        let milestone =
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

        let activityCode = '';
        if (modeOfTransport === ModeOfTransportType.Sea) {
            activityCode = '7001';
        }

        if (modeOfTransport === ModeOfTransportType.Air) {
            activityCode = '7003';
        }

        if (activityCode) {
            milestone.push({
                activityCode: activityCode,
                milestone: 'label.shipmentDispatch'
            });
        }

        milestone = milestone.concat(
            [
                {
                    activityCode: '1071',
                    milestone: 'label.closed'
                },
            ]
        )

        return milestone.reverse();
    }

    // Override methods

    // Override default method to add flag updateOrganizationPreferences
    public create(model: any): Observable<any> {
        return this.httpService.create(`${this.apiUrl}?updateOrganizationPreferences=true`, model);
    }

    // Override default method to add flag updateOrganizationPreferences
    public update(id: string, model: any): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}?updateOrganizationPreferences=true`, model);
    }

    // End Override methods

    public reload(id: string, model: any): Observable<any> {
        return this.httpService.update(`${this.apiUrl}/${id}/re-load`, model);
    }

    getOrganizations(): Observable<OrganizationReferenceDataModel[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/organizations/orgReferenceData/active`)
            .pipe(
                tap((data: Array<OrganizationReferenceDataModel>) => {
                    this._organizationsCached = data;
                })
            );
    }

    getBuyerCompliance(customerId: any): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/buyercompliances`, { organizationId: customerId }).pipe(
            take(1),
            tap(x => {
                this._buyerCompliance = x;
                this.buyerCompliance$.next(x);
            })
        );
    }

    getItineraryByShipmentId(shipmentNo, viewSettingModuleId: string) {
        return this.httpService.get(`${environment.apiUrl}/shipments/${shipmentNo}/itineraries?&viewSettingModuleId=${viewSettingModuleId}`);
    }

    getCountries(): Observable<any> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/countries/dropDownCode`);
    }

    getBuyers(): Observable<any[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/organizations/buyers`);
    }

    getOwnerOrgInfo(username: string): Observable<UserOrganizationProfileModel> {
        return this.httpService.get<UserOrganizationProfileModel>(`${environment.apiUrl}/users/${username}/organization`);
    }

    getBuyersByOrgId(id): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}/buyers`);
    }

    getAllLocations(): Observable<any[]> {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/locations`);
    }

    getLocation(id: number): Observable<any> {
        return this.httpService.get<any>(`${environment.commonApiUrl}/locations/${id}`);
    }

    getDropdownCarriers(): Observable<any[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/carriers/DropDown`);
    }

    getAllCarriers(): Observable<CarrierModel[]> {
        return this.httpService.getWithCache(`${environment.commonApiUrl}/carriers`)
            .pipe(
                tap((data: Array<CarrierModel>) => {
                    this._carriersCached = data;
                })
            );
    }

    getCarrier(carrierCode) {
        return this.httpService.get<any>(`${environment.commonApiUrl}/carriers`, { code: carrierCode });
    }

    getCarrierByCode(carrierCode): Observable<CarrierModel> {
        // Check if available in cache
        const cachedData = this._carriersCached.find(y => carrierCode === y.carrierCode);
        if (cachedData) {
            return of(cachedData);
        }
        return this.httpService.get<CarrierModel>(`${environment.commonApiUrl}/carriers/code`, { code: carrierCode });
    }

    /**
     * Fetch organization reference information. Lookup in cache before calling request to server
     */
    getOrganizationsByIds(idList: Array<number>): Observable<OrganizationReferenceDataModel[]> {
        // Distinct the values
        idList = idList.filter(this._onlyUnique);

        // Check if available in cache
        const cachedData = this._organizationsCached.filter(y => idList.indexOf(y.id) >= 0);
        if (cachedData.length === idList.length) {
            return of(cachedData);
        }

        // Call to server
        return this.httpService.get<OrganizationReferenceDataModel[]>(`${environment.commonApiUrl}/organizations/orgReferenceData`, { idList: idList });
    }

    getOrganizationRoles(): Observable<any[]> {
        return this.httpService.getWithCache<any[]>(`${environment.commonApiUrl}/organizationroles`);
    }

    getAffiliateCodes(id: number): Observable<any[]> {
        return this.httpService.get(`${environment.commonApiUrl}/organizations/${id}/affiliatecodes`);
    }

    /**To get list of customer PO by Purchase Order Ids
     * It fires request to server, then store into variable on memory
    */
    getCustomerPOs(purchaseOrderIds: Array<number>, customerOrgId: number) {

        // Distinct the values
        purchaseOrderIds = purchaseOrderIds.filter(this._onlyUnique);

        let paramPurchaseOrderIds = '';
        if (!StringHelper.isNullOrEmpty(purchaseOrderIds)) {
            paramPurchaseOrderIds = `${purchaseOrderIds.toString()}`;
        }
        // Try to get organization code from cached
        let orgCode = this._organizationsCached.find(x => x.id === customerOrgId)?.code;

        if (orgCode) {
            this.httpService
                .get<any[]>(`${environment.apiUrl}/purchaseOrders/customerPurchaseOrders/list?replacedByOrganizationReferences=true&poIds=${paramPurchaseOrderIds}&customerOrgCode=${encodeURIComponent(orgCode)}`)
                .subscribe(
                    res => {
                        this._customerPOs = res;
                        this.currentCustomerPOs$.next(res);
                    }
                );
        } else {
            // If not, fire request to get org code by id
            // Then call another request to get customer POs
            this.getOrganizationsByIds([customerOrgId]).map((data) => {
                orgCode = data[0].code;
                return this.httpService
                    .get<any[]>(`${environment.apiUrl}/purchaseOrders/customerPurchaseOrders/list?replacedByOrganizationReferences=true&poIds=${paramPurchaseOrderIds}&customerOrgCode=${encodeURIComponent(orgCode)}`)
                    .subscribe(
                        res => {
                            this._customerPOs = res;
                            this.currentCustomerPOs$.next(res);
                        }
                    );
            }).subscribe();
        }
    }

    /**Store loaded purchase orders into memory.
     * If already, ignore.
     */
    cacheCustomerPOs(purchaseOrders: Array<any>) {
        if (!StringHelper.isNullOrEmpty(purchaseOrders)) {
            if (StringHelper.isNullOrEmpty(this._customerPOs)) {
                this._customerPOs = [];
            }
            purchaseOrders.map(po => {
                const isExisted = this._customerPOs.find(x => x.id === po.id);
                if (StringHelper.isNullOrEmpty(isExisted)) {
                    this._customerPOs.push(po);
                }
            });
        }
    }

    resetCustomerPOs = () => {
        this._customerPOs = null;
        this.currentCustomerPOs$.next(null);
    }

    getAllPorts(): any {
        return this.httpService.get<any[]>(`${environment.commonApiUrl}/ports`);
    }

    getPurchaseOrderReplacedByOrgPreferences(poId: number) {
        return this.httpService.get<any>(`${environment.apiUrl}/purchaseorders/${poId}?replacedByOrganizationReferences=true&affiliates=${this.affiliateCodes}`);
    }

    bookingValidation(id): Observable<any[]> {
        return this.httpService.update(`${environment.apiUrl}/pofulfillments/${id}/bookingValidation`);
    }

    cancelPOFulfillment(id: number, reason: string): Observable<any> {
        return this.httpService.update(`${environment.apiUrl}/pofulfillments/${id}/cancelPOFulfillment`, { reason });
    }

    createPOFulfillmentBookingRequest(id: number): Observable<any> {
        return this.httpService.create(`${environment.apiUrl}/pofulfillments/${id}/bookingRequests`);
    }

    /**
    * To check result of booking validation without any data updates.
    * @param id Purchase order fulfillment id
    * @returns Json object
    */
    trialValidateBooking(id: number): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/pofulfillments/${id}/trialValidateBooking`);
    }

    amendPOFulfillment(id: number): Observable<any> {
        return this.httpService.update(`${environment.apiUrl}/pofulfillments/${id}/amendPOFulfillment`);
    }

    planToShip(id: number): Observable<any> {
        return this.httpService.update(`${environment.apiUrl}/pofulfillments/${id}/planToShip`);
    }

    dispatch(id: number): Observable<any> {
        return this.httpService.update(`${environment.apiUrl}/pofulfillments/${id}/dispatch`);
    }

    getActivities(id: number, request: any): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/globalidactivities/get-by-poff/${id}`, request);
    }

    getTotalActivity(id: number): Observable<number> {
        return this.httpService.get(`${environment.apiUrl}/globalidactivities/get-total-by-poff/${id}`);
    }

    concatenateAddressLines(address: string, addressLine2?: string, addressLine3?: string, addressLine4?: string) {
        let concatenatedAddress = address;
        if (!StringHelper.isNullOrEmpty(addressLine2)) {
            concatenatedAddress += '\n' + addressLine2;
        }
        if (!StringHelper.isNullOrEmpty(addressLine3)) {
            concatenatedAddress += '\n' + addressLine3;
        }
        if (!StringHelper.isNullOrEmpty(addressLine4)) {
            concatenatedAddress += '\n' + addressLine4;
        }
        return concatenatedAddress;
    }

    /**To fetch purchase orders from server with paging */
    fetchPOsDataSource(
        customerOrgId: number,
        customerOrgCode: string,
        supplierOrgId: number,
        supplierCompanyName: string,
        skip: number,
        take: number,
        searchTeam: string,
        searchType: string,
        selectedPOType: POType,
        selectedPOId?: number): Observable<any> {

        if (StringHelper.isNullOrEmpty(customerOrgId) || customerOrgId === 0
            || StringHelper.isNullOrEmpty(supplierOrgId)) {
            return of([]);
        } else {
            const searchTermQueryString = !StringHelper.isNullOrEmpty(searchTeam) ? `&searchTerm=${encodeURIComponent(searchTeam.trim())}` : '';
            let url = `${environment.apiUrl}/purchaseorders/customerPurchaseOrders/search?replacedByOrganizationReferences=true&customerOrgId=${customerOrgId}&customerOrgCode=${customerOrgCode}&SupplierOrgId=${supplierOrgId}&supplierCompanyName=${encodeURIComponent(supplierCompanyName)}`;
            url += `&selectedPOId=${selectedPOId}&selectedPOType=${selectedPOType}&affiliates=${this.affiliateCodes}`;
            url += `&skip=${skip}&take=${take}&searchType=${searchType}${searchTermQueryString}`;
            return this.httpService.get<any>(url);
        }
    }

    getNotes(poFulfillmentId: number): Observable<NoteModel[]> {
        return this.httpService.get(`${environment.apiUrl}/pofulfillments/${poFulfillmentId}/notes`);
    }

    getMasterNotes(poFulfillmentId: number) {
        return this.httpService.get(`${environment.apiUrl}/pofulfillments/${poFulfillmentId}/masterDialogs`);
    }

    /**
     * 
     * @param organizationId Agent OrganizationId
     * @param customerId Principal OrganizationId
     * @param locationId ShipfromId
     * @returns 
     */
    getEmailNotification(organizationId: number, customerId: number, locationId: number) {
        return this.httpService.get(`${environment.commonApiUrl}/emailNotifications?organizationId=${organizationId}&customerId=${customerId}&locationId=${locationId}`);
    }

    getFilterActivityValueDropdown(filterBy: string, poffId: number): Observable<DropDownListItemModel<string>[]> {
        return this.httpService.get<DropDownListItemModel<string>[]>(`${environment.apiUrl}/globalidactivities/filter-value-dropdown?filterBy=${filterBy}&entityType=${EntityType.POFulfillment}&entityId=${poffId}`);
    }
}