import { Injectable } from '@angular/core';
import { HttpService, FormService } from 'src/app/core';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { UserProfileModel } from 'src/app/core/models/user/user-profile.model';

@Injectable()
export class HomeService {
    affiliateCodes = null;
    delegatedOrgId: number = 0;
    customerRelationships: string = "";

    constructor(private httpService: HttpService) {
    }

    getTop5OceanVolumeByOrigin(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/shipments/top5OceanVolumeByOrigin?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getTop10ConsigneeThisWeek(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/shipments/top10ConsigneeThisWeek?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getTop10ShipperThisWeek(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/shipments/top10ShipperThisWeek?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getTop10CarrierThisWeek(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/shipments/top10CarrierThisWeek?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getTop5OceanVolumeByDestination(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/shipments/top5OceanVolumeByDestination?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getWeeklyReportingShipments(statisticFilter: string): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/shipments/reporting/weeklyShipments?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getWeeklyReportingPOs(statisticFilter: string): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/reporting?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getWeeklyReportingOceanVolume(statisticFilter: string): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/shipments/reporting/weeklyOceanVolume?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getMonthlyOceanVolumeByMovement(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/shipments/reporting/monthlyOceanVolume?groupBy=movement&affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getMonthlyOceanVolumeByServiceType(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/shipments/reporting/monthlyOceanVolume?groupBy=servicetype&affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getBookedPOStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/statistics/booked?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getUnbookedPOStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/statistics/unbooked?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getManagedToDatePOStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/statistics/managed-to-date?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getInOriginDCPOStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/statistics/in-origin-dc?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getInTransitPOStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/statistics/in-transit?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getCustomsClearedPOStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/statistics/customs-cleared?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getPendingDCDeliveryPOStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/statistics/pending-dc-delivery?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    getDCDeliveryConfirmedPOStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/purchaseOrders/statistics/dc-delivery-confirmed?affiliates=${this.affiliateCodes}&statisticFilter=${statisticFilter}`);
    }

    GetBookingAwaitingForSubmission(organizationId: number, userRole: string): Observable<number> {
        return this.httpService.get(`${environment.apiUrl}/pofulfillments/statistics/awaiting-for-submission?organizationId=${organizationId}&userRole=${userRole}`);
    }

    GetBookingPendingForApproval(): Observable<number> {
        return this.httpService.get(`${environment.apiUrl}/pofulfillments/statistics/pending-for-approval?affiliates=${this.affiliateCodes ?? ''}`);
    }

    getShortships(orgId: number): Observable<number> {
        return this.httpService.get(`${environment.apiUrl}/shortships/statistics/unread?affiliates=${this.affiliateCodes ?? ''}&orgId=${orgId}`);
    }

    GetMobileTodayUpdate(): Observable<any> {
        return this.httpService.get(`${environment.apiUrl}/mobileApplications/updates/today`);
    }

    reSubmit(userId) {
        return this.httpService.get(`${environment.apiUrl}/userRequests/${userId}/resubmit`);
    }

    getVesselArrivalStatistics(statisticFilter: string) {
        return this.httpService.get(`${environment.apiUrl}/FreightSchedulers/statistics/vessel-arrival?affiliates=${this.affiliateCodes}&delegatedOrgId=${this.delegatedOrgId}&customerRelationships=${this.customerRelationships}&statisticFilter=${statisticFilter}`);
    }
}
