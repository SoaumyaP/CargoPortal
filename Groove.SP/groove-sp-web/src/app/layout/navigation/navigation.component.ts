import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { PanelBarExpandMode } from '@progress/kendo-angular-layout';
import { AppPermissions } from 'src/app/core/auth/auth-constants';
import { Router } from '@angular/router';

@Component({
    selector: 'app-navigation',
    templateUrl: './navigation.component.html',
    styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent implements OnInit {
    @ViewChild('kendoPanelBarInstance', { static: true }) kendoPanelBarInstance: any;
    public expandMode: number = PanelBarExpandMode.Single;
    @Input() navbarExpand: boolean;
    public expandedItems: Array<string> = [];
    translationLet = [];
    public lastSelectedItem: any;
    public Menu = {
        dashboard: 'home',
        products: 'products',
        orders: 'orders',
        cruiseOrders: 'cruise-orders',
        shipments: 'shipments',
        billOfLadings: 'billOfLadings',
        masterBOL: 'master-bill-of-ladings',
        consignments: 'consignments',
        invoices: 'invoices',
        organizations: 'organizations',
        usersAndRoles: 'users-and-roles',
        reports: 'reports',
        integrationLogs: 'integration-logs',
        balanceOfGoods: 'balanceOfGoods',
    };

    readonly AppPermissions = AppPermissions;

    constructor(private router: Router) {
        const endOfparam =  this.router.url.indexOf('?') === -1 ? this.router.url.length : (this.router.url.indexOf('?') - 1);
        this.lastSelectedItem = this.router.url.substr(1, endOfparam);
    }

    ngOnInit() {
    }

    public onMenuItemClick(itemName: string, event: any = null, isChild: boolean = false): void {
        if (isChild) {
            if (event) {
                event.stopPropagation();
            }
            if (!this.navbarExpand) {
               this.expandedItems = this.expandedItems.filter(item => item !== itemName);
            }
        } else {
            if (this.navbarExpand) {
                if (this.expandedItems.indexOf(itemName) >= 0) {
                    this.expandedItems = [];
                } else {
                    this.expandedItems = [itemName];
                }
            } else {
                if (this.expandedItems.indexOf(itemName) >= 0) {
                    this.expandedItems = this.expandedItems.filter(item => item !== itemName);
                    if (this.expandedItems.length - 1 >= 0) {
                        this.lastSelectedItem = this.expandedItems[this.expandedItems.length - 1];
                    } else {
                        this.lastSelectedItem = '';
                    }
                } else {
                    this.expandedItems.pop();
                    this.expandedItems.push(itemName);
                    this.lastSelectedItem = itemName;
                }
            }
        }
    }

    public checkExpandItem(itemName: string) {
        const result = this.expandedItems.indexOf(itemName) !== -1;
        return result;
    }

    isSelect(keyword) {
        const result = keyword === this.lastSelectedItem;
        return result;
    }
}