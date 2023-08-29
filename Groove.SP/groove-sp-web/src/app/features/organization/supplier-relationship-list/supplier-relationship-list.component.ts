import { Component, OnInit, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { ListComponent } from 'src/app/core/list/list.component';
import { ConnectionType, OrganizationType } from 'src/app/core/models/enums/enums';
import { DropDowns, StringHelper, UserContextService } from 'src/app/core';
import { SupplierRelationshipListService } from './supplier-relationship-list.service';
import { faPlus, faPaperPlane } from '@fortawesome/free-solid-svg-icons';
import { NotificationPopup } from 'src/app/ui/notification-popup/notification-popup';
import { OrganizationFormService } from '../organization-form/organization-form.service';
import { AddSupplierRelationshipFormComponent } from '../add-supplier-relationship-form/add-supplier-relationship-form.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-supplier-relationship-list',
    templateUrl: './supplier-relationship-list.component.html',
    styleUrls: ['./supplier-relationship-list.component.scss']
})
export class SupplierRelationshipListComponent extends ListComponent implements OnInit {
    @Input() isViewMode: boolean;
    @Input() canEditInfor = false;
    @Input() affiliateList: any[];
    @Input() isAdmin = false;
    @Input() customerOrg = null;
    @Output() add: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild(AddSupplierRelationshipFormComponent, {static: false})
    AddSupplierRelationshipForm: AddSupplierRelationshipFormComponent;
    listName = 'organizations';
    isFormList = true;
    connectionType = ConnectionType;
    pageSizes = [10, 20];
    faPlus = faPlus;
    faPaperPlane = faPaperPlane;
    selectedSupplierList: any[];
    currentUser: any;

    addSupplierFormOpened: boolean;
    supplierModel: any;

    constructor(
        public notification: NotificationPopup,
        public service: SupplierRelationshipListService,
        public organizationService: OrganizationFormService,
        route: ActivatedRoute,
        location: Location,
        public translateService: TranslateService,
        private _userContextService: UserContextService) {
        super(service, route, location);
        this.route.params.subscribe((params) => {
            this.service.formListId = params.id;
            this.bindSelectedSupplierList();
        });

        this._userContextService.getCurrentUser().subscribe((user) => {
            if (user != null) {
                this.currentUser = user;
            }
        });
    }

    get isEditableCustomerRefId(): boolean {return this.currentUser && this.currentUser.isInternal;}


    onSupplierClick() {}

