import { Injectable } from '@angular/core';
import { FormService, HttpService, StringHelper, BuyerComplianceStatus, BuyerComplianceStage, POType, Roles } from 'src/app/core';
import { Observable, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { map } from 'rxjs/operators';

export interface VerificationPrincipal {
    PrincipalName: string;
    WillCompareIfValue: number;
    PropertyToCompare: string;
}

@Injectable()
export class SelectPosFormService {

    private _poVerificationPrincipals: Array<VerificationPrincipal> = [
        {PrincipalName: 'expectedShipDateVerification', WillCompareIfValue: 10, PropertyToCompare: 'expectedShipDate'},
        {PrincipalName: 'expectedDeliveryDateVerification', WillCompareIfValue: 10, PropertyToCompare: 'expectedDeliveryDate'},
        {PrincipalName: 'consigneeVerification', WillCompareIfValue: 10, PropertyToCompare: 'consigneeId'},
        {PrincipalName: 'shipperVerification', WillCompareIfValue: 10, PropertyToCompare: 'supplierId'},
        {PrincipalName: 'shipFromLocationVerification', WillCompareIfValue: 10, PropertyToCompare: 'shipFromId'},
        {PrincipalName: 'shipToLocationVerification', WillCompareIfValue: 10, PropertyToCompare: 'shipToId'},
        {PrincipalName: 'modeOfTransportVerification', WillCompareIfValue: 10, PropertyToCompare: 'modeOfTransport'},
        {PrincipalName: 'incotermVerification', WillCompareIfValue: 10, PropertyToCompare: 'incoterm'},
        {PrincipalName: 'preferredCarrierVerification', WillCompareIfValue: 10, PropertyToCompare: 'carrierCode'}
    ];

    constructor(private _httpService: HttpService) {
    }

    getPrincipalDataSource(roleId: number, organizationId: number, affiliates: string): Observable<any> {
        let url = `${environment.apiUrl}/purchaseOrders/principalsbypos?roleId=${roleId}&affiliates=${affiliates || '[]'}&organizationId=${organizationId || 0}`;
        return this._httpService.get<any>(url);
    }

    getSourcePOsDataSource(principalId: number,
        organizationId: number,
        customerRelationships: string,
        skip: number,
        take: number,
        searchTeam: string,
        roleId: number,
        selectedPOId?: number,
        affiliates?:string): Observable<any> {
        if (StringHelper.isNullOrEmpty(principalId) || principalId === 0) {
            return of([]);
        } else {
        const searchTermQueryString = !StringHelper.isNullOrEmpty(searchTeam) ? `&searchTerm=${encodeURIComponent(searchTeam.trim())}` : '';
        let url = `${environment.apiUrl}/purchaseorders/purchaseOrderSelections/${principalId}?organizationId=${organizationId || 0}`;
        url += `&selectedPOId=${selectedPOId || 0}&roleId=${roleId}&affiliates=${affiliates}&customerRelationships=${customerRelationships || 0}`;
        url += `&skip=${skip}&take=${take}${searchTermQueryString}`;
        return this._httpService.get<any>(url);
        }
    }

    getActiveBuyerCompliance(customerId: any): Observable<any> {
        if (StringHelper.isNullOrEmpty(customerId) || customerId === 0) {
            return of([]);
        } else {
        return this._httpService.get<any[]>(`${environment.apiUrl}/buyercompliances`, { organizationId: customerId }).pipe(
            map(compliances => compliances.find(bc => bc.status == BuyerComplianceStatus.Active && bc.stage == BuyerComplianceStage.Activated)));
        }
    }

    validatePOsAgainstBuyerCompliance$(purchaseOrders: Array<any>, preferredBuyerCompliance?: any) {

        if (StringHelper.isNullOrEmpty(purchaseOrders) || purchaseOrders.length === 0) {
            return of(false);
        }

        if (StringHelper.isNullOrEmpty(preferredBuyerCompliance)) {

            const comparedTo = purchaseOrders[0];
            const principalContact = comparedTo.contacts && comparedTo.contacts.filter(x => x.organizationRole === 'Principal');
            let principalOrganizationId = 0;

            if (principalContact && principalContact.length > 0) {
                principalOrganizationId =  principalContact[0].organizationId;
            }

            if (principalOrganizationId === 0) {
                return of(false);
            }

            // Check all purchase order with same principal
            const isValid = !purchaseOrders.some(x => x.contacts.filter(y => y.organizationRole === 'Principal')[0].organizationId !== principalOrganizationId);
            if (!isValid) {
                return of(false);
            }

            return this.getActiveBuyerCompliance(principalOrganizationId).pipe(
                map(buyerCompliance => {
                    return this._validatePurchaseOrders(purchaseOrders, buyerCompliance);
                })
            );

        } else {
            return of(preferredBuyerCompliance).pipe(
                map(buyerCompliance => {
                    return this._validatePurchaseOrders(purchaseOrders, buyerCompliance);
                })
            );
        }
    }

    private _validatePurchaseOrders(purchaseOrders: Array<any>, buyerCompliance: any): boolean {
        let isValid = true;

        // Check on PO Types here: all same type and matched with buyer compliance settings
        const comparedTo = purchaseOrders[0];
        isValid = !(purchaseOrders.some(x => x.poType !== comparedTo.poType) ||
        purchaseOrders.some(x => x.poType !== buyerCompliance.allowToBookIn && x.poType !== POType.Bulk));

        if (!isValid) {
            return isValid;
        }

        // In case there is one PO selected, not need to check on po verifications.
        if (purchaseOrders.length === 1) {
            return true;
        }

        // Check against PO verification settings
        const poPrincipalChecks = this._poVerificationPrincipals;

        const poVerificationSettings = buyerCompliance.purchaseOrderVerificationSetting;

        poPrincipalChecks.map((principal) => {
                const needCheck = poVerificationSettings[principal.PrincipalName] === principal.WillCompareIfValue;
                if (needCheck) {
                    isValid = isValid && !purchaseOrders.some(
                        (x) =>
                            (x[principal.PropertyToCompare] && x[principal.PropertyToCompare].toString())
                            !== (comparedTo[principal.PropertyToCompare] && comparedTo[principal.PropertyToCompare].toString())
                        );
                }
            });

        return isValid;
    }

    get poVerificationPrincipals() {
        return this._poVerificationPrincipals;
    }
}
