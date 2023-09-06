import { Component } from '@angular/core';
import { FormComponent } from 'src/app/core/form';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { TranslateService } from '@ngx-translate/core';
import { RoleFormService } from './role-form.service';
import { of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { CheckableSettings, TreeItemLookup } from '@progress/kendo-angular-treeview';
import { DATE_FORMAT } from 'src/app/core/helpers/date.helper';
import { RoleStatus } from 'src/app/core';
import { StringHelper } from 'src/app/core/helpers/string.helper';

@Component({
    selector: 'app-role-form',
    templateUrl: './role-form.component.html',
    styleUrls: ['./role-form.component.scss']
})
export class RoleFormComponent extends FormComponent {

    modelName = 'roles';
    roleStatus = RoleStatus;
    DATE_FORMAT = DATE_FORMAT;
    initData = [
        { sourceUrl: `${environment.apiUrl}/permissions/list` }
    ];

    permissionList: any[] = [];
    checkedKeys: any[] = [];
    checkedPermissionId: any[] = [];
    expandedKeys: any[] = [];

    hasChildren(item: any) {
        return item.items && item.items.length > 0;
    }

    fetchChildren(item: any) {
        return of(item.items);
    }

    public get checkableSettings(): CheckableSettings {
        return {
            checkChildren: false,
            checkParents: true,
            enabled: true,
            mode: 'multiple',
            checkOnClick: false
        };
    }

    constructor(protected route: ActivatedRoute,
        public service: RoleFormService,
        public notification: NotificationPopup,
        public translateService: TranslateService,
        public router: Router) {
        super(route, service, notification, translateService, router);
    }

    onInitDataLoaded(data) {
        this.checkedKeys = [];
        this.permissionList = [];
        data[0].forEach(element => {
            element.text = element.name;
        });
        this.buildTree(this.permissionList, data[0], '');
        this.buildCheckedList(this.permissionList);
    }

    buildTree(currentNode, currentList, node) {
        for (let i = 0, kIndex = 0; i < currentList.length; i++, kIndex++) {
            if (currentList[i].name.trim().length > 0) {
                const index = currentList[i].name.indexOf('.');
                // parent node
                if (index < 0) {
                    const parentNode = currentList[i].name;
                    const newNode = node + (node.length > 0 ? '_' : '') + kIndex;
                    currentNode.push({
                        text: StringHelper.translateFormat(currentList[i].text),
                        customIndex: newNode,
                        id: currentList[i].id,
                        items: []
                    });

                    // filter child list
                    const childList = [];

                    currentList.splice(i, 1);
                    i--;

                    for (let j = 0; j < currentList.length; j++) {
                        if (currentList[j].name.startsWith(parentNode)) {
                            childList.push({
                                name: currentList[j].name,
                                id: currentList[j].id
                            });

                            // remove parent node
                            childList[childList.length - 1].name =
                            childList[childList.length - 1].name.substring(parentNode.length + 1);

                            childList[childList.length - 1].text = currentList[j].text;

                            currentList.splice(j, 1);
                            j--;
                        }
                    }
                    this.buildTree(currentNode[currentNode.length - 1].items, childList, newNode);
                }
            }
        }
    }

    handleChecking(itemLookup: TreeItemLookup): void {
        // select parent node
        const currentNode = itemLookup.item.index;
        const nodeList = currentNode.split('_');
        if (nodeList.length > 0) {
            let currentIndex = nodeList[0];

            // not exists
            if (this.checkedKeys.indexOf(currentIndex) < 0 && currentNode !== currentIndex) {
                this.checkedKeys.push(currentIndex);
            }
            for (let i = 1; i < nodeList.length; i++) {
                currentIndex += '_' + nodeList[i];
                // not exists
                if (this.checkedKeys.indexOf(currentIndex) < 0 && currentNode !== currentIndex) {
                    this.checkedKeys.push(currentIndex);
                }
            }

            // loop to clear all children if un tick a parent node
            // if un tick
            if (this.checkedKeys.indexOf(currentNode) >= 0) {
                this.untickChildList(itemLookup.item.dataItem.items);
            }
        }
    }

    untickChildList(currentList) {
        for (let i = 0; i < currentList.length; i++) {
            // remove child from checked list
            const index = this.checkedKeys.indexOf(currentList[i].customIndex);
            if (index >= 0) {
                this.checkedKeys.splice(index, 1);
            }
            // has children
            if (currentList[i].items.length > 0) {
                this.untickChildList(currentList[i].items);
            }
        }
    }

    backList() {
        this.router.navigate(['/roles']);
    }

    filterPermissionIdList(currentList) {
        for (let i = 0; i < currentList.length; i++) {
            // filter permission id
            if (this.checkedKeys.indexOf(currentList[i].customIndex) >= 0) {
                this.checkedPermissionId.push(currentList[i].id);
            }
            // has children
            if (currentList[i].items.length > 0) {
                this.filterPermissionIdList(currentList[i].items);
            }
        }
    }

    buildCheckedList(currentList) {
        for (let i = 0; i < currentList.length; i++) {
            // check permission
            if (this.model.permissionIds.indexOf(currentList[i].id) >= 0) {
                this.checkedKeys.push(currentList[i].customIndex);
            }
            // has children
            if (currentList[i].items.length > 0) {
                this.buildCheckedList(currentList[i].items);
            }
        }
    }

    saveRole() {
        this.checkedPermissionId = [];
        this.filterPermissionIdList(this.permissionList);
        this.model.permissionIds = this.checkedPermissionId;
        this.service.update(this.model.id, this.model).subscribe(
            role => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.role');
                this.ngOnInit();
            },
            error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.role');
            });
    }
}