    resendEmail(supplier) {
        const msg = this.translateService.instant('msg.resendConnection',
        {
            orgName: supplier.contactEmail
        });
        const confirmDlg = this.notification.showConfirmationDialog(msg, 'label.organization');

        confirmDlg.result.subscribe(
        (result: any) => {
            if (result.value) {
                this.service.resendConnectionToSupplier(supplier.id, this.customerOrg.id).subscribe(user => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                    this.bindSelectedSupplierList();
                    this.ngOnInit();
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                });
            }
        });
    }

    bindSelectedSupplierList() {
        this.service.getSuppliers(this.service.formListId).subscribe(
            data => {
                this.selectedSupplierList = data;
        });
    }

    removeSupplier(dataItem, rowIndex) {
        const confirmDlg = this.notification.showConfirmationDialog('delete.saveConfirmation', 'label.organization');
            confirmDlg.result.subscribe(
                (result: any) => {
                    if (result.value) {
                        this.service.removeSupplierRelationship(this.service.formListId, dataItem.id).subscribe(
                            data => {
                                this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                                this.bindSelectedSupplierList();
                                this.ngOnInit();
                            },
                            error => {
                                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                            });
                    }
                });
    }

    onAddNewSupplierClick() {
        this.addSupplierFormOpened = true;
    }

    onSupplierFormClosed() {
        this.addSupplierFormOpened = false;
    }

    onSupplierSelected(supplier) {
        this.addSupplierFormOpened = false;

        if (this.affiliateList && this.affiliateList.length) {
            const confirmDlg = this.notification.showConfirmationDialog('msg.confirmAddSupplierToAffiliates', 'label.organization');

            confirmDlg.result.subscribe(
                (result: any) => {
                    supplier.isApplyAffiliates = result.value ? true : false;
                    this.service.addSupplierRelationship(this.service.formListId, supplier.id, supplier).subscribe(
                        data => {
                            this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                            this.bindSelectedSupplierList();
                            this.add.emit(supplier);
                            this.ngOnInit();
                        },
                        error => {
                            this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                        }
                    );
                });
        } else {
            this.service.addSupplierRelationship(this.service.formListId, supplier.id, supplier).subscribe(
                data => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                    this.bindSelectedSupplierList();
                    this.add.emit(supplier);
                    this.ngOnInit();
                },
                error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                }
            );
        }
    }

    onSupplierAdded(supplier) {
        this.addSupplierFormOpened = false;
        const comparedFields = ['name', 'contactEmail', 'contactNumber', 'websiteDomain'];
        let duplicatedFields = '';
        let duplicatedSupplier = null;
        supplier.isApplyAffiliates = false;

        this.service.getSuppliersBy(supplier.name,
            supplier.contactEmail,
            supplier.contactNumber,
            supplier.websiteDomain
            ).subscribe(rsp => {
                if (rsp.data.length > 0) {
                    const supplierList = rsp.data.filter(s => s.organizationType === OrganizationType.General);
                    for (let i = 0; i < supplierList.length; i++) {
                        comparedFields.forEach(field => {
                            if (supplierList[i][field] && supplier[field]
                                && supplierList[i][field].toLowerCase() === supplier[field].toLowerCase()) {
                                duplicatedFields += supplierList[i][field] + ', ';
                            }
                        });

                        if (duplicatedFields.length > 0) {
                            duplicatedSupplier = supplierList[i];
                            break;
                        }
                    }

                    duplicatedFields = duplicatedFields.trim().slice(0, -1);
                    if (duplicatedFields.length > 0) {
                        const confirmDlg = this.notification.showConfirmationDialog(`The creating Organization has the same
                        ${duplicatedFields} with ${duplicatedSupplier.name}. Do you want to connect with
                        ${duplicatedSupplier.name} instead?`, 'label.organization');

                        confirmDlg.result.subscribe(
                            (result: any) => {
                                if (result.value) {
                                    this.AddSupplierRelationshipForm.setSelectedSupplier(duplicatedSupplier);
                                    this.addSupplierFormOpened = true;
                                } else {
                                    this.addNewSupplier(supplier);
                                }
                        });
                    } else {
                        this.addNewSupplier(supplier);
                    }
                }
            });
    }

    private addNewSupplier(supplier: any) {
        if (this.affiliateList && this.affiliateList.length) {
            const confirmDlg = this.notification.showConfirmationDialog('msg.confirmAddSupplierToAffiliates', 'label.organization');
            confirmDlg.result.subscribe((result: any) => {
                if (result.value) {
                    supplier.isApplyAffiliates = true;
                }
                this.service.addSupplier(this.service.formListId, supplier).subscribe((data: any) => {
                    this.notification.showSuccessPopup('save.sucessNotification', 'label.organization'); 
                    this.bindSelectedSupplierList();
                    this.add.emit(data);
                    this.ngOnInit();
                }, error => {
                    this.notification.showErrorPopup('save.failureNotification', 'label.organization');
                });
            });
        } else {
            this.service.addSupplier(this.service.formListId, supplier).subscribe((data: any) => {
                this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
                this.bindSelectedSupplierList();
                this.add.emit(data);
                this.ngOnInit();
            }, error => {
                this.notification.showErrorPopup('save.failureNotification', 'label.organization');
            });
        }
    }

    saveCustomerRefId(supplierId, event) {
        this.organizationService.updateCustomerRefId(this.customerOrg.id, supplierId, event).subscribe(data => {
            this.notification.showSuccessPopup('save.sucessNotification', 'label.organization');
        }, error => {
            this.notification.showErrorPopup('save.failureNotification', 'label.organization');
        });
    }
}
